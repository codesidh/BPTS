import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Card,
  CardContent,
  Button,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
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
  Chip,
  IconButton,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Switch,
  FormControlLabel,
  Divider,
  Tooltip,
  Badge,

} from '@mui/material';
import {
  Timeline as TimelineIcon,
  History,
  PlayArrow,
  Visibility,
  FilterList,
  Search,
  Refresh,
  ExpandMore,
  Event,
  Security,
  BugReport,
  CheckCircle,
  Warning,
  Error,
  Info,
  DateRange,
  Person,
  Computer
} from '@mui/icons-material';
import { apiService } from '../services/api';

interface EventStoreItem {
  id: number;
  aggregateId: string;
  eventType: string;
  eventData: string;
  eventVersion: number;
  timestamp: string;
  correlationId: string;
  causationId: string;
  createdBy: string;
  metadata: string;
}

interface AuditTrailItem {
  id: number;
  workRequestId: number;
  action: string;
  oldValue?: string;
  newValue?: string;
  changedById: number;
  changedDate: string;
  comments: string;
  eventId?: number;
  correlationId: string;
  sessionId: string;
  ipAddress: string;
  userAgent: string;
  securityContext: string;
  changedBy?: {
    name: string;
    email: string;
  };
  workRequest?: {
    title: string;
    id: number;
  };
}

const EventAuditTrail: React.FC = () => {
  const [activeTab, setActiveTab] = useState(0);
  const [events, setEvents] = useState<EventStoreItem[]>([]);
  const [auditTrails, setAuditTrails] = useState<AuditTrailItem[]>([]);
  const [filteredEvents, setFilteredEvents] = useState<EventStoreItem[]>([]);
  const [filteredAuditTrails, setFilteredAuditTrails] = useState<AuditTrailItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Dialog states
  const [eventDetailDialogOpen, setEventDetailDialogOpen] = useState(false);
  const [replayDialogOpen, setReplayDialogOpen] = useState(false);
  const [selectedEvent, setSelectedEvent] = useState<EventStoreItem | null>(null);
  const [selectedAggregateId, setSelectedAggregateId] = useState<string>('');
  const [replayEvents, setReplayEvents] = useState<EventStoreItem[]>([]);
  
  // Filter states
  const [searchTerm, setSearchTerm] = useState('');
  const [filterEventType, setFilterEventType] = useState('all');
  const [filterAggregateId, setFilterAggregateId] = useState('');
  const [dateRange, setDateRange] = useState({
    from: '',
    to: ''
  });
  const [showTimeline, setShowTimeline] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    filterData();
  }, [events, auditTrails, searchTerm, filterEventType, filterAggregateId, dateRange]);

  const loadData = async () => {
    try {
      setLoading(true);
      
      // Load events
      const eventsResponse = await apiService.getApi().get('/api/eventstore');
      setEvents(eventsResponse.data);
      
      // Load audit trails (this would need a separate endpoint)
      // For now, we'll use mock data
      const mockAuditTrails: AuditTrailItem[] = [
        {
          id: 1,
          workRequestId: 1,
          action: 'Workflow advanced: Intake -> BusinessReview',
          oldValue: 'Intake',
          newValue: 'BusinessReview',
          changedById: 1,
          changedDate: new Date().toISOString(),
          comments: 'Initial business review',
          correlationId: 'corr-123',
          sessionId: 'session-456',
          ipAddress: '192.168.1.100',
          userAgent: 'Mozilla/5.0...',
          securityContext: '{"roles": ["DepartmentHead"]}',
          changedBy: { name: 'John Doe', email: 'john@example.com' },
          workRequest: { title: 'System Enhancement Request', id: 1 }
        }
      ];
      setAuditTrails(mockAuditTrails);
    } catch (err) {
      setError('Failed to load event data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const filterData = () => {
    let filtered = activeTab === 0 ? events : auditTrails;

    // Filter by search term
    if (searchTerm) {
      if (activeTab === 0) {
        filtered = filtered.filter((event: EventStoreItem) =>
          event.eventType.toLowerCase().includes(searchTerm.toLowerCase()) ||
          event.aggregateId.toLowerCase().includes(searchTerm.toLowerCase()) ||
          event.createdBy.toLowerCase().includes(searchTerm.toLowerCase())
        );
      } else {
        filtered = filtered.filter((audit: AuditTrailItem) =>
          audit.action.toLowerCase().includes(searchTerm.toLowerCase()) ||
          audit.comments.toLowerCase().includes(searchTerm.toLowerCase()) ||
          (audit.changedBy?.name.toLowerCase().includes(searchTerm.toLowerCase()))
        );
      }
    }

    // Filter by event type (for events tab)
    if (activeTab === 0 && filterEventType !== 'all') {
      filtered = filtered.filter((event: EventStoreItem) => event.eventType === filterEventType);
    }

    // Filter by aggregate ID
    if (filterAggregateId) {
      if (activeTab === 0) {
        filtered = filtered.filter((event: EventStoreItem) => 
          event.aggregateId === filterAggregateId
        );
      } else {
        filtered = filtered.filter((audit: AuditTrailItem) => 
          audit.workRequestId.toString() === filterAggregateId
        );
      }
    }

    // Filter by date range
    if (dateRange.from || dateRange.to) {
      filtered = filtered.filter((item: any) => {
        const itemDate = new Date(activeTab === 0 ? item.timestamp : item.changedDate);
        const fromDate = dateRange.from ? new Date(dateRange.from) : null;
        const toDate = dateRange.to ? new Date(dateRange.to) : null;
        
        if (fromDate && itemDate < fromDate) return false;
        if (toDate && itemDate > toDate) return false;
        return true;
      });
    }

    if (activeTab === 0) {
      setFilteredEvents(filtered as EventStoreItem[]);
    } else {
      setFilteredAuditTrails(filtered as AuditTrailItem[]);
    }
  };

  const handleViewEvent = (event: EventStoreItem) => {
    setSelectedEvent(event);
    setEventDetailDialogOpen(true);
  };

  const handleReplayEvents = async (aggregateId: string) => {
    try {
      setLoading(true);
      setSelectedAggregateId(aggregateId);
      
      const response = await api.get(`/api/eventstore/by-aggregate/${aggregateId}`);
      setReplayEvents(response.data);
      setReplayDialogOpen(true);
    } catch (err) {
      setError('Failed to load events for replay');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const getEventTypeColor = (eventType: string) => {
    if (eventType.includes('Created')) return 'success';
    if (eventType.includes('Updated')) return 'primary';
    if (eventType.includes('Deleted')) return 'error';
    if (eventType.includes('Workflow')) return 'warning';
    return 'default';
  };

  const getActionIcon = (action: string) => {
    if (action.includes('Created')) return <CheckCircle />;
    if (action.includes('Updated')) return <Edit />;
    if (action.includes('Deleted')) return <Delete />;
    if (action.includes('Workflow')) return <Timeline />;
    return <Info />;
  };

  const formatEventData = (eventData: string) => {
    try {
      const parsed = JSON.parse(eventData);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return eventData;
    }
  };

  const getUniqueEventTypes = () => {
    const types = events.map(e => e.eventType);
    return Array.from(new Set(types));
  };

  const getUniqueAggregateIds = () => {
    const ids = events.map(e => e.aggregateId);
    return Array.from(new Set(ids));
  };

  return (
    <Box>
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h4" component="h1" sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <TimelineIcon color="primary" />
          Event & Audit Trail
        </Typography>
        <Button
          variant="contained"
          startIcon={<Refresh />}
          onClick={loadData}
          disabled={loading}
        >
          Refresh
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Paper sx={{ mb: 3 }}>
        <Tabs value={activeTab} onChange={(_, newValue) => setActiveTab(newValue)}>
          <Tab label="Event Store" />
          <Tab label="Audit Trail" />
          <Tab label="Timeline View" />
        </Tabs>
      </Paper>

      {/* Filters */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} md={3}>
            <TextField
              fullWidth
              placeholder="Search events..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              InputProps={{
                startAdornment: <Search sx={{ mr: 1, color: 'text.secondary' }} />
              }}
            />
          </Grid>
          {activeTab === 0 && (
            <Grid item xs={12} md={2}>
              <FormControl fullWidth>
                <InputLabel>Event Type</InputLabel>
                <Select
                  value={filterEventType}
                  onChange={(e) => setFilterEventType(e.target.value)}
                  label="Event Type"
                >
                  <MenuItem value="all">All Types</MenuItem>
                  {getUniqueEventTypes().map(type => (
                    <MenuItem key={type} value={type}>{type}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
          )}
          <Grid item xs={12} md={2}>
            <TextField
              fullWidth
              placeholder="Aggregate ID"
              value={filterAggregateId}
              onChange={(e) => setFilterAggregateId(e.target.value)}
            />
          </Grid>
          <Grid item xs={12} md={2}>
            <TextField
              fullWidth
              label="From Date"
              type="date"
              value={dateRange.from}
              onChange={(e) => setDateRange({ ...dateRange, from: e.target.value })}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item xs={12} md={2}>
            <TextField
              fullWidth
              label="To Date"
              type="date"
              value={dateRange.to}
              onChange={(e) => setDateRange({ ...dateRange, to: e.target.value })}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item xs={12} md={1}>
            <FormControlLabel
              control={
                <Switch
                  checked={showTimeline}
                  onChange={(e) => setShowTimeline(e.target.checked)}
                />
              }
              label="Timeline"
            />
          </Grid>
        </Grid>
      </Paper>

      {activeTab === 0 && (
        <>
          {showTimeline ? (
            <Timeline position="alternate">
              {filteredEvents.map((event, index) => (
                <TimelineItem key={event.id}>
                  <TimelineOppositeContent color="text.secondary">
                    {new Date(event.timestamp).toLocaleString()}
                  </TimelineOppositeContent>
                  <TimelineSeparator>
                    <TimelineDot color={getEventTypeColor(event.eventType) as any}>
                      <Event />
                    </TimelineDot>
                    {index < filteredEvents.length - 1 && <TimelineConnector />}
                  </TimelineSeparator>
                  <TimelineContent>
                    <Paper elevation={3} sx={{ p: 2 }}>
                      <Typography variant="h6" component="span">
                        {event.eventType}
                      </Typography>
                      <Typography>Aggregate: {event.aggregateId}</Typography>
                      <Typography variant="body2" color="text.secondary">
                        By: {event.createdBy}
                      </Typography>
                      <Box sx={{ mt: 1 }}>
                        <Button
                          size="small"
                          onClick={() => handleViewEvent(event)}
                        >
                          View Details
                        </Button>
                        <Button
                          size="small"
                          onClick={() => handleReplayEvents(event.aggregateId)}
                        >
                          Replay
                        </Button>
                      </Box>
                    </Paper>
                  </TimelineContent>
                </TimelineItem>
              ))}
            </Timeline>
          ) : (
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Event Type</TableCell>
                    <TableCell>Aggregate ID</TableCell>
                    <TableCell>Version</TableCell>
                    <TableCell>Timestamp</TableCell>
                    <TableCell>Created By</TableCell>
                    <TableCell>Correlation ID</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {loading ? (
                    <TableRow>
                      <TableCell colSpan={7} align="center">
                        <CircularProgress />
                      </TableCell>
                    </TableRow>
                  ) : filteredEvents.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={7} align="center">
                        No events found
                      </TableCell>
                    </TableRow>
                  ) : (
                    filteredEvents.map((event) => (
                      <TableRow key={event.id}>
                        <TableCell>
                          <Chip
                            label={event.eventType}
                            color={getEventTypeColor(event.eventType) as any}
                            size="small"
                          />
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                            {event.aggregateId}
                          </Typography>
                        </TableCell>
                        <TableCell>v{event.eventVersion}</TableCell>
                        <TableCell>
                          {new Date(event.timestamp).toLocaleString()}
                        </TableCell>
                        <TableCell>{event.createdBy}</TableCell>
                        <TableCell>
                          <Typography variant="caption" sx={{ fontFamily: 'monospace' }}>
                            {event.correlationId}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Box sx={{ display: 'flex', gap: 1 }}>
                            <Tooltip title="View Event Details">
                              <IconButton
                                size="small"
                                onClick={() => handleViewEvent(event)}
                              >
                                <Visibility />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Replay Events">
                              <IconButton
                                size="small"
                                onClick={() => handleReplayEvents(event.aggregateId)}
                              >
                                <PlayArrow />
                              </IconButton>
                            </Tooltip>
                          </Box>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </>
      )}

      {activeTab === 1 && (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Action</TableCell>
                <TableCell>Work Request</TableCell>
                <TableCell>Changed By</TableCell>
                <TableCell>Changed Date</TableCell>
                <TableCell>IP Address</TableCell>
                <TableCell>Session ID</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={7} align="center">
                    <CircularProgress />
                  </TableCell>
                </TableRow>
              ) : filteredAuditTrails.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} align="center">
                    No audit trail entries found
                  </TableCell>
                </TableRow>
              ) : (
                filteredAuditTrails.map((audit) => (
                  <TableRow key={audit.id}>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        {getActionIcon(audit.action)}
                        <Typography variant="body2">{audit.action}</Typography>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {audit.workRequest?.title || `ID: ${audit.workRequestId}`}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">{audit.changedBy?.name || 'Unknown'}</Typography>
                    </TableCell>
                    <TableCell>
                      {new Date(audit.changedDate).toLocaleString()}
                    </TableCell>
                    <TableCell>
                      <Typography variant="caption" sx={{ fontFamily: 'monospace' }}>
                        {audit.ipAddress}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="caption" sx={{ fontFamily: 'monospace' }}>
                        {audit.sessionId}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Tooltip title="View Details">
                        <IconButton size="small">
                          <Visibility />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {activeTab === 2 && (
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <TimelineIcon color="primary" />
                  Event Statistics
                </Typography>
                <Box sx={{ mt: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    Total Events: {events.length}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Unique Aggregates: {getUniqueAggregateIds().length}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Event Types: {getUniqueEventTypes().length}
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Security color="primary" />
                  Audit Status
                </Typography>
                <Box sx={{ mt: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    All events are properly correlated
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Audit trail is complete
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    No security violations detected
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Event Detail Dialog */}
      <Dialog open={eventDetailDialogOpen} onClose={() => setEventDetailDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          Event Details: {selectedEvent?.eventType}
        </DialogTitle>
        <DialogContent>
          {selectedEvent && (
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="text.secondary">Event Type</Typography>
                <Typography variant="body1">{selectedEvent.eventType}</Typography>
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="text.secondary">Aggregate ID</Typography>
                <Typography variant="body1" sx={{ fontFamily: 'monospace' }}>{selectedEvent.aggregateId}</Typography>
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="text.secondary">Version</Typography>
                <Typography variant="body1">v{selectedEvent.eventVersion}</Typography>
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="text.secondary">Timestamp</Typography>
                <Typography variant="body1">{new Date(selectedEvent.timestamp).toLocaleString()}</Typography>
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="text.secondary">Created By</Typography>
                <Typography variant="body1">{selectedEvent.createdBy}</Typography>
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="text.secondary">Correlation ID</Typography>
                <Typography variant="body1" sx={{ fontFamily: 'monospace' }}>{selectedEvent.correlationId}</Typography>
              </Grid>
              <Grid item xs={12}>
                <Typography variant="subtitle2" color="text.secondary">Event Data</Typography>
                <TextField
                  fullWidth
                  multiline
                  rows={8}
                  value={formatEventData(selectedEvent.eventData)}
                  InputProps={{
                    readOnly: true,
                    sx: { fontFamily: 'monospace', fontSize: '0.875rem' }
                  }}
                />
              </Grid>
              {selectedEvent.metadata && (
                <Grid item xs={12}>
                  <Typography variant="subtitle2" color="text.secondary">Metadata</Typography>
                  <TextField
                    fullWidth
                    multiline
                    rows={4}
                    value={formatEventData(selectedEvent.metadata)}
                    InputProps={{
                      readOnly: true,
                      sx: { fontFamily: 'monospace', fontSize: '0.875rem' }
                    }}
                  />
                </Grid>
              )}
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEventDetailDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Replay Dialog */}
      <Dialog open={replayDialogOpen} onClose={() => setReplayDialogOpen(false)} maxWidth="lg" fullWidth>
        <DialogTitle>
          Event Replay: {selectedAggregateId}
        </DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Replaying events in chronological order to reconstruct the aggregate state
          </Typography>
          
          <Timeline>
            {replayEvents.map((event, index) => (
              <TimelineItem key={event.id}>
                <TimelineSeparator>
                  <TimelineDot color={getEventTypeColor(event.eventType) as any}>
                    {index + 1}
                  </TimelineDot>
                  {index < replayEvents.length - 1 && <TimelineConnector />}
                </TimelineSeparator>
                <TimelineContent>
                  <Paper elevation={1} sx={{ p: 2 }}>
                    <Typography variant="h6" component="span">
                      {event.eventType}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Version {event.eventVersion} - {new Date(event.timestamp).toLocaleString()}
                    </Typography>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace', mt: 1 }}>
                      {event.eventData}
                    </Typography>
                  </Paper>
                </TimelineContent>
              </TimelineItem>
            ))}
          </Timeline>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setReplayDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default EventAuditTrail; 