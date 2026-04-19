import { App, ConfigProvider } from 'antd'
import ptBR from 'antd/locale/pt_BR'
import { QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider } from 'react-router-dom'
import { queryClient } from './queryClient'
import { router } from './router'
import { theme } from './theme'

export function Providers() {
  return (
    <ConfigProvider locale={ptBR} theme={theme}>
      <App>
        <QueryClientProvider client={queryClient}>
          <RouterProvider router={router} />
        </QueryClientProvider>
      </App>
    </ConfigProvider>
  )
}
