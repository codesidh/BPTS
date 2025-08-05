import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../components/ui/tabs';
import { Alert, AlertDescription } from '../components/ui/alert';
import { Progress } from '../components/ui/progress';
import { api } from '../services/api';

interface HealthCheckResult {
  isHealthy: boolean;
  status: string;
  description: string;
  responseTime: number;
  data: Record<string, any>;
}

interface ComprehensiveHealthReport {
  timestamp: string;
  overallHealth: boolean;
  overallStatus: string;
  totalCheckTime: number;
  summary: Record<string, any>;
  databaseChecks: HealthCheckResult[];
  cacheChecks: HealthCheckResult[];
  externalServiceChecks: HealthCheckResult[];
  systemChecks: HealthCheckResult[];
  applicationChecks: HealthCheckResult[];
}

interface MetricData {
  metricName: string;
  value: number;
  timestamp: string;
  tags?: Record<string, string>;
}

const Monitoring: React.FC = () => {
  const [healthReport, setHealthReport] = useState<ComprehensiveHealthReport | null>(null);
  const [metrics, setMetrics] = useState<MetricData[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [autoRefresh, setAutoRefresh] = useState(true);

  const fetchHealthReport = async () => {
    try {
      const response = await api.get('/api/monitoring/health');
      setHealthReport(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to fetch health report');
      console.error('Error fetching health report:', err);
    }
  };

  const fetchMetrics = async () => {
    try {
      // This would typically fetch metrics from the API
      // For now, we'll simulate some metrics
      const mockMetrics: MetricData[] = [
        {
          metricName: 'cpu_usage_percent',
          value: Math.random() * 100,
          timestamp: new Date().toISOString(),
          tags: { instance: 'web-server-1' }
        },
        {
          metricName: 'memory_usage_mb',
          value: Math.random() * 1000,
          timestamp: new Date().toISOString(),
          tags: { instance: 'web-server-1' }
        },
        {
          metricName: 'request_count',
          value: Math.floor(Math.random() * 1000),
          timestamp: new Date().toISOString(),
          tags: { endpoint: '/api/workrequests' }
        }
      ];
      setMetrics(mockMetrics);
    } catch (err) {
      console.error('Error fetching metrics:', err);
    }
  };

  useEffect(() => {
    fetchHealthReport();
    fetchMetrics();
    setLoading(false);

    if (autoRefresh) {
      const interval = setInterval(() => {
        fetchHealthReport();
        fetchMetrics();
      }, 30000); // Refresh every 30 seconds

      return () => clearInterval(interval);
    }
  }, [autoRefresh]);

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'healthy':
        return 'bg-green-500';
      case 'warning':
        return 'bg-yellow-500';
      case 'unhealthy':
        return 'bg-red-500';
      case 'disabled':
        return 'bg-gray-500';
      default:
        return 'bg-blue-500';
    }
  };

  const renderHealthChecks = (checks: HealthCheckResult[], title: string) => (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center justify-between">
          {title}
          <Badge className={getStatusColor(
            checks.every(c => c.isHealthy) ? 'Healthy' : 'Unhealthy'
          )}>
            {checks.every(c => c.isHealthy) ? 'Healthy' : 'Issues Detected'}
          </Badge>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {checks.map((check, index) => (
            <div key={index} className="border rounded-lg p-4">
              <div className="flex items-center justify-between mb-2">
                <span className="font-medium">{check.description}</span>
                <Badge className={getStatusColor(check.status)}>
                  {check.status}
                </Badge>
              </div>
              <div className="text-sm text-gray-600">
                Response Time: {check.responseTime}ms
              </div>
              {Object.keys(check.data).length > 0 && (
                <div className="mt-2">
                  <details className="text-sm">
                    <summary className="cursor-pointer">Details</summary>
                    <pre className="mt-2 p-2 bg-gray-100 rounded text-xs overflow-auto">
                      {JSON.stringify(check.data, null, 2)}
                    </pre>
                  </details>
                </div>
              )}
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );

  const renderMetrics = () => (
    <Card>
      <CardHeader>
        <CardTitle>System Metrics</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {metrics.map((metric, index) => (
            <div key={index} className="border rounded-lg p-4">
              <div className="flex items-center justify-between mb-2">
                <span className="font-medium">{metric.metricName}</span>
                <span className="text-sm text-gray-600">
                  {new Date(metric.timestamp).toLocaleTimeString()}
                </span>
              </div>
              <div className="text-2xl font-bold">{metric.value.toFixed(2)}</div>
              {metric.tags && (
                <div className="mt-2 flex gap-1">
                  {Object.entries(metric.tags).map(([key, value]) => (
                    <Badge key={key} variant="outline" className="text-xs">
                      {key}: {value}
                    </Badge>
                  ))}
                </div>
              )}
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );

  if (loading) {
    return (
      <div className="container mx-auto p-6">
        <div className="flex items-center justify-center h-64">
          <div className="text-lg">Loading monitoring data...</div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-3xl font-bold">System Monitoring</h1>
        <div className="flex items-center gap-4">
          <Button
            onClick={() => {
              setAutoRefresh(!autoRefresh);
              if (!autoRefresh) {
                fetchHealthReport();
                fetchMetrics();
              }
            }}
            variant={autoRefresh ? "default" : "outline"}
          >
            {autoRefresh ? "Auto-refresh ON" : "Auto-refresh OFF"}
          </Button>
          <Button onClick={fetchHealthReport} variant="outline">
            Refresh Now
          </Button>
        </div>
      </div>

      {error && (
        <Alert className="mb-6">
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      {healthReport && (
        <div className="mb-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center justify-between">
                Overall System Health
                <Badge className={getStatusColor(healthReport.overallStatus)}>
                  {healthReport.overallStatus}
                </Badge>
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                <div className="text-center">
                  <div className="text-2xl font-bold">
                    {healthReport.summary.healthy_checks || 0}
                  </div>
                  <div className="text-sm text-gray-600">Healthy Checks</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-red-600">
                    {healthReport.summary.unhealthy_checks || 0}
                  </div>
                  <div className="text-sm text-gray-600">Unhealthy Checks</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold">
                    {healthReport.summary.total_checks || 0}
                  </div>
                  <div className="text-sm text-gray-600">Total Checks</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold">
                    {healthReport.totalCheckTime.toFixed(0)}ms
                  </div>
                  <div className="text-sm text-gray-600">Check Time</div>
                </div>
              </div>
              <div className="mt-4">
                <div className="text-sm text-gray-600 mb-2">Health Score</div>
                <Progress 
                  value={
                    healthReport.summary.total_checks > 0
                      ? (healthReport.summary.healthy_checks / healthReport.summary.total_checks) * 100
                      : 0
                  }
                  className="w-full"
                />
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      <Tabs defaultValue="health" className="space-y-6">
        <TabsList>
          <TabsTrigger value="health">Health Checks</TabsTrigger>
          <TabsTrigger value="metrics">Metrics</TabsTrigger>
          <TabsTrigger value="logs">Logs</TabsTrigger>
        </TabsList>

        <TabsContent value="health" className="space-y-6">
          {healthReport && (
            <>
              {renderHealthChecks(healthReport.databaseChecks, 'Database Health')}
              {renderHealthChecks(healthReport.cacheChecks, 'Cache Health')}
              {renderHealthChecks(healthReport.externalServiceChecks, 'External Services Health')}
              {renderHealthChecks(healthReport.systemChecks, 'System Health')}
              {renderHealthChecks(healthReport.applicationChecks, 'Application Health')}
            </>
          )}
        </TabsContent>

        <TabsContent value="metrics" className="space-y-6">
          {renderMetrics()}
        </TabsContent>

        <TabsContent value="logs" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Application Logs</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-center text-gray-600 py-8">
                <p>Log viewing functionality will be implemented here.</p>
                <p className="text-sm mt-2">
                  This will connect to Elasticsearch to display real-time application logs.
                </p>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default Monitoring; 