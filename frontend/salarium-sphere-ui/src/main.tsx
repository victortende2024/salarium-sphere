import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from '@/components/ui/sonner';
import { DashboardLayout } from './components/layout/DashboardLayout';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { EmployeesPage } from './pages/EmployeesPage';
import { EmployeeForm } from './pages/EmployeeForm';
import { EmployeeDetail } from './pages/EmployeeDetail';
import { ProfileForm } from './pages/ProfileForm';
import { ProfileListPage } from './pages/ProfileListPage';
import { PayrollRunDetail } from './pages/PayrollRunDetail';
import { PayrollHistory } from './pages/PayrollHistory';
import { PayslipViewer } from './pages/PayslipViewer';
import { TaxRulesList } from './pages/TaxRulesList';
import { CompanyList } from './pages/CompanyList';
import { CompanyForm } from './pages/CompanyForm';
import { UserList } from './pages/UserList';
import { UserForm } from './pages/UserForm';
import './index.css';

const queryClient = new QueryClient();

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const token = localStorage.getItem('accessToken');
  if (!token) {
    return <Navigate to="/auth/login" replace />;
  }
  return <>{children}</>;
}

function HomePage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50">
      <div className="text-center">
        <h1 className="text-4xl font-bold text-indigo-600">Salarium Sphere</h1>
        <p className="mt-4 text-gray-600">Welcome to your world-class payroll system</p>
        <div className="mt-8 space-x-4">
          <a
            href="/auth/login"
            className="inline-block px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Sign In
          </a>
          <a
            href="/dashboard"
            className="inline-block px-6 py-3 bg-gray-200 text-gray-800 rounded-lg hover:bg-gray-300 transition-colors"
          >
            Dashboard
          </a>
        </div>
      </div>
    </div>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          {/* Public routes */}
          <Route path="/" element={<HomePage />} />
          <Route path="/auth/login" element={<LoginPage />} />

          {/* Protected routes with sidebar */}
          <Route
            element={
              <ProtectedRoute>
                <DashboardLayout />
              </ProtectedRoute>
            }
          >
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/employees" element={<EmployeesPage />} />
            <Route path="/employees/new" element={<EmployeeForm />} />
            <Route path="/employees/:id" element={<EmployeeDetail />} />
            <Route path="/employees/:id/edit" element={<EmployeeForm />} />
            <Route path="/profiles" element={<ProfileListPage />} />
            <Route path="/profiles/new" element={<ProfileForm />} />
            <Route path="/profiles/:id" element={<ProfileForm />} />
            <Route path="/profiles/:id/edit" element={<ProfileForm />} />
            <Route path="/payroll/runs/:id" element={<ProtectedRoute><PayrollRunDetail /></ProtectedRoute>} />
            <Route path="/payroll/history" element={<ProtectedRoute><PayrollHistory /></ProtectedRoute>} />
            <Route path="/payslips" element={<ProtectedRoute><PayslipViewer /></ProtectedRoute>} />
            <Route path="/taxrules" element={<ProtectedRoute><TaxRulesList /></ProtectedRoute>} />
            <Route path="/companies" element={<ProtectedRoute><CompanyList /></ProtectedRoute>} />
            <Route path="/companies/new" element={<ProtectedRoute><CompanyForm /></ProtectedRoute>} />
            <Route path="/companies/:id" element={<ProtectedRoute><CompanyForm /></ProtectedRoute>} />
            <Route path="/companies/:id/edit" element={<ProtectedRoute><CompanyForm /></ProtectedRoute>} />
            <Route path="/users" element={<ProtectedRoute><UserList /></ProtectedRoute>} />
            <Route path="/users/new" element={<ProtectedRoute><UserForm /></ProtectedRoute>} />
            <Route path="/users/:id/edit" element={<ProtectedRoute><UserForm /></ProtectedRoute>} />
          </Route>
        </Routes>
        <Toaster richColors position="top-right" />
      </BrowserRouter>
    </QueryClientProvider>
  );
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);