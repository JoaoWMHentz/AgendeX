import { Form, Select } from 'antd'
import { FormModal } from '@/shared/components/FormModal'

type Option = {
  value: string
  label: string
}

type ReassignAppointmentModalProps = {
  open: boolean
  loading: boolean
  value?: string
  options: Option[]
  onChange: (agentId?: string) => void
  onClose: () => void
  onSubmit: () => Promise<void>
}

export function ReassignAppointmentModal({
  open,
  loading,
  value,
  options,
  onChange,
  onClose,
  onSubmit,
}: ReassignAppointmentModalProps) {
  return (
    <FormModal
      title="Reatribuir agendamento"
      open={open}
      loading={loading}
      onClose={onClose}
      onSubmit={onSubmit}
      okButtonProps={{ disabled: !value }}
    >
        <Form.Item label="Novo agente" required>
          <Select
            style={{ width: '100%' }}
            options={options}
            value={value}
            onChange={onChange}
            placeholder="Selecione um agente"
          />
        </Form.Item>
    </FormModal>
  )
}
