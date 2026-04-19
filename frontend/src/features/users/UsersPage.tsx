import { useState } from 'react'
import {
  Table, Button, Tag, Space, Modal, Form, Input, Select,
  Typography, Popconfirm, message,
} from 'antd'
import { PlusOutlined } from '@ant-design/icons'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useUsers, useCreateUser, useUpdateUserName, useDeleteUser, useSetClientDetail } from './useUsers'
import { userRoleLabel, UserRole, type User, type UserRoleValue } from './types'
import { useAuthStore } from '@/features/auth/authStore'
import { Roles } from '@/shared/constants/roles'
import { extractApiError } from '@/shared/utils/apiError'
import type { ColumnsType } from 'antd/es/table'

const { Title } = Typography

// --- Create user schema ---
const createSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório'),
  email: z.string().email('E-mail inválido'),
  password: z.string().min(6, 'Mínimo 6 caracteres'),
  role: z.number(),
})
type CreateForm = z.infer<typeof createSchema>

// --- Client detail schema ---
const clientDetailSchema = z.object({
  cpf: z.string().min(11, 'CPF inválido').max(14),
  birthDate: z.string().min(1, 'Data obrigatória'),
  phone: z.string().min(1, 'Telefone obrigatório'),
  notes: z.string().optional(),
})
type ClientDetailForm = z.infer<typeof clientDetailSchema>

export function UsersPage() {
  const { user: me } = useAuthStore()
  const isAdmin = me?.role === Roles.Administrator
  const [roleFilter, setRoleFilter] = useState<UserRoleValue | undefined>()
  const [createOpen, setCreateOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<User | null>(null)
  const [clientDetailTarget, setClientDetailTarget] = useState<User | null>(null)

  const { data = [], isLoading } = useUsers(roleFilter)
  const createUser = useCreateUser()
  const updateName = useUpdateUserName()
  const deleteUser = useDeleteUser()
  const setClientDetail = useSetClientDetail()

  // Create form
  const createForm = useForm<CreateForm>({
    resolver: zodResolver(createSchema),
    defaultValues: { role: UserRole.Client },
  })

  // Edit name form
  const editForm = useForm<{ name: string }>({
    resolver: zodResolver(z.object({ name: z.string().min(1) })),
  })

  // Client detail form
  const clientDetailForm = useForm<ClientDetailForm>({
    resolver: zodResolver(clientDetailSchema),
  })

  const handleCreate = async (values: CreateForm) => {
    try {
      await createUser.mutateAsync({ ...values, role: values.role as UserRoleValue })
      message.success('Usuário criado com sucesso')
      setCreateOpen(false)
      createForm.reset()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleUpdateName = async (values: { name: string }) => {
    if (!editTarget) return
    try {
      await updateName.mutateAsync({ id: editTarget.id, name: values.name })
      message.success('Nome atualizado')
      setEditTarget(null)
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleSetClientDetail = async (values: ClientDetailForm) => {
    if (!clientDetailTarget) return
    try {
      await setClientDetail.mutateAsync({ id: clientDetailTarget.id, data: values })
      message.success('Dados do cliente salvos')
      setClientDetailTarget(null)
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
              editForm.setValue('name', record.name)
            }}
          >
            Editar
          </Button>
          {record.role === UserRole.Client && (
            <Button
              size="small"
              onClick={() => {
                setClientDetailTarget(record)
                clientDetailForm.reset({
                  cpf: record.clientDetail?.cpf ?? '',
                  birthDate: record.clientDetail?.birthDate ?? '',
                  phone: record.clientDetail?.phone ?? '',
                  notes: record.clientDetail?.notes ?? '',
                })
              }}
            >
              Dados do cliente
            </Button>
          )}
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
        <Title level={4} style={{ margin: 0 }}>
          Usuários
        </Title>
        <Space>
          <Select
            allowClear
            placeholder="Filtrar por perfil"
            style={{ width: 160 }}
            onChange={(v) => setRoleFilter(v as UserRoleValue | undefined)}
            options={[
              { value: UserRole.Administrator, label: 'Administrador' },
              { value: UserRole.Agent, label: 'Agente' },
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
        confirmLoading={createUser.isPending}
      >
        <Form layout="vertical">
          {(['name', 'email', 'password'] as const).map((field) => (
            <Form.Item
              key={field}
              label={field === 'name' ? 'Nome' : field === 'email' ? 'E-mail' : 'Senha'}
              validateStatus={createForm.formState.errors[field] ? 'error' : ''}
              help={createForm.formState.errors[field]?.message}
            >
              <Controller
                name={field}
                control={createForm.control}
                render={({ field: f }) =>
                  field === 'password' ? (
                    <Input.Password {...f} />
                  ) : (
                    <Input {...f} />
                  )
                }
              />
            </Form.Item>
          ))}
          <Form.Item label="Perfil">
            <Controller
              name="role"
              control={createForm.control}
              render={({ field: f }) => (
                <Select
                  {...f}
                  options={[
                    { value: UserRole.Administrator, label: 'Administrador' },
                    { value: UserRole.Agent, label: 'Agente' },
                    { value: UserRole.Client, label: 'Cliente' },
                  ]}
                />
              )}
            />
          </Form.Item>
        </Form>
      </Modal>

      {/* Edit name modal */}
      <Modal
        title="Editar nome"
        open={!!editTarget}
        onCancel={() => setEditTarget(null)}
        onOk={editForm.handleSubmit(handleUpdateName)}
        confirmLoading={updateName.isPending}
      >
        <Form layout="vertical">
          <Form.Item
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
        </Form>
      </Modal>

      {/* Client detail modal */}
      <Modal
        title="Dados do cliente"
        open={!!clientDetailTarget}
        onCancel={() => setClientDetailTarget(null)}
        onOk={clientDetailForm.handleSubmit(handleSetClientDetail)}
        confirmLoading={setClientDetail.isPending}
      >
        <Form layout="vertical">
          {(
            [
              { name: 'cpf', label: 'CPF' },
              { name: 'birthDate', label: 'Data de nascimento (YYYY-MM-DD)' },
              { name: 'phone', label: 'Telefone' },
              { name: 'notes', label: 'Observações' },
            ] as const
          ).map(({ name, label }) => (
            <Form.Item
              key={name}
              label={label}
              validateStatus={clientDetailForm.formState.errors[name] ? 'error' : ''}
              help={clientDetailForm.formState.errors[name]?.message}
            >
              <Controller
                name={name}
                control={clientDetailForm.control}
                render={({ field }) => <Input {...field} />}
              />
            </Form.Item>
          ))}
        </Form>
      </Modal>
    </>
  )
}
