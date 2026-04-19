import { Table, Typography } from 'antd'
import { useServiceTypes } from './useServiceTypes'
import type { ServiceType } from './types'

const { Title } = Typography

const columns = [
  { title: 'ID', dataIndex: 'id', width: 80 },
  { title: 'Descrição', dataIndex: 'description' },
]

export function ServiceTypesPage() {
  const { data = [], isLoading } = useServiceTypes()

  return (
    <>
      <Title level={4} style={{ margin: '0 0 16px' }}>
        Tipos de Serviço
      </Title>
      <Table<ServiceType>
        columns={columns}
        dataSource={data}
        rowKey="id"
        loading={isLoading}
        pagination={false}
      />
    </>
  )
}
