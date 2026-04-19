import { useState } from 'react'
import {
  Table, Button, Tag, Space, Modal, Form, Input, Select,
  Typography, message, DatePicker,
} from 'antd'
import { PlusOutlined } from '@ant-design/icons'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import dayjs from 'dayjs'
import {
  useAppointments, useCreateAppointment, useConfirmAppointment,
  useRejectAppointment, useCancelAppointment, useCompleteAppointment,
  useReassignAppointment,
} from './useAppointments'
import {
  appointmentStatusLabel, appointmentStatusColor,
  AppointmentStatus, type Appointment, type AppointmentFilters,
} from './types'
import { useServiceTypes } from '@/features/service-types/useServiceTypes'
import { useAgents } from '@/features/users/useUsers'
import { useAuthStore } from '@/features/auth/authStore'
import { Roles } from '@/shared/constants/roles'
import { extractApiError } from '@/shared/utils/apiError'
import type { ColumnsType } from 'antd/es/table'

const { Title } = Typography
const { RangePicker } = DatePicker

type ModalType = 'create' | 'reject' | 'reassign' | 'complete' | null

const createSchema = z.object({
  title: z.string().min(1, 'Título obrigatório'),
  description: z.string().optional(),
  serviceTypeId: z.number({ message: 'Selecione o tipo' }),
  agentId: z.string().uuid('Selecione um agente'),
  date: z.string().min(1, 'Data obrigatória'),
  time: z.string().regex(/^([01]\d|2[0-3]):[0-5]\d:[0-5]\d$/, 'Formato HH:mm:ss'),
  notes: z.string().optional(),
})
type CreateForm = z.infer<typeof createSchema>

export function AppointmentsPage() {
  const { user: me } = useAuthStore()
  const isAdmin = me?.role === Roles.Administrator
  const isAgent = me?.role === Roles.Agent
  const isClient = me?.role === Roles.Client

  const [filters, setFilters] = useState<AppointmentFilters>({})
  const [modal, setModal] = useState<{ type: ModalType; appointment?: Appointment }>({ type: null })
  const [rejectReason, setRejectReason] = useState('')
  const [completeSummary, setCompleteSummary] = useState('')
  const [reassignAgentId, setReassignAgentId] = useState<string | undefined>()

  const { data = [], isLoading } = useAppointments(filters)
  const { data: serviceTypes = [] } = useServiceTypes()
  const { data: agents = [] } = useAgents()

  const createAppointment = useCreateAppointment()
  const confirmAppointment = useConfirmAppointment()
  const rejectAppointment = useRejectAppointment()
  const cancelAppointment = useCancelAppointment()
  const completeAppointment = useCompleteAppointment()
  const reassignAppointment = useReassignAppointment()

  const createForm = useForm<CreateForm>({
    resolver: zodResolver(createSchema),
  })

  const closeModal = () => {
    setModal({ type: null })
    setRejectReason('')
    setCompleteSummary('')
    setReassignAgentId(undefined)
  }

  const handleCreate = async (values: CreateForm) => {
    try {
      await createAppointment.mutateAsync(values)
      message.success('Agendamento criado com sucesso')
      closeModal()
      createForm.reset()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleConfirm = (id: string) => {
    Modal.confirm({
      title: 'Confirmar agendamento?',
      onOk: async () => {
        await confirmAppointment.mutateAsync(id)
        message.success('Agendamento confirmado')
      },
    })
  }

  const handleCancel = (id: string) => {
    Modal.confirm({
      title: 'Cancelar agendamento?',
      okButtonProps: { danger: true },
      onOk: async () => {
        await cancelAppointment.mutateAsync(id)
        message.success('Agendamento cancelado')
      },
    })
  }

  const handleReject = async () => {
    if (!modal.appointment || !rejectReason.trim()) return
    try {
      await rejectAppointment.mutateAsync({ id: modal.appointment.id, reason: rejectReason })
      message.success('Agendamento rejeitado')
      closeModal()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleComplete = async () => {
    if (!modal.appointment) return
    try {
      await completeAppointment.mutateAsync({ id: modal.appointment.id, summary: completeSummary || undefined })
      message.success('Agendamento concluído')
      closeModal()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const handleReassign = async () => {
    if (!modal.appointment || !reassignAgentId) return
    try {
      await reassignAppointment.mutateAsync({ id: modal.appointment.id, agentId: reassignAgentId })
      message.success('Agendamento reatribuído')
      closeModal()
    } catch (err) {
      message.error(extractApiError(err))
    }
  }

  const isFuture = (a: Appointment) => {
    const dt = dayjs(`${a.date}T${a.time}`)
    return dt.isAfter(dayjs())
  }

  const renderActions = (record: Appointment) => {
    const buttons: React.ReactNode[] = []
    const { status } = record

    if (isAgent) {
      if (status === AppointmentStatus.PendingConfirmation) {
        buttons.push(
          <Button key="confirm" size="small" type="primary" onClick={() => handleConfirm(record.id)}>Confirmar</Button>,
          <Button key="reject" size="small" danger onClick={() => setModal({ type: 'reject', appointment: record })}>Rejeitar</Button>,
        )
      }
      if (status === AppointmentStatus.Confirmed && !isFuture(record)) {
        buttons.push(
          <Button key="complete" size="small" onClick={() => setModal({ type: 'complete', appointment: record })}>Concluir</Button>,
        )
      }
    }

    if (isClient) {
      const canCancel = (status === AppointmentStatus.PendingConfirmation || status === AppointmentStatus.Confirmed) && isFuture(record)
      if (canCancel) {
        buttons.push(<Button key="cancel" size="small" danger onClick={() => handleCancel(record.id)}>Cancelar</Button>)
      }
    }

    if (isAdmin) {
      buttons.push(
        <Button key="reassign" size="small" onClick={() => setModal({ type: 'reassign', appointment: record })}>Reatribuir</Button>,
      )
      if (status === AppointmentStatus.PendingConfirmation || status === AppointmentStatus.Confirmed) {
        buttons.push(<Button key="cancel" size="small" danger onClick={() => handleCancel(record.id)}>Cancelar</Button>)
      }
    }

    return <Space>{buttons}</Space>
  }

  const columns: ColumnsType<Appointment> = [
    { title: 'Título', dataIndex: 'title', ellipsis: true },
    { title: 'Cliente', dataIndex: 'clientName' },
    { title: 'Agente', dataIndex: 'agentName' },
    { title: 'Tipo', dataIndex: 'serviceTypeDescription' },
    {
      title: 'Data / Hora',
      render: (_: unknown, r: Appointment) => `${r.date} ${r.time.slice(0, 5)}`,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      render: (s: number) => (
        <Tag color={appointmentStatusColor[s as keyof typeof appointmentStatusColor]}>
          {appointmentStatusLabel[s as keyof typeof appointmentStatusLabel]}
        </Tag>
      ),
    },
    { title: 'Ações', render: (_: unknown, r: Appointment) => renderActions(r) },
  ]

  const statusOptions = Object.entries(appointmentStatusLabel).map(([value, label]) => ({
    value: Number(value),
    label,
  }))

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Agendamentos</Title>
        {isClient && (
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setModal({ type: 'create' })}>
            Novo agendamento
          </Button>
        )}
      </div>

      {/* Filters */}
      <Space style={{ marginBottom: 16 }} wrap>
        <Select
          allowClear
          placeholder="Status"
          style={{ width: 160 }}
          options={statusOptions}
          onChange={(v) => setFilters((f) => ({ ...f, status: v }))}
        />
        <Select
          allowClear
          placeholder="Tipo de serviço"
          style={{ width: 180 }}
          options={serviceTypes.map((s) => ({ value: s.id, label: s.description }))}
          onChange={(v) => setFilters((f) => ({ ...f, serviceTypeId: v }))}
        />
        {isAdmin && (
          <Select
            allowClear
            placeholder="Agente"
            style={{ width: 180 }}
            options={agents.map((a) => ({ value: a.id, label: a.name }))}
            onChange={(v) => setFilters((f) => ({ ...f, agentId: v }))}
          />
        )}
        <RangePicker
          onChange={(dates) => {
            setFilters((f) => ({
              ...f,
              from: dates?.[0]?.format('YYYY-MM-DD'),
              to: dates?.[1]?.format('YYYY-MM-DD'),
            }))
          }}
        />
      </Space>

      <Table<Appointment>
        columns={columns}
        dataSource={data}
        rowKey="id"
        loading={isLoading}
        scroll={{ x: 900 }}
      />

      {/* Create modal */}
      <Modal
        title="Novo agendamento"
        open={modal.type === 'create'}
        onCancel={() => { closeModal(); createForm.reset() }}
        onOk={createForm.handleSubmit(handleCreate)}
        confirmLoading={createAppointment.isPending}
        width={520}
      >
        <Form layout="vertical">
          <Form.Item required label="Título" validateStatus={createForm.formState.errors.title ? 'error' : ''} help={createForm.formState.errors.title?.message}>
            <Controller name="title" control={createForm.control} render={({ field }) => <Input {...field} />} />
          </Form.Item>
          <Form.Item label="Descrição">
            <Controller name="description" control={createForm.control} render={({ field }) => <Input.TextArea {...field} rows={2} />} />
          </Form.Item>
          <Form.Item required label="Tipo de serviço" validateStatus={createForm.formState.errors.serviceTypeId ? 'error' : ''} help={createForm.formState.errors.serviceTypeId?.message}>
            <Controller
              name="serviceTypeId"
              control={createForm.control}
              render={({ field }) => (
                <Select {...field} options={serviceTypes.map((s) => ({ value: s.id, label: s.description }))} placeholder="Selecione" />
              )}
            />
          </Form.Item>
          <Form.Item required label="Agente" validateStatus={createForm.formState.errors.agentId ? 'error' : ''} help={createForm.formState.errors.agentId?.message}>
            <Controller
              name="agentId"
              control={createForm.control}
              render={({ field }) => (
                <Select {...field} options={agents.map((a) => ({ value: a.id, label: a.name }))} placeholder="Selecione" />
              )}
            />
          </Form.Item>
          <Form.Item required label="Data" validateStatus={createForm.formState.errors.date ? 'error' : ''} help={createForm.formState.errors.date?.message}>
            <Controller
              name="date"
              control={createForm.control}
              render={({ field }) => (
                <DatePicker
                  style={{ width: '100%' }}
                  value={field.value ? dayjs(field.value) : null}
                  onChange={(d) => field.onChange(d ? d.format('YYYY-MM-DD') : '')}
                  disabledDate={(d) => d.isBefore(dayjs(), 'day')}
                />
              )}
            />
          </Form.Item>
          <Form.Item required label="Horário (HH:mm:ss)" validateStatus={createForm.formState.errors.time ? 'error' : ''} help={createForm.formState.errors.time?.message}>
            <Controller name="time" control={createForm.control} render={({ field }) => <Input {...field} placeholder="09:00:00" />} />
          </Form.Item>
          <Form.Item label="Observações">
            <Controller name="notes" control={createForm.control} render={({ field }) => <Input.TextArea {...field} rows={2} />} />
          </Form.Item>
        </Form>
      </Modal>

      {/* Reject modal */}
      <Modal
        title="Rejeitar agendamento"
        open={modal.type === 'reject'}
        onCancel={closeModal}
        onOk={handleReject}
        confirmLoading={rejectAppointment.isPending}
        okButtonProps={{ danger: true }}
      >
        <Form layout="vertical">
          <Form.Item label="Motivo da rejeição" required>
            <Input.TextArea rows={3} value={rejectReason} onChange={(e) => setRejectReason(e.target.value)} />
          </Form.Item>
        </Form>
      </Modal>

      {/* Complete modal */}
      <Modal
        title="Concluir agendamento"
        open={modal.type === 'complete'}
        onCancel={closeModal}
        onOk={handleComplete}
        confirmLoading={completeAppointment.isPending}
      >
        <Form layout="vertical">
          <Form.Item label="Resumo do atendimento (opcional)">
            <Input.TextArea rows={3} value={completeSummary} onChange={(e) => setCompleteSummary(e.target.value)} />
          </Form.Item>
        </Form>
      </Modal>

      {/* Reassign modal */}
      <Modal
        title="Reatribuir agendamento"
        open={modal.type === 'reassign'}
        onCancel={closeModal}
        onOk={handleReassign}
        confirmLoading={reassignAppointment.isPending}
        okButtonProps={{ disabled: !reassignAgentId }}
      >
        <Form layout="vertical">
          <Form.Item label="Novo agente" required>
            <Select
              style={{ width: '100%' }}
              options={agents.map((a) => ({ value: a.id, label: a.name }))}
              value={reassignAgentId}
              onChange={setReassignAgentId}
              placeholder="Selecione um agente"
            />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}
