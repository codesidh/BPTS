import React from 'react';
import { Box, Typography, Card, CardContent } from '@mui/material';

const CreateWorkRequest: React.FC = () => {
  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Create Work Request
      </Typography>
      <Card>
        <CardContent>
          <Typography variant="body1">
            Work request creation form will be implemented here.
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
};

export default CreateWorkRequest;