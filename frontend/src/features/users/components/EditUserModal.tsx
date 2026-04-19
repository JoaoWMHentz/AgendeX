import { useEffect } from 'react'
import { Form, Input, Select, Switch, Divider } from 'antd'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { UserRole, type User } from '../models/types'
import { maskCpf, maskPhone } from '@/shared/utils/masks'
import { DatePickerField } from '@/shared/components/DatePickerField'
import { FormModal } from '@/shared/components/FormModal'

const editSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório'),
  role: z.number().optional(),
  isActive: z.boolean().optional(),
  cpf: z.string().optional(),
  birthDate: z.string().optional(),
  phone: z.string().optional(),
  notes: z.string().optional(),
})

export type EditUserFormValues = z.infer<typeof editSchema>

type EditUserModalProps = {
  open: boolean
  loading: boolean
  isAdmin: boolean
  targetUser: User | null
  onClose: () => void
  onSubmit: (values: EditUserFormValues) => Promise<void>
}

export function EditUserModal({
  open,
  loading,
  isAdmin,
  targetUser,
  onClose,
  onSubmit,
}: EditUserModalProps) {
  const form = useForm<EditUserFormValues>({
    resolver: zodResolver(editSchema),
  })

  const editedRole = useWatch({ control: form.control, name: 'role' })
  const showClientFields = editedRole === UserRole.Client

  useEffect(() => {
    if (!targetUser || !open) {
      return
    }

    form.reset({
      name: targetUser.name,
      role: targetUser.role,
      isActive: targetUser.isActive,
      cpf: targetUser.clientDetail?.cpf ?? '',
      birthDate: targetUser.clientDetail?.birthDate ?? '',
      phone: targetUser.clientDetail?.phone ?? '',
      notes: targetUser.clientDetail?.notes ?? '',
    })
  }, [open, targetUser, form])

  const handleCancel = () => {
    onClose()
    form.reset()
  }

  const handleSubmit = form.handleSubmit(async (values) => {
    await onSubmit(values)
  })

  return (
    <FormModal
      title="Editar usuário"
      open={open}
      loading={loading}
      onClose={handleCancel}
      onSubmit={handleSubmit}
    >
        <Form.Item
          required
          label="Nome"
          validateStatus={form.formState.errors.name ? 'error' : ''}
          help={form.formState.errors.name?.message}
        >
          <Controller
            name="name"
            control={form.control}
            render={({ field }) => <Input {...field} />}
          />
        </Form.Item>

        {isAdmin && (
          <Form.Item label="Perfil">
            <Controller
              name="role"
              control={form.control}
              render={({ field }) => (
                <Select
                  {...field}
                  options={[
                    { value: UserRole.Administrator, label: 'Administrador' },
                    { value: UserRole.Agent, label: 'Atendente' },
                    { value: UserRole.Client, label: 'Cliente' },
                  ]}
                />
              )}
            />
          </Form.Item>
        )}

        {isAdmin && (
          <Form.Item label="Conta ativa">
            <Controller
              name="isActive"
              control={form.control}
              render={({ field }) => (
                <Switch checked={field.value} onChange={field.onChange} />
              )}
            />
          </Form.Item>
        )}

        {showClientFields && (
          <>
            <Divider style={{ borderColor: '#f0f0f0', margin: '12px 0' }} />
            <Form.Item label="CPF" required>
              <Controller
                name="cpf"
                control={form.control}
                render={({ field }) => (
                  <Input
                    {...field}
                    placeholder="000.000.000-00"
                    onChange={(event) => field.onChange(maskCpf(event.target.value))}
                  />
                )}
              />
            </Form.Item>
            <Form.Item label="Data de nascimento" required>
              <Controller
                name="birthDate"
                control={form.control}
                render={({ field }) => <DatePickerField value={field.value} onChange={field.onChange} />}
              />
            </Form.Item>
            <Form.Item label="Telefone" required>
              <Controller
                name="phone"
                control={form.control}
                render={({ field }) => (
                  <Input
                    {...field}
                    placeholder="(00) 00000-0000"
                    onChange={(event) => field.onChange(maskPhone(event.target.value))}
                  />
                )}
              />
            </Form.Item>
            <Form.Item label="Observações">
              <Controller
                name="notes"
                control={form.control}
                render={({ field }) => <Input {...field} />}
              />
            </Form.Item>
          </>
        )}
    </FormModal>
  )
}
