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
  Chip,
  IconButton,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  LinearProgress,
  Rating,
  CardHeader,
  CardActions
} from '@mui/material';
import {
  CheckCircle,
  Visibility
} from '@mui/icons-material';
import {
  LineChart,
  Line,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  Legend,
  ResponsiveContainer
} from 'recharts';
import { apiService } from '../services/api';

interface PriorityPrediction {
  workRequestId: number;
  predictedPriority: number;
  predictedLevel: string;
  confidence: number;
  reasoning: string;
  predictedDate: string;
}

interface CapacityPrediction {
  departmentId: number;
  departmentName: string;
  targetDate: string;
  predictedUtilization: number;
  currentUtilization: number;
  capacity: number;
  confidence: number;
  trend: string;
  status: string;
  recommendations: string[];
}

interface RiskAssessment {
  workRequestId: number;
  workRequestTitle: string;
  overallRiskScore: number;
  riskLevel: string;
  riskProbability: number;
  riskImpact: string;
  riskFactors: Record<string, number>;
  mitigationStrategies: string[];
  assessmentDate: string;
  nextReviewDate: string;
}

interface WorkloadPrediction {
  departmentId: number;
  departmentName: string;
  targetDate: string;
  predictedUtilization: number;
  predictedWorkItems: number;
  currentWorkItems: number;
  trend: string;
  confidence: number;
}

interface PredictiveInsight {
  id: number;
  type: string;
  title: string;
  description: string;
  confidence: number;
  impact: string;
  recommendations: string[];
  createdDate: string;
}

const AdvancedAnalytics: React.FC = () => {
  const [activeTab, setActiveTab] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Data states
  const [priorityPredictions, setPriorityPredictions] = useState<PriorityPrediction[]>([]);
  const [capacityPredictions, setCapacityPredictions] = useState<CapacityPrediction[]>([]);
  const [riskAssessments, setRiskAssessments] = useState<RiskAssessment[]>([]);
  const [workloadPredictions, setWorkloadPredictions] = useState<WorkloadPrediction[]>([]);
  const [predictiveInsights, setPredictiveInsights] = useState<PredictiveInsight[]>([]);

  // Filter states
  const [selectedDepartment, setSelectedDepartment] = useState<string>('all');
  const [dateRange, setDateRange] = useState<{ start: string; end: string }>({
    start: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    end: new Date().toISOString().split('T')[0]
  });
  const [riskLevelFilter, setRiskLevelFilter] = useState<string>('all');

  // Dialog states

  useEffect(() => {
    loadAnalyticsData();
  }, [selectedDepartment, dateRange, riskLevelFilter]);

  const loadAnalyticsData = async () => {
    try {
      setLoading(true);
      
      // Load priority predictions
      const priorityResponse = await apiService.getApi().get('/api/advancedanalytics/priority-predictions', {
        params: { departmentId: selectedDepartment !== 'all' ? selectedDepartment : undefined }
      });
      setPriorityPredictions(priorityResponse.data);

      // Load capacity predictions
      const capacityResponse = await apiService.getApi().get('/api/advancedanalytics/capacity-predictions', {
        params: { departmentId: selectedDepartment !== 'all' ? selectedDepartment : undefined }
      });
      setCapacityPredictions(capacityResponse.data);

      // Load risk assessments
      const riskResponse = await apiService.getApi().get('/api/advancedanalytics/risk-assessments', {
        params: { 
          departmentId: selectedDepartment !== 'all' ? selectedDepartment : undefined,
          riskLevel: riskLevelFilter !== 'all' ? riskLevelFilter : undefined
        }
      });
      setRiskAssessments(riskResponse.data);

      // Load workload predictions
      const workloadResponse = await apiService.getApi().get('/api/advancedanalytics/workload-predictions', {
        params: { departmentId: selectedDepartment !== 'all' ? selectedDepartment : undefined }
      });
      setWorkloadPredictions(workloadResponse.data);

      // Load predictive insights
      const insightsResponse = await apiService.getApi().get('/api/advancedanalytics/predictive-insights', {
        params: { departmentId: selectedDepartment !== 'all' ? selectedDepartment : undefined }
      });
      setPredictiveInsights(insightsResponse.data);

    } catch (err) {
      setError('Failed to load analytics data');
      console.error('Error loading analytics data:', err);
    } finally {
      setLoading(false);
    }
  };


  const getRiskLevelColor = (level: string) => {
    switch (level.toLowerCase()) {
      case 'critical': return 'error';
      case 'high': return 'warning';
      case 'medium': return 'info';
      case 'low': return 'success';
      default: return 'default';
    }
  };

  const getConfidenceColor = (confidence: number) => {
    if (confidence >= 0.8) return 'success';
    if (confidence >= 0.6) return 'warning';
    return 'error';
  };

  const formatPercentage = (value: number) => `${(value * 100).toFixed(1)}%`;

  const chartData = [
    { name: 'Jan', priority: 7.2, capacity: 85, risk: 6.1 },
    { name: 'Feb', priority: 7.8, capacity: 88, risk: 5.8 },
    { name: 'Mar', priority: 8.1, capacity: 92, risk: 6.5 },
    { name: 'Apr', priority: 7.5, capacity: 87, risk: 5.9 },
    { name: 'May', priority: 8.3, capacity: 94, risk: 7.2 },
    { name: 'Jun', priority: 7.9, capacity: 89, risk: 6.3 }
  ];

  const pieData = [
    { name: 'Critical', value: 15, color: '#f44336' },
    { name: 'High', value: 25, color: '#ff9800' },
    { name: 'Medium', value: 35, color: '#2196f3' },
    { name: 'Low', value: 25, color: '#4caf50' }
  ];

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Advanced Analytics Dashboard
      </Typography>

      {error && (
        <Alert severity="error" onClose={() => setError(null)} sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" onClose={() => setSuccess(null)} sx={{ mb: 2 }}>
          {success}
        </Alert>
      )}

      <Paper sx={{ mb: 3 }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={activeTab} onChange={(_, newValue) => setActiveTab(newValue)}>
            <Tab label="Overview" />
            <Tab label="Priority Predictions" />
            <Tab label="Capacity Planning" />
            <Tab label="Risk Assessment" />
            <Tab label="Workload Forecasting" />
            <Tab label="Predictive Insights" />
          </Tabs>
        </Box>

        <Box sx={{ p: 2 }}>
          {activeTab === 0 && (
            <Box>
              <Grid container spacing={3}>
                {/* Key Metrics */}
                <Grid item xs={12} md={3}>
                  <Card>
                    <CardContent>
                      <Typography color="textSecondary" gutterBottom>
                        Average Priority Score
                      </Typography>
                      <Typography variant="h4">
                        7.8
                      </Typography>
                      <Typography variant="body2" color="textSecondary">
                        +0.3 from last month
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={3}>
                  <Card>
                    <CardContent>
                      <Typography color="textSecondary" gutterBottom>
                        Capacity Utilization
                      </Typography>
                      <Typography variant="h4">
                        87%
                      </Typography>
                      <Typography variant="body2" color="textSecondary">
                        +2% from last month
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={3}>
                  <Card>
                    <CardContent>
                      <Typography color="textSecondary" gutterBottom>
                        Risk Score
                      </Typography>
                      <Typography variant="h4">
                        6.2
                      </Typography>
                      <Typography variant="body2" color="textSecondary">
                        -0.1 from last month
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={3}>
                  <Card>
                    <CardContent>
                      <Typography color="textSecondary" gutterBottom>
                        Predictive Accuracy
                      </Typography>
                      <Typography variant="h4">
                        89%
                      </Typography>
                      <Typography variant="body2" color="textSecondary">
                        +3% from last month
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>

                {/* Charts */}
                <Grid item xs={12} md={8}>
                  <Card>
                    <CardHeader title="Trends Over Time" />
                    <CardContent>
                      <ResponsiveContainer width="100%" height={300}>
                        <LineChart data={chartData}>
                          <CartesianGrid strokeDasharray="3 3" />
                          <XAxis dataKey="name" />
                          <YAxis />
                          <RechartsTooltip />
                          <Legend />
                          <Line type="monotone" dataKey="priority" stroke="#8884d8" name="Priority" />
                          <Line type="monotone" dataKey="capacity" stroke="#82ca9d" name="Capacity %" />
                          <Line type="monotone" dataKey="risk" stroke="#ffc658" name="Risk Score" />
                        </LineChart>
                      </ResponsiveContainer>
                    </CardContent>
                  </Card>
                </Grid>

                <Grid item xs={12} md={4}>
                  <Card>
                    <CardHeader title="Risk Distribution" />
                    <CardContent>
                      <ResponsiveContainer width="100%" height={300}>
                        <PieChart>
                          <Pie
                            data={pieData}
                            cx="50%"
                            cy="50%"
                            labelLine={false}
                            label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                            outerRadius={80}
                            fill="#8884d8"
                            dataKey="value"
                          >
                            {pieData.map((entry, index) => (
                              <Cell key={`cell-${index}`} fill={entry.color} />
                            ))}
                          </Pie>
                          <RechartsTooltip />
                        </PieChart>
                      </ResponsiveContainer>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>
            </Box>
          )}

          {activeTab === 1 && (
            <Box>
              <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                <Typography variant="h6">Priority Predictions</Typography>
                <Button
                  variant="contained"
                  startIcon={<Visibility />}
                  onClick={() => {}}
                >
                  Generate Prediction
                </Button>
              </Box>

              {loading ? (
                <Box display="flex" justifyContent="center" p={3}>
                  <CircularProgress />
                </Box>
              ) : (
                <TableContainer>
                  <Table>
                    <TableHead>
                      <TableRow>
                        <TableCell>Work Request</TableCell>
                        <TableCell>Predicted Priority</TableCell>
                        <TableCell>Level</TableCell>
                        <TableCell>Confidence</TableCell>
                        <TableCell>Reasoning</TableCell>
                        <TableCell>Actions</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {priorityPredictions.map((prediction) => (
                        <TableRow key={prediction.workRequestId}>
                          <TableCell>WR-{prediction.workRequestId}</TableCell>
                          <TableCell>
                            <Box display="flex" alignItems="center">
                              <Typography variant="h6" sx={{ mr: 1 }}>
                                {prediction.predictedPriority.toFixed(1)}
                              </Typography>
                              <Rating value={prediction.predictedPriority / 2} readOnly size="small" />
                            </Box>
                          </TableCell>
                          <TableCell>
                            <Chip 
                              label={prediction.predictedLevel} 
                              color={prediction.predictedLevel === 'Critical' ? 'error' : 'primary'}
                              size="small"
                            />
                          </TableCell>
                          <TableCell>
                            <Chip 
                              label={formatPercentage(prediction.confidence)}
                              color={getConfidenceColor(prediction.confidence)}
                              size="small"
                            />
                          </TableCell>
                          <TableCell>
                            <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>
                              {prediction.reasoning}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <IconButton size="small">
                              <Visibility />
                            </IconButton>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              )}
            </Box>
          )}

          {activeTab === 2 && (
            <Box>
              <Typography variant="h6" gutterBottom>Capacity Planning</Typography>
              
              <Grid container spacing={3}>
                {capacityPredictions.map((prediction) => (
                  <Grid item xs={12} md={6} key={prediction.departmentId}>
                    <Card>
                      <CardHeader 
                        title={prediction.departmentName}
                        subheader={`Target: ${new Date(prediction.targetDate).toLocaleDateString()}`}
                      />
                      <CardContent>
                        <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                          <Typography variant="h6">
                            {formatPercentage(prediction.predictedUtilization)}
                          </Typography>
                          <Chip 
                            label={prediction.status}
                            color={prediction.status === 'Critical' ? 'error' : 'primary'}
                            size="small"
                          />
                        </Box>
                        
                        <LinearProgress 
                          variant="determinate" 
                          value={prediction.predictedUtilization * 100}
                          sx={{ mb: 2 }}
                        />
                        
                        <Typography variant="body2" color="textSecondary" gutterBottom>
                          Current: {formatPercentage(prediction.currentUtilization)}
                        </Typography>
                        
                        <Typography variant="body2" color="textSecondary" gutterBottom>
                          Confidence: {formatPercentage(prediction.confidence)}
                        </Typography>
                        
                        <Typography variant="body2" color="textSecondary">
                          Trend: {prediction.trend}
                        </Typography>
                      </CardContent>
                      <CardActions>
                        <Button size="small">View Details</Button>
                        <Button size="small">Generate Report</Button>
                      </CardActions>
                    </Card>
                  </Grid>
                ))}
              </Grid>
            </Box>
          )}

          {activeTab === 3 && (
            <Box>
              <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                <Typography variant="h6">Risk Assessment</Typography>
                <Button
                  variant="contained"
                  startIcon={<Visibility />}
                  onClick={() => {}}
                >
                  Assess Risk
                </Button>
              </Box>

              {loading ? (
                <Box display="flex" justifyContent="center" p={3}>
                  <CircularProgress />
                </Box>
              ) : (
                <Grid container spacing={3}>
                  {riskAssessments.map((assessment) => (
                    <Grid item xs={12} md={6} key={assessment.workRequestId}>
                      <Card>
                        <CardHeader 
                          title={assessment.workRequestTitle}
                          subheader={`Assessed: ${new Date(assessment.assessmentDate).toLocaleDateString()}`}
                        />
                        <CardContent>
                          <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                            <Typography variant="h6">
                              Score: {assessment.overallRiskScore.toFixed(1)}
                            </Typography>
                            <Chip 
                              label={assessment.riskLevel}
                              color={getRiskLevelColor(assessment.riskLevel)}
                              size="small"
                            />
                          </Box>
                          
                          <Typography variant="body2" color="textSecondary" gutterBottom>
                            Probability: {formatPercentage(assessment.riskProbability)}
                          </Typography>
                          
                          <Typography variant="body2" color="textSecondary" gutterBottom>
                            Impact: {assessment.riskImpact}
                          </Typography>
                          
                          <Typography variant="body2" color="textSecondary">
                            Next Review: {new Date(assessment.nextReviewDate).toLocaleDateString()}
                          </Typography>
                        </CardContent>
                        <CardActions>
                          <Button size="small">View Details</Button>
                          <Button size="small">Mitigation Plan</Button>
                        </CardActions>
                      </Card>
                    </Grid>
                  ))}
                </Grid>
              )}
            </Box>
          )}

          {activeTab === 4 && (
            <Box>
              <Typography variant="h6" gutterBottom>Workload Forecasting</Typography>
              
              <Grid container spacing={3}>
                {workloadPredictions.map((prediction) => (
                  <Grid item xs={12} md={4} key={prediction.departmentId}>
                    <Card>
                      <CardHeader title={prediction.departmentName} />
                      <CardContent>
                        <Typography variant="h6" gutterBottom>
                          {prediction.predictedWorkItems} items
                        </Typography>
                        
                        <Typography variant="body2" color="textSecondary" gutterBottom>
                          Utilization: {formatPercentage(prediction.predictedUtilization)}
                        </Typography>
                        
                        <Typography variant="body2" color="textSecondary" gutterBottom>
                          Current: {prediction.currentWorkItems} items
                        </Typography>
                        
                        <Typography variant="body2" color="textSecondary">
                          Trend: {prediction.trend}
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                ))}
              </Grid>
            </Box>
          )}

          {activeTab === 5 && (
            <Box>
              <Typography variant="h6" gutterBottom>Predictive Insights</Typography>
              
              <Grid container spacing={3}>
                {predictiveInsights.map((insight) => (
                  <Grid item xs={12} md={6} key={insight.id}>
                    <Card>
                      <CardHeader 
                        title={insight.title}
                        subheader={`Confidence: ${formatPercentage(insight.confidence)}`}
                      />
                      <CardContent>
                        <Typography variant="body2" gutterBottom>
                          {insight.description}
                        </Typography>
                        
                        <Typography variant="body2" color="textSecondary" gutterBottom>
                          Impact: {insight.impact}
                        </Typography>
                        
                        <List dense>
                          {insight.recommendations.map((rec, index) => (
                            <ListItem key={index}>
                              <ListItemIcon>
                                <CheckCircle fontSize="small" />
                              </ListItemIcon>
                              <ListItemText primary={rec} />
                            </ListItem>
                          ))}
                        </List>
                      </CardContent>
                      <CardActions>
                        <Button size="small">View Details</Button>
                        <Button size="small">Apply Insights</Button>
                      </CardActions>
                    </Card>
                  </Grid>
                ))}
              </Grid>
            </Box>
          )}
        </Box>
      </Paper>

      {/* Filters */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} md={3}>
            <FormControl fullWidth>
              <InputLabel>Department</InputLabel>
              <Select
                value={selectedDepartment}
                onChange={(e) => setSelectedDepartment(e.target.value)}
              >
                <MenuItem value="all">All Departments</MenuItem>
                <MenuItem value="1">IT Department</MenuItem>
                <MenuItem value="2">Finance Department</MenuItem>
                <MenuItem value="3">HR Department</MenuItem>
                <MenuItem value="4">Operations Department</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} md={3}>
            <TextField
              fullWidth
              label="Start Date"
              type="date"
              value={dateRange.start}
              onChange={(e) => setDateRange({ ...dateRange, start: e.target.value })}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item xs={12} md={3}>
            <TextField
              fullWidth
              label="End Date"
              type="date"
              value={dateRange.end}
              onChange={(e) => setDateRange({ ...dateRange, end: e.target.value })}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item xs={12} md={3}>
            <FormControl fullWidth>
              <InputLabel>Risk Level</InputLabel>
              <Select
                value={riskLevelFilter}
                onChange={(e) => setRiskLevelFilter(e.target.value)}
              >
                <MenuItem value="all">All Levels</MenuItem>
                <MenuItem value="critical">Critical</MenuItem>
                <MenuItem value="high">High</MenuItem>
                <MenuItem value="medium">Medium</MenuItem>
                <MenuItem value="low">Low</MenuItem>
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      </Paper>
    </Box>
  );
};

export default AdvancedAnalytics; 