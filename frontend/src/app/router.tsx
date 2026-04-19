import { createBrowserRouter } from 'react-router-dom'
import { AppLayout } from './AppLayout'

export const router = createBrowserRouter([
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
])
