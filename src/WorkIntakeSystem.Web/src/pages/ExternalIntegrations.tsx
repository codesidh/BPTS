import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from 'react-query';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  Button,
  Chip,
  CircularProgress,
  Alert,
  useTheme,
  useMediaQuery,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
} from '@mui/material';
import {
  ExpandMore,
  CheckCircle,
  Error,
  Warning,
  Refresh,
  Settings,
  Send,
  CalendarToday,
  Notifications,
  Business,
  Sync,
  Add,
  CloudSync,
  IntegrationInstructions,
  Api,
  AssignmentInd,
} from '@mui/icons-material';
import { apiService } from '../services/api';
import { useMsal } from '@azure/msal-react';

interface ExternalSystemStatus {
  systemName: string;
  isConnected: boolean;
  lastSyncTime: string;
  status: string;
  errorMessage?: string;
  metrics: Record<string, any>;
}

interface IntegrationLog {
  id: number;
  systemName: string;
  eventType: string;
  data: string;
  success: boolean;
  errorMessage?: string;
  timestamp: string;
  duration: string;
}

interface ProjectTask {
  id: string;
  title: string;
  description: string;
  status: string;
  dueDate?: string;
  assignedTo: string;
  estimatedHours: number;
  actualHours: number;
}

interface CalendarEvent {
  id: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  location: string;
  attendees: string[];
  isAllDay: boolean;
}

const ExternalIntegrations: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const { } = useMsal();
  // Using the singleton apiService instance
  const queryClient = useQueryClient();

  const [selectedSystem, setSelectedSystem] = useState<string>('');
  const [syncDialogOpen, setSyncDialogOpen] = useState(false);
  const [notificationDialogOpen, setNotificationDialogOpen] = useState(false);
  const [calendarDialogOpen, setCalendarDialogOpen] = useState(false);
  const [projectDialogOpen, setProjectDialogOpen] = useState(false);

  // Queries
  const { data: systemStatuses, isLoading: statusLoading, refetch: refetchStatus } = useQuery(
    'externalSystemStatus',
    async () => {
      const response = await apiService.getApi().get('/externalintegrations/status');
      return response.data as ExternalSystemStatus[];
    }
  );

  const { data: integrationLogs, isLoading: logsLoading } = useQuery(
    'integrationLogs',
    async () => {
      const response = await apiService.getApi().get('/externalintegrations/logs');
      return response.data as IntegrationLog[];
    }
  );

  const { data: projectTasks } = useQuery(
    ['projectTasks', selectedSystem],
    async () => {
      if (!selectedSystem) return [];
      const response = await apiService.getApi().get(`/externalintegrations/projects/${selectedSystem}/tasks`);
      return response.data as ProjectTask[];
    },
    { enabled: !!selectedSystem }
  );

  const { data: calendarEvents } = useQuery(
    'calendarEvents',
    async () => {
      const fromDate = new Date();
      const toDate = new Date();
      toDate.setDate(toDate.getDate() + 30);
      const response = await apiService.getApi().get(`/externalintegrations/calendar/events?fromDate=${fromDate.toISOString()}&toDate=${toDate.toISOString()}`);
      return response.data as CalendarEvent[];
    }
  );

  // Mutations
  const syncMutation = useMutation(
    async (data: { systemName: string; endpoint: string; data: any }) => {
      const response = await apiService.getApi().post('/externalintegrations/sync', data);
      return response.data;
    },
    {
      onSuccess: () => {
        queryClient.invalidateQueries('externalSystemStatus');
        queryClient.invalidateQueries('integrationLogs');
        setSyncDialogOpen(false);
      },
    }
  );

  const notificationMutation = useMutation(
    async (data: { recipient: string; subject: string; message: string; type: string }) => {
      const response = await apiService.getApi().post('/externalintegrations/notifications', data);
      return response.data;
    },
    {
      onSuccess: () => {
        setNotificationDialogOpen(false);
      },
    }
  );

  const calendarMutation = useMutation(
    async (data: { workRequestId: number; title: string; description: string; startDate: string; endDate: string }) => {
      const response = await apiService.getApi().post('/externalintegrations/calendar/events', data);
      return response.data;
    },
    {
      onSuccess: () => {
        queryClient.invalidateQueries('calendarEvents');
        setCalendarDialogOpen(false);
      },
    }
  );

  const projectMutation = useMutation(
    async (data: { workRequestId: number; title: string; description: string; category: string; priorityLevel: string; estimatedEffort: number; targetDate?: string }) => {
      const response = await apiService.getApi().post('/externalintegrations/projects', data);
      return response.data;
    },
    {
      onSuccess: () => {
        queryClient.invalidateQueries('projectTasks');
        setProjectDialogOpen(false);
      },
    }
  );

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'connected':
        return 'success';
      case 'disconnected':
        return 'error';
      case 'error':
        return 'error';
      default:
        return 'warning';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'connected':
        return <CheckCircle color="success" />;
      case 'disconnected':
        return <Error color="error" />;
      case 'error':
        return <Error color="error" />;
      default:
        return <Warning color="warning" />;
    }
  };

  if (statusLoading || logsLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress size={60} />
      </Box>
    );
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3} flexWrap="wrap" gap={2}>
        <Typography variant="h4" component="h1">
          External System Integrations
        </Typography>
        <Box display="flex" gap={1} flexWrap="wrap">
          <Button
            variant="outlined"
            startIcon={<Refresh />}
            onClick={() => refetchStatus()}
            size={isMobile ? 'small' : 'medium'}
          >
            Refresh Status
          </Button>
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => setSyncDialogOpen(true)}
            size={isMobile ? 'small' : 'medium'}
          >
            New Integration
          </Button>
        </Box>
      </Box>

      {/* System Status Overview */}
      <Grid container spacing={3} mb={3}>
        {systemStatuses?.map((system) => (
          <Grid item xs={12} sm={6} md={4} key={system.systemName}>
            <Card>
              <CardContent>
                <Box display="flex" alignItems="center" justifyContent="space-between" mb={2}>
                  <Typography variant="h6" component="div">
                    {system.systemName}
                  </Typography>
                  {getStatusIcon(system.status)}
                </Box>
                <Box display="flex" alignItems="center" mb={1}>
                  <Chip
                    label={system.status}
                    color={getStatusColor(system.status) as any}
                    size="small"
                    sx={{ mr: 1 }}
                  />
                  <Typography variant="body2" color="textSecondary">
                    Last sync: {new Date(system.lastSyncTime).toLocaleDateString()}
                  </Typography>
                </Box>
                {system.errorMessage && (
                  <Alert severity="error" sx={{ mt: 1 }}>
                    {system.errorMessage}
                  </Alert>
                )}
                <Box display="flex" gap={1} mt={2}>
                  <Button
                    size="small"
                    startIcon={<Sync />}
                    onClick={() => {
                      setSelectedSystem(system.systemName);
                      setSyncDialogOpen(true);
                    }}
                  >
                    Sync
                  </Button>
                  <Button
                    size="small"
                    startIcon={<Settings />}
                    variant="outlined"
                  >
                    Configure
                  </Button>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Integration Features */}
      <Grid container spacing={3}>
        {/* Project Management Integration */}
        <Grid item xs={12} md={6}>
          <Accordion>
            <AccordionSummary expandIcon={<ExpandMore />}>
              <Box display="flex" alignItems="center">
                <Business sx={{ mr: 1 }} />
                <Typography variant="h6">Project Management</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Box>
                <Typography variant="body2" color="textSecondary" mb={2}>
                  Integrate with Azure DevOps, Jira, and other project management tools
                </Typography>
                <Box display="flex" gap={1} flexWrap="wrap">
                  <Button
                    size="small"
                    startIcon={<Add />}
                    onClick={() => setProjectDialogOpen(true)}
                  >
                    Create Project
                  </Button>
                  <Button
                    size="small"
                    startIcon={<Sync />}
                    variant="outlined"
                    onClick={() => {
                      setSelectedSystem('AzureDevOps');
                      queryClient.invalidateQueries(['projectTasks', 'AzureDevOps']);
                    }}
                  >
                    Sync Tasks
                  </Button>
                </Box>
                {projectTasks && projectTasks.length > 0 && (
                  <Box mt={2}>
                    <Typography variant="subtitle2" gutterBottom>
                      Recent Tasks
                    </Typography>
                    <List dense>
                      {projectTasks.slice(0, 3).map((task) => (
                        <ListItem key={task.id}>
                          <ListItemIcon>
                            <AssignmentInd fontSize="small" />
                          </ListItemIcon>
                          <ListItemText
                            primary={task.title}
                            secondary={`${task.status} • ${task.assignedTo}`}
                          />
                        </ListItem>
                      ))}
                    </List>
                  </Box>
                )}
              </Box>
            </AccordionDetails>
          </Accordion>
        </Grid>

        {/* Calendar Integration */}
        <Grid item xs={12} md={6}>
          <Accordion>
            <AccordionSummary expandIcon={<ExpandMore />}>
              <Box display="flex" alignItems="center">
                <CalendarToday sx={{ mr: 1 }} />
                <Typography variant="h6">Calendar Integration</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Box>
                <Typography variant="body2" color="textSecondary" mb={2}>
                  Sync with Microsoft Teams, Outlook, and other calendar systems
                </Typography>
                <Box display="flex" gap={1} flexWrap="wrap">
                  <Button
                    size="small"
                    startIcon={<Add />}
                    onClick={() => setCalendarDialogOpen(true)}
                  >
                    Create Event
                  </Button>
                  <Button
                    size="small"
                    startIcon={<Sync />}
                    variant="outlined"
                    onClick={() => queryClient.invalidateQueries('calendarEvents')}
                  >
                    Sync Events
                  </Button>
                </Box>
                {calendarEvents && calendarEvents.length > 0 && (
                  <Box mt={2}>
                    <Typography variant="subtitle2" gutterBottom>
                      Upcoming Events
                    </Typography>
                    <List dense>
                      {calendarEvents.slice(0, 3).map((event) => (
                        <ListItem key={event.id}>
                          <ListItemIcon>
                            <CalendarToday fontSize="small" />
                          </ListItemIcon>
                          <ListItemText
                            primary={event.title}
                            secondary={new Date(event.startDate).toLocaleDateString()}
                          />
                        </ListItem>
                      ))}
                    </List>
                  </Box>
                )}
              </Box>
            </AccordionDetails>
          </Accordion>
        </Grid>

        {/* Notification Integration */}
        <Grid item xs={12} md={6}>
          <Accordion>
            <AccordionSummary expandIcon={<ExpandMore />}>
              <Box display="flex" alignItems="center">
                <Notifications sx={{ mr: 1 }} />
                <Typography variant="h6">Notifications</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Box>
                <Typography variant="body2" color="textSecondary" mb={2}>
                  Send notifications via email, Teams, Slack, and SMS
                </Typography>
                <Box display="flex" gap={1} flexWrap="wrap">
                  <Button
                    size="small"
                    startIcon={<Send />}
                    onClick={() => setNotificationDialogOpen(true)}
                  >
                    Send Notification
                  </Button>
                  <Button
                    size="small"
                    startIcon={<Settings />}
                    variant="outlined"
                  >
                    Configure
                  </Button>
                </Box>
              </Box>
            </AccordionDetails>
          </Accordion>
        </Grid>

        {/* API Integration */}
        <Grid item xs={12} md={6}>
          <Accordion>
            <AccordionSummary expandIcon={<ExpandMore />}>
              <Box display="flex" alignItems="center">
                <Api sx={{ mr: 1 }} />
                <Typography variant="h6">API Management</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Box>
                <Typography variant="body2" color="textSecondary" mb={2}>
                  Manage API endpoints and external system connections
                </Typography>
                <Box display="flex" gap={1} flexWrap="wrap">
                  <Button
                    size="small"
                    startIcon={<IntegrationInstructions />}
                  >
                    API Docs
                  </Button>
                  <Button
                    size="small"
                    startIcon={<CloudSync />}
                    variant="outlined"
                  >
                    Health Check
                  </Button>
                </Box>
              </Box>
            </AccordionDetails>
          </Accordion>
        </Grid>
      </Grid>

      {/* Integration Logs */}
      <Card sx={{ mt: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Integration Logs
          </Typography>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>System</TableCell>
                  <TableCell>Event</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Timestamp</TableCell>
                  <TableCell>Duration</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {integrationLogs?.slice(0, 10).map((log) => (
                  <TableRow key={log.id}>
                    <TableCell>{log.systemName}</TableCell>
                    <TableCell>{log.eventType}</TableCell>
                    <TableCell>
                      <Chip
                        label={log.success ? 'Success' : 'Failed'}
                        color={log.success ? 'success' : 'error'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>{new Date(log.timestamp).toLocaleString()}</TableCell>
                    <TableCell>{log.duration}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>

      {/* Sync Dialog */}
      <Dialog open={syncDialogOpen} onClose={() => setSyncDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Sync with External System</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            label="System Name"
            value={selectedSystem}
            onChange={(e) => setSelectedSystem(e.target.value)}
            margin="normal"
          />
          <TextField
            fullWidth
            label="Endpoint"
            defaultValue="/api/sync"
            margin="normal"
          />
          <TextField
            fullWidth
            label="Data (JSON)"
            multiline
            rows={4}
            defaultValue='{"key": "value"}'
            margin="normal"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSyncDialogOpen(false)}>Cancel</Button>
          <Button
            onClick={() => {
              syncMutation.mutate({
                systemName: selectedSystem,
                endpoint: '/api/sync',
                data: { key: 'value' }
              });
            }}
            variant="contained"
            disabled={syncMutation.isLoading}
          >
            {syncMutation.isLoading ? <CircularProgress size={20} /> : 'Sync'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Notification Dialog */}
      <Dialog open={notificationDialogOpen} onClose={() => setNotificationDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Send Notification</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            label="Recipient"
            margin="normal"
          />
          <TextField
            fullWidth
            label="Subject"
            margin="normal"
          />
          <TextField
            fullWidth
            label="Message"
            multiline
            rows={4}
            margin="normal"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setNotificationDialogOpen(false)}>Cancel</Button>
          <Button
            onClick={() => {
              notificationMutation.mutate({
                recipient: 'test@example.com',
                subject: 'Test Notification',
                message: 'This is a test notification',
                type: 'Email'
              });
            }}
            variant="contained"
            disabled={notificationMutation.isLoading}
          >
            {notificationMutation.isLoading ? <CircularProgress size={20} /> : 'Send'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Calendar Event Dialog */}
      <Dialog open={calendarDialogOpen} onClose={() => setCalendarDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Create Calendar Event</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            label="Title"
            margin="normal"
          />
          <TextField
            fullWidth
            label="Description"
            multiline
            rows={3}
            margin="normal"
          />
          <TextField
            fullWidth
            label="Start Date"
            type="datetime-local"
            margin="normal"
            InputLabelProps={{ shrink: true }}
          />
          <TextField
            fullWidth
            label="End Date"
            type="datetime-local"
            margin="normal"
            InputLabelProps={{ shrink: true }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCalendarDialogOpen(false)}>Cancel</Button>
          <Button
            onClick={() => {
              calendarMutation.mutate({
                workRequestId: 1,
                title: 'Test Event',
                description: 'Test calendar event',
                startDate: new Date().toISOString(),
                endDate: new Date(Date.now() + 3600000).toISOString()
              });
            }}
            variant="contained"
            disabled={calendarMutation.isLoading}
          >
            {calendarMutation.isLoading ? <CircularProgress size={20} /> : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Project Dialog */}
      <Dialog open={projectDialogOpen} onClose={() => setProjectDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Create Project</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            label="Title"
            margin="normal"
          />
          <TextField
            fullWidth
            label="Description"
            multiline
            rows={3}
            margin="normal"
          />
          <TextField
            fullWidth
            label="Category"
            margin="normal"
          />
          <TextField
            fullWidth
            label="Priority Level"
            margin="normal"
          />
          <TextField
            fullWidth
            label="Estimated Effort (hours)"
            type="number"
            margin="normal"
          />
          <TextField
            fullWidth
            label="Target Date"
            type="date"
            margin="normal"
            InputLabelProps={{ shrink: true }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setProjectDialogOpen(false)}>Cancel</Button>
          <Button
            onClick={() => {
              projectMutation.mutate({
                workRequestId: 1,
                title: 'Test Project',
                description: 'Test project description',
                category: 'WorkRequest',
                priorityLevel: 'Medium',
                estimatedEffort: 40,
                targetDate: new Date().toISOString().split('T')[0]
              });
            }}
            variant="contained"
            disabled={projectMutation.isLoading}
          >
            {projectMutation.isLoading ? <CircularProgress size={20} /> : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ExternalIntegrations; 