import { Form, Input } from 'antd'
import { FormModal } from '@/shared/components/FormModal'

type RejectAppointmentModalProps = {
  open: boolean
  loading: boolean
  reason: string
  onReasonChange: (reason: string) => void
  onClose: () => void
  onSubmit: () => Promise<void>
}

export function RejectAppointmentModal({
  open,
  loading,
  reason,
  onReasonChange,
  onClose,
  onSubmit,
}: RejectAppointmentModalProps) {
  return (
    <FormModal
      title="Rejeitar agendamento"
      open={open}
      loading={loading}
      onClose={onClose}
      onSubmit={onSubmit}
      okButtonProps={{ danger: true }}
    >
        <Form.Item label="Motivo da rejeição" required>
          <Input.TextArea rows={3} value={reason} onChange={(event) => onReasonChange(event.target.value)} />
        </Form.Item>
    </FormModal>
  )
}
