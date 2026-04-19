import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/features/auth/authStore'
import type { Role } from '@/shared/constants/roles'

interface Props {
  allowedRoles?: Role[]
}

export function ProtectedRoute({ allowedRoles }: Props) {
  const { isAuthenticated, user } = useAuthStore()
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />
  }

  if (allowedRoles && user && !allowedRoles.includes(user.role as Role)) {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}
