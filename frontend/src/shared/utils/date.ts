import dayjs from 'dayjs'

export const BR_DATE_FORMAT = 'DD/MM/YYYY'
export const BR_DATE_TIME_FORMAT = 'DD/MM/YYYY HH:mm'
export const API_DATE_FORMAT = 'YYYY-MM-DD'

export type DateRange = {
  from: string
  to: string
}

export function getDefaultPeriodRange(): DateRange {
  return {
    from: dayjs().format(API_DATE_FORMAT),
    to: dayjs().add(7, 'day').format(API_DATE_FORMAT),
  }
}

export function formatDateBr(value?: string | null, emptyText = '-') {
  if (!value) return emptyText

  const parsed = dayjs(value)
  return parsed.isValid() ? parsed.format(BR_DATE_FORMAT) : emptyText
}

export function formatDateTimeBr(value?: string | null, emptyText = '-') {
  if (!value) return emptyText

  const parsed = dayjs(value)
  return parsed.isValid() ? parsed.format(BR_DATE_TIME_FORMAT) : emptyText
}

export function formatDateTimeFromParts(
  dateValue?: string | null,
  timeValue?: string | null,
  emptyText = '-',
) {
  if (!dateValue || !timeValue) return emptyText

  const formattedDate = formatDateBr(dateValue, '')
  if (!formattedDate) return emptyText

  return `${formattedDate} ${timeValue.slice(0, 5)}`
}
