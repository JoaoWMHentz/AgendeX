import { useEffect } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm } from 'react-hook-form'
import { Form, Modal } from 'antd'
import { z } from 'zod'
import type { Availability } from '../types'
import { TimePickerField } from '@/shared/components/TimePickerField'

const timeRegex = /^([01]\d|2[0-3]):[0-5]\d$/

const editAvailabilitySchema = z.object({
  startTime: z.string().regex(timeRegex, 'Formato HH:mm'),
  endTime: z.string().regex(timeRegex, 'Formato HH:mm'),
})

export type EditAvailabilityFormValues = z.infer<typeof editAvailabilitySchema>

type EditAvailabilityModalProps = {
  open: boolean
  loading: boolean
  targetAvailability: Availability | null
  onClose: () => void
  onSubmit: (values: EditAvailabilityFormValues) => Promise<void>
}

export function EditAvailabilityModal({
  open,
  loading,
  targetAvailability,
  onClose,
  onSubmit,
}: EditAvailabilityModalProps) {
  const form = useForm<EditAvailabilityFormValues>({ resolver: zodResolver(editAvailabilitySchema) })

  useEffect(() => {
    if (!targetAvailability) return

    form.reset({
      startTime: targetAvailability.startTime,
      endTime: targetAvailability.endTime,
    })
  }, [targetAvailability, form])

  const handleSubmit = form.handleSubmit(async (values) => {
    await onSubmit(values)
    form.reset()
  })

  const handleClose = () => {
    onClose()
    form.reset()
  }

  return (
    <Modal
      title="Editar disponibilidade"
      open={open}
      onCancel={handleClose}
      onOk={handleSubmit}
      confirmLoading={loading}
    >
      <Form layout="vertical">
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
