import dayjs from 'dayjs'
import { Button, DatePicker, Descriptions, Modal, Select, Space, Table, Tag, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import {
  AppointmentStatus,
  appointmentStatusColor,
  appointmentStatusLabel,
  type Appointment,
  type AppointmentStatusValue,
} from '../types'
import { useClientMyAppointmentsController } from '../hooks/useClientMyAppointmentsController'

const { Title } = Typography
const { RangePicker } = DatePicker

function isFutureAppointment(a: Appointment) {
  return dayjs(`${a.date}T${a.time}`).isAfter(dayjs())
}

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

  const canCancel =
    selectedAppointment &&
    (selectedAppointment.status === AppointmentStatus.PendingConfirmation ||
      selectedAppointment.status === AppointmentStatus.Confirmed) &&
    isFutureAppointment(selectedAppointment)

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
      </Space>

      <Table<Appointment>
        columns={columns}
        dataSource={appointments}
        rowKey="id"
        loading={isLoading}
        scroll={{ x: 600 }}
        onRow={(record) => ({ onClick: () => openDetail(record), style: { cursor: 'pointer' } })}
      />

      <Modal
        open={!!selectedAppointment}
        title={selectedAppointment?.title}
        onCancel={closeDetail}
        footer={
          canCancel ? (
            <Button danger onClick={() => handleCancel(selectedAppointment!.id)}>
              Cancelar agendamento
            </Button>
          ) : null
        }
      >
        {selectedAppointment && (
          <>
            <Descriptions column={2} size="small" style={{ marginTop: 8, marginBottom: 16 }}>
              <Descriptions.Item label="Tipo">{selectedAppointment.serviceTypeDescription}</Descriptions.Item>
              <Descriptions.Item label="Atendente">{selectedAppointment.agentName}</Descriptions.Item>
              <Descriptions.Item label="Data">
                {dayjs(selectedAppointment.date).format('DD/MM/YYYY')}
              </Descriptions.Item>
              <Descriptions.Item label="Hora">{selectedAppointment.time.slice(0, 5)}</Descriptions.Item>
              <Descriptions.Item label="Status" span={2}>
                <Tag color={appointmentStatusColor[selectedAppointment.status]}>
                  {appointmentStatusLabel[selectedAppointment.status]}
                </Tag>
              </Descriptions.Item>
              {selectedAppointment.rejectionReason && (
                <Descriptions.Item label="Motivo de rejeição" span={2}>
                  {selectedAppointment.rejectionReason}
                </Descriptions.Item>
              )}
              {selectedAppointment.serviceSummary && (
                <Descriptions.Item label="Resumo do atendimento" span={2}>
                  {selectedAppointment.serviceSummary}
                </Descriptions.Item>
              )}
            </Descriptions>

            {selectedAppointment.description && (
              <Typography.Paragraph style={{ margin: 0 }}>
                {selectedAppointment.description}
              </Typography.Paragraph>
            )}
          </>
        )}
      </Modal>
    </>
  )
}
