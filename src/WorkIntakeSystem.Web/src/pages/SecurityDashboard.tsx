import React, { useState, useEffect } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Alert,
  AlertTitle,
  LinearProgress,
  IconButton,
  Tooltip,
  Badge,
  Tabs,
  Tab,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Divider,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem
} from '@mui/material';
import {
  Security,
  Warning,
  CheckCircle,
  Error,
  Info,
  Refresh,
  Visibility,
  Shield,
  Lock,
  BugReport,
  Timeline,
  Assessment,
  Notifications,
  Settings,
  Download
} from '@mui/icons-material';
import { apiService } from '../services/api';

interface SecurityDashboard {
  generatedAt: string;
  metrics: SecurityMetrics;
  activeThreats: SecurityThreat[];
  activeAlerts: SecurityAlert[];
  complianceStatuses: ComplianceStatus[];
  recentIncidents: SecurityIncident[];
  systemHealth: Record<string, any>;
}

interface SecurityMetrics {
  fromDate: string;
  toDate: string;
  totalSecurityEvents: number;
  suspiciousEvents: number;
  failedLogins: number;
  successfulLogins: number;
  dataAccessEvents: number;
  privilegeEscalations: number;
  securityIncidents: number;
  averageResponseTime: number;
  eventsByType: Record<string, number>;
  eventsBySeverity: Record<string, number>;
  topThreats: TopThreat[];
}

interface SecurityThreat {
  id: number;
  threatType: string;
  description: string;
  severity: string;
  status: string;
  detectedAt: string;
  sourceIP: string;
  targetResource: string;
  mitigationSteps: string;
  isResolved: boolean;
  resolvedAt?: string;
}

interface SecurityAlert {
  id: number;
  alertType: string;
  title: string;
  description: string;
  severity: string;
  status: string;
  createdAt: string;
  acknowledgedAt?: string;
  acknowledgedBy?: string;
  source: string;
  category: string;
}

interface ComplianceStatus {
  framework: string;
  isCompliant: boolean;
  complianceScore: number;
  violations: ComplianceViolation[];
  requirements: ComplianceRequirement[];
  lastChecked: string;
  status: string;
}

interface ComplianceViolation {
  id: number;
  framework: number;
  requirement: string;
  description: string;
  severity: string;
  detectedAt: string;
  resource: string;
  userId: string;
  isResolved: boolean;
}

interface ComplianceRequirement {
  id: string;
  title: string;
  description: string;
  isImplemented: boolean;
  implementationStatus: string;
  lastVerified: string;
}

interface SecurityIncident {
  id: number;
  title: string;
  description: string;
  severity: string;
  status: string;
  detectedAt: string;
  resolvedAt?: string;
  assignedTo: string;
  impact: string;
  incidentType: string;
  priority: string;
}

interface TopThreat {
  threatType: string;
  count: number;
  severity: string;
  lastOccurrence: string;
}

const SecurityDashboard: React.FC = () => {
  const [dashboard, setDashboard] = useState<SecurityDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState(0);
  const [selectedThreat, setSelectedThreat] = useState<SecurityThreat | null>(null);
  const [threatDialogOpen, setThreatDialogOpen] = useState(false);
  const [monitoringStatus, setMonitoringStatus] = useState<boolean>(false);

  useEffect(() => {
    loadDashboard();
    checkMonitoringStatus();
  }, []);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      const response = await apiService.getApi().get('/api/securitymonitoring/dashboard');
      setDashboard(response.data);
    } catch (err) {
      setError('Failed to load security dashboard');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const checkMonitoringStatus = async () => {
    try {
      const response = await apiService.getApi().get('/api/securitymonitoring/monitoring/status');
      setMonitoringStatus(response.data.isActive);
    } catch (err) {
      console.error('Failed to check monitoring status:', err);
    }
  };

  const startMonitoring = async () => {
    try {
      await apiService.getApi().post('/api/securitymonitoring/monitoring/start');
      setMonitoringStatus(true);
    } catch (err) {
      console.error('Failed to start monitoring:', err);
    }
  };

  const stopMonitoring = async () => {
    try {
      await apiService.getApi().post('/api/securitymonitoring/monitoring/stop');
      setMonitoringStatus(false);
    } catch (err) {
      console.error('Failed to stop monitoring:', err);
    }
  };

  const acknowledgeAlert = async (alertId: number) => {
    try {
      await apiService.getApi().post(`/api/securitymonitoring/alerts/${alertId}/acknowledge`, {
        acknowledgedBy: 'Current User' // In real implementation, get from auth context
      });
      loadDashboard(); // Refresh dashboard
    } catch (err) {
      console.error('Failed to acknowledge alert:', err);
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity.toLowerCase()) {
      case 'high': return 'error';
      case 'medium': return 'warning';
      case 'low': return 'info';
      default: return 'default';
    }
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'active': return 'error';
      case 'acknowledged': return 'warning';
      case 'resolved': return 'success';
      default: return 'default';
    }
  };

  const getComplianceColor = (score: number) => {
    if (score >= 90) return 'success';
    if (score >= 70) return 'warning';
    return 'error';
  };

  if (loading) {
    return (
      <Box sx={{ p: 3 }}>
        <LinearProgress />
        <Typography variant="h6" sx={{ mt: 2 }}>Loading Security Dashboard...</Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="error">
          <AlertTitle>Error</AlertTitle>
          {error}
        </Alert>
      </Box>
    );
  }

  if (!dashboard) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="info">
          <AlertTitle>No Data</AlertTitle>
          No security dashboard data available.
        </Alert>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Security color="primary" />
          Security Dashboard
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Chip
            icon={monitoringStatus ? <CheckCircle /> : <Error />}
            label={monitoringStatus ? 'Monitoring Active' : 'Monitoring Inactive'}
            color={monitoringStatus ? 'success' : 'error'}
            variant="outlined"
          />
          <Button
            variant="outlined"
            startIcon={<Refresh />}
            onClick={loadDashboard}
          >
            Refresh
          </Button>
          {monitoringStatus ? (
            <Button
              variant="outlined"
              color="error"
              startIcon={<Error />}
              onClick={stopMonitoring}
            >
              Stop Monitoring
            </Button>
          ) : (
            <Button
              variant="outlined"
              color="success"
              startIcon={<CheckCircle />}
              onClick={startMonitoring}
            >
              Start Monitoring
            </Button>
          )}
        </Box>
      </Box>

      {/* Tabs */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 3 }}>
        <Tabs value={activeTab} onChange={(e, newValue) => setActiveTab(newValue)}>
          <Tab label="Overview" icon={<Assessment />} />
          <Tab label="Threats" icon={<Warning />} />
          <Tab label="Alerts" icon={<Notifications />} />
          <Tab label="Compliance" icon={<Shield />} />
          <Tab label="Incidents" icon={<BugReport />} />
        </Tabs>
      </Box>

      {/* Overview Tab */}
      {activeTab === 0 && (
        <Grid container spacing={3}>
          {/* Key Metrics */}
          <Grid item xs={12} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Total Security Events
                </Typography>
                <Typography variant="h4">
                  {dashboard.metrics.totalSecurityEvents.toLocaleString()}
                </Typography>
                <Typography color="textSecondary" variant="body2">
                  Last 30 days
                </Typography>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Suspicious Events
                </Typography>
                <Typography variant="h4" color="error">
                  {dashboard.metrics.suspiciousEvents.toLocaleString()}
                </Typography>
                <Typography color="textSecondary" variant="body2">
                  {((dashboard.metrics.suspiciousEvents / dashboard.metrics.totalSecurityEvents) * 100).toFixed(1)}% of total
                </Typography>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Active Threats
                </Typography>
                <Typography variant="h4" color="warning">
                  {dashboard.activeThreats.length}
                </Typography>
                <Typography color="textSecondary" variant="body2">
                  Require attention
                </Typography>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Active Alerts
                </Typography>
                <Typography variant="h4" color="error">
                  {dashboard.activeAlerts.length}
                </Typography>
                <Typography color="textSecondary" variant="body2">
                  Unacknowledged
                </Typography>
              </CardContent>
            </Card>
          </Grid>

          {/* Events by Type Chart */}
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Events by Type
                </Typography>
                <List>
                  {Object.entries(dashboard.metrics.eventsByType).map(([type, count]) => (
                    <ListItem key={type}>
                      <ListItemText primary={type} secondary={`${count} events`} />
                      <Chip label={count} size="small" />
                    </ListItem>
                  ))}
                </List>
              </CardContent>
            </Card>
          </Grid>

          {/* Events by Severity Chart */}
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Events by Severity
                </Typography>
                <List>
                  {Object.entries(dashboard.metrics.eventsBySeverity).map(([severity, count]) => (
                    <ListItem key={severity}>
                      <ListItemText primary={severity} secondary={`${count} events`} />
                      <Chip 
                        label={count} 
                        size="small" 
                        color={getSeverityColor(severity) as any}
                      />
                    </ListItem>
                  ))}
                </List>
              </CardContent>
            </Card>
          </Grid>

          {/* Top Threats */}
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Top Threats
                </Typography>
                <TableContainer>
                  <Table>
                    <TableHead>
                      <TableRow>
                        <TableCell>Threat Type</TableCell>
                        <TableCell>Count</TableCell>
                        <TableCell>Severity</TableCell>
                        <TableCell>Last Occurrence</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {dashboard.metrics.topThreats.map((threat) => (
                        <TableRow key={threat.threatType}>
                          <TableCell>{threat.threatType}</TableCell>
                          <TableCell>{threat.count}</TableCell>
                          <TableCell>
                            <Chip 
                              label={threat.severity} 
                              size="small" 
                              color={getSeverityColor(threat.severity) as any}
                            />
                          </TableCell>
                          <TableCell>
                            {new Date(threat.lastOccurrence).toLocaleString()}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Threats Tab */}
      {activeTab === 1 && (
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Active Security Threats
            </Typography>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Threat Type</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell>Severity</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Detected At</TableCell>
                    <TableCell>Source IP</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {dashboard.activeThreats.map((threat) => (
                    <TableRow key={threat.id}>
                      <TableCell>{threat.threatType}</TableCell>
                      <TableCell>{threat.description}</TableCell>
                      <TableCell>
                        <Chip 
                          label={threat.severity} 
                          size="small" 
                          color={getSeverityColor(threat.severity) as any}
                        />
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={threat.status} 
                          size="small" 
                          color={getStatusColor(threat.status) as any}
                        />
                      </TableCell>
                      <TableCell>
                        {new Date(threat.detectedAt).toLocaleString()}
                      </TableCell>
                      <TableCell>{threat.sourceIP}</TableCell>
                      <TableCell>
                        <Tooltip title="View Details">
                          <IconButton 
                            size="small"
                            onClick={() => {
                              setSelectedThreat(threat);
                              setThreatDialogOpen(true);
                            }}
                          >
                            <Visibility />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      )}

      {/* Alerts Tab */}
      {activeTab === 2 && (
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Active Security Alerts
            </Typography>
            <List>
              {dashboard.activeAlerts.map((alert) => (
                <React.Fragment key={alert.id}>
                  <ListItem>
                    <ListItemIcon>
                      <Badge color="error" variant="dot">
                        <Notifications />
                      </Badge>
                    </ListItemIcon>
                    <ListItemText
                      primary={alert.title}
                      secondary={
                        <Box>
                          <Typography variant="body2" color="textSecondary">
                            {alert.description}
                          </Typography>
                          <Box sx={{ display: 'flex', gap: 1, mt: 1 }}>
                            <Chip 
                              label={alert.severity} 
                              size="small" 
                              color={getSeverityColor(alert.severity) as any}
                            />
                            <Chip 
                              label={alert.category} 
                              size="small" 
                              variant="outlined"
                            />
                            <Typography variant="caption" color="textSecondary">
                              {new Date(alert.createdAt).toLocaleString()}
                            </Typography>
                          </Box>
                        </Box>
                      }
                    />
                    <Button
                      size="small"
                      variant="outlined"
                      onClick={() => acknowledgeAlert(alert.id)}
                    >
                      Acknowledge
                    </Button>
                  </ListItem>
                  <Divider />
                </React.Fragment>
              ))}
            </List>
          </CardContent>
        </Card>
      )}

      {/* Compliance Tab */}
      {activeTab === 3 && (
        <Grid container spacing={3}>
          {dashboard.complianceStatuses.map((status) => (
            <Grid item xs={12} md={6} key={status.framework}>
              <Card>
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                    <Typography variant="h6">{status.framework}</Typography>
                    <Chip
                      label={status.isCompliant ? 'Compliant' : 'Non-Compliant'}
                      color={status.isCompliant ? 'success' : 'error'}
                    />
                  </Box>
                  <Box sx={{ mb: 2 }}>
                    <Typography variant="body2" color="textSecondary" gutterBottom>
                      Compliance Score
                    </Typography>
                    <LinearProgress
                      variant="determinate"
                      value={status.complianceScore}
                      color={getComplianceColor(status.complianceScore) as any}
                      sx={{ height: 8, borderRadius: 4 }}
                    />
                    <Typography variant="h6" sx={{ mt: 1 }}>
                      {status.complianceScore.toFixed(1)}%
                    </Typography>
                  </Box>
                  <Typography variant="body2" color="textSecondary">
                    Last checked: {new Date(status.lastChecked).toLocaleString()}
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    Violations: {status.violations.length}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      {/* Incidents Tab */}
      {activeTab === 4 && (
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Recent Security Incidents
            </Typography>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Title</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell>Severity</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Priority</TableCell>
                    <TableCell>Detected At</TableCell>
                    <TableCell>Assigned To</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {dashboard.recentIncidents.map((incident) => (
                    <TableRow key={incident.id}>
                      <TableCell>{incident.title}</TableCell>
                      <TableCell>{incident.incidentType}</TableCell>
                      <TableCell>
                        <Chip 
                          label={incident.severity} 
                          size="small" 
                          color={getSeverityColor(incident.severity) as any}
                        />
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={incident.status} 
                          size="small" 
                          color={getStatusColor(incident.status) as any}
                        />
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={incident.priority} 
                          size="small" 
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>
                        {new Date(incident.detectedAt).toLocaleString()}
                      </TableCell>
                      <TableCell>{incident.assignedTo}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      )}

      {/* Threat Details Dialog */}
      <Dialog open={threatDialogOpen} onClose={() => setThreatDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>Threat Details</DialogTitle>
        <DialogContent>
          {selectedThreat && (
            <Box>
              <Typography variant="h6" gutterBottom>
                {selectedThreat.threatType}
              </Typography>
              <Typography variant="body1" paragraph>
                {selectedThreat.description}
              </Typography>
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" gutterBottom>
                  Mitigation Steps:
                </Typography>
                <Typography variant="body2" sx={{ whiteSpace: 'pre-line' }}>
                  {selectedThreat.mitigationSteps}
                </Typography>
              </Box>
              <Box sx={{ display: 'flex', gap: 1, mb: 2 }}>
                <Chip 
                  label={selectedThreat.severity} 
                  color={getSeverityColor(selectedThreat.severity) as any}
                />
                <Chip 
                  label={selectedThreat.status} 
                  color={getStatusColor(selectedThreat.status) as any}
                />
              </Box>
              <Typography variant="body2" color="textSecondary">
                <strong>Source IP:</strong> {selectedThreat.sourceIP}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                <strong>Target Resource:</strong> {selectedThreat.targetResource}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                <strong>Detected At:</strong> {new Date(selectedThreat.detectedAt).toLocaleString()}
              </Typography>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setThreatDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default SecurityDashboard;
