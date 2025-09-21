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
import { PriorityLevel, WorkCategory, WorkStatus } from '../types';

// Mock data for demonstration - replace with actual API calls
const mockDashboardData = {
  totalActiveRequests: 156,
  totalByCategory: {
    [WorkCategory.WorkRequest]: 89,
    [WorkCategory.Project]: 45,
    [WorkCategory.BreakFix]: 22,
  },
  totalByPriority: {
    [PriorityLevel.Critical]: 12,
    [PriorityLevel.High]: 34,
    [PriorityLevel.Medium]: 67,
    [PriorityLevel.Low]: 43,
  },
  totalByStatus: {
    [WorkStatus.Draft]: 15,
    [WorkStatus.Submitted]: 28,
    [WorkStatus.UnderReview]: 22,
    [WorkStatus.Approved]: 18,
    [WorkStatus.InProgress]: 35,
    [WorkStatus.Testing]: 12,
    [WorkStatus.Deployed]: 8,
    [WorkStatus.Closed]: 145,
  },
  averageCompletionTime: 24.5,
  slaCompliance: 87.3,
  resourceUtilization: 78.2,
};

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
  // In a real application, these would be actual API calls
  const { data: dashboardData, isLoading, error } = useQuery(
    'dashboardStats',
    async () => {
      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 1000));
      return mockDashboardData;
    },
    {
      refetchInterval: 30000, // Refresh every 30 seconds
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
    { name: 'Work Requests', value: data.totalByCategory[WorkCategory.WorkRequest] },
    { name: 'Projects', value: data.totalByCategory[WorkCategory.Project] },
    { name: 'Break Fix', value: data.totalByCategory[WorkCategory.BreakFix] },
  ];

  const priorityData = [
    { name: 'Critical', value: data.totalByPriority[PriorityLevel.Critical], color: '#FF4444' },
    { name: 'High', value: data.totalByPriority[PriorityLevel.High], color: '#FF8800' },
    { name: 'Medium', value: data.totalByPriority[PriorityLevel.Medium], color: '#FFBB00' },
    { name: 'Low', value: data.totalByPriority[PriorityLevel.Low], color: '#00AA00' },
  ];

  const statusData = [
    { name: 'Draft', value: data.totalByStatus[WorkStatus.Draft] },
    { name: 'Submitted', value: data.totalByStatus[WorkStatus.Submitted] },
    { name: 'Under Review', value: data.totalByStatus[WorkStatus.UnderReview] },
    { name: 'Approved', value: data.totalByStatus[WorkStatus.Approved] },
    { name: 'In Progress', value: data.totalByStatus[WorkStatus.InProgress] },
    { name: 'Testing', value: data.totalByStatus[WorkStatus.Testing] },
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
            value={`${data.averageCompletionTime} days`}
            icon={<Schedule />}
            color="#2e7d32"
            subtitle="Time from intake to closure"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <StatCard
            title="SLA Compliance"
            value={`${data.slaCompliance}%`}
            icon={<CheckCircle />}
            color="#ed6c02"
            subtitle="On-time delivery rate"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <StatCard
            title="Resource Utilization"
            value={`${data.resourceUtilization}%`}
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