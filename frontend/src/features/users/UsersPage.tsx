import { useState } from 'react'
import {
  Table, Button, Tag, Space, Modal, Form, Input, Select, Switch, Divider, DatePicker,
  Typography, Popconfirm, message,
} from 'antd'
import { PlusOutlined } from '@ant-design/icons'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import dayjs from 'dayjs'
import { useUsers, useCreateUser, useUpdateUser, useDeleteUser, useSetClientDetail } from './useUsers'
import { userRoleLabel, UserRole, type User, type UserRoleValue } from './types'
import { useAuthStore } from '@/features/auth/authStore'
import { Roles } from '@/shared/constants/roles'
import { extractApiError } from '@/shared/utils/apiError'
import { maskCpf, maskPhone } from '@/shared/utils/masks'
import type { ColumnsType } from 'antd/es/table'

const { Title } = Typography

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
type CreateForm = z.infer<typeof createSchema>

const editSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório'),
  role: z.number().optional(),
  isActive: z.boolean().optional(),
  cpf: z.string().optional(),
  birthDate: z.string().optional(),
  phone: z.string().optional(),
  notes: z.string().optional(),
})
type EditForm = z.infer<typeof editSchema>

export function UsersPage() {
  const { user: me } = useAuthStore()
  const isAdmin = me?.role === Roles.Administrator
  const [roleFilter, setRoleFilter] = useState<UserRoleValue | undefined>()
  const [createOpen, setCreateOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<User | null>(null)

  const { data = [], isLoading } = useUsers(roleFilter)
  const createUser = useCreateUser()
  const updateUser = useUpdateUser()
  const deleteUser = useDeleteUser()
  const setClientDetail = useSetClientDetail()

  const createForm = useForm<CreateForm>({
    resolver: zodResolver(createSchema),
    defaultValues: { role: UserRole.Client },
  })

  const editForm = useForm<EditForm>({
    resolver: zodResolver(editSchema),
  })

  const createRole = useWatch({ control: createForm.control, name: 'role' })
  const showCreateClientFields = createRole === UserRole.Client

  const editedRole = useWatch({ control: editForm.control, name: 'role' })
  const showClientFields = editedRole === UserRole.Client

  const handleCreate = async (values: CreateForm) => {
    try {
      const { confirmPassword: _, cpf, birthDate, phone, notes, ...payload } = values
      const created = await createUser.mutateAsync({ ...payload, role: payload.role as UserRoleValue })
      if (payload.role === UserRole.Client && cpf) {
        await setClientDetail.mutateAsync({
          id: created.id,
          data: { cpf, birthDate: birthDate ?? '', phone: phone ?? '', notes },
        })
      }
      message.success('Usuário criado com sucesso')
      setCreateOpen(false)
      createForm.reset()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleUpdate = async (values: EditForm) => {
    if (!editTarget) return
    try {
      await updateUser.mutateAsync({
        id: editTarget.id,
        data: {
          name: values.name,
          role: isAdmin ? (values.role as UserRoleValue) : undefined,
          isActive: isAdmin ? values.isActive : undefined,
        },
      })
      const effectiveRole = isAdmin ? values.role : editTarget.role
      if (effectiveRole === UserRole.Client && values.cpf) {
        await setClientDetail.mutateAsync({
          id: editTarget.id,
          data: {
            cpf: values.cpf,
            birthDate: values.birthDate ?? '',
            phone: values.phone ?? '',
            notes: values.notes,
          },
        })
      }
      message.success('Usuário atualizado')
      setEditTarget(null)
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleDelete = async (id: string) => {
    try {
      await deleteUser.mutateAsync(id)
      message.success('Usuário desativado')
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const columns: ColumnsType<User> = [
    { title: 'Nome', dataIndex: 'name' },
    { title: 'E-mail', dataIndex: 'email' },
    {
      title: 'Perfil',
      dataIndex: 'role',
      render: (role: number) => <Tag>{userRoleLabel[role as keyof typeof userRoleLabel]}</Tag>,
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      render: (active: boolean) => (
        <Tag color={active ? 'green' : 'default'}>{active ? 'Ativo' : 'Inativo'}</Tag>
      ),
    },
    {
      title: 'Ações',
      render: (_: unknown, record: User) => (
        <Space>
          <Button
            size="small"
            onClick={() => {
              setEditTarget(record)
              editForm.reset({
                name: record.name,
                role: record.role,
                isActive: record.isActive,
                cpf: record.clientDetail?.cpf ?? '',
                birthDate: record.clientDetail?.birthDate ?? '',
                phone: record.clientDetail?.phone ?? '',
                notes: record.clientDetail?.notes ?? '',
              })
            }}
          >
            Editar
          </Button>
          <Popconfirm
            title="Desativar este usuário?"
            onConfirm={() => handleDelete(record.id)}
            okText="Sim"
            cancelText="Não"
          >
            <Button size="small" danger disabled={!record.isActive}>
              Desativar
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ]

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Usuários</Title>
        <Space>
          <Select
            allowClear
            placeholder="Filtrar por perfil"
            style={{ width: 160 }}
            onChange={(v) => setRoleFilter(v as UserRoleValue | undefined)}
            options={[
              { value: UserRole.Administrator, label: 'Administrador' },
              { value: UserRole.Agent, label: 'Atendente' },
              { value: UserRole.Client, label: 'Cliente' },
            ]}
          />
          {isAdmin && (
            <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>
              Novo usuário
            </Button>
          )}
        </Space>
      </div>

      <Table<User> columns={columns} dataSource={data} rowKey="id" loading={isLoading} />

      {/* Create modal */}
      <Modal
        title="Novo usuário"
        open={createOpen}
        onCancel={() => { setCreateOpen(false); createForm.reset() }}
        onOk={createForm.handleSubmit(handleCreate)}
        confirmLoading={createUser.isPending || setClientDetail.isPending}
      >
        <Form layout="vertical">
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
              validateStatus={createForm.formState.errors[field] ? 'error' : ''}
              help={createForm.formState.errors[field]?.message}
            >
              <Controller
                name={field}
                control={createForm.control}
                render={({ field: f }) =>
                  field === 'password' || field === 'confirmPassword'
                    ? <Input.Password {...f} />
                    : <Input {...f} />
                }
              />
            </Form.Item>
          ))}
          <Form.Item label="Perfil" required>
            <Controller
              name="role"
              control={createForm.control}
              render={({ field: f }) => (
                <Select
                  {...f}
                  options={[
                    { value: UserRole.Administrator, label: 'Administrador' },
                    { value: UserRole.Agent, label: 'Atendente' },
                    { value: UserRole.Client, label: 'Cliente' },
                  ]}
                />
              )}
            />
          </Form.Item>
          {showCreateClientFields && (
            <>
              <Divider style={{ borderColor: '#f0f0f0', margin: '12px 0' }} />
              <Form.Item
                required
                label="CPF"
                validateStatus={createForm.formState.errors.cpf ? 'error' : ''}
                help={createForm.formState.errors.cpf?.message}
              >
                <Controller
                  name="cpf"
                  control={createForm.control}
                  render={({ field }) => (
                    <Input
                      {...field}
                      placeholder="000.000.000-00"
                      onChange={(e) => field.onChange(maskCpf(e.target.value))}
                    />
                  )}
                />
              </Form.Item>
              <Form.Item
                required
                label="Data de nascimento"
                validateStatus={createForm.formState.errors.birthDate ? 'error' : ''}
                help={createForm.formState.errors.birthDate?.message}
              >
                <Controller
                  name="birthDate"
                  control={createForm.control}
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
                validateStatus={createForm.formState.errors.phone ? 'error' : ''}
                help={createForm.formState.errors.phone?.message}
              >
                <Controller
                  name="phone"
                  control={createForm.control}
                  render={({ field }) => (
                    <Input
                      {...field}
                      placeholder="(00) 00000-0000"
                      onChange={(e) => field.onChange(maskPhone(e.target.value))}
                    />
                  )}
                />
              </Form.Item>
              <Form.Item label="Observações">
                <Controller
                  name="notes"
                  control={createForm.control}
                  render={({ field }) => <Input {...field} />}
                />
              </Form.Item>
              <Form.Item label="Ativo?">
                <Switch checked disabled />
              </Form.Item>
            </>
          )}
        </Form>
      </Modal>

      {/* Edit modal */}
      <Modal
        title="Editar usuário"
        open={!!editTarget}
        onCancel={() => setEditTarget(null)}
        onOk={editForm.handleSubmit(handleUpdate)}
        confirmLoading={updateUser.isPending || setClientDetail.isPending}
      >
        <Form layout="vertical">
          <Form.Item
            required
            label="Nome"
            validateStatus={editForm.formState.errors.name ? 'error' : ''}
            help={editForm.formState.errors.name?.message}
          >
            <Controller
              name="name"
              control={editForm.control}
              render={({ field }) => <Input {...field} />}
            />
          </Form.Item>
          {isAdmin && (
            <Form.Item label="Perfil">
              <Controller
                name="role"
                control={editForm.control}
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
                control={editForm.control}
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
                  control={editForm.control}
                  render={({ field }) => (
                    <Input
                      {...field}
                      placeholder="000.000.000-00"
                      onChange={(e) => field.onChange(maskCpf(e.target.value))}
                    />
                  )}
                />
              </Form.Item>
              <Form.Item label="Data de nascimento" required>
                <Controller
                  name="birthDate"
                  control={editForm.control}
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
              <Form.Item label="Telefone" required>
                <Controller
                  name="phone"
                  control={editForm.control}
                  render={({ field }) => (
                    <Input
                      {...field}
                      placeholder="(00) 00000-0000"
                      onChange={(e) => field.onChange(maskPhone(e.target.value))}
                    />
                  )}
                />
              </Form.Item>
              <Form.Item label="Observações">
                <Controller
                  name="notes"
                  control={editForm.control}
                  render={({ field }) => <Input {...field} />}
                />
              </Form.Item>
            </>
          )}
        </Form>
      </Modal>
    </>
  )
}
