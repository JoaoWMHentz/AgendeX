import { useEffect } from 'react'
import type { ThemeConfig } from 'antd'
import { darkTheme } from './themes/dark/theme'
import { lightTheme } from './themes/light/theme'
import { create } from 'zustand'

export type ThemePreference = 'system' | 'light' | 'dark'
export type ResolvedTheme = 'light' | 'dark'

const THEME_STORAGE_KEY = 'agendex-theme-preference'

interface ThemeState {
  preference: ThemePreference
  systemTheme: ResolvedTheme
  setPreference: (preference: ThemePreference) => void
  setSystemTheme: (theme: ResolvedTheme) => void
  toggleTheme: () => void
}

function readStoredPreference(): ThemePreference {
  if (typeof window === 'undefined') {
    return 'system'
  }

  const storedPreference = window.localStorage.getItem(THEME_STORAGE_KEY)
  if (storedPreference === 'light' || storedPreference === 'dark' || storedPreference === 'system') {
    return storedPreference
  }

  return 'system'
}

function getBrowserTheme(): ResolvedTheme {
  if (typeof window === 'undefined') {
    return 'light'
  }

  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

export function resolveTheme(preference: ThemePreference, systemTheme: ResolvedTheme): ResolvedTheme {
  return preference === 'system' ? systemTheme : preference
}

export function getThemeConfig(resolvedTheme: ResolvedTheme): ThemeConfig {
  return resolvedTheme === 'dark' ? darkTheme : lightTheme
}

export const useThemeStore = create<ThemeState>((set, get) => ({
  preference: readStoredPreference(),
  systemTheme: getBrowserTheme(),
  setPreference: (preference) => {
    if (typeof window !== 'undefined') {
      window.localStorage.setItem(THEME_STORAGE_KEY, preference)
    }

    set({ preference })
  },
  setSystemTheme: (systemTheme) => {
    set({ systemTheme })
  },
  toggleTheme: () => {
    const currentTheme = resolveTheme(get().preference, get().systemTheme)
    const nextTheme: ResolvedTheme = currentTheme === 'dark' ? 'light' : 'dark'
    get().setPreference(nextTheme)
  },
}))

export function useThemeSync() {
  useEffect(() => {
    if (typeof window === 'undefined') {
      return
    }

    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
    const handleChange = (event: MediaQueryListEvent) => {
      useThemeStore.getState().setSystemTheme(event.matches ? 'dark' : 'light')
    }

    useThemeStore.getState().setSystemTheme(mediaQuery.matches ? 'dark' : 'light')

    if (typeof mediaQuery.addEventListener === 'function') {
      mediaQuery.addEventListener('change', handleChange)
      return () => mediaQuery.removeEventListener('change', handleChange)
    }

    mediaQuery.addListener(handleChange)
    return () => mediaQuery.removeListener(handleChange)
  }, [])
}