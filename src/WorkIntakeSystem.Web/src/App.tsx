import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import WorkRequests from './pages/WorkRequests';
import WorkRequestDetail from './pages/WorkRequestDetail';
import CreateWorkRequest from './pages/CreateWorkRequest';
import PriorityVoting from './pages/PriorityVoting';
import Departments from './pages/Departments';
import BusinessVerticals from './pages/BusinessVerticals';
import Users from './pages/Users';
import Reports from './pages/Reports';
import ExternalIntegrations from './pages/ExternalIntegrations';
import ConfigurationManagement from './pages/ConfigurationManagement';
import EventAuditTrail from './pages/EventAuditTrail';
import WorkflowDesigner from './pages/WorkflowDesigner';
import PriorityConfiguration from './pages/PriorityConfiguration';
import Login from './pages/Login';
import Register from './pages/Register';
import { AuthProvider, useAuth } from './components/AuthProvider';

// Protected Route Component
const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
};

// Public Route Component (redirects to dashboard if authenticated)
const PublicRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, loading } = useAuth();

  console.log('PublicRoute: isAuthenticated =', isAuthenticated, 'loading =', loading);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }

  if (isAuthenticated) {
    console.log('PublicRoute: User is authenticated, redirecting to dashboard');
    return <Navigate to="/dashboard" replace />;
  }

  console.log('PublicRoute: User not authenticated, showing login form');
  return <>{children}</>;
};

const AppRoutes: React.FC = () => {
  return (
    <Routes>
      {/* Public Routes */}
      <Route path="/login" element={
        <PublicRoute>
          <Login />
        </PublicRoute>
      } />
      <Route path="/register" element={
        <PublicRoute>
          <Register />
        </PublicRoute>
      } />

      {/* Protected Routes */}
      <Route path="/" element={
        <ProtectedRoute>
          <Layout>
            <Dashboard />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/dashboard" element={
        <ProtectedRoute>
          <Layout>
            <Dashboard />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/work-requests" element={
        <ProtectedRoute>
          <Layout>
            <WorkRequests />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/work-requests/:id" element={
        <ProtectedRoute>
          <Layout>
            <WorkRequestDetail />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/work-requests/create" element={
        <ProtectedRoute>
          <Layout>
            <CreateWorkRequest />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/priority-voting" element={
        <ProtectedRoute>
          <Layout>
            <PriorityVoting />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/departments" element={
        <ProtectedRoute>
          <Layout>
            <Departments />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/business-verticals" element={
        <ProtectedRoute>
          <Layout>
            <BusinessVerticals />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/users" element={
        <ProtectedRoute>
          <Layout>
            <Users />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/reports" element={
        <ProtectedRoute>
          <Layout>
            <Reports />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/external-integrations" element={
        <ProtectedRoute>
          <Layout>
            <ExternalIntegrations />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/configuration" element={
        <ProtectedRoute>
          <Layout>
            <ConfigurationManagement />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/audit-trail" element={
        <ProtectedRoute>
          <Layout>
            <EventAuditTrail />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/workflow-designer" element={
        <ProtectedRoute>
          <Layout>
            <WorkflowDesigner />
          </Layout>
        </ProtectedRoute>
      } />
      <Route path="/priority-configuration" element={
        <ProtectedRoute>
          <Layout>
            <PriorityConfiguration />
          </Layout>
        </ProtectedRoute>
      } />

      {/* Catch all route */}
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
};

function App() {
  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </Box>
  );
}

export default App;