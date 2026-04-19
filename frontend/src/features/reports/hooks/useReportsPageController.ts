import { useMemo, useState } from 'react'
import { message } from 'antd'
import { useAuthStore } from '@/features/auth/authStore'
import { useServiceTypes } from '@/features/service-types/useServiceTypes'
import { useUsers } from '@/features/users/hooks/useUsers'
import { UserRole } from '@/features/users/models/types'
import { Roles } from '@/shared/constants/roles'
import { notifyApiError } from '@/shared/utils/notifyApiError'
import { getDefaultPeriodRange } from '@/shared/utils/date'
import { useExportReportsCsv, useExportReportsXlsx, useReports } from '../useReports'
import {
  ReportSortDirection,
  ReportType,
  reportTypeLabel,
  type ReportFilters,
  type ReportSortBy,
} from '../types'
import { appointmentStatusLabel } from '@/features/appointments/types'
import { orderToDirection } from '../components/ReportsTable'

function triggerDownload(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = fileName
  document.body.appendChild(anchor)
  anchor.click()
  anchor.remove()
  URL.revokeObjectURL(url)
}

export function useReportsPageController() {
  const user = useAuthStore((state) => state.user)
  const isAdmin = user?.role === Roles.Administrator

  const [draftFilters, setDraftFilters] = useState<ReportFilters>({
    ...getDefaultPeriodRange(),
    reportType: ReportType.TotalAppointmentsByAgent,
    sortBy: 'appointmentDate',
    sortDirection: ReportSortDirection.Desc,
  })
  const [appliedFilters, setAppliedFilters] = useState<ReportFilters | null>(null)

  const { data, isLoading } = useReports(appliedFilters ?? draftFilters, { enabled: !!appliedFilters })
  const { data: serviceTypes = [] } = useServiceTypes()
  const { data: clients = [] } = useUsers(UserRole.Client, { enabled: isAdmin })
  const { data: agents = [] } = useUsers(UserRole.Agent, { enabled: isAdmin })

  const exportCsvMutation = useExportReportsCsv()
  const exportXlsxMutation = useExportReportsXlsx()

  const updateFilters = (patch: Partial<ReportFilters>) => {
    setDraftFilters((current) => ({ ...current, ...patch }))
  }

  const handleSortChange = (sortBy?: ReportSortBy, order?: 'ascend' | 'descend' | null) => {
    updateFilters({
      sortBy,
      sortDirection: orderToDirection(order),
    })
  }

  const handleSearch = () => {
    const nextAppliedFilters: ReportFilters = { ...draftFilters }
    setAppliedFilters(nextAppliedFilters)
  }

  const handleExportCsv = async () => {
    if (!appliedFilters) {
      message.warning('Clique em Buscar para carregar os dados antes de exportar.')
      return
    }

    try {
      const file = await exportCsvMutation.mutateAsync(appliedFilters)
      triggerDownload(file.blob, file.fileName)
      message.success('Relatório CSV exportado com sucesso.')
    } catch (err) {
      notifyApiError(err)
    }
  }

  const handleExportXlsx = async () => {
    if (!appliedFilters) {
      message.warning('Clique em Buscar para carregar os dados antes de exportar.')
      return
    }

    try {
      const file = await exportXlsxMutation.mutateAsync(appliedFilters)
      triggerDownload(file.blob, file.fileName)
      message.success('Relatório XLSX exportado com sucesso.')
    } catch (err) {
      notifyApiError(err)
    }
  }

  const rows = data?.rows ?? []
  const tableFilters = appliedFilters ?? draftFilters

  const statusOptions = useMemo(
    () =>
      Object.entries(appointmentStatusLabel).map(([value, label]) => ({
        value: Number(value),
        label,
      })),
    [],
  )

  const reportTypeOptions = useMemo(
    () =>
      Object.entries(reportTypeLabel).map(([value, label]) => ({
        value: Number(value),
        label,
      })),
    [],
  )

  const serviceTypeOptions = useMemo(
    () => serviceTypes.map((serviceType) => ({ value: serviceType.id, label: serviceType.description })),
    [serviceTypes],
  )

  const clientOptions = useMemo(
    () => clients.map((client) => ({ value: client.id, label: client.name })),
    [clients],
  )

  const agentOptions = useMemo(
    () => agents.map((agent) => ({ value: agent.id, label: agent.name })),
    [agents],
  )

  return {
    filters: draftFilters,
    tableFilters,
    rows,
    isLoading,
    exportCsvLoading: exportCsvMutation.isPending,
    exportXlsxLoading: exportXlsxMutation.isPending,
    showClientFilter: isAdmin,
    showAgentFilter: isAdmin,
    statusOptions,
    reportTypeOptions,
    serviceTypeOptions,
    clientOptions,
    agentOptions,
    updateFilters,
    handleSortChange,
    handleSearch,
    handleExportCsv,
    handleExportXlsx,
  }
}
