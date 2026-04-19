import { createBrowserRouter } from 'react-router-dom'
import { AppLayout } from './AppLayout'
import { ProtectedRoute } from './ProtectedRoute'
import { LoginPage } from '@/features/auth/LoginPage'
import { AppointmentsPage } from '@/features/appointments/AppointmentsPage'
import { AvailabilityPage } from '@/features/availability/AvailabilityPage'
import { UsersPage } from '@/features/users/UsersPage'
import { ProfilePage } from '@/features/users/ProfilePage'
import { ServiceTypesPage } from '@/features/service-types/ServiceTypesPage'
import { Roles } from '@/shared/constants/roles'

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    // Must be authenticated
    element: <ProtectedRoute />,
    children: [
      {
        path: '/',
        element: <AppLayout />,
        children: [
          { index: true, element: <AppointmentsPage /> },
          { path: 'appointments', element: <AppointmentsPage /> },
          { path: 'availability', element: <AvailabilityPage /> },
          { path: 'profile', element: <ProfilePage /> },

          // Administrator only
          {
            element: <ProtectedRoute allowedRoles={[Roles.Administrator]} />,
            children: [
              { path: 'users', element: <UsersPage /> },
              { path: 'service-types', element: <ServiceTypesPage /> },
            ],
          },
        ],
      },
    ],
  },
])
