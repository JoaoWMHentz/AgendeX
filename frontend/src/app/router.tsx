import { Navigate } from 'react-router-dom'
import { createBrowserRouter } from 'react-router-dom'
import { AppLayout } from './AppLayout'
import { ProtectedRoute } from './ProtectedRoute'
import { LoginPage } from '@/features/auth/LoginPage'
import { AppointmentsPage } from '@/features/appointments/pages/AppointmentsPage'
import { AgentMyAppointmentsPage } from '@/features/appointments/pages/AgentMyAppointmentsPage'
import { ClientNewAppointmentPage } from '@/features/appointments/pages/ClientNewAppointmentPage'
import { ClientMyAppointmentsPage } from '@/features/appointments/pages/ClientMyAppointmentsPage'
import { AvailabilityPage } from '@/features/availability/pages/AvailabilityPage'
import { UsersPage } from '@/features/users/pages/UsersPage'
import { ProfilePage } from '@/features/users/pages/ProfilePage'
import { ServiceTypesPage } from '@/features/service-types/ServiceTypesPage'
import { ReportsPage } from '@/features/reports/pages/ReportsPage'
import { Roles } from '@/shared/constants/roles'
import { useAuthStore } from '@/features/auth/authStore'

function RoleBasedRedirect() {
  const user = useAuthStore((s) => s.user)
  if (user?.role === Roles.Client) return <Navigate to="/client/my-appointments" replace />
  if (user?.role === Roles.Agent) return <Navigate to="/agent/my-appointments" replace />
  return <Navigate to="/appointments" replace />
}

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: '/',
        element: <AppLayout />,
        children: [
          { index: true, element: <RoleBasedRedirect /> },
          { path: 'profile', element: <ProfilePage /> },

          // Administrator only
          {
            element: <ProtectedRoute allowedRoles={[Roles.Administrator]} />,
            children: [
              { path: 'appointments', element: <AppointmentsPage /> },
              { path: 'users', element: <UsersPage /> },
              { path: 'service-types', element: <ServiceTypesPage /> },
            ],
          },

          // Administrator + Agent
          {
            element: <ProtectedRoute allowedRoles={[Roles.Administrator, Roles.Agent]} />,
            children: [
              { path: 'availability', element: <AvailabilityPage /> },
              { path: 'reports', element: <ReportsPage /> },
            ],
          },

          // Agent only
          {
            element: <ProtectedRoute allowedRoles={[Roles.Agent]} />,
            children: [
              { path: 'agent/my-appointments', element: <AgentMyAppointmentsPage /> },
            ],
          },

          // Client only
          {
            element: <ProtectedRoute allowedRoles={[Roles.Client]} />,
            children: [
              { path: 'client/new-appointment', element: <ClientNewAppointmentPage /> },
              { path: 'client/my-appointments', element: <ClientMyAppointmentsPage /> },
            ],
          },
        ],
      },
    ],
  },
])
