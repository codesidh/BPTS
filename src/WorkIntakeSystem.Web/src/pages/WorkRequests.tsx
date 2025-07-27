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
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  CircularProgress,
  Tooltip,
  Badge,
  Tabs,
  Tab
} from '@mui/material';
import {
  Add,
  Edit,
  Delete,
  Visibility,
  TrendingUp,
  TrendingDown,
  Schedule,
  CheckCircle,
  Warning,
  Error,
  FilterList,
  Sort,
  Refresh
} from '@mui/icons-material';
import { createApiService } from '../services/api';
import { WorkRequest, WorkCategory, PriorityLevel, WorkStatus, WorkflowStage } from '../types';

interface WorkRequestWithDetails extends WorkRequest {
  departmentName: string;
  submitterName: string;
  businessVerticalName: string;
}

const WorkRequests: React.FC = () => {
  const [workRequests, setWorkRequests] = useState<WorkRequestWithDetails[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [activeTab, setActiveTab] = useState(0);
  const [selectedWorkRequest, setSelectedWorkRequest] = useState<WorkRequestWithDetails | null>(null);
  const [detailDialogOpen, setDetailDialogOpen] = useState(false);

  // Create API service instance
  const apiService = createApiService();

  // Filters
  const [filters, setFilters] = useState({
    search: '',
    category: '',
    priorityLevel: '',
    status: '',
    department: '',
    businessVertical: ''
  });

  // Statistics
  const [stats, setStats] = useState({
    total: 0,
    active: 0,
    completed: 0,
    critical: 0,
    high: 0
  });

  useEffect(() => {
    loadWorkRequests();
    loadStatistics();
  }, [filters, page, rowsPerPage]);

  const loadWorkRequests = async () => {
    try {
      setLoading(true);
      const workRequestsData = await apiService.getWorkRequests();
      setWorkRequests(workRequestsData as WorkRequestWithDetails[]);
    } catch (err) {
      setError('Failed to load work requests');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const loadStatistics = async () => {
    try {
      const analytics = await apiService.getDashboardStats();
      setStats({
        total: analytics.totalActiveRequests + (analytics.totalByStatus[WorkStatus.Closed] || 0),
        active: analytics.totalActiveRequests,
        completed: analytics.totalByStatus[WorkStatus.Closed] || 0,
        critical: analytics.totalByPriority[PriorityLevel.Critical] || 0,
        high: analytics.totalByPriority[PriorityLevel.High] || 0
      });
    } catch (err) {
      console.error('Failed to load statistics:', err);
    }
  };

  const handleFilterChange = (field: string, value: string) => {
    setFilters(prev => ({ ...prev, [field]: value }));
    setPage(0);
  };

  const handleAdvanceWorkflow = async (workRequestId: number, nextStage: WorkflowStage) => {
    try {
      // This would need to be implemented in the API service
      console.log('Advancing workflow:', workRequestId, nextStage);
      loadWorkRequests();
    } catch (err) {
      setError('Failed to advance workflow');
      console.error(err);
    }
  };

  const handleRecalculatePriority = async (workRequestId: number) => {
    try {
      await apiService.recalculatePriority(workRequestId);
      loadWorkRequests();
    } catch (err) {
      setError('Failed to recalculate priority');
      console.error(err);
    }
  };

  const handleViewDetails = (workRequest: WorkRequestWithDetails) => {
    setSelectedWorkRequest(workRequest);
    setDetailDialogOpen(true);
  };

  const getStatusColor = (status: WorkStatus) => {
    switch (status) {
      case WorkStatus.Submitted: return 'primary';
      case WorkStatus.InProgress: return 'warning';
      case WorkStatus.UnderReview: return 'info';
      case WorkStatus.Approved: return 'success';
      case WorkStatus.Closed: return 'default';
      default: return 'default';
    }
  };

  const getPriorityColor = (level: PriorityLevel) => {
    switch (level) {
      case PriorityLevel.Critical: return 'error';
      case PriorityLevel.High: return 'warning';
      case PriorityLevel.Medium: return 'info';
      case PriorityLevel.Low: return 'success';
      default: return 'default';
    }
  };

  const getWorkflowStageColor = (stage: WorkflowStage) => {
    switch (stage) {
      case WorkflowStage.Intake: return 'primary';
      case WorkflowStage.BusinessReview: return 'warning';
      case WorkflowStage.PriorityAssessment: return 'info';
      case WorkflowStage.Approval: return 'success';
      case WorkflowStage.Development: return 'secondary';
      case WorkflowStage.Closure: return 'default';
      default: return 'default';
    }
  };

  const getNextWorkflowStage = (currentStage: WorkflowStage): WorkflowStage | null => {
    const stages = Object.values(WorkflowStage).filter(x => typeof x === 'number') as WorkflowStage[];
    const currentIndex = stages.indexOf(currentStage);
    return currentIndex < stages.length - 1 ? stages[currentIndex + 1] : null;
  };

  const filteredWorkRequests = workRequests.filter(wr => {
    if (filters.search && !wr.title.toLowerCase().includes(filters.search.toLowerCase())) return false;
    if (filters.category && wr.category !== parseInt(filters.category)) return false;
    if (filters.priorityLevel && wr.priorityLevel !== parseInt(filters.priorityLevel)) return false;
    if (filters.status && wr.status !== parseInt(filters.status)) return false;
    return true;
  });

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Work Requests
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Statistics Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Total Requests
              </Typography>
              <Typography variant="h4">
                {stats.total}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Active
              </Typography>
              <Typography variant="h4" color="primary">
                {stats.active}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Completed
              </Typography>
              <Typography variant="h4" color="success.main">
                {stats.completed}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Critical
              </Typography>
              <Typography variant="h4" color="error.main">
                {stats.critical}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                High Priority
              </Typography>
              <Typography variant="h4" color="warning.main">
                {stats.high}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent>
              <Button
                variant="contained"
                startIcon={<Add />}
                fullWidth
                href="/create-work-request"
              >
                New Request
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Filters */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} md={3}>
            <TextField
              fullWidth
              label="Search"
              value={filters.search}
              onChange={(e) => handleFilterChange('search', e.target.value)}
              placeholder="Search by title..."
            />
          </Grid>
          <Grid item xs={12} md={2}>
            <FormControl fullWidth>
              <InputLabel>Category</InputLabel>
              <Select
                value={filters.category}
                onChange={(e) => handleFilterChange('category', e.target.value)}
                label="Category"
              >
                <MenuItem value="">All</MenuItem>
                {Object.values(WorkCategory).map(category => (
                  <MenuItem key={category} value={category}>{category}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} md={2}>
            <FormControl fullWidth>
              <InputLabel>Priority</InputLabel>
              <Select
                value={filters.priorityLevel}
                onChange={(e) => handleFilterChange('priorityLevel', e.target.value)}
                label="Priority"
              >
                <MenuItem value="">All</MenuItem>
                {Object.values(PriorityLevel).map(level => (
                  <MenuItem key={level} value={level}>{level}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} md={2}>
            <FormControl fullWidth>
              <InputLabel>Status</InputLabel>
              <Select
                value={filters.status}
                onChange={(e) => handleFilterChange('status', e.target.value)}
                label="Status"
              >
                <MenuItem value="">All</MenuItem>
                {Object.values(WorkStatus).map(status => (
                  <MenuItem key={status} value={status}>{status}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} md={2}>
            <Button
              variant="outlined"
              startIcon={<Refresh />}
              onClick={loadWorkRequests}
              fullWidth
            >
              Refresh
            </Button>
          </Grid>
        </Grid>
      </Paper>

      {/* Work Requests Table */}
      <Paper>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>ID</TableCell>
                <TableCell>Title</TableCell>
                <TableCell>Category</TableCell>
                <TableCell>Priority</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Stage</TableCell>
                <TableCell>Department</TableCell>
                <TableCell>Submitter</TableCell>
                <TableCell>Created</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={10} align="center">
                    <CircularProgress />
                  </TableCell>
                </TableRow>
              ) : filteredWorkRequests.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={10} align="center">
                    No work requests found
                  </TableCell>
                </TableRow>
              ) : (
                filteredWorkRequests.map((workRequest) => (
                  <TableRow key={workRequest.id}>
                    <TableCell>{workRequest.id}</TableCell>
                    <TableCell>
                      <Typography variant="body2" fontWeight="medium">
                        {workRequest.title}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip label={workRequest.category} size="small" />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={workRequest.priorityLevel}
                        color={getPriorityColor(workRequest.priorityLevel) as any}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={workRequest.status}
                        color={getStatusColor(workRequest.status) as any}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={workRequest.currentStage}
                        color={getWorkflowStageColor(workRequest.currentStage) as any}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>{workRequest.departmentName}</TableCell>
                    <TableCell>{workRequest.submitterName}</TableCell>
                    <TableCell>
                      {new Date(workRequest.createdDate).toLocaleDateString()}
                    </TableCell>
                    <TableCell>
                      <Box display="flex" gap={1}>
                        <Tooltip title="View Details">
                          <IconButton
                            size="small"
                            onClick={() => handleViewDetails(workRequest)}
                          >
                            <Visibility />
                          </IconButton>
                        </Tooltip>
                        
                        {getNextWorkflowStage(workRequest.currentStage) && (
                          <Tooltip title="Advance Workflow">
                            <IconButton
                              size="small"
                              onClick={() => handleAdvanceWorkflow(
                                workRequest.id, 
                                getNextWorkflowStage(workRequest.currentStage)!
                              )}
                            >
                              <TrendingUp />
                            </IconButton>
                          </Tooltip>
                        )}
                        
                        <Tooltip title="Recalculate Priority">
                          <IconButton
                            size="small"
                            onClick={() => handleRecalculatePriority(workRequest.id)}
                          >
                            <Refresh />
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
        
        <TablePagination
          component="div"
          count={filteredWorkRequests.length}
          page={page}
          onPageChange={(_, newPage) => setPage(newPage)}
          rowsPerPage={rowsPerPage}
          onRowsPerPageChange={(e) => {
            setRowsPerPage(parseInt(e.target.value, 10));
            setPage(0);
          }}
        />
      </Paper>

      {/* Detail Dialog */}
      <Dialog
        open={detailDialogOpen}
        onClose={() => setDetailDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          Work Request Details: {selectedWorkRequest?.title}
        </DialogTitle>
        <DialogContent>
          {selectedWorkRequest && (
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="textSecondary">
                  ID
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {selectedWorkRequest.id}
                </Typography>
              </Grid>
              
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="textSecondary">
                  Status
                </Typography>
                <Chip
                  label={selectedWorkRequest.status}
                  color={getStatusColor(selectedWorkRequest.status) as any}
                  size="small"
                />
              </Grid>
              
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="textSecondary">
                  Priority
                </Typography>
                <Chip
                  label={selectedWorkRequest.priorityLevel}
                  color={getPriorityColor(selectedWorkRequest.priorityLevel) as any}
                  size="small"
                />
              </Grid>
              
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="textSecondary">
                  Current Stage
                </Typography>
                <Chip
                  label={selectedWorkRequest.currentStage}
                  color={getWorkflowStageColor(selectedWorkRequest.currentStage) as any}
                  size="small"
                />
              </Grid>
              
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="textSecondary">
                  Department
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {selectedWorkRequest.departmentName}
                </Typography>
              </Grid>
              
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="textSecondary">
                  Submitter
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {selectedWorkRequest.submitterName}
                </Typography>
              </Grid>
              
              <Grid item xs={12}>
                <Typography variant="subtitle2" color="textSecondary">
                  Description
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {selectedWorkRequest.description}
                </Typography>
              </Grid>
              
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="textSecondary">
                  Created Date
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {new Date(selectedWorkRequest.createdDate).toLocaleString()}
                </Typography>
              </Grid>
              
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" color="textSecondary">
                  Modified Date
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {new Date(selectedWorkRequest.modifiedDate).toLocaleString()}
                </Typography>
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDetailDialogOpen(false)}>
            Close
          </Button>
          <Button
            variant="contained"
            href={`/work-request/${selectedWorkRequest?.id}`}
          >
            Edit
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default WorkRequests;