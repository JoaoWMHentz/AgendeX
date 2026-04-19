import { Card, Col, Empty, Row, Statistic, Typography } from 'antd'
import type { ReportAggregate } from '../types'

const { Title, Text } = Typography

const aggregateLabelMap: Record<string, string> = {
  CompletedRatePercent: 'Taxa de concluídos (%)',
  CanceledRatePercent: 'Taxa de cancelados (%)',
  CompletedTotal: 'Total concluídos',
  CanceledTotal: 'Total cancelados',
}

type ReportsAggregatesProps = {
  aggregates: ReportAggregate[]
}

function formatAggregateLabel(label: string): string {
  return aggregateLabelMap[label] ?? label
}

function formatAggregateValue(aggregate: ReportAggregate): string | number {
  if (aggregate.label.endsWith('Percent')) {
    return `${Number(aggregate.value).toFixed(2)}%`
  }

  if (Number.isInteger(aggregate.value)) {
    return Number(aggregate.value)
  }

  return Number(aggregate.value).toFixed(2)
}

export function ReportsAggregates({ aggregates }: ReportsAggregatesProps) {
  return (
    <Card style={{ marginBottom: 16 }}>
      <Title level={5}>Resumo</Title>

      {aggregates.length === 0 ? (
        <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Sem dados para o resumo." />
      ) : (
        <Row gutter={[12, 12]}>
          {aggregates.map((aggregate) => (
            <Col xs={24} sm={12} md={8} lg={6} key={aggregate.label}>
              <Card size="small">
                <Statistic
                  title={<Text>{formatAggregateLabel(aggregate.label)}</Text>}
                  value={formatAggregateValue(aggregate)}
                />
              </Card>
            </Col>
          ))}
        </Row>
      )}
    </Card>
  )
}
