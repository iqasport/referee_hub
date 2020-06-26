import { DateTime, Info } from 'luxon'

export function toDateTime(timestamp: string): DateTime {
  return DateTime.fromSQL(timestamp.slice(0, -3).trim())
}

export function getMonths(): string[] {
  const allMonths = Info.months('short')
  const monthsCopy = [...allMonths] // copy the months so we're not manipulating the original array
  const currentMonth = DateTime.local().monthShort
  const currentMonthIndex = allMonths.indexOf(currentMonth)
  const splicedMonths = monthsCopy.splice(currentMonthIndex + 1)

  return splicedMonths.concat(monthsCopy)
}