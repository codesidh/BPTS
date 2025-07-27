import React from 'react';
import { useIsAuthenticated, useMsal } from '@azure/msal-react';

const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const isAuthenticated = useIsAuthenticated();
  const { instance } = useMsal();

  React.useEffect(() => {
    if (!isAuthenticated) {
      instance.loginRedirect();
    }
  }, [isAuthenticated, instance]);

  if (!isAuthenticated) {
    return <div>Redirecting to login...</div>;
  }
  return <>{children}</>;
};

export default AuthProvider; 