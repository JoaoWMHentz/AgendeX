import dayjs from 'dayjs'
import { Button, DatePicker, Select, Space } from 'antd'
import { DownloadOutlined, SearchOutlined } from '@ant-design/icons'
import { reportTypeLabel, type ReportFilters, type ReportTypeValue } from '../types'
import type { AppointmentStatusValue } from '@/features/appointments/types'

const { RangePicker } = DatePicker

type Option = {
  value: string | number
  label: string
}

type ReportsFiltersProps = {
  filters: ReportFilters
  reportTypeOptions: Option[]
  statusOptions: Option[]
  serviceTypeOptions: Option[]
  clientOptions: Option[]
  agentOptions: Option[]
  showClientFilter: boolean
  showAgentFilter: boolean
  loading: boolean
  exportCsvLoading: boolean
  exportXlsxLoading: boolean
  onFiltersChange: (patch: Partial<ReportFilters>) => void
  onSearch: () => void
  onExportCsv: () => void
  onExportXlsx: () => void
}

export function ReportsFilters({
  filters,
  reportTypeOptions,
  statusOptions,
  serviceTypeOptions,
  clientOptions,
  agentOptions,
  showClientFilter,
  showAgentFilter,
  loading,
  exportCsvLoading,
  exportXlsxLoading,
  onFiltersChange,
  onSearch,
  onExportCsv,
  onExportXlsx,
}: ReportsFiltersProps) {
  return (
    <Space wrap style={{ marginBottom: 16 }}>
      <Select
        style={{ width: 320 }}
        value={filters.reportType}
        options={reportTypeOptions}
        onChange={(reportType) => onFiltersChange({ reportType: reportType as ReportTypeValue })}
        optionFilterProp="label"
      />

      {showClientFilter && (
        <Select
          mode="multiple"
          allowClear
          maxTagCount="responsive"
          style={{ width: 260 }}
          placeholder="Clientes"
          value={filters.clientIds}
          options={clientOptions}
          onChange={(clientIds) => onFiltersChange({ clientIds: clientIds.length > 0 ? clientIds : undefined })}
          optionFilterProp="label"
        />
      )}

      {showAgentFilter && (
        <Select
          mode="multiple"
          allowClear
          maxTagCount="responsive"
          style={{ width: 240 }}
          placeholder="Agentes"
          value={filters.agentIds}
          options={agentOptions}
          onChange={(agentIds) => onFiltersChange({ agentIds: agentIds.length > 0 ? agentIds : undefined })}
          optionFilterProp="label"
        />
      )}

      <Select
        mode="multiple"
        allowClear
        maxTagCount="responsive"
        style={{ width: 220 }}
        placeholder="Status"
        value={filters.statuses}
        options={statusOptions}
        onChange={(statuses) =>
          onFiltersChange({
            statuses: statuses.length > 0 ? (statuses as AppointmentStatusValue[]) : undefined,
          })
        }
      />

      <Select
        mode="multiple"
        allowClear
        maxTagCount="responsive"
        style={{ width: 240 }}
        placeholder="Tipos de serviço"
        value={filters.serviceTypeIds}
        options={serviceTypeOptions}
        onChange={(serviceTypeIds) =>
          onFiltersChange({ serviceTypeIds: serviceTypeIds.length > 0 ? serviceTypeIds : undefined })
        }
      />

      <RangePicker
        value={[
          filters.from ? dayjs(filters.from, 'YYYY-MM-DD') : null,
          filters.to ? dayjs(filters.to, 'YYYY-MM-DD') : null,
        ]}
        onChange={(dates) =>
          onFiltersChange({
            from: dates?.[0]?.format('YYYY-MM-DD'),
            to: dates?.[1]?.format('YYYY-MM-DD'),
          })
        }
      />

      <Button type="primary" icon={<SearchOutlined />} onClick={onSearch} loading={loading}>
        Buscar
      </Button>

      <Button icon={<DownloadOutlined />} onClick={onExportCsv} loading={exportCsvLoading}>
        Exportar CSV
      </Button>

      <Button icon={<DownloadOutlined />} onClick={onExportXlsx} loading={exportXlsxLoading}>
        Exportar XLSX
      </Button>
    </Space>
  )
}

export const reportTypeOptions = Object.entries(reportTypeLabel).map(([value, label]) => ({
  value: Number(value),
  label,
}))
