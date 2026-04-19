import dayjs from 'dayjs'
import { Button, Descriptions, Modal, Space, Tag, Typography } from 'antd'
import { formatDateBr } from '@/shared/utils/date'
import {
  AppointmentStatus,
  appointmentStatusColor,
  appointmentStatusLabel,
  type Appointment,
} from '../types'

type Props = {
  appointment: Appointment | null
  onClose: () => void
  // Agent actions
  onConfirm?: (id: string) => void
  onOpenReject?: (a: Appointment) => void
  onOpenComplete?: (a: Appointment) => void
  // Admin actions
  onOpenReassign?: (a: Appointment) => void
  onCancel?: (id: string) => void
  // Client cancel requires appointment to be in the future
  cancelRequiresFuture?: boolean
}

function isFuture(a: Appointment) {
  return dayjs(`${a.date}T${a.time}`).isAfter(dayjs())
}

export function AppointmentDetailModal({
  appointment,
  onClose,
  onConfirm,
  onOpenReject,
  onOpenComplete,
  onOpenReassign,
  onCancel,
  cancelRequiresFuture = false,
}: Props) {
  if (!appointment) return null

  const future = isFuture(appointment)
  const isPending = appointment.status === AppointmentStatus.PendingConfirmation
  const isConfirmed = appointment.status === AppointmentStatus.Confirmed

  const showConfirm = !!onConfirm && isPending
  const showReject = !!onOpenReject && isPending
  const showComplete = !!onOpenComplete && isConfirmed && !future
  const showCancel =
    !!onCancel &&
    (isPending || isConfirmed) &&
    (!cancelRequiresFuture || future)
  const showReassign = !!onOpenReassign

  const hasActions = showConfirm || showReject || showComplete || showCancel || showReassign

  return (
    <Modal
      open
      title={appointment.title}
      onCancel={onClose}
      footer={
        hasActions ? (
          <Space wrap>
            {showConfirm && (
              <Button type="primary" onClick={() => onConfirm!(appointment.id)}>
                Confirmar
              </Button>
            )}
            {showReject && (
              <Button danger onClick={() => onOpenReject!(appointment)}>
                Rejeitar
              </Button>
            )}
            {showComplete && (
              <Button onClick={() => onOpenComplete!(appointment)}>
                Concluir
              </Button>
            )}
            {showReassign && (
              <Button onClick={() => onOpenReassign!(appointment)}>
                Reatribuir
              </Button>
            )}
            {showCancel && (
              <Button danger onClick={() => onCancel!(appointment.id)}>
                Cancelar agendamento
              </Button>
            )}
          </Space>
        ) : null
      }
    >
      <Descriptions column={2} size="small" style={{ marginTop: 8, marginBottom: 16 }}>
        <Descriptions.Item label="Tipo">{appointment.serviceTypeDescription}</Descriptions.Item>
        <Descriptions.Item label="Agente">{appointment.agentName}</Descriptions.Item>
        <Descriptions.Item label="Cliente">{appointment.clientName}</Descriptions.Item>
        <Descriptions.Item label="Data">
          {formatDateBr(appointment.date)}
        </Descriptions.Item>
        <Descriptions.Item label="Hora">{appointment.time.slice(0, 5)}</Descriptions.Item>
        <Descriptions.Item label="Status" span={2}>
          <Tag color={appointmentStatusColor[appointment.status]}>
            {appointmentStatusLabel[appointment.status]}
          </Tag>
        </Descriptions.Item>
        {appointment.rejectionReason && (
          <Descriptions.Item label="Motivo de rejeição" span={2}>
            {appointment.rejectionReason}
          </Descriptions.Item>
        )}
        {appointment.serviceSummary && (
          <Descriptions.Item label="Resumo do atendimento" span={2}>
            {appointment.serviceSummary}
          </Descriptions.Item>
        )}
      </Descriptions>

      {appointment.description && (
        <Typography.Paragraph style={{ margin: 0 }}>
          {appointment.description}
        </Typography.Paragraph>
      )}
    </Modal>
  )
}
