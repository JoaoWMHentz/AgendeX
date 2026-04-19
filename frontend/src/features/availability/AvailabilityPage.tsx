import { useState } from 'react'
import {
  Table, Button, Select, Space, Modal, Form, Tag,
  Typography, Popconfirm, message,
} from 'antd'
import { PlusOutlined } from '@ant-design/icons'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import {
  useAvailabilityByAgent, useCreateAvailability,
  useUpdateAvailability, useDeleteAvailability,
} from './useAvailability'
import { useAgents } from '@/features/users/useUsers'
import { weekDayLabel, WeekDay, type Availability, type WeekDayValue } from './types'
import { useAuthStore } from '@/features/auth/authStore'
import { Roles } from '@/shared/constants/roles'
import { extractApiError } from '@/shared/utils/apiError'
import type { ColumnsType } from 'antd/es/table'

const { Title } = Typography

const timeRegex = /^([01]\d|2[0-3]):[0-5]\d:[0-5]\d$/

const availabilitySchema = z.object({
  agentId: z.string().uuid('Selecione um agente'),
  weekDay: z.number(),
  startTime: z.string().regex(timeRegex, 'Formato HH:mm:ss'),
  endTime: z.string().regex(timeRegex, 'Formato HH:mm:ss'),
})
type AvailabilityForm = z.infer<typeof availabilitySchema>

const updateSchema = z.object({
  startTime: z.string().regex(timeRegex, 'Formato HH:mm:ss'),
  endTime: z.string().regex(timeRegex, 'Formato HH:mm:ss'),
})
type UpdateForm = z.infer<typeof updateSchema>

export function AvailabilityPage() {
  const { user: me } = useAuthStore()
  const isAdmin = me?.role === Roles.Administrator
  const [selectedAgent, setSelectedAgent] = useState<string | undefined>()
  const [createOpen, setCreateOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<Availability | null>(null)

  const { data: agents = [] } = useAgents()
  const { data: availabilities = [], isLoading } = useAvailabilityByAgent(selectedAgent)
  const createAvailability = useCreateAvailability()
  const updateAvailability = useUpdateAvailability(selectedAgent ?? '')
  const deleteAvailability = useDeleteAvailability(selectedAgent ?? '')

  const createForm = useForm<AvailabilityForm>({
    resolver: zodResolver(availabilitySchema),
    defaultValues: { weekDay: WeekDay.Monday },
  })
  const updateForm = useForm<UpdateForm>({ resolver: zodResolver(updateSchema) })

  const handleCreate = async (values: AvailabilityForm) => {
    try {
      await createAvailability.mutateAsync({ ...values, weekDay: values.weekDay as WeekDayValue })
      message.success('Disponibilidade criada')
      setCreateOpen(false)
      createForm.reset()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleUpdate = async (values: UpdateForm) => {
    if (!editTarget) return
    try {
      await updateAvailability.mutateAsync({ id: editTarget.id, data: values })
      message.success('Disponibilidade atualizada')
      setEditTarget(null)
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleDelete = async (id: string) => {
    try {
      await deleteAvailability.mutateAsync(id)
      message.success('Disponibilidade desativada')
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const columns: ColumnsType<Availability> = [
    {
      title: 'Dia da semana',
      dataIndex: 'weekDay',
      render: (d: number) => weekDayLabel[d as keyof typeof weekDayLabel],
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
                <Button
                  size="small"
                  onClick={() => {
                    setEditTarget(record)
                    updateForm.reset({ startTime: record.startTime, endTime: record.endTime })
                  }}
                >
                  Editar
                </Button>
                <Popconfirm
                  title="Desativar esta disponibilidade?"
                  onConfirm={() => handleDelete(record.id)}
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

  const weekDayOptions = Object.entries(weekDayLabel).map(([value, label]) => ({
    value: Number(value),
    label,
  }))

  const agentOptions = agents.map((a) => ({ value: a.id, label: a.name }))

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>
          Disponibilidade
        </Title>
        {isAdmin && (
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>
            Nova janela
          </Button>
        )}
      </div>

      <div style={{ marginBottom: 16 }}>
        <Select
          placeholder="Selecione um agente"
          style={{ width: 240 }}
          options={agentOptions}
          onChange={setSelectedAgent}
          allowClear
        />
      </div>

      <Table<Availability>
        columns={columns}
        dataSource={availabilities}
        rowKey="id"
        loading={isLoading}
        locale={{ emptyText: selectedAgent ? 'Nenhuma disponibilidade' : 'Selecione um agente' }}
      />

      {/* Create modal */}
      <Modal
        title="Nova disponibilidade"
        open={createOpen}
        onCancel={() => { setCreateOpen(false); createForm.reset() }}
        onOk={createForm.handleSubmit(handleCreate)}
        confirmLoading={createAvailability.isPending}
      >
        <Form layout="vertical">
          <Form.Item label="Agente" validateStatus={createForm.formState.errors.agentId ? 'error' : ''} help={createForm.formState.errors.agentId?.message}>
            <Controller
              name="agentId"
              control={createForm.control}
              render={({ field }) => <Select {...field} options={agentOptions} placeholder="Selecione" />}
            />
          </Form.Item>
          <Form.Item label="Dia da semana">
            <Controller
              name="weekDay"
              control={createForm.control}
              render={({ field }) => <Select {...field} options={weekDayOptions} />}
            />
          </Form.Item>
          {(['startTime', 'endTime'] as const).map((f) => (
            <Form.Item
              key={f}
              label={f === 'startTime' ? 'Horário início' : 'Horário fim'}
              validateStatus={createForm.formState.errors[f] ? 'error' : ''}
              help={createForm.formState.errors[f]?.message}
            >
              <Controller
                name={f}
                control={createForm.control}
                render={({ field }) => <input {...field} placeholder="09:00:00" className="ant-input" style={{ width: '100%', padding: '4px 11px', border: '1px solid #d9d9d9', borderRadius: 6 }} />}
              />
            </Form.Item>
          ))}
        </Form>
      </Modal>

      {/* Edit modal */}
      <Modal
        title="Editar disponibilidade"
        open={!!editTarget}
        onCancel={() => setEditTarget(null)}
        onOk={updateForm.handleSubmit(handleUpdate)}
        confirmLoading={updateAvailability.isPending}
      >
        <Form layout="vertical">
          {(['startTime', 'endTime'] as const).map((f) => (
            <Form.Item
              key={f}
              label={f === 'startTime' ? 'Horário início' : 'Horário fim'}
              validateStatus={updateForm.formState.errors[f] ? 'error' : ''}
              help={updateForm.formState.errors[f]?.message}
            >
              <Controller
                name={f}
                control={updateForm.control}
                render={({ field }) => <input {...field} placeholder="09:00:00" className="ant-input" style={{ width: '100%', padding: '4px 11px', border: '1px solid #d9d9d9', borderRadius: 6 }} />}
              />
            </Form.Item>
          ))}
        </Form>
      </Modal>
    </>
  )
}
