import React, { useState } from 'react';
import { useQuery } from 'react-query';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  Tabs,
  Tab,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
  Chip,
  CircularProgress,
  useTheme,
  useMediaQuery,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  Legend,
  PieChart,
  Pie,
  Cell,
  LineChart,
  Line,
  ResponsiveContainer,
  AreaChart,
  Area,
} from 'recharts';
import {
  Download,
  Warning,
  CheckCircle,
  Schedule,
  People,
  Assignment,
} from '@mui/icons-material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs from 'dayjs';
import { apiService } from '../services/api';
import { useMsal } from '@azure/msal-react';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`reports-tabpanel-${index}`}
      aria-labelledby={`reports-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8', '#82CA9D'];

const Reports: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const { } = useMsal();
  // Using the singleton apiService instance

  const [tabValue, setTabValue] = useState(0);
  const [dateRange, setDateRange] = useState({
    fromDate: dayjs().subtract(30, 'day'),
    toDate: dayjs(),
  });
  const [selectedDepartment, setSelectedDepartment] = useState<string>('all');
  const [selectedBusinessVertical, setSelectedBusinessVertical] = useState<string>('all');

  // Mock data - replace with actual API calls
  const { data: dashboardAnalytics, isLoading: dashboardLoading } = useQuery(
    ['dashboardAnalytics', dateRange, selectedDepartment, selectedBusinessVertical],
    async () => {
      const params = new URLSearchParams({
        fromDate: dateRange.fromDate.toISOString(),
        toDate: dateRange.toDate.toISOString(),
      });
      if (selectedDepartment !== 'all') params.append('departmentId', selectedDepartment);
      if (selectedBusinessVertical !== 'all') params.append('businessVerticalId', selectedBusinessVertical);
      
      const response = await apiService.getApi().get(`/analytics/dashboard?${params}`);
      return response.data;
    },
    {
      refetchInterval: 300000, // 5 minutes
    }
  );

  const { data: workflowAnalytics, isLoading: workflowLoading } = useQuery(
    ['workflowAnalytics', dateRange],
    async () => {
      const params = new URLSearchParams({
        fromDate: dateRange.fromDate.toISOString(),
        toDate: dateRange.toDate.toISOString(),
      });
      const response = await apiService.getApi().get(`/analytics/workflow?${params}`);
      return response.data;
    }
  );

  const { data: resourceAnalytics, isLoading: resourceLoading } = useQuery(
    ['resourceAnalytics', dateRange],
    async () => {
      const params = new URLSearchParams({
        fromDate: dateRange.fromDate.toISOString(),
        toDate: dateRange.toDate.toISOString(),
      });
      const response = await apiService.getApi().get(`/analytics/resource-utilization?${params}`);
      return response.data;
    }
  );

  const { data: slaAnalytics, isLoading: slaLoading } = useQuery(
    ['slaAnalytics', dateRange],
    async () => {
      const params = new URLSearchParams({
        fromDate: dateRange.fromDate.toISOString(),
        toDate: dateRange.toDate.toISOString(),
      });
      const response = await apiService.getApi().get(`/analytics/sla-compliance?${params}`);
      return response.data;
    }
  );

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleExport = (reportType: string) => {
    const params = new URLSearchParams({
      reportType,
      fromDate: dateRange.fromDate.toISOString(),
      toDate: dateRange.toDate.toISOString(),
    });
    if (selectedDepartment !== 'all') params.append('departmentId', selectedDepartment);
    if (selectedBusinessVertical !== 'all') params.append('businessVerticalId', selectedBusinessVertical);
    
    // In a real implementation, this would trigger a file download
    console.log(`Exporting ${reportType} report with params:`, params.toString());
  };

  const StatCard: React.FC<{ title: string; value: string | number; icon: React.ReactElement; color: string; subtitle?: string }> = ({ title, value, icon, color, subtitle }) => (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Box display="flex" alignItems="center" justifyContent="space-between">
          <Box>
            <Typography variant="h6" color="textSecondary" gutterBottom>
              {title}
            </Typography>
            <Typography variant="h4" component="div" sx={{ color }}>
              {value}
            </Typography>
            {subtitle && (
              <Typography variant="body2" color="textSecondary">
                {subtitle}
              </Typography>
            )}
          </Box>
          <Box sx={{ color }}>
            {icon}
          </Box>
        </Box>
      </CardContent>
    </Card>
  );

  if (dashboardLoading || workflowLoading || resourceLoading || slaLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress size={60} />
      </Box>
    );
  }

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <Box>
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={3} flexWrap="wrap" gap={2}>
          <Typography variant="h4" component="h1">
            Advanced Analytics & Reports
          </Typography>
          <Box display="flex" gap={1} flexWrap="wrap">
            <Button
              variant="outlined"
              startIcon={<Download />}
              onClick={() => handleExport('dashboard')}
              size={isMobile ? 'small' : 'medium'}
            >
              Export Dashboard
            </Button>
            <Button
              variant="outlined"
              startIcon={<Download />}
              onClick={() => handleExport('workflow')}
              size={isMobile ? 'small' : 'medium'}
            >
              Export Workflow
            </Button>
          </Box>
        </Box>

        {/* Filters */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Grid container spacing={2} alignItems="center">
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="From Date"
                  value={dateRange.fromDate}
                  onChange={(newValue) => setDateRange(prev => ({ ...prev, fromDate: newValue || dayjs() }))}
                  slotProps={{ textField: { fullWidth: true, size: 'small' } }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="To Date"
                  value={dateRange.toDate}
                  onChange={(newValue) => setDateRange(prev => ({ ...prev, toDate: newValue || dayjs() }))}
                  slotProps={{ textField: { fullWidth: true, size: 'small' } }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Department</InputLabel>
                  <Select
                    value={selectedDepartment}
                    label="Department"
                    onChange={(e) => setSelectedDepartment(e.target.value)}
                  >
                    <MenuItem value="all">All Departments</MenuItem>
                    <MenuItem value="1">IT Department</MenuItem>
                    <MenuItem value="2">Business Operations</MenuItem>
                    <MenuItem value="3">Customer Service</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Business Vertical</InputLabel>
                  <Select
                    value={selectedBusinessVertical}
                    label="Business Vertical"
                    onChange={(e) => setSelectedBusinessVertical(e.target.value)}
                  >
                    <MenuItem value="all">All Verticals</MenuItem>
                    <MenuItem value="1">Medicaid</MenuItem>
                    <MenuItem value="2">Medicare</MenuItem>
                    <MenuItem value="3">Exchange</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          </CardContent>
        </Card>

        {/* Tabs */}
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={tabValue} onChange={handleTabChange} variant={isMobile ? 'scrollable' : 'fullWidth'} scrollButtons="auto">
            <Tab label="Dashboard Overview" />
            <Tab label="Workflow Analytics" />
            <Tab label="Resource Utilization" />
            <Tab label="SLA Compliance" />
            <Tab label="Trend Analysis" />
          </Tabs>
        </Box>

        {/* Dashboard Overview Tab */}
        <TabPanel value={tabValue} index={0}>
          <Grid container spacing={3}>
            {/* Key Metrics */}
            <Grid item xs={12} sm={6} md={3}>
              <StatCard
                title="Active Requests"
                value={dashboardAnalytics?.totalActiveRequests || 0}
                icon={<Assignment />}
                color="#1976d2"
                subtitle="Currently in progress"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <StatCard
                title="SLA Compliance"
                value={`${dashboardAnalytics?.slaComplianceRate?.toFixed(1) || 0}%`}
                icon={<CheckCircle />}
                color="#2e7d32"
                subtitle="On-time delivery"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <StatCard
                title="Resource Utilization"
                value={`${dashboardAnalytics?.resourceUtilization?.toFixed(1) || 0}%`}
                icon={<People />}
                color="#ed6c02"
                subtitle="Team capacity usage"
              />
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <StatCard
                title="Avg Completion Time"
                value={`${dashboardAnalytics?.averageCompletionTime?.toFixed(1) || 0} days`}
                icon={<Schedule />}
                color="#9c27b0"
                subtitle="Time to delivery"
              />
            </Grid>

            {/* Charts */}
            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Work Category Distribution
                  </Typography>
                  <ResponsiveContainer width="100%" height={300}>
                    <PieChart>
                      <Pie
                        data={[
                          { name: 'Work Requests', value: 45 },
                          { name: 'Projects', value: 30 },
                          { name: 'Break Fix', value: 25 },
                        ]}
                        cx="50%"
                        cy="50%"
                        labelLine={false}
                        label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                        outerRadius={80}
                        fill="#8884d8"
                        dataKey="value"
                      >
                        {COLORS.map((color, index) => (
                          <Cell key={`cell-${index}`} fill={color} />
                        ))}
                      </Pie>
                      <RechartsTooltip />
                    </PieChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Priority Distribution
                  </Typography>
                  <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={[
                      { name: 'Critical', value: 12, color: '#FF4444' },
                      { name: 'High', value: 34, color: '#FF8800' },
                      { name: 'Medium', value: 67, color: '#FFBB00' },
                      { name: 'Low', value: 43, color: '#00AA00' },
                    ]}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="name" />
                      <YAxis />
                      <RechartsTooltip />
                      <Bar dataKey="value" fill="#8884d8">
                        {COLORS.map((color, index) => (
                          <Cell key={`cell-${index}`} fill={color} />
                        ))}
                      </Bar>
                    </BarChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </TabPanel>

        {/* Workflow Analytics Tab */}
        <TabPanel value={tabValue} index={1}>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Workflow Stage Performance
                  </Typography>
                  <ResponsiveContainer width="100%" height={400}>
                    <BarChart data={[
                      { stage: 'Intake', requests: 25, avgTime: 2.5, completionRate: 95 },
                      { stage: 'Business Review', requests: 18, avgTime: 3.2, completionRate: 88 },
                      { stage: 'Priority Assessment', requests: 15, avgTime: 1.8, completionRate: 92 },
                      { stage: 'Architecture', requests: 12, avgTime: 4.1, completionRate: 85 },
                      { stage: 'Development', requests: 8, avgTime: 12.5, completionRate: 78 },
                    ]}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="stage" />
                      <YAxis yAxisId="left" />
                      <YAxis yAxisId="right" orientation="right" />
                      <RechartsTooltip />
                      <Legend />
                      <Bar yAxisId="left" dataKey="requests" fill="#8884d8" name="Active Requests" />
                      <Bar yAxisId="right" dataKey="avgTime" fill="#82ca9d" name="Avg Days" />
                    </BarChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Workflow Bottlenecks
                  </Typography>
                  <Box>
                    {workflowAnalytics?.bottlenecks?.map((bottleneck: any, index: number) => (
                      <Box key={index} display="flex" alignItems="center" mb={2} p={2} bgcolor="background.paper" borderRadius={1}>
                        <Warning color="warning" sx={{ mr: 2 }} />
                        <Box flex={1}>
                          <Typography variant="subtitle2">{bottleneck.stage}</Typography>
                          <Typography variant="body2" color="textSecondary">
                            {bottleneck.reason}
                          </Typography>
                        </Box>
                        <Chip label={`${bottleneck.requestCount} requests`} size="small" />
                      </Box>
                    )) || (
                      <Typography variant="body2" color="textSecondary">
                        No bottlenecks identified
                      </Typography>
                    )}
                  </Box>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Stage Transition Trends
                  </Typography>
                  <ResponsiveContainer width="100%" height={300}>
                    <LineChart data={[
                      { date: 'Jan', transitions: 45 },
                      { date: 'Feb', transitions: 52 },
                      { date: 'Mar', transitions: 48 },
                      { date: 'Apr', transitions: 61 },
                      { date: 'May', transitions: 55 },
                      { date: 'Jun', transitions: 67 },
                    ]}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="date" />
                      <YAxis />
                      <RechartsTooltip />
                      <Line type="monotone" dataKey="transitions" stroke="#8884d8" strokeWidth={2} />
                    </LineChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </TabPanel>

        {/* Resource Utilization Tab */}
        <TabPanel value={tabValue} index={2}>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Department Resource Utilization
                  </Typography>
                  <ResponsiveContainer width="100%" height={400}>
                    <BarChart data={[
                      { department: 'IT', allocated: 85, available: 100, utilization: 85 },
                      { department: 'Business Ops', allocated: 72, available: 100, utilization: 72 },
                      { department: 'Customer Service', allocated: 95, available: 100, utilization: 95 },
                      { department: 'Development', allocated: 88, available: 100, utilization: 88 },
                    ]}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="department" />
                      <YAxis />
                      <RechartsTooltip />
                      <Legend />
                      <Bar dataKey="allocated" fill="#8884d8" name="Allocated Hours" />
                      <Bar dataKey="available" fill="#82ca9d" name="Available Hours" />
                    </BarChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Capacity Gaps
                  </Typography>
                  <TableContainer>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Department</TableCell>
                          <TableCell align="right">Required</TableCell>
                          <TableCell align="right">Available</TableCell>
                          <TableCell align="right">Gap</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {resourceAnalytics?.capacityGaps?.map((gap: any, index: number) => (
                          <TableRow key={index}>
                            <TableCell>{gap.departmentName}</TableCell>
                            <TableCell align="right">{gap.requiredHours}</TableCell>
                            <TableCell align="right">{gap.availableHours}</TableCell>
                            <TableCell align="right">
                              <Chip 
                                label={`${gap.gap}h`} 
                                color="error" 
                                size="small" 
                                variant="outlined"
                              />
                            </TableCell>
                          </TableRow>
                        )) || (
                          <TableRow>
                            <TableCell colSpan={4} align="center">
                              <Typography variant="body2" color="textSecondary">
                                No capacity gaps identified
                              </Typography>
                            </TableCell>
                          </TableRow>
                        )}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Utilization Trends
                  </Typography>
                  <ResponsiveContainer width="100%" height={300}>
                    <AreaChart data={[
                      { month: 'Jan', utilization: 75 },
                      { month: 'Feb', utilization: 78 },
                      { month: 'Mar', utilization: 82 },
                      { month: 'Apr', utilization: 79 },
                      { month: 'May', utilization: 85 },
                      { month: 'Jun', utilization: 88 },
                    ]}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="month" />
                      <YAxis />
                      <RechartsTooltip />
                      <Area type="monotone" dataKey="utilization" stroke="#8884d8" fill="#8884d8" fillOpacity={0.3} />
                    </AreaChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </TabPanel>

        {/* SLA Compliance Tab */}
        <TabPanel value={tabValue} index={3}>
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    SLA Compliance by Category
                  </Typography>
                  <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={[
                      { category: 'Work Requests', compliance: 92 },
                      { category: 'Projects', compliance: 87 },
                      { category: 'Break Fix', compliance: 95 },
                    ]}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="category" />
                      <YAxis />
                      <RechartsTooltip />
                      <Bar dataKey="compliance" fill="#82ca9d">
                        {COLORS.map((color, index) => (
                          <Cell key={`cell-${index}`} fill={color} />
                        ))}
                      </Bar>
                    </BarChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    SLA Violations
                  </Typography>
                  <TableContainer>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Request</TableCell>
                          <TableCell>Stage</TableCell>
                          <TableCell align="right">Days Overdue</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {slaAnalytics?.violations?.slice(0, 5).map((violation: any, index: number) => (
                          <TableRow key={index}>
                            <TableCell>{violation.title}</TableCell>
                            <TableCell>{violation.stage}</TableCell>
                            <TableCell align="right">
                              <Chip 
                                label={`${violation.daysOverdue}d`} 
                                color="error" 
                                size="small" 
                                variant="outlined"
                              />
                            </TableCell>
                          </TableRow>
                        )) || (
                          <TableRow>
                            <TableCell colSpan={3} align="center">
                              <Typography variant="body2" color="textSecondary">
                                No SLA violations
                              </Typography>
                            </TableCell>
                          </TableRow>
                        )}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    SLA Compliance Trends
                  </Typography>
                  <ResponsiveContainer width="100%" height={300}>
                    <LineChart data={[
                      { month: 'Jan', compliance: 85 },
                      { month: 'Feb', compliance: 87 },
                      { month: 'Mar', compliance: 89 },
                      { month: 'Apr', compliance: 91 },
                      { month: 'May', compliance: 93 },
                      { month: 'Jun', compliance: 95 },
                    ]}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="month" />
                      <YAxis />
                      <RechartsTooltip />
                      <Line type="monotone" dataKey="compliance" stroke="#82ca9d" strokeWidth={2} />
                    </LineChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </TabPanel>

        {/* Trend Analysis Tab */}
        <TabPanel value={tabValue} index={4}>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Request Volume Trends
                  </Typography>
                  <ResponsiveContainer width="100%" height={400}>
                    <LineChart data={[
                      { date: 'Jan 1', requests: 12, completed: 8 },
                      { date: 'Jan 8', requests: 15, completed: 12 },
                      { date: 'Jan 15', requests: 18, completed: 14 },
                      { date: 'Jan 22', requests: 22, completed: 19 },
                      { date: 'Jan 29', requests: 25, completed: 21 },
                      { date: 'Feb 5', requests: 28, completed: 24 },
                    ]}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="date" />
                      <YAxis />
                      <RechartsTooltip />
                      <Legend />
                      <Line type="monotone" dataKey="requests" stroke="#8884d8" strokeWidth={2} name="New Requests" />
                      <Line type="monotone" dataKey="completed" stroke="#82ca9d" strokeWidth={2} name="Completed" />
                    </LineChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Priority Trends
                  </Typography>
                  <ResponsiveContainer width="100%" height={300}>
                    <AreaChart data={[
                      { month: 'Jan', critical: 5, high: 12, medium: 18, low: 8 },
                      { month: 'Feb', critical: 7, high: 15, medium: 22, low: 10 },
                      { month: 'Mar', critical: 4, high: 11, medium: 20, low: 9 },
                      { month: 'Apr', critical: 6, high: 14, medium: 25, low: 12 },
                      { month: 'May', critical: 8, high: 18, medium: 28, low: 15 },
                      { month: 'Jun', critical: 5, high: 13, medium: 24, low: 11 },
                    ]}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="month" />
                      <YAxis />
                      <RechartsTooltip />
                      <Legend />
                      <Area type="monotone" dataKey="critical" stackId="1" stroke="#FF4444" fill="#FF4444" />
                      <Area type="monotone" dataKey="high" stackId="1" stroke="#FF8800" fill="#FF8800" />
                      <Area type="monotone" dataKey="medium" stackId="1" stroke="#FFBB00" fill="#FFBB00" />
                      <Area type="monotone" dataKey="low" stackId="1" stroke="#00AA00" fill="#00AA00" />
                    </AreaChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Completion Time Trends
                  </Typography>
                  <ResponsiveContainer width="100%" height={300}>
                    <LineChart data={[
                      { month: 'Jan', avgTime: 25 },
                      { month: 'Feb', avgTime: 23 },
                      { month: 'Mar', avgTime: 21 },
                      { month: 'Apr', avgTime: 19 },
                      { month: 'May', avgTime: 18 },
                      { month: 'Jun', avgTime: 16 },
                    ]}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="month" />
                      <YAxis />
                      <RechartsTooltip />
                      <Line type="monotone" dataKey="avgTime" stroke="#9c27b0" strokeWidth={2} />
                    </LineChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </TabPanel>
      </Box>
    </LocalizationProvider>
  );
};

export default Reports;