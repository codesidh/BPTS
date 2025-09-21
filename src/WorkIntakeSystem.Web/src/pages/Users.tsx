import React, { useState, useEffect } from 'react';
import { apiService } from '../services/api';
import { Box, Typography, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, CircularProgress, Alert } from '@mui/material';
import { User } from '../types';

const Users: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      try {
        const [usersData, currentUserData] = await Promise.all([
          apiService.getUsers(),
          apiService.getCurrentUser()
        ]);
        setUsers(usersData);
        setCurrentUser(currentUserData);
      } catch (err: any) {
        setError(err.response?.data?.message || 'Failed to load users');
      } finally {
        setLoading(false);
      }
    };
    loadData();
  }, []);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Box>
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Users Management
      </Typography>

      {currentUser && (
        <Box mb={3}>
          <Typography variant="h6" gutterBottom>
            Current User
          </Typography>
          <Paper sx={{ p: 2 }}>
            <Typography><strong>Name:</strong> {currentUser.name}</Typography>
            <Typography><strong>Email:</strong> {currentUser.email}</Typography>
            <Typography><strong>Department:</strong> {currentUser.departmentName}</Typography>
            <Typography><strong>Business Vertical:</strong> {currentUser.businessVerticalName}</Typography>
            <Typography><strong>Role:</strong> {currentUser.role}</Typography>
          </Paper>
        </Box>
      )}

      <Typography variant="h6" gutterBottom>
        All Users
      </Typography>
      
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Department</TableCell>
              <TableCell>Business Vertical</TableCell>
              <TableCell>Role</TableCell>
              <TableCell>Capacity</TableCell>
              <TableCell>Current Workload</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {users.map((user) => (
              <TableRow key={user.id}>
                <TableCell>{user.name}</TableCell>
                <TableCell>{user.email}</TableCell>
                <TableCell>{user.departmentName}</TableCell>
                <TableCell>{user.businessVerticalName}</TableCell>
                <TableCell>{user.role}</TableCell>
                <TableCell>{user.capacity} hours/week</TableCell>
                <TableCell>{user.currentWorkload}%</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};

export default Users;