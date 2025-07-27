import React from 'react';
import { useMsal } from '@azure/msal-react';
import { createApiService } from '../services/api';
import { Box, Typography, Button } from '@mui/material';

const Users: React.FC = () => {
  const { instance } = useMsal();
  const api = React.useMemo(() => createApiService(instance as any), [instance]);
  const [me, setMe] = React.useState<any>(null);
  const [aadUsers, setAadUsers] = React.useState<any[]>([]);
  const [loading, setLoading] = React.useState(false);

  React.useEffect(() => {
    api.getApi().get('/users/me').then(res => setMe(res.data));
  }, [api]);

  const syncAzureAd = async () => {
    setLoading(true);
    try {
      const res = await api.getApi().get('/users/sync-azure-ad');
      setAadUsers(res.data);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box>
      <Typography variant="h4">Current User</Typography>
      <pre>{JSON.stringify(me, null, 2)}</pre>
      <Button variant="contained" onClick={syncAzureAd} disabled={loading} sx={{ mt: 2 }}>
        Sync Azure AD Users (Admin Only)
      </Button>
      {aadUsers.length > 0 && (
        <Box mt={2}>
          <Typography variant="h5">Azure AD Users</Typography>
          <pre>{JSON.stringify(aadUsers, null, 2)}</pre>
        </Box>
      )}
    </Box>
  );
};

export default Users;