import { Form, Input, Modal } from 'antd'

type CompleteAppointmentModalProps = {
  open: boolean
  loading: boolean
  summary: string
  onSummaryChange: (summary: string) => void
  onClose: () => void
  onSubmit: () => Promise<void>
}

export function CompleteAppointmentModal({
  open,
  loading,
  summary,
  onSummaryChange,
  onClose,
  onSubmit,
}: CompleteAppointmentModalProps) {
  return (
    <Modal
      title="Concluir agendamento"
      open={open}
      onCancel={onClose}
      onOk={onSubmit}
      confirmLoading={loading}
    >
      <Form layout="vertical">
        <Form.Item label="Resumo do atendimento (opcional)">
          <Input.TextArea rows={3} value={summary} onChange={(event) => onSummaryChange(event.target.value)} />
        </Form.Item>
      </Form>
    </Modal>
  )
}
