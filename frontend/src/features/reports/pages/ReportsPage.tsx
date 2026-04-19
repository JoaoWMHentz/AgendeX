import { Typography } from 'antd'
import { ReportsFilters } from '../components/ReportsFilters'
import { ReportsTable } from '../components/ReportsTable'
import { useReportsPageController } from '../hooks/useReportsPageController'

const { Title } = Typography

export function ReportsPage() {
  const {
    filters,
    tableFilters,
    rows,
    isLoading,
    exportCsvLoading,
    exportXlsxLoading,
    showClientFilter,
    showAgentFilter,
    statusOptions,
    reportTypeOptions,
    serviceTypeOptions,
    clientOptions,
    agentOptions,
    updateFilters,
    handleSortChange,
    handleSearch,
    handleExportCsv,
    handleExportXlsx,
  } = useReportsPageController()

  return (
    <>
      <Title level={4} style={{ marginTop: 0 }}>
        Relatórios
      </Title>

      <ReportsFilters
        filters={filters}
        reportTypeOptions={reportTypeOptions}
        statusOptions={statusOptions}
        serviceTypeOptions={serviceTypeOptions}
        clientOptions={clientOptions}
        agentOptions={agentOptions}
        showClientFilter={showClientFilter}
        showAgentFilter={showAgentFilter}
        loading={isLoading}
        exportCsvLoading={exportCsvLoading}
        exportXlsxLoading={exportXlsxLoading}
        onFiltersChange={updateFilters}
        onSearch={handleSearch}
        onExportCsv={handleExportCsv}
        onExportXlsx={handleExportXlsx}
      />

      <ReportsTable
        rows={rows}
        loading={isLoading}
        filters={tableFilters}
        onSortChange={handleSortChange}
      />
    </>
  )
}
