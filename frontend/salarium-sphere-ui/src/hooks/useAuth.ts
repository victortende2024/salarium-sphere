import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/authService';
import type { LoginCredentials } from '../services/authService';

export function useAuth() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  

  const loginMutation = useMutation({
    mutationFn: (credentials: LoginCredentials) => {
      console.log('loginMutation called with:', credentials);
      return authService.login(credentials);
    },
    onSuccess: (data) => {
      console.log('Login success:', data);
      localStorage.setItem('employeeId', data.employeeId || '');

      localStorage.setItem('accessToken', data.token);
      localStorage.setItem('userRole', data.role); // <-- Store role
      localStorage.setItem('userEmail', data.email);
      localStorage.setItem('userId', data.userId);

    

      queryClient.invalidateQueries({ queryKey: ['user'] });
      navigate('/dashboard');
    },
    onError: (error) => {
      console.error('Login failed:', error);
    },
  });

  const logout = () => {
    console.log('Logging out...');
    localStorage.removeItem('accessToken');
    localStorage.removeItem('userRole');
    localStorage.removeItem('userEmail');
    localStorage.removeItem('userId');
    queryClient.clear();
    navigate('/');
  };

  const isAuthenticated = !!localStorage.getItem('accessToken');
  const userRole = localStorage.getItem('userRole') || 'Employee';
  const employeeId = localStorage.getItem('employeeId') || undefined;

  return {
    login: loginMutation.mutate,
    isLoggingIn: loginMutation.isPending,
    loginError: loginMutation.error,
    isAuthenticated,
    userRole,
    employeeId,
    logout,
  };
}