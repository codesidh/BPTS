import React from 'react';
import { Box, Typography, Card, CardContent } from '@mui/material';

const WorkRequests: React.FC = () => {
  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Work Requests
      </Typography>
      <Card>
        <CardContent>
          <Typography variant="body1">
            Work Requests management interface will be implemented here.
            This will include filtering, sorting, and CRUD operations for work requests.
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
};

export default WorkRequests;