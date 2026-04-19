import dayjs from 'dayjs'
import { Table, Tag } from 'antd'
import type { TableColumnsType, TableProps } from 'antd'
import {
  appointmentStatusColor,
  appointmentStatusLabel,
  type AppointmentStatusValue,
} from '@/features/appointments/types'
import {
  ReportSortDirection,
  type ReportFilters,
  type ReportRow,
  type ReportSortBy,
  type ReportSortDirectionValue,
} from '../types'

type SortOrder = 'ascend' | 'descend' | null

type ReportsTableProps = {
  rows: ReportRow[]
  loading: boolean
  filters: ReportFilters
  onSortChange: (sortBy?: ReportSortBy, order?: SortOrder) => void
}

function resolveSortOrder(
  filters: ReportFilters,
  field: ReportSortBy,
): SortOrder {
  if (filters.sortBy !== field) return null

  return filters.sortDirection === ReportSortDirection.Asc ? 'ascend' : 'descend'
}

export function ReportsTable({ rows, loading, filters, onSortChange }: ReportsTableProps) {
  const columns: TableColumnsType<ReportRow> = [
    {
      title: 'Cliente',
      dataIndex: 'clientName',
      sorter: true,
      sortOrder: resolveSortOrder(filters, 'clientName'),
    },
    {
      title: 'Agente',
      dataIndex: 'agentName',
      sorter: true,
      sortOrder: resolveSortOrder(filters, 'agentName'),
    },
    {
      title: 'Data',
      dataIndex: 'appointmentDate',
      sorter: true,
      sortOrder: resolveSortOrder(filters, 'appointmentDate'),
      render: (value: string) => dayjs(value).format('DD/MM/YYYY'),
    },
    {
      title: 'Hora',
      dataIndex: 'appointmentTime',
      sorter: true,
      sortOrder: resolveSortOrder(filters, 'appointmentTime'),
      render: (value: string) => value.slice(0, 5),
    },
    {
      title: 'Tipo de serviço',
      dataIndex: 'serviceType',
      sorter: true,
      sortOrder: resolveSortOrder(filters, 'serviceType'),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      sorter: true,
      sortOrder: resolveSortOrder(filters, 'status'),
      render: (status: number) => (
        <Tag color={appointmentStatusColor[status as AppointmentStatusValue]}>
          {appointmentStatusLabel[status as AppointmentStatusValue]}
        </Tag>
      ),
    },
    {
      title: 'Criado em',
      dataIndex: 'createdAt',
      sorter: true,
      sortOrder: resolveSortOrder(filters, 'createdAt'),
      render: (value: string) => dayjs(value).format('DD/MM/YYYY HH:mm'),
    },
    {
      title: 'Confirmado em',
      dataIndex: 'confirmedAt',
      sorter: true,
      sortOrder: resolveSortOrder(filters, 'confirmedAt'),
      render: (value?: string) => (value ? dayjs(value).format('DD/MM/YYYY HH:mm') : '-'),
    },
    {
      title: 'Cancelado em',
      dataIndex: 'canceledAt',
      sorter: true,
      sortOrder: resolveSortOrder(filters, 'canceledAt'),
      render: (value?: string) => (value ? dayjs(value).format('DD/MM/YYYY HH:mm') : '-'),
    },
    {
      title: 'Motivo',
      dataIndex: 'rejectionOrCancellationReason',
      sorter: true,
      sortOrder: resolveSortOrder(filters, 'reason'),
      render: (value?: string) => value ?? '-',
      ellipsis: true,
    },
  ]

  const handleTableChange: TableProps<ReportRow>['onChange'] = (_pagination, _tableFilters, sorter) => {
    const currentSorter = Array.isArray(sorter) ? sorter[0] : sorter

    const sortBy = currentSorter?.field as ReportSortBy | undefined
    const order = currentSorter?.order ?? null

    onSortChange(sortBy, order)
  }

  return (
    <Table<ReportRow>
      rowKey="appointmentId"
      columns={columns}
      dataSource={rows}
      loading={loading}
      onChange={handleTableChange}
      scroll={{ x: 1400 }}
    />
  )
}

export function orderToDirection(order?: SortOrder): ReportSortDirectionValue | undefined {
  if (!order) return undefined
  return order === 'ascend' ? ReportSortDirection.Asc : ReportSortDirection.Desc
}
