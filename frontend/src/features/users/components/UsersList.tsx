import { Table, Button, Tag, Space, Select, Typography, Popconfirm } from 'antd'
import { PlusOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { userRoleLabel, userRoleOptions, UserRole, type User, type UserRoleValue } from '../models/types'

const { Title } = Typography

type UsersListProps = {
  users: User[]
  loading: boolean
  isAdmin: boolean
  onRoleFilterChange: (role?: UserRoleValue) => void
  onOpenCreate: () => void
  onEdit: (user: User) => void
  onDelete: (id: string) => void
}

export function UsersList({
  users,
  loading,
  isAdmin,
  onRoleFilterChange,
  onOpenCreate,
  onEdit,
  onDelete,
}: UsersListProps) {
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
          <Button size="small" onClick={() => onEdit(record)}>
            Editar
          </Button>
          <Popconfirm
            title="Desativar este usuário?"
            onConfirm={() => onDelete(record.id)}
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
            onChange={(value) => onRoleFilterChange(value as UserRoleValue | undefined)}
            options={userRoleOptions}
          />
          {isAdmin && (
            <Button type="primary" icon={<PlusOutlined />} onClick={onOpenCreate}>
              Novo usuário
            </Button>
          )}
        </Space>
      </div>

      <Table<User> columns={columns} dataSource={users} rowKey="id" loading={loading} />
    </>
  )
}
