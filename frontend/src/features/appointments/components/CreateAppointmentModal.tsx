import dayjs from 'dayjs'
import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { Form, Input, Select } from 'antd'
import { z } from 'zod'
import { TimePickerField } from '@/shared/components/TimePickerField'
import { DatePickerField } from '@/shared/components/DatePickerField'
import { FormModal } from '@/shared/components/FormModal'

const createAppointmentSchema = z.object({
  title: z.string().min(1, 'Título obrigatório'),
  description: z.string().optional(),
  serviceTypeId: z.number({ message: 'Selecione o tipo' }),
  agentId: z.string().uuid('Selecione um agente'),
  date: z.string().min(1, 'Data obrigatória'),
  time: z.string().regex(/^([01]\d|2[0-3]):[0-5]\d$/, 'Formato HH:mm'),
  notes: z.string().optional(),
})

export type CreateAppointmentFormValues = z.infer<typeof createAppointmentSchema>

type Option = {
  value: string | number
  label: string
}

type CreateAppointmentModalProps = {
  open: boolean
  loading: boolean
  serviceTypeOptions: Option[]
  agentOptions: Option[]
  onClose: () => void
  onSubmit: (values: CreateAppointmentFormValues) => Promise<void>
}

export function CreateAppointmentModal({
  open,
  loading,
  serviceTypeOptions,
  agentOptions,
  onClose,
  onSubmit,
}: CreateAppointmentModalProps) {
  const form = useForm<CreateAppointmentFormValues>({
    resolver: zodResolver(createAppointmentSchema),
  })

  const handleClose = () => {
    onClose()
    form.reset()
  }

  const handleSubmit = form.handleSubmit(async (values) => {
    await onSubmit(values)
    form.reset()
  })

  return (
    <FormModal
      title="Novo agendamento"
      open={open}
      loading={loading}
      onClose={handleClose}
      onSubmit={handleSubmit}
      width={520}
    >
        <Form.Item
          required
          label="Título"
          validateStatus={form.formState.errors.title ? 'error' : ''}
          help={form.formState.errors.title?.message}
        >
          <Controller name="title" control={form.control} render={({ field }) => <Input {...field} />} />
        </Form.Item>

        <Form.Item label="Descrição">
          <Controller
            name="description"
            control={form.control}
            render={({ field }) => <Input.TextArea {...field} rows={2} />}
          />
        </Form.Item>

        <Form.Item
          required
          label="Tipo de serviço"
          validateStatus={form.formState.errors.serviceTypeId ? 'error' : ''}
          help={form.formState.errors.serviceTypeId?.message}
        >
          <Controller
            name="serviceTypeId"
            control={form.control}
            render={({ field }) => (
              <Select {...field} options={serviceTypeOptions} placeholder="Selecione" />
            )}
          />
        </Form.Item>

        <Form.Item
          required
          label="Agente"
          validateStatus={form.formState.errors.agentId ? 'error' : ''}
          help={form.formState.errors.agentId?.message}
        >
          <Controller
            name="agentId"
            control={form.control}
            render={({ field }) => <Select {...field} options={agentOptions} placeholder="Selecione" />}
          />
        </Form.Item>

        <Form.Item
          required
          label="Data"
          validateStatus={form.formState.errors.date ? 'error' : ''}
          help={form.formState.errors.date?.message}
        >
          <Controller
            name="date"
            control={form.control}
            render={({ field }) => (
              <DatePickerField
                value={field.value}
                onChange={field.onChange}
                disabledDate={(date) => date.isBefore(dayjs(), 'day')}
              />
            )}
          />
        </Form.Item>

        <Form.Item
          required
          label="Horário (HH:mm)"
          validateStatus={form.formState.errors.time ? 'error' : ''}
          help={form.formState.errors.time?.message}
        >
          <Controller
            name="time"
            control={form.control}
            render={({ field }) => <TimePickerField value={field.value} onChange={field.onChange} />}
          />
        </Form.Item>

        <Form.Item label="Observações">
          <Controller
            name="notes"
            control={form.control}
            render={({ field }) => <Input.TextArea {...field} rows={2} />}
          />
        </Form.Item>
    </FormModal>
  )
}
