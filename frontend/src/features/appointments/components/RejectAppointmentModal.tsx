import { Form, Input, Modal } from 'antd'

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
    <Modal
      title="Rejeitar agendamento"
      open={open}
      onCancel={onClose}
      onOk={onSubmit}
      confirmLoading={loading}
      okButtonProps={{ danger: true }}
    >
      <Form layout="vertical">
        <Form.Item label="Motivo da rejeição" required>
          <Input.TextArea rows={3} value={reason} onChange={(event) => onReasonChange(event.target.value)} />
        </Form.Item>
      </Form>
    </Modal>
  )
}
