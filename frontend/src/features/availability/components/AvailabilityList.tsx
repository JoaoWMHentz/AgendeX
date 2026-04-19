import { PlusOutlined } from '@ant-design/icons'
import { Button, Popconfirm, Select, Space, Table, Tag, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { weekDayLabel, type Availability } from '../types'

const { Title } = Typography

type AgentOption = {
  value: string
  label: string
}

type AvailabilityListProps = {
  isAdmin: boolean
  isAgent: boolean
  availabilities: Availability[]
  loading: boolean
  selectedAgent?: string
  agentOptions: AgentOption[]
  onAgentChange: (agentId?: string) => void
  onOpenCreate: () => void
  onEdit: (availability: Availability) => void
  onDelete: (id: string) => void
}

export function AvailabilityList({
  isAdmin,
  isAgent,
  availabilities,
  loading,
  selectedAgent,
  agentOptions,
  onAgentChange,
  onOpenCreate,
  onEdit,
  onDelete,
}: AvailabilityListProps) {
  const columns: ColumnsType<Availability> = [
    {
      title: 'Dia da semana',
      dataIndex: 'weekDay',
      render: (day: number) => weekDayLabel[day as keyof typeof weekDayLabel],
    },
    { title: 'Início', dataIndex: 'startTime' },
    { title: 'Fim', dataIndex: 'endTime' },
    {
      title: 'Status',
      dataIndex: 'isActive',
      render: (active: boolean) => (
        <Tag color={active ? 'green' : 'default'}>{active ? 'Ativo' : 'Inativo'}</Tag>
      ),
    },
    ...(isAdmin
      ? [
          {
            title: 'Ações',
            render: (_: unknown, record: Availability) => (
              <Space>
                <Button size="small" onClick={() => onEdit(record)}>
                  Editar
                </Button>
                <Popconfirm
                  title="Desativar esta disponibilidade?"
                  onConfirm={() => onDelete(record.id)}
                  okText="Sim"
                  cancelText="Não"
                >
                  <Button size="small" danger>
                    Desativar
                  </Button>
                </Popconfirm>
              </Space>
            ),
          },
        ]
      : []),
  ]

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>
          Disponibilidade
        </Title>
        {isAdmin && (
          <Button type="primary" icon={<PlusOutlined />} onClick={onOpenCreate}>
            Novo Agendamento
          </Button>
        )}
      </div>

      {isAdmin && (
        <div style={{ marginBottom: 16 }}>
          <Select
            placeholder="Selecione um agente"
            style={{ width: 240 }}
            options={agentOptions}
            value={selectedAgent}
            onChange={onAgentChange}
            allowClear
          />
        </div>
      )}

      <Table<Availability>
        columns={columns}
        dataSource={availabilities}
        rowKey="id"
        loading={loading}
        locale={{
          emptyText: selectedAgent
            ? 'Nenhuma disponibilidade'
            : isAgent
              ? 'Nenhuma disponibilidade'
              : 'Selecione um agente',
        }}
      />
    </>
  )
}
