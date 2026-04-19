import { createBrowserRouter } from 'react-router-dom'
import { AppLayout } from './AppLayout'
import { ProtectedRoute } from './ProtectedRoute'
import { LoginPage } from '@/features/auth/LoginPage'

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
          {
            index: true,
            element: <p>Selecione um módulo no menu.</p>,
          },
          {
            path: 'appointments',
            element: <p>Agendamentos — em breve</p>,
          },
          {
            path: 'availability',
            element: <p>Disponibilidade — em breve</p>,
          },
          {
            path: 'users',
            element: <p>Usuários — em breve</p>,
          },
          {
            path: 'service-types',
            element: <p>Tipos de Serviço — em breve</p>,
          },
        ],
      },
    ],
  },
])
