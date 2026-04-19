import dayjs from 'dayjs'
import { PlusOutlined } from '@ant-design/icons'
import { Button, DatePicker, Select, Space, Table, Tag, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import type { ReactNode } from 'react'
import {
  AppointmentStatus,
  appointmentStatusColor,
  appointmentStatusLabel,
  type Appointment,
  type AppointmentFilters,
  type AppointmentStatusValue,
} from '../types'

const { Title } = Typography
const { RangePicker } = DatePicker

type Option = {
  value: string | number
  label: string
}

type AppointmentsListProps = {
  isAdmin: boolean
  isAgent: boolean
  isClient: boolean
  appointments: Appointment[]
  loading: boolean
  filters: AppointmentFilters
  statusOptions: Option[]
  serviceTypeOptions: Option[]
  agentOptions: Option[]
  onFiltersChange: (filters: AppointmentFilters) => void
  onOpenCreate: () => void
  onConfirm: (id: string) => void
  onOpenReject: (appointment: Appointment) => void
  onCancel: (id: string) => void
  onOpenComplete: (appointment: Appointment) => void
  onOpenReassign: (appointment: Appointment) => void
}

function isFutureAppointment(appointment: Appointment) {
  return dayjs(`${appointment.date}T${appointment.time}`).isAfter(dayjs())
}

export function AppointmentsList({
  isAdmin,
  isAgent,
  isClient,
  appointments,
  loading,
  filters,
  statusOptions,
  serviceTypeOptions,
  agentOptions,
  onFiltersChange,
  onOpenCreate,
  onConfirm,
  onOpenReject,
  onCancel,
  onOpenComplete,
  onOpenReassign,
}: AppointmentsListProps) {
  const renderActions = (record: Appointment) => {
    const actions: ReactNode[] = []
    const isFuture = isFutureAppointment(record)

    if (isAgent) {
      if (record.status === AppointmentStatus.PendingConfirmation) {
        actions.push(
          <Button key="confirm" size="small" type="primary" onClick={() => onConfirm(record.id)}>
            Confirmar
          </Button>,
          <Button key="reject" size="small" danger onClick={() => onOpenReject(record)}>
            Rejeitar
          </Button>,
        )
      }

      if (record.status === AppointmentStatus.Confirmed && !isFuture) {
        actions.push(
          <Button key="complete" size="small" onClick={() => onOpenComplete(record)}>
            Concluir
          </Button>,
        )
      }
    }

    if (isClient) {
      const canCancel =
        (record.status === AppointmentStatus.PendingConfirmation ||
          record.status === AppointmentStatus.Confirmed) &&
        isFuture

      if (canCancel) {
        actions.push(
          <Button key="cancel" size="small" danger onClick={() => onCancel(record.id)}>
            Cancelar
          </Button>,
        )
      }
    }

    if (isAdmin) {
      actions.push(
        <Button key="reassign" size="small" onClick={() => onOpenReassign(record)}>
          Reatribuir
        </Button>,
      )

      if (
        record.status === AppointmentStatus.PendingConfirmation ||
        record.status === AppointmentStatus.Confirmed
      ) {
        actions.push(
          <Button key="cancel" size="small" danger onClick={() => onCancel(record.id)}>
            Cancelar
          </Button>,
        )
      }
    }

    return <Space>{actions}</Space>
  }

  const columns: ColumnsType<Appointment> = [
    { title: 'Título', dataIndex: 'title', ellipsis: true },
    { title: 'Cliente', dataIndex: 'clientName' },
    { title: 'Agente', dataIndex: 'agentName' },
    { title: 'Tipo', dataIndex: 'serviceTypeDescription' },
    {
      title: 'Data / Hora',
      render: (_: unknown, appointment: Appointment) =>
        `${appointment.date} ${appointment.time.slice(0, 5)}`,
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
    { title: 'Ações', render: (_: unknown, appointment: Appointment) => renderActions(appointment) },
  ]

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>
          Agendamentos
        </Title>

        {isClient && (
          <Button type="primary" icon={<PlusOutlined />} onClick={onOpenCreate}>
            Novo agendamento
          </Button>
        )}
      </div>

      <Space style={{ marginBottom: 16 }} wrap>
        <Select
          allowClear
          placeholder="Status"
          style={{ width: 160 }}
          value={filters.status}
          options={statusOptions}
          onChange={(status) => onFiltersChange({ ...filters, status: status as AppointmentStatusValue })}
        />

        <Select
          allowClear
          placeholder="Tipo de serviço"
          style={{ width: 180 }}
          value={filters.serviceTypeId}
          options={serviceTypeOptions}
          onChange={(serviceTypeId) => onFiltersChange({ ...filters, serviceTypeId })}
        />

        {isAdmin && (
          <Select
            allowClear
            placeholder="Agente"
            style={{ width: 180 }}
            value={filters.agentId}
            options={agentOptions}
            onChange={(agentId) => onFiltersChange({ ...filters, agentId })}
          />
        )}

        <RangePicker
          onChange={(dates) => {
            onFiltersChange({
              ...filters,
              from: dates?.[0]?.format('YYYY-MM-DD'),
              to: dates?.[1]?.format('YYYY-MM-DD'),
            })
          }}
        />
      </Space>

      <Table<Appointment>
        columns={columns}
        dataSource={appointments}
        rowKey="id"
        loading={loading}
        scroll={{ x: 900 }}
      />
    </>
  )
}
