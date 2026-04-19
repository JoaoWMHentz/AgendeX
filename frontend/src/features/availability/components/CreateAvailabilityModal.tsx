import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { Form, Modal, Select } from 'antd'
import { z } from 'zod'
import { WeekDay, weekDayLabel, type WeekDayValue } from '../types'
import { TimePickerField } from '@/shared/components/TimePickerField'

const timeRegex = /^([01]\d|2[0-3]):[0-5]\d$/

const createAvailabilitySchema = z.object({
  agentId: z.string().uuid('Selecione um agente'),
  weekDay: z.number(),
  startTime: z.string().regex(timeRegex, 'Formato HH:mm'),
  endTime: z.string().regex(timeRegex, 'Formato HH:mm'),
})

export type CreateAvailabilityFormValues = z.infer<typeof createAvailabilitySchema>

type AgentOption = {
  value: string
  label: string
}

type CreateAvailabilityModalProps = {
  open: boolean
  loading: boolean
  agentOptions: AgentOption[]
  onClose: () => void
  onSubmit: (values: CreateAvailabilityFormValues) => Promise<void>
}

const weekDayOptions = Object.entries(weekDayLabel).map(([value, label]) => ({
  value: Number(value),
  label,
}))

export function CreateAvailabilityModal({
  open,
  loading,
  agentOptions,
  onClose,
  onSubmit,
}: CreateAvailabilityModalProps) {
  const form = useForm<CreateAvailabilityFormValues>({
    resolver: zodResolver(createAvailabilitySchema),
    defaultValues: { weekDay: WeekDay.Monday },
  })

  const handleCancel = () => {
    onClose()
    form.reset()
  }

  const handleSubmit = form.handleSubmit(async (values) => {
    await onSubmit({ ...values, weekDay: values.weekDay as WeekDayValue })
    form.reset()
  })

  return (
    <Modal
      title="Nova disponibilidade"
      open={open}
      onCancel={handleCancel}
      onOk={handleSubmit}
      confirmLoading={loading}
    >
      <Form layout="vertical">
        <Form.Item
          required
          label="Agente"
          validateStatus={form.formState.errors.agentId ? 'error' : ''}
          help={form.formState.errors.agentId?.message}
        >
          <Controller
            name="agentId"
            control={form.control}
            render={({ field }) => (
              <Select {...field} options={agentOptions} placeholder="Selecione" />
            )}
          />
        </Form.Item>

        <Form.Item required label="Dia da semana">
          <Controller
            name="weekDay"
            control={form.control}
            render={({ field }) => <Select {...field} options={weekDayOptions} />}
          />
        </Form.Item>

        {(['startTime', 'endTime'] as const).map((fieldName) => (
          <Form.Item
            key={fieldName}
            required
            label={fieldName === 'startTime' ? 'Horário início' : 'Horário fim'}
            validateStatus={form.formState.errors[fieldName] ? 'error' : ''}
            help={form.formState.errors[fieldName]?.message}
          >
            <Controller
              name={fieldName}
              control={form.control}
              render={({ field }) => <TimePickerField value={field.value} onChange={field.onChange} />}
            />
          </Form.Item>
        ))}
      </Form>
    </Modal>
  )
}
