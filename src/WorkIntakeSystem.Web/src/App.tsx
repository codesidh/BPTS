import React from 'react';
import { Routes, Route } from 'react-router-dom';
import { Box } from '@mui/material';
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
import AuthProvider from './components/AuthProvider';

function App() {
  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <Layout>
        <AuthProvider>
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/work-requests" element={<WorkRequests />} />
            <Route path="/work-requests/:id" element={<WorkRequestDetail />} />
            <Route path="/work-requests/create" element={<CreateWorkRequest />} />
            <Route path="/priority-voting" element={<PriorityVoting />} />
            <Route path="/departments" element={<Departments />} />
            <Route path="/business-verticals" element={<BusinessVerticals />} />
            <Route path="/users" element={<Users />} />
            <Route path="/reports" element={<Reports />} />
            <Route path="/external-integrations" element={<ExternalIntegrations />} />
            <Route path="/configuration" element={<ConfigurationManagement />} />
            <Route path="/audit-trail" element={<EventAuditTrail />} />
          </Routes>
        </AuthProvider>
      </Layout>
    </Box>
  );
}

export default App;