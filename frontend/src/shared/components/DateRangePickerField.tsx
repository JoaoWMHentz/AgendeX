import dayjs, { type Dayjs } from 'dayjs'
import { DatePicker } from 'antd'
import type { RangePickerProps } from 'antd/es/date-picker'

const { RangePicker } = DatePicker

export type DateRangePickerFieldProps = Omit<RangePickerProps, 'value' | 'onChange' | 'format'> & {
  from?: string
  to?: string
  onChange: (value: { from?: string; to?: string }) => void
  displayFormat?: string
  valueFormat?: string
}

export function DateRangePickerField({
  from,
  to,
  onChange,
  displayFormat = 'DD/MM/YYYY',
  valueFormat = 'YYYY-MM-DD',
  ...rest
}: DateRangePickerFieldProps) {
  const parsedValue: [Dayjs | null, Dayjs | null] = [
    from ? dayjs(from, valueFormat) : null,
    to ? dayjs(to, valueFormat) : null,
  ]

  return (
    <RangePicker
      {...rest}
      format={displayFormat}
      value={parsedValue}
      onChange={(dates) =>
        onChange({
          from: dates?.[0]?.format(valueFormat),
          to: dates?.[1]?.format(valueFormat),
        })
      }
    />
  )
}
