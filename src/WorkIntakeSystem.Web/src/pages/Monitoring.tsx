import React, { useState, useEffect } from 'react';
import {
  Card,
  CardContent,
  CardHeader,
  Button,
  Tabs,
  Tab,
  Alert,
  LinearProgress,
  Box,
  Typography,
  Grid,
  Chip
} from '@mui/material';
import { apiService } from '../services/api';

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
  const [autoRefresh] = useState(true);
  const [activeTab, setActiveTab] = useState(0);

  const fetchHealthReport = async () => {
    try {
      const response = await apiService.getApi().get('/api/monitoring/health');
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
        return 'success';
      case 'warning':
        return 'warning';
      case 'unhealthy':
        return 'error';
      case 'disabled':
        return 'default';
      default:
        return 'primary';
    }
  };

  const renderHealthChecks = (checks: HealthCheckResult[], title: string) => (
    <Card sx={{ mb: 2 }}>
      <CardHeader 
        title={
          <Box display="flex" justifyContent="space-between" alignItems="center">
            <Typography variant="h6">{title}</Typography>
            <Chip 
              label={checks.every(c => c.isHealthy) ? 'Healthy' : 'Issues Detected'}
              color={getStatusColor(checks.every(c => c.isHealthy) ? 'success' : 'error')}
            />
          </Box>
        }
      />
      <CardContent>
        {checks.map((check, index) => (
          <Box key={index} sx={{ border: 1, borderColor: 'divider', borderRadius: 1, p: 2, mb: 2 }}>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={1}>
              <Typography variant="body2" fontWeight="medium">
                {check.description}
              </Typography>
              <Chip 
                label={check.status}
                color={getStatusColor(check.status) as any}
                size="small"
              />
            </Box>
            <Typography variant="caption" color="text.secondary">
              Response Time: {check.responseTime}ms
            </Typography>
          </Box>
        ))}
      </CardContent>
    </Card>
  );

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <LinearProgress sx={{ width: '100%' }} />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        System Monitoring
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h6">
          Last Updated: {healthReport?.timestamp ? new Date(healthReport.timestamp).toLocaleString() : 'Never'}
        </Typography>
        <Button 
          variant="contained" 
          onClick={() => {
            fetchHealthReport();
            fetchMetrics();
          }}
          disabled={loading}
        >
          Refresh
        </Button>
      </Box>

      <Tabs value={activeTab} onChange={(_, newValue) => setActiveTab(newValue)} sx={{ mb: 2 }}>
        <Tab label="Health Checks" />
        <Tab label="Metrics" />
      </Tabs>

      {activeTab === 0 && healthReport && (
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Card sx={{ mb: 2 }}>
              <CardHeader 
                title={
                  <Box display="flex" justifyContent="space-between" alignItems="center">
                    <Typography variant="h6">Overall System Health</Typography>
                    <Chip 
                      label={healthReport.overallStatus}
                      color={getStatusColor(healthReport.overallStatus) as any}
                      size="medium"
                    />
                  </Box>
                }
              />
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Total Check Time: {healthReport.totalCheckTime}ms
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={12} md={6}>
            {renderHealthChecks(healthReport.databaseChecks, 'Database Checks')}
            {renderHealthChecks(healthReport.cacheChecks, 'Cache Checks')}
          </Grid>
          
          <Grid item xs={12} md={6}>
            {renderHealthChecks(healthReport.externalServiceChecks, 'External Services')}
            {renderHealthChecks(healthReport.systemChecks, 'System Checks')}
            {renderHealthChecks(healthReport.applicationChecks, 'Application Checks')}
          </Grid>
        </Grid>
      )}

      {activeTab === 1 && (
        <Grid container spacing={2}>
          {metrics.map((metric, index) => (
            <Grid item xs={12} md={4} key={index}>
              <Card>
                <CardHeader title={metric.metricName} />
                <CardContent>
                  <Typography variant="h4" color="primary">
                    {metric.value.toFixed(2)}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {new Date(metric.timestamp).toLocaleString()}
                  </Typography>
                  {metric.tags && (
                    <Box mt={1}>
                      {Object.entries(metric.tags).map(([key, value]) => (
                        <Chip 
                          key={key}
                          label={`${key}: ${value}`}
                          size="small"
                          sx={{ mr: 1, mb: 1 }}
                        />
                      ))}
                    </Box>
                  )}
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}
    </Box>
  );
};

export default Monitoring;