import { DateTime, Info } from 'luxon'

export function toDateTime(timestamp: string): DateTime {
  return DateTime.fromSQL(timestamp.slice(0, -3).trim())
}

export function getMonths(): string[] {
  const allMonths = Info.months('short')
  const monthsCopy = [...allMonths] // copy the months so we're not manipulating the original array
  const currentMonth = DateTime.local().month
  // get the previous month because the current month will never have a stat
  const prevMonthIndex = currentMonth === 1 ? 12 : currentMonth - 1
  const splicedMonths = monthsCopy.splice(prevMonthIndex)

  return splicedMonths.concat(monthsCopy)
}
