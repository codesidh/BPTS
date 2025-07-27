import React from 'react';
import { Box, Typography, Card, CardContent } from '@mui/material';

const Users: React.FC = () => {
  return (
    <Box>
      <Typography variant="h4" gutterBottom>Users</Typography>
      <Card><CardContent><Typography variant="body1">User management interface will be implemented here.</Typography></CardContent></Card>
    </Box>
  );
};

export default Users;