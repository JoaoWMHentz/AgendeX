import dayjs from 'dayjs'
import { Controller } from 'react-hook-form'
import { Button, Col, DatePicker, Form, Input, Modal, Row, Select, Spin, Table, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import {
  useClientNewAppointmentController,
  type AvailableSlotRow,
} from '../hooks/useClientNewAppointmentController'

const { Title, Text } = Typography

export function ClientNewAppointmentPage() {
  const {
    form,
    selectedDate,
    setSelectedDate,
    selectedSlot,
    openConfirm,
    closeConfirm,
    handleConfirm,
    confirmLoading,
    serviceTypeOptions,
    availableSlots,
    slotsLoading,
  } = useClientNewAppointmentController()

  const columns: ColumnsType<AvailableSlotRow> = [
    {
      title: 'Horário',
      key: 'time',
      render: (_: unknown, row: AvailableSlotRow) => `${row.startTime} – ${row.endTime}`,
    },
    { title: 'Atendente', dataIndex: 'agentName' },
    {
      title: 'Ação',
      key: 'action',
      render: (_: unknown, row: AvailableSlotRow) => (
        <Button type="primary" size="small" onClick={() => openConfirm(row)}>
          Marcar
        </Button>
      ),
    },
  ]

  return (
    <>
      <Title level={4} style={{ marginBottom: 24 }}>
        Novo Agendamento
      </Title>

      <Form layout="vertical">
        <Row gutter={16}>
          <Col xs={24} sm={8}>
            <Form.Item
              required
              label="Título"
              validateStatus={form.formState.errors.title ? 'error' : ''}
              help={form.formState.errors.title?.message}
            >
              <Controller
                name="title"
                control={form.control}
                render={({ field }) => <Input {...field} placeholder="Ex: Consulta de suporte" />}
              />
            </Form.Item>
          </Col>

          <Col xs={24} sm={8}>
            <Form.Item
              required
              label="Tipo de serviço"
              validateStatus={form.formState.errors.serviceTypeId ? 'error' : ''}
              help={form.formState.errors.serviceTypeId?.message}
            >
              <Controller
                name="serviceTypeId"
                control={form.control}
                render={({ field }) => (
                  <Select
                    {...field}
                    options={serviceTypeOptions}
                    placeholder="Selecione o tipo"
                    style={{ width: '100%' }}
                  />
                )}
              />
            </Form.Item>
          </Col>

          <Col xs={24} sm={8}>
            <Form.Item required label="Data">
              <DatePicker
                style={{ width: '100%' }}
                disabledDate={(date) => date.isBefore(dayjs(), 'day')}
                onChange={(date) => setSelectedDate(date ? date.format('YYYY-MM-DD') : undefined)}
              />
            </Form.Item>
          </Col>
        </Row>

        <Row>
          <Col span={24}>
            <Form.Item label="Descrição">
              <Controller
                name="description"
                control={form.control}
                render={({ field }) => <Input.TextArea {...field} rows={2} />}
              />
            </Form.Item>
          </Col>
        </Row>
      </Form>

      {selectedDate && (
        <>
          <Title level={5} style={{ marginBottom: 12 }}>
            Horários disponíveis
          </Title>
          <Spin spinning={slotsLoading}>
            <Table<AvailableSlotRow>
              columns={columns}
              dataSource={availableSlots}
              rowKey="key"
              pagination={false}
              locale={{ emptyText: 'Nenhum horário disponível para este dia' }}
            />
          </Spin>
        </>
      )}

      <Modal
        open={!!selectedSlot}
        title="Confirmar agendamento"
        okText="Confirmar"
        cancelText="Cancelar"
        confirmLoading={confirmLoading}
        onCancel={closeConfirm}
        onOk={handleConfirm}
      >
        {selectedSlot && (
          <Text>
            Confirma agendamento com <Text strong>{selectedSlot.agentName}</Text> no dia{' '}
            <Text strong>{dayjs(selectedSlot.date).format('DD/MM/YYYY')}</Text> às{' '}
            <Text strong>{selectedSlot.startTime}</Text>?
          </Text>
        )}
      </Modal>
    </>
  )
}
