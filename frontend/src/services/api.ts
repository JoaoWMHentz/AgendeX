import axios from 'axios'
import { tokenStorage } from './tokenStorage'
import type { AuthResponse } from '@/features/auth/types'

const runtimeApiBaseUrl = window.__APP_CONFIG__?.API_BASE_URL
const BASE_URL = runtimeApiBaseUrl ?? 'http://localhost:5150'

export const api = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

// Attach access token to every request
api.interceptors.request.use((config) => {
  const token = tokenStorage.getAccess()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// --- Refresh token rotation on 401 ---
let isRefreshing = false
let queue: Array<{ resolve: (token: string) => void; reject: (err: unknown) => void }> = []

const processQueue = (error: unknown, token: string | null = null) => {
  queue.forEach((p) => (error ? p.reject(error) : p.resolve(token!)))
  queue = []
}

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config as typeof error.config & { _retry?: boolean }

    if (error.response?.status !== 401 || original._retry) {
      return Promise.reject(error)
    }

    const refreshToken = tokenStorage.getRefresh()
    if (!refreshToken) {
      window.dispatchEvent(new CustomEvent('auth:unauthorized'))
      return Promise.reject(error)
    }

    if (isRefreshing) {
      return new Promise<string>((resolve, reject) => {
        queue.push({ resolve, reject })
      }).then((token) => {
        original.headers.Authorization = `Bearer ${token}`
        return api(original)
      })
    }

    original._retry = true
    isRefreshing = true

    try {
      const { data } = await axios.post<AuthResponse>(`${BASE_URL}/api/auth/refresh`, { refreshToken })
      tokenStorage.setAccess(data.accessToken)
      tokenStorage.setRefresh(data.refreshToken)
      processQueue(null, data.accessToken)
      original.headers.Authorization = `Bearer ${data.accessToken}`
      return api(original)
    } catch (err) {
      processQueue(err)
      window.dispatchEvent(new CustomEvent('auth:unauthorized'))
      return Promise.reject(err)
    } finally {
      isRefreshing = false
    }
  },
)
