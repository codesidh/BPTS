import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { apiService } from '../services/api';
import { User } from '../types';

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (userData: any) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const initializeAuth = async () => {
      try {
        if (apiService.isAuthenticated()) {
          try {
            const currentUser = await apiService.getCurrentUser();
            setUser(currentUser);
          } catch (error) {
            console.error('Failed to get current user, clearing auth:', error);
            apiService.logout();
          }
        }
      } catch (error) {
        console.error('Failed to initialize auth:', error);
        apiService.logout();
      } finally {
        setLoading(false);
      }
    };

    initializeAuth();
  }, []);

  const login = async (email: string, password: string) => {
    try {
      console.log('AuthProvider: Starting login...');
      console.log('AuthProvider: Email:', email);
      
      const response = await apiService.login({ email, password });
      console.log('AuthProvider: Login response received:', response);
      
      // Validate response
      if (!response) {
        throw new Error('No response from API service');
      }
      
      if (!response.user) {
        throw new Error('No user data in response');
      }
      
      console.log('AuthProvider: Setting user to state:', response.user);
      setUser(response.user);
      console.log('AuthProvider: User set to state successfully');
      
      // Force a small delay to ensure state is updated
      await new Promise(resolve => setTimeout(resolve, 100));
      console.log('AuthProvider: State update delay completed');
    } catch (error) {
      console.error('AuthProvider: Login error:', error);
      console.error('AuthProvider: Error details:', {
        message: error.message,
        stack: error.stack
      });
      throw error;
    }
  };

  const register = async (userData: any) => {
    try {
      const response = await apiService.register(userData);
      setUser(response.user);
    } catch (error) {
      throw error;
    }
  };

  const logout = () => {
    apiService.logout();
    setUser(null);
  };

  const value: AuthContextType = {
    user,
    loading,
    login,
    register,
    logout,
    isAuthenticated: !!user,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}; 