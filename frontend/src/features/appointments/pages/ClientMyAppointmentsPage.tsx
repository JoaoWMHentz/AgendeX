import { Button, DatePicker, Select, Space, Table, Tag, Typography } from 'antd'
import { ReloadOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { appointmentStatusColor, appointmentStatusLabel, type Appointment, type AppointmentStatusValue } from '../types'
import { AppointmentDetailModal } from '../components/AppointmentDetailModal'
import { useClientMyAppointmentsController } from '../hooks/useClientMyAppointmentsController'

const { Title } = Typography
const { RangePicker } = DatePicker

export function ClientMyAppointmentsPage() {
  const {
    appointments,
    isLoading,
    filters,
    setFilters,
    selectedAppointment,
    openDetail,
    closeDetail,
    statusOptions,
    serviceTypeOptions,
    refetch,
    handleCancel,
  } = useClientMyAppointmentsController()

  const columns: ColumnsType<Appointment> = [
    { title: 'Título', dataIndex: 'title', ellipsis: true },
    { title: 'Tipo', dataIndex: 'serviceTypeDescription' },
    {
      title: 'Data / Hora',
      render: (_: unknown, a: Appointment) => `${a.date} ${a.time.slice(0, 5)}`,
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
      <Title level={4} style={{ marginBottom: 16 }}>
        Seus Agendamentos
      </Title>

      <Space style={{ marginBottom: 16 }} wrap>
        <Select
          allowClear
          placeholder="Status"
          style={{ width: 160 }}
          value={filters.status}
          options={statusOptions}
          onChange={(status) => setFilters({ ...filters, status: status as AppointmentStatusValue })}
        />
        <Select
          allowClear
          placeholder="Tipo de serviço"
          style={{ width: 180 }}
          value={filters.serviceTypeId}
          options={serviceTypeOptions}
          onChange={(serviceTypeId) => setFilters({ ...filters, serviceTypeId })}
        />
        <RangePicker
          onChange={(dates) =>
            setFilters({
              ...filters,
              from: dates?.[0]?.format('YYYY-MM-DD'),
              to: dates?.[1]?.format('YYYY-MM-DD'),
            })
          }
        />

        <Button icon={<ReloadOutlined />} onClick={() => refetch()}>
          Atualizar
        </Button>
      </Space>

      <Table<Appointment>
        columns={columns}
        dataSource={appointments}
        rowKey="id"
        loading={isLoading}
        scroll={{ x: 600 }}
        onRow={(record) => ({ onClick: () => openDetail(record), style: { cursor: 'pointer' } })}
      />

      <AppointmentDetailModal
        appointment={selectedAppointment}
        onClose={closeDetail}
        onCancel={handleCancel}
        cancelRequiresFuture
      />
    </>
  )
}
