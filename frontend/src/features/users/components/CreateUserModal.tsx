import { Grid, Modal, Form, Input, Select, DatePicker, Row, Col } from 'antd'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import dayjs from 'dayjs'
import { UserRole } from '../models/types'
import { maskCpf, maskPhone } from '@/shared/utils/masks'

const { useBreakpoint } = Grid

const createSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório'),
  email: z.string().email('E-mail inválido'),
  password: z.string().min(8, 'Mínimo 8 caracteres'),
  confirmPassword: z.string().min(1, 'Confirme a senha'),
  role: z.number(),
  cpf: z.string().optional(),
  birthDate: z.string().optional(),
  phone: z.string().optional(),
  notes: z.string().optional(),
}).superRefine((d, ctx) => {
  if (d.password !== d.confirmPassword) {
    ctx.addIssue({ code: 'custom', message: 'As senhas não coincidem', path: ['confirmPassword'] })
  }

  if (d.role === UserRole.Client) {
    if (!d.cpf || d.cpf.replace(/\D/g, '').length < 11) {
      ctx.addIssue({ code: 'custom', message: 'CPF inválido', path: ['cpf'] })
    }

    if (!d.birthDate) {
      ctx.addIssue({ code: 'custom', message: 'Data obrigatória', path: ['birthDate'] })
    }

    if (!d.phone || d.phone.replace(/\D/g, '').length < 10) {
      ctx.addIssue({ code: 'custom', message: 'Telefone obrigatório', path: ['phone'] })
    }
  }
})

export type CreateUserFormValues = z.infer<typeof createSchema>

type CreateUserModalProps = {
  open: boolean
  loading: boolean
  onClose: () => void
  onSubmit: (values: CreateUserFormValues) => Promise<void>
}

export function CreateUserModal({ open, loading, onClose, onSubmit }: CreateUserModalProps) {
  const screens = useBreakpoint()

  const form = useForm<CreateUserFormValues>({
    resolver: zodResolver(createSchema),
    defaultValues: { role: UserRole.Client },
  })

  const selectedRole = useWatch({ control: form.control, name: 'role' })
  const showClientFields = selectedRole === UserRole.Client
  const isMobile = !screens.md

  const clientFieldsDividerStyle = isMobile
    ? {
      marginTop: 4,
      paddingTop: 16,
      borderTop: '1px solid rgba(0,0,0,0.08)',
    }
    : {
      paddingLeft: 16,
      borderLeft: '1px solid rgba(0,0,0,0.08)',
    }

  const handleCancel = () => {
    onClose()
    form.reset()
  }

  const handleSubmit = form.handleSubmit(async (values) => {
    await onSubmit(values)
    form.reset()
  })

  return (
    <Modal
      title="Novo usuário"
      open={open}
      onCancel={handleCancel}
      onOk={handleSubmit}
      confirmLoading={loading}
      width={showClientFields ? 800 : 500}
    >
      <Form layout="vertical">
        <Row gutter={[16, 16]}>
          <Col xs={24} md={showClientFields ? 12 : 24}>
            {(['name', 'email', 'password', 'confirmPassword'] as const).map((field) => (
              <Form.Item
                key={field}
                required
                label={
                  field === 'name' ? 'Nome'
                  : field === 'email' ? 'E-mail'
                  : field === 'password' ? 'Senha'
                  : 'Repetir senha'
                }
                validateStatus={form.formState.errors[field] ? 'error' : ''}
                help={form.formState.errors[field]?.message}
              >
                <Controller
                  name={field}
                  control={form.control}
                  render={({ field: controllerField }) =>
                    field === 'password' || field === 'confirmPassword'
                      ? <Input.Password {...controllerField} />
                      : <Input {...controllerField} />
                  }
                />
              </Form.Item>
            ))}

            <Form.Item label="Perfil" required>
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
          </Col>

          {showClientFields && (
            <Col xs={24} md={12}>
              <div className="client-fields-container" style={clientFieldsDividerStyle}>
                <Form.Item
                  required
                  label="CPF"
                  validateStatus={form.formState.errors.cpf ? 'error' : ''}
                  help={form.formState.errors.cpf?.message}
                >
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

                <Form.Item
                  required
                  label="Data de nascimento"
                  validateStatus={form.formState.errors.birthDate ? 'error' : ''}
                  help={form.formState.errors.birthDate?.message}
                >
                  <Controller
                    name="birthDate"
                    control={form.control}
                    render={({ field }) => (
                      <DatePicker
                        style={{ width: '100%' }}
                        format="DD/MM/YYYY"
                        value={field.value ? dayjs(field.value, 'YYYY-MM-DD') : null}
                        onChange={(date) => field.onChange(date ? date.format('YYYY-MM-DD') : '')}
                      />
                    )}
                  />
                </Form.Item>

                <Form.Item
                  required
                  label="Telefone"
                  validateStatus={form.formState.errors.phone ? 'error' : ''}
                  help={form.formState.errors.phone?.message}
                >
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
              </div>
            </Col>
          )}
        </Row>
      </Form>
    </Modal>
  )
}
