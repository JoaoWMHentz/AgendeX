import { useMutation, useQuery } from '@tanstack/react-query'
import { reportsService } from '@/services/reports.service'
import { queryKeys } from '@/shared/queryKeys'
import type { ReportFilters } from './types'

type UseReportsOptions = {
  enabled?: boolean
}

export function useReports(filters: ReportFilters, options?: UseReportsOptions) {
  return useQuery({
    queryKey: queryKeys.reports.all(filters),
    queryFn: () => reportsService.get(filters),
    enabled: options?.enabled ?? true,
    placeholderData: (previousData) => previousData,
  })
}

export function useExportReportsCsv() {
  return useMutation({
    mutationFn: reportsService.exportCsv,
  })
}

export function useExportReportsXlsx() {
  return useMutation({
    mutationFn: reportsService.exportXlsx,
  })
}
