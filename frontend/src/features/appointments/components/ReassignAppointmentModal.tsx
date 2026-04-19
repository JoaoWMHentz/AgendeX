import { Form, Modal, Select } from 'antd'

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
    <Modal
      title="Reatribuir agendamento"
      open={open}
      onCancel={onClose}
      onOk={onSubmit}
      confirmLoading={loading}
      okButtonProps={{ disabled: !value }}
    >
      <Form layout="vertical">
        <Form.Item label="Novo agente" required>
          <Select
            style={{ width: '100%' }}
            options={options}
            value={value}
            onChange={onChange}
            placeholder="Selecione um agente"
          />
        </Form.Item>
      </Form>
    </Modal>
  )
}
