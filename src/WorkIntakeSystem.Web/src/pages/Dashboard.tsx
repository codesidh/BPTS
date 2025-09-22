import React from 'react';
import { useQuery } from 'react-query';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  CircularProgress,
  Alert,
  Chip,
} from '@mui/material';
import {
  TrendingUp,
  Assignment,
  Schedule,
  CheckCircle,
  Warning,
  Speed,
} from '@mui/icons-material';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
} from 'recharts';
import { PriorityLevel, WorkCategory, WorkStatus, DashboardAnalytics } from '../types';
import { apiService } from '../services/api';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8'];

interface StatCardProps {
  title: string;
  value: string | number;
  icon: React.ReactElement;
  color: string;
  subtitle?: string;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, icon, color, subtitle }) => (
  <Card sx={{ height: '100%' }}>
    <CardContent>
      <Box display="flex" alignItems="center" justifyContent="space-between">
        <Box>
          <Typography color="textSecondary" gutterBottom variant="h6">
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
        <Box sx={{ color, fontSize: 48 }}>
          {icon}
        </Box>
      </Box>
    </CardContent>
  </Card>
);

const Dashboard: React.FC = () => {
  // Fetch real dashboard data from API
  const { data: dashboardData, isLoading, error } = useQuery<DashboardAnalytics>(
    'dashboardStats',
    async () => {
      console.log('Dashboard: Fetching dashboard analytics...');
      const analytics = await apiService.getDashboardAnalytics();
      console.log('Dashboard: Received analytics data:', analytics);
      return analytics;
    },
    {
      refetchInterval: 30000, // Refresh every 30 seconds
      retry: 3,
      staleTime: 5 * 60 * 1000, // 5 minutes
    }
  );

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress size={60} />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        Failed to load dashboard data. Please try again later.
      </Alert>
    );
  }

  const data = dashboardData!;

  // Prepare chart data
  const categoryData = [
    { name: 'Work Requests', value: data.requestsByCategory[WorkCategory.WorkRequest] || 0 },
    { name: 'Projects', value: data.requestsByCategory[WorkCategory.Project] || 0 },
    { name: 'Break Fix', value: data.requestsByCategory[WorkCategory.BreakFix] || 0 },
    { name: 'Other', value: data.requestsByCategory[WorkCategory.Other] || 0 },
  ];

  const priorityData = [
    { name: 'Critical', value: data.requestsByPriority[PriorityLevel.Critical] || 0, color: '#FF4444' },
    { name: 'High', value: data.requestsByPriority[PriorityLevel.High] || 0, color: '#FF8800' },
    { name: 'Medium', value: data.requestsByPriority[PriorityLevel.Medium] || 0, color: '#FFBB00' },
    { name: 'Low', value: data.requestsByPriority[PriorityLevel.Low] || 0, color: '#00AA00' },
  ];

  const statusData = [
    { name: 'Draft', value: data.requestsByStatus[WorkStatus.Draft] || 0 },
    { name: 'Submitted', value: data.requestsByStatus[WorkStatus.Submitted] || 0 },
    { name: 'Under Review', value: data.requestsByStatus[WorkStatus.UnderReview] || 0 },
    { name: 'Approved', value: data.requestsByStatus[WorkStatus.Approved] || 0 },
    { name: 'In Progress', value: data.requestsByStatus[WorkStatus.InProgress] || 0 },
    { name: 'Testing', value: data.requestsByStatus[WorkStatus.Testing] || 0 },
    { name: 'Deployed', value: data.requestsByStatus[WorkStatus.Deployed] || 0 },
    { name: 'Completed', value: data.requestsByStatus[WorkStatus.Completed] || 0 },
    { name: 'Closed', value: data.requestsByStatus[WorkStatus.Closed] || 0 },
  ];

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Executive Dashboard
      </Typography>
      
      {/* Key Performance Indicators */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={4}>
          <StatCard
            title="Total Active Requests"
            value={data.totalActiveRequests}
            icon={<Assignment />}
            color="#1976d2"
            subtitle="All open work items"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <StatCard
            title="Avg. Completion Time"
            value={`${Math.round(data.averageCompletionTime)} days`}
            icon={<Schedule />}
            color="#2e7d32"
            subtitle="Time from intake to closure"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <StatCard
            title="SLA Compliance"
            value={`${Math.round(data.slaComplianceRate * 100)}%`}
            icon={<CheckCircle />}
            color="#ed6c02"
            subtitle="On-time delivery rate"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <StatCard
            title="Resource Utilization"
            value={`${Math.round(data.resourceUtilization * 100)}%`}
            icon={<Speed />}
            color="#9c27b0"
            subtitle="Team capacity usage"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <StatCard
            title="Critical Items"
            value={data.totalByPriority[PriorityLevel.Critical]}
            icon={<Warning />}
            color="#d32f2f"
            subtitle="Requires immediate attention"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <StatCard
            title="In Progress"
            value={data.totalByStatus[WorkStatus.InProgress]}
            icon={<TrendingUp />}
            color="#1976d2"
            subtitle="Currently being worked"
          />
        </Grid>
      </Grid>

      {/* Charts Section */}
      <Grid container spacing={3}>
        {/* Work Category Distribution */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Work Category Distribution
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={categoryData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="value"
                  >
                    {categoryData.map((_, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        {/* Priority Distribution */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Priority Distribution
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={priorityData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="value" fill="#8884d8">
                    {priorityData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        {/* Status Overview */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Work Request Status Overview
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={statusData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Bar dataKey="value" fill="#1976d2" />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        {/* Recent Activity */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Recent Activity
              </Typography>
              <Box>
                <Box display="flex" alignItems="center" mb={2}>
                  <Chip 
                    label="High Priority" 
                    color="warning" 
                    size="small" 
                    sx={{ mr: 1 }} 
                  />
                  <Typography variant="body2">
                    New work request "Implement SSO Integration" submitted by John Doe
                  </Typography>
                </Box>
                <Box display="flex" alignItems="center" mb={2}>
                  <Chip 
                    label="Critical" 
                    color="error" 
                    size="small" 
                    sx={{ mr: 1 }} 
                  />
                  <Typography variant="body2">
                    Priority voting completed for "Database Performance Issues"
                  </Typography>
                </Box>
                <Box display="flex" alignItems="center" mb={2}>
                  <Chip 
                    label="Completed" 
                    color="success" 
                    size="small" 
                    sx={{ mr: 1 }} 
                  />
                  <Typography variant="body2">
                    "User Management Enhancement" moved to deployment stage
                  </Typography>
                </Box>
                <Box display="flex" alignItems="center" mb={2}>
                  <Chip 
                    label="Medium Priority" 
                    color="info" 
                    size="small" 
                    sx={{ mr: 1 }} 
                  />
                  <Typography variant="body2">
                    "Mobile App Updates" assigned to development team
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Dashboard;