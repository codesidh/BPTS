import React from 'react';
import { Box, Typography, Card, CardContent } from '@mui/material';

const WorkRequestDetail: React.FC = () => {
  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Work Request Detail
      </Typography>
      <Card>
        <CardContent>
          <Typography variant="body1">
            Detailed work request view will be implemented here.
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
};

export default WorkRequestDetail;