import dayjs, { type Dayjs } from 'dayjs'
import { TimePicker } from 'antd'
import type { TimePickerProps } from 'antd'

export type TimePickerFieldProps = Omit<TimePickerProps, 'value' | 'onChange' | 'format'> & {
  value: string | null | undefined
  onChange: (value: string) => void
  format?: string
  minuteStep?: number
  popupClassName?: string
}

export function TimePickerField({
  value,
  onChange,
  format = 'HH:mm',
  minuteStep = 15,
  popupClassName = 'compact-time-picker-popup',
  style,
  ...rest
}: TimePickerFieldProps) {
  const inputFormat = format === 'HH:mm' ? { format: 'HH:mm', type: 'mask' as const } : format
  const parsedValue: Dayjs | null = value ? dayjs(value, format) : null

  return (
    <TimePicker
      {...rest}
      style={{ width: '100%', ...style }}
      format={inputFormat}
      minuteStep={minuteStep}
      popupClassName={popupClassName}
      value={parsedValue}
      onChange={(nextValue) => onChange(nextValue ? nextValue.format(format) : '')}
    />
  )
}
