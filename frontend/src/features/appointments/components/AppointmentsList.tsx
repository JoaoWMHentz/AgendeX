import { Button, Select, Space, Table, Tag, Typography } from 'antd'
import { ReloadOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { DateRangePickerField } from '@/shared/components/DateRangePickerField'
import { formatDateTimeFromParts } from '@/shared/utils/date'
import {
  appointmentStatusColor,
  appointmentStatusLabel,
  type Appointment,
  type AppointmentFilters,
  type AppointmentStatusValue,
} from '../types'

const { Title } = Typography

type Option = {
  value: string | number
  label: string
}

type AppointmentsListProps = {
  title?: string
  appointments: Appointment[]
  loading: boolean
  filters: AppointmentFilters
  statusOptions: Option[]
  serviceTypeOptions: Option[]
  agentOptions?: Option[]
  showAgentFilter?: boolean
  showClientColumn?: boolean
  showAgentColumn?: boolean
  onFiltersChange: (filters: AppointmentFilters) => void
  onRowClick: (appointment: Appointment) => void
  onRefresh?: () => void
}

export function AppointmentsList({
  title = 'Agendamentos',
  appointments,
  loading,
  filters,
  statusOptions,
  serviceTypeOptions,
  agentOptions = [],
  showAgentFilter = false,
  showClientColumn = true,
  showAgentColumn = true,
  onFiltersChange,
  onRowClick,
  onRefresh,
}: AppointmentsListProps) {
  const columns: ColumnsType<Appointment> = [
    { title: 'Título', dataIndex: 'title', ellipsis: true },
    ...(showClientColumn ? [{ title: 'Cliente', dataIndex: 'clientName' } as const] : []),
    ...(showAgentColumn ? [{ title: 'Agente', dataIndex: 'agentName' } as const] : []),
    { title: 'Tipo', dataIndex: 'serviceTypeDescription' },
    {
      title: 'Data / Hora',
      render: (_: unknown, a: Appointment) => formatDateTimeFromParts(a.date, a.time),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      render: (status: number) => (
        <Tag color={appointmentStatusColor[status as AppointmentStatusValue]}>
          {appointmentStatusLabel[status as AppointmentStatusValue]}
        </Tag>
      ),
    },
  ]

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>
          {title}
        </Title>
      </div>

      <Space style={{ marginBottom: 16 }} wrap>
        <Select
          allowClear
          placeholder="Status"
          style={{ width: 160 }}
          value={filters.status}
          options={statusOptions}
          onChange={(status) =>
            onFiltersChange({ ...filters, status: status as AppointmentStatusValue })
          }
        />

        <Select
          allowClear
          placeholder="Tipo de serviço"
          style={{ width: 180 }}
          value={filters.serviceTypeId}
          options={serviceTypeOptions}
          onChange={(serviceTypeId) => onFiltersChange({ ...filters, serviceTypeId })}
        />

        {showAgentFilter && (
          <Select
            allowClear
            placeholder="Agente"
            style={{ width: 180 }}
            value={filters.agentId}
            options={agentOptions}
            onChange={(agentId) => onFiltersChange({ ...filters, agentId })}
          />
        )}

        <DateRangePickerField
          from={filters.from}
          to={filters.to}
          onChange={({ from, to }) => onFiltersChange({ ...filters, from, to })}
        />

        {onRefresh && (
          <Button icon={<ReloadOutlined />} onClick={onRefresh}>
            Atualizar
          </Button>
        )}
      </Space>

      <Table<Appointment>
        columns={columns}
        dataSource={appointments}
        rowKey="id"
        loading={loading}
        scroll={{ x: 900 }}
        onRow={(record) => ({ onClick: () => onRowClick(record), style: { cursor: 'pointer' } })}
      />
    </>
  )
}
