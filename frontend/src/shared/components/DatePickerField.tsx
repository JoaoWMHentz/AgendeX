import dayjs, { type Dayjs } from 'dayjs'
import { DatePicker } from 'antd'
import type { DatePickerProps } from 'antd'

export type DatePickerFieldProps = Omit<DatePickerProps, 'value' | 'onChange' | 'format'> & {
  value: string | null | undefined
  onChange: (value: string) => void
  displayFormat?: string
  valueFormat?: string
}

export function DatePickerField({
  value,
  onChange,
  displayFormat = 'DD/MM/YYYY',
  valueFormat = 'YYYY-MM-DD',
  style,
  ...rest
}: DatePickerFieldProps) {
  const parsedValue: Dayjs | null = value ? dayjs(value, valueFormat) : null

  return (
    <DatePicker
      {...rest}
      style={{ width: '100%', ...style }}
      format={displayFormat}
      value={parsedValue}
      onChange={(nextValue) => onChange(nextValue ? nextValue.format(valueFormat) : '')}
    />
  )
}
