import { Col, Row, Table, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { Bar, BarChart, CartesianGrid, Cell, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { ReportAggregate } from '../types'

const { Title } = Typography

const aggregateLabelMap: Record<string, string> = {
  CompletedRatePercent: 'Taxa de concluídos (%)',
  CanceledRatePercent: 'Taxa de cancelados (%)',
  CompletedTotal: 'Total concluídos',
  CanceledTotal: 'Total cancelados',
}

const CHART_COLORS = [
  '#4096ff', '#52c41a', '#faad14', '#ff4d4f',
  '#722ed1', '#13c2c2', '#fa8c16', '#eb2f96',
]

type ReportsAggregatesProps = {
  aggregates: ReportAggregate[]
}

function formatLabel(label: string): string {
  return aggregateLabelMap[label] ?? label
}

function formatValue(aggregate: ReportAggregate): string | number {
  if (aggregate.label.endsWith('Percent')) {
    return `${Number(aggregate.value).toFixed(2)}%`
  }
  if (Number.isInteger(aggregate.value)) {
    return Number(aggregate.value)
  }
  return Number(aggregate.value).toFixed(2)
}

const columns: ColumnsType<ReportAggregate> = [
  {
    title: 'Métrica',
    dataIndex: 'label',
    key: 'label',
    render: (label: string) => formatLabel(label),
  },
  {
    title: 'Valor',
    dataIndex: 'value',
    key: 'value',
    align: 'right',
    render: (_: number, record: ReportAggregate) => formatValue(record),
  },
]

export function ReportsAggregates({ aggregates }: ReportsAggregatesProps) {
  if (aggregates.length === 0) {
    return null
  }

  const chartData = aggregates.map((a) => ({
    name: formatLabel(a.label),
    value: Number(a.value),
  }))

  return (
    <Row gutter={16} style={{ marginBottom: 16 }}>
      <Col xs={24} md={10}>
        <Title level={5} style={{ marginBottom: 8 }}>Resumo</Title>
        <Table
          dataSource={aggregates}
          columns={columns}
          rowKey="label"
          pagination={false}
          size="small"
        />
      </Col>

      <Col xs={24} md={14}>
        <Title level={5} style={{ marginBottom: 8 }}>&nbsp;</Title>
        <ResponsiveContainer width="100%" height={aggregates.length * 52 + 40}>
          <BarChart data={chartData} layout="vertical" margin={{ left: 8, right: 24, top: 4, bottom: 4 }}>
            <CartesianGrid strokeDasharray="3 3" horizontal={false} />
            <XAxis type="number" tick={{ fontSize: 12 }} />
            <YAxis type="category" dataKey="name" width={160} tick={{ fontSize: 12 }} />
            <Tooltip formatter={(value) => value ?? 0} />
            <Bar dataKey="value" radius={[0, 4, 4, 0]}>
              {chartData.map((_, index) => (
                <Cell key={index} fill={CHART_COLORS[index % CHART_COLORS.length]} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </Col>
    </Row>
  )
}
