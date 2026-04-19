import { Form, Input } from 'antd'
import { FormModal } from '@/shared/components/FormModal'

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
    <FormModal
      title="Concluir agendamento"
      open={open}
      loading={loading}
      onClose={onClose}
      onSubmit={onSubmit}
    >
        <Form.Item label="Resumo do atendimento (opcional)">
          <Input.TextArea rows={3} value={summary} onChange={(event) => onSummaryChange(event.target.value)} />
        </Form.Item>
    </FormModal>
  )
}
