import { DateTime, Info } from "luxon";

export function toDateTime(timestamp: string): DateTime {
  if (!timestamp) return DateTime.local();

  return DateTime.fromISO(timestamp, {zone: "utc"});
}

export function getMonths(): string[] {
  const allMonths = Info.months("short");
  const monthsCopy = [...allMonths]; // copy the months so we're not manipulating the original array
  const currentMonth = DateTime.local().month;
  const splicedMonths = monthsCopy.splice(currentMonth);
  return splicedMonths.concat(monthsCopy);
}
