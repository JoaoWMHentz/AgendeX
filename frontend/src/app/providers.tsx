import { App, ConfigProvider } from 'antd'
import { useEffect } from 'react'
import ptBR from 'antd/locale/pt_BR'
import { QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider } from 'react-router-dom'
import { queryClient } from './queryClient'
import { router } from './router'
import { getThemeConfig, resolveTheme, useThemeStore, useThemeSync } from '@/app/theme'

export function Providers() {
  useThemeSync()

  const resolvedTheme = useThemeStore((state) => resolveTheme(state.preference, state.systemTheme))

  useEffect(() => {
    document.documentElement.style.colorScheme = resolvedTheme
  }, [resolvedTheme])

  return (
    <ConfigProvider locale={ptBR} theme={getThemeConfig(resolvedTheme)}>
      <App>
        <QueryClientProvider client={queryClient}>
          <RouterProvider router={router} />
        </QueryClientProvider>
      </App>
    </ConfigProvider>
  )
}
