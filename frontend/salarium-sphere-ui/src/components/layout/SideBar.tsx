import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import {
  LayoutDashboard,
  Users,
  Briefcase,
  FileText,
  Building2,
  Settings,
  LogOut,
  ChevronLeft,
  ChevronRight,
  Scale,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { useState } from 'react';


interface NavItem {
  label: string;
  icon: React.ReactNode;
  path: string;
  roles: string[];
}

export function Sidebar() {
  const navigate = useNavigate();
  const location = useLocation();
  const { userRole, logout } = useAuth();
  
  const [collapsed, setCollapsed] = useState(false);

  const navItems: NavItem[] = [
    {
      label: 'Dashboard',
      icon: <LayoutDashboard className="h-5 w-5" />,
      path: '/dashboard',
      roles: ['Admin', 'PayrollManager', 'Employee'],
    },
    {
      label: 'Employees',
      icon: <Users className="h-5 w-5" />,
      path: '/employees',
      roles: ['Admin', 'PayrollManager'],
    },
    {
      label: 'Profiles',
      icon: <Briefcase className="h-5 w-5" />,
      path: '/profiles',
      roles: ['Admin', 'PayrollManager'],
    },
   {
      label: 'Payslips',
      icon: <FileText className="h-5 w-5" />,
      path: '/payslips',
      roles: ['Admin', 'PayrollManager', 'Employee'],
    },
      {
  label: 'Tax Rules',
  icon: <Scale className="h-5 w-5" />,
  path: '/taxrules',
  roles: ['Admin'],
},
    {
      label: 'Companies',
      icon: <Building2 className="h-5 w-5" />,
      path: '/companies',
      roles: ['Admin'],
    },
    {
      label: 'Users',
      icon: <Users className="h-5 w-5" />,  // or any icon like UserCog
      path: '/users',
      roles: ['Admin'],
    },
    {
      label: 'Settings',
      icon: <Settings className="h-5 w-5" />,
      path: '/settings',
      roles: ['Admin'],
    },
  
  ];

  const filteredItems = navItems.filter((item) =>
    item.roles.includes(userRole)
  );

  const toggleCollapse = () => setCollapsed(!collapsed);

  return (
    <div
      className={cn(
        'flex h-screen flex-col transition-all duration-300',
        // Royal blue to sky blue gradient background
        'bg-gradient-to-b from-[#1a3a8a] via-[#2a5cb8] to-[#4a90d9]',
        // Dark mode version (optional)
        'dark:from-[#0d1f4a] dark:via-[#1a3a8a] dark:to-[#2a5cb8]',
        collapsed ? 'w-16' : 'w-64'
      )}
    >
      {/* Logo / Brand */}
      <div className="flex h-16 items-center justify-between border-b border-white/10 px-4">
        {!collapsed ? (
    <div className="flex items-center gap-2">
      <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-white/20">
        <span className="text-lg font-bold text-white">S</span>
      </div>
      <span className="text-xl font-bold text-white">Salarium</span>
    </div>
  ) : (
    <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-white/20">
      <span className="text-lg font-bold text-white">S</span>
    </div>
  )}
        <Button
          variant="ghost"
          size="sm"
          onClick={toggleCollapse}
          className="text-white/70 hover:bg-white/10 hover:text-white"
        >
          {collapsed ? (
            <ChevronRight className="h-4 w-4" />
          ) : (
            <ChevronLeft className="h-4 w-4" />
          )}
        </Button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto py-4">
        <ul className="space-y-1 px-2">
          {filteredItems.map((item) => {
            const isActive = location.pathname === item.path ||
              location.pathname.startsWith(item.path + '/');
            return (
              <li key={item.path}>
                <Button
                  variant={isActive ? 'secondary' : 'ghost'}
                  className={cn(
                    'w-full justify-start gap-3 text-white/80 hover:bg-white/15 hover:text-white',
                    collapsed && 'justify-center px-2',
                    isActive  && 'bg-white/20 text-white shadow-lg shadow-blue-500/30'
                  )}
                  onClick={() => navigate(item.path)}
                >
                  {item.icon}
                  {!collapsed && <span>{item.label}</span>}
                </Button>
              </li>
            );
          })}
        </ul>
      </nav>
      {/* Decorative element - optional */}
{!collapsed && (
  <div className="flex justify-center px-4">
    <div className="h-px w-16 bg-white/20" />
  </div>
)}

      {/* User & Logout */}
      <div className="border-t border-white/10 p-4">

        <Button
          variant="ghost"
          className={cn(
            'w-full justify-start gap-3 text-white/80 hover:bg-white/15 hover:text-white',
            collapsed && 'justify-center px-2'
          )}
          onClick={logout}
        >
          <LogOut className="h-5 w-5" />
          {!collapsed && <span>Logout</span>}
        </Button>
        {!collapsed && (
          <p className="mt-2 text-center text-xs text-white/60">
            Role: {userRole}
          </p>
          
        )}
       
      </div>
    </div>
  );
}