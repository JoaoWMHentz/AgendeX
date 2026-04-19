import type { AxiosResponse } from 'axios'
import { api } from './api'
import type { ReportFileResponse, ReportFilters, ReportResult } from '@/features/reports/types'

function buildQueryString(filters: ReportFilters): string {
  const params = new URLSearchParams()

  for (const clientId of filters.clientIds ?? []) {
    params.append('ClientIds', clientId)
  }

  for (const agentId of filters.agentIds ?? []) {
    params.append('AgentIds', agentId)
  }

  for (const serviceTypeId of filters.serviceTypeIds ?? []) {
    params.append('ServiceTypeIds', String(serviceTypeId))
  }

  for (const status of filters.statuses ?? []) {
    params.append('Statuses', String(status))
  }

  if (filters.from) params.append('From', filters.from)
  if (filters.to) params.append('To', filters.to)

  params.append('ReportType', String(filters.reportType))

  if (filters.sortBy) params.append('SortBy', filters.sortBy)
  if (filters.sortDirection) params.append('SortDirection', filters.sortDirection)

  return params.toString()
}

function extractFileName(response: AxiosResponse<Blob>, fallback: string): string {
  const contentDisposition = response.headers['content-disposition']
  if (!contentDisposition) return fallback

  const utfMatch = contentDisposition.match(/filename\\*=UTF-8''([^;]+)/i)
  if (utfMatch?.[1]) return decodeURIComponent(utfMatch[1].replace(/"/g, ''))

  const plainMatch = contentDisposition.match(/filename="?([^";]+)"?/i)
  if (plainMatch?.[1]) return plainMatch[1]

  return fallback
}

async function exportReportFile(
  endpoint: string,
  filters: ReportFilters,
  fallbackFileName: string,
): Promise<ReportFileResponse> {
  const queryString = buildQueryString(filters)
  const response = await api.get<Blob>(`${endpoint}?${queryString}`, { responseType: 'blob' })

  return {
    blob: response.data,
    fileName: extractFileName(response, fallbackFileName),
  }
}

export const reportsService = {
  get: (filters: ReportFilters) => {
    const queryString = buildQueryString(filters)
    return api.get<ReportResult>(`/api/reports?${queryString}`).then((response) => response.data)
  },

  exportCsv: (filters: ReportFilters) => exportReportFile('/api/reports/export/csv', filters, 'reports.csv'),

  exportXlsx: (filters: ReportFilters) => exportReportFile('/api/reports/export/xlsx', filters, 'reports.xlsx'),
}
