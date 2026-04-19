import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { Form, Select } from 'antd'
import { z } from 'zod'
import { WeekDay, weekDayLabel } from '../types'
import { TimePickerField } from '@/shared/components/TimePickerField'
import { FormModal } from '@/shared/components/FormModal'

const timeRegex = /^([01]\d|2[0-3]):[0-5]\d$/

const weekDaySchema = z.union([
  z.literal(WeekDay.Sunday),
  z.literal(WeekDay.Monday),
  z.literal(WeekDay.Tuesday),
  z.literal(WeekDay.Wednesday),
  z.literal(WeekDay.Thursday),
  z.literal(WeekDay.Friday),
  z.literal(WeekDay.Saturday),
])

const createAvailabilitySchema = z.object({
  agentId: z.string().uuid('Selecione um agente'),
  weekDays: z
    .array(weekDaySchema)
    .min(1, 'Selecione ao menos um dia')
    .refine((days) => days.every((day) => day >= WeekDay.Monday && day <= WeekDay.Friday), {
      message: 'Selecione apenas dias entre segunda e sexta',
    }),
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

const businessWeekDayOptions = weekDayOptions.filter((option) =>
  option.value >= WeekDay.Monday && option.value <= WeekDay.Friday)

export function CreateAvailabilityModal({
  open,
  loading,
  agentOptions,
  onClose,
  onSubmit,
}: CreateAvailabilityModalProps) {
  const form = useForm<CreateAvailabilityFormValues>({
    resolver: zodResolver(createAvailabilitySchema),
    defaultValues: { weekDays: [WeekDay.Monday] },
  })

  const handleCancel = () => {
    onClose()
    form.reset()
  }

  const handleSubmit = form.handleSubmit(async (values) => {
    await onSubmit(values)
    form.reset()
  })

  return (
    <FormModal
      title="Nova disponibilidade"
      open={open}
      loading={loading}
      onClose={handleCancel}
      onSubmit={handleSubmit}
    >
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

      <Form.Item
        required
        label="Dias da semana"
        validateStatus={form.formState.errors.weekDays ? 'error' : ''}
        help={form.formState.errors.weekDays?.message}
      >
        <Controller
          name="weekDays"
          control={form.control}
          render={({ field }) => (
            <Select
              {...field}
              mode="multiple"
              options={businessWeekDayOptions}
              placeholder="Selecione um ou mais dias"
            />
          )}
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
    </FormModal>
  )
}
