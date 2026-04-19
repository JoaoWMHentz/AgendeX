import { Navigate } from 'react-router-dom'
import { createBrowserRouter } from 'react-router-dom'
import { AppLayout } from './AppLayout'
import { ProtectedRoute } from './ProtectedRoute'
import { LoginPage } from '@/features/auth/LoginPage'
import { AppointmentsPage } from '@/features/appointments/pages/AppointmentsPage'
import { ClientNewAppointmentPage } from '@/features/appointments/pages/ClientNewAppointmentPage'
import { ClientMyAppointmentsPage } from '@/features/appointments/pages/ClientMyAppointmentsPage'
import { AvailabilityPage } from '@/features/availability/pages/AvailabilityPage'
import { UsersPage } from '@/features/users/pages/UsersPage'
import { ProfilePage } from '@/features/users/pages/ProfilePage'
import { ServiceTypesPage } from '@/features/service-types/ServiceTypesPage'
import { Roles } from '@/shared/constants/roles'
import { useAuthStore } from '@/features/auth/authStore'

function RoleBasedRedirect() {
  const user = useAuthStore((s) => s.user)
  if (user?.role === Roles.Client) return <Navigate to="/client/my-appointments" replace />
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

          // Administrator + Agent
          {
            element: <ProtectedRoute allowedRoles={[Roles.Administrator, Roles.Agent]} />,
            children: [
              { path: 'appointments', element: <AppointmentsPage /> },
              { path: 'availability', element: <AvailabilityPage /> },
            ],
          },

          // Administrator only
          {
            element: <ProtectedRoute allowedRoles={[Roles.Administrator]} />,
            children: [
              { path: 'users', element: <UsersPage /> },
              { path: 'service-types', element: <ServiceTypesPage /> },
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
