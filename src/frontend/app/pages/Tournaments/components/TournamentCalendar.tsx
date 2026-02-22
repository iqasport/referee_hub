import React, { useState, useMemo, useEffect } from "react";
import { faChevronLeft, faChevronRight } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { TournamentData } from "./TournamentsSection";
import { useNavigate } from "../../../utils/navigationUtils";

interface TournamentCalendarProps {
  tournaments: TournamentData[];
}

const BASE_DAY_NAMES = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

const MONTH_NAMES = [
  "January", "February", "March", "April", "May", "June",
  "July", "August", "September", "October", "November", "December",
];

function isoToLocalDate(iso: string): Date {
  // Take only the date part (first 10 characters "YYYY-MM-DD") in case the
  // server ever returns a datetime string like "2026-02-22T00:00:00".
  const datePart = iso.slice(0, 10);
  const [y, m, d] = datePart.split("-").map(Number);
  if (!y || !m || !d) return new Date(NaN);
  return new Date(y, m - 1, d);
}

/**
 * Returns the week start day as a JS day index (0=Sun, 1=Mon).
 * Uses Intl.Locale.weekInfo when available (modern browsers), otherwise
 * defaults to Monday (used by most of the world).
 */
function getWeekStart(): number {
  try {
    // weekInfo.firstDay follows ISO 8601: 1=Mon, 2=Tue, ..., 7=Sun
    // weekInfo is not yet in the TypeScript Intl types, so we use a typed interface
    interface IntlLocaleWithWeekInfo extends Intl.Locale {
      weekInfo?: { firstDay: number };
    }
    const locale = new Intl.Locale(navigator.language) as IntlLocaleWithWeekInfo;
    const firstDay = locale.weekInfo?.firstDay ?? 1;
    return firstDay === 7 ? 0 : firstDay; // convert ISO 7=Sun → JS 0
  } catch {
    return 1; // Monday
  }
}

// Derive ordered day names starting from week-start (computed once at module load)
const WEEK_START = getWeekStart();
const DAY_NAMES = [
  ...BASE_DAY_NAMES.slice(WEEK_START),
  ...BASE_DAY_NAMES.slice(0, WEEK_START),
];

// Year range shown in the year picker: current year ± 5
const THIS_YEAR = new Date().getFullYear();
const YEAR_OPTIONS = Array.from({ length: 11 }, (_, i) => THIS_YEAR - 5 + i);

/** Returns sorted {year, month} pairs (0-based month) for months that have at
 *  least one tournament, starting from the current month. */
function monthsWithTournaments(tournaments: TournamentData[]): Array<{ year: number; month: number }> {
  const now = new Date();
  const todayYM = now.getFullYear() * 12 + now.getMonth();
  const set = new Map<number, { year: number; month: number }>();

  tournaments.forEach((t) => {
    if (!t.startDate || !t.endDate) return;
    const start = isoToLocalDate(t.startDate);
    const end = isoToLocalDate(t.endDate);
    if (isNaN(start.getTime()) || isNaN(end.getTime())) return;

    let y = start.getFullYear();
    let mo = start.getMonth();
    const endYM = end.getFullYear() * 12 + end.getMonth();
    while (y * 12 + mo <= endYM) {
      const ym = y * 12 + mo;
      if (ym >= todayYM) set.set(ym, { year: y, month: mo });
      if (mo === 11) { mo = 0; y++; } else { mo++; }
    }
  });

  return Array.from(set.values()).sort((a, b) => (a.year * 12 + a.month) - (b.year * 12 + b.month));
}

const TournamentCalendar: React.FC<TournamentCalendarProps> = ({ tournaments }) => {
  const navigate = useNavigate();
  const today = useMemo(() => new Date(), []);
  const [year, setYear] = useState(today.getFullYear());
  const [month, setMonth] = useState(today.getMonth()); // 0-based
  const [selectedDay, setSelectedDay] = useState<number | null>(null);

  // On load, if the current month has no tournaments, jump to the nearest
  // upcoming month that does so the user sees chips immediately.
  useEffect(() => {
    if (tournaments.length === 0) return;
    const upcoming = monthsWithTournaments(tournaments);
    if (upcoming.length === 0) return;
    const currentYM = today.getFullYear() * 12 + today.getMonth();
    const hasCurrentMonth = upcoming.some((m) => m.year * 12 + m.month === currentYM);
    if (!hasCurrentMonth) {
      setYear(upcoming[0].year);
      setMonth(upcoming[0].month);
      setSelectedDay(null);
    }
  // Only trigger when the tournaments array changes (i.e. data loaded/filtered)
  }, [tournaments, today]);

  function prevMonth() {
    if (month === 0) { setYear(y => y - 1); setMonth(11); }
    else { setMonth(m => m - 1); }
    setSelectedDay(null);
  }

  function nextMonth() {
    if (month === 11) { setYear(y => y + 1); setMonth(0); }
    else { setMonth(m => m + 1); }
    setSelectedDay(null);
  }

  function goToToday() {
    setYear(today.getFullYear());
    setMonth(today.getMonth());
    setSelectedDay(null);
  }

  // Map day-of-month → tournaments active on that day
  const tournamentsByDay = useMemo(() => {
    const map = new Map<number, TournamentData[]>();

    tournaments.forEach((t) => {
      if (!t.startDate || !t.endDate) return;

      const start = isoToLocalDate(t.startDate);
      const end = isoToLocalDate(t.endDate);
      if (isNaN(start.getTime()) || isNaN(end.getTime())) return;

      const daysInMonth = new Date(year, month + 1, 0).getDate();
      for (let day = 1; day <= daysInMonth; day++) {
        const cell = new Date(year, month, day);
        if (cell >= start && cell <= end) {
          const list = map.get(day) ?? [];
          list.push(t);
          map.set(day, list);
        }
      }
    });

    return map;
  }, [tournaments, year, month]);

  const daysInMonth = new Date(year, month + 1, 0).getDate();

  // Offset of the 1st day relative to the current week-start column
  // e.g. Mon-first (WEEK_START=1): Mon→0, Tue→1, ..., Sun→6
  const firstDayOfWeek = (new Date(year, month, 1).getDay() - WEEK_START + 7) % 7;

  // Total cells rounded up to full weeks
  const totalCells = Math.ceil((firstDayOfWeek + daysInMonth) / 7) * 7;

  const isToday = (day: number) =>
    today.getFullYear() === year && today.getMonth() === month && today.getDate() === day;

  const selectedTournaments = selectedDay !== null ? (tournamentsByDay.get(selectedDay) ?? []) : [];
  const monthHasTournaments = tournamentsByDay.size > 0;

  return (
    <div className="tournament-calendar">
      {/* Header: navigation */}
      <div className="tc-header">
        <div className="tc-header-nav">
          <button className="tc-nav-btn" onClick={prevMonth} aria-label="Previous month">
            <FontAwesomeIcon icon={faChevronLeft} />
          </button>
          <button className="tc-nav-btn" onClick={nextMonth} aria-label="Next month">
            <FontAwesomeIcon icon={faChevronRight} />
          </button>
        </div>

        <div className="tc-header-selects">
          <select
            className="tc-select"
            value={month}
            onChange={(e) => { setMonth(Number(e.target.value)); setSelectedDay(null); }}
            aria-label="Select month"
          >
            {MONTH_NAMES.map((name, idx) => (
              <option key={name} value={idx}>{name}</option>
            ))}
          </select>
          <select
            className="tc-select"
            value={year}
            onChange={(e) => { setYear(Number(e.target.value)); setSelectedDay(null); }}
            aria-label="Select year"
          >
            {YEAR_OPTIONS.map((y) => (
              <option key={y} value={y}>{y}</option>
            ))}
          </select>
        </div>

        <button className="tc-today-btn" onClick={goToToday}>
          Today
        </button>
      </div>

      {/* Day-of-week row + cells */}
      <div className="tc-grid">
        {DAY_NAMES.map((name) => (
          <div key={name} className="tc-day-name">{name}</div>
        ))}

        {Array.from({ length: totalCells }).map((_, idx) => {
          const day = idx - firstDayOfWeek + 1;
          const isValid = day >= 1 && day <= daysInMonth;
          const hasTournaments = isValid && tournamentsByDay.has(day);
          const isSelected = isValid && selectedDay === day;

          return (
            <div
              key={idx}
              className={[
                "tc-cell",
                !isValid ? "tc-cell-empty" : "",
                isToday(day) ? "tc-cell-today" : "",
                hasTournaments ? "tc-cell-has-events" : "",
                isSelected ? "tc-cell-selected" : "",
              ].filter(Boolean).join(" ")}
              onClick={() => isValid && hasTournaments && setSelectedDay(isSelected ? null : day)}
              role={isValid && hasTournaments ? "button" : undefined}
              tabIndex={isValid && hasTournaments ? 0 : undefined}
              onKeyDown={(e) => {
                if ((e.key === "Enter" || e.key === " ") && isValid && hasTournaments) {
                  e.preventDefault();
                  setSelectedDay(isSelected ? null : day);
                }
              }}
              aria-label={isValid && hasTournaments ? `${MONTH_NAMES[month]} ${day}: ${tournamentsByDay.get(day)?.length ?? 0} tournament(s)` : undefined}
            >
              {isValid && (
                <>
                  <span className="tc-day-number">{day}</span>
                  {hasTournaments && (
                    <div className="tc-chip-list">
                      {(tournamentsByDay.get(day) ?? []).slice(0, 2).map((t) => (
                        <span key={t.id} className="tc-event-chip" title={t.title}>{t.title}</span>
                      ))}
                      {(tournamentsByDay.get(day)?.length ?? 0) > 2 && (
                        <span className="tc-event-more">+{(tournamentsByDay.get(day)?.length ?? 0) - 2} more</span>
                      )}
                    </div>
                  )}
                </>
              )}
            </div>
          );
        })}
      </div>

      {/* Empty-month notice */}
      {!monthHasTournaments && tournaments.length > 0 && (
        <div className="tc-empty-month">
          No tournaments in {MONTH_NAMES[month]} {year}.
          <button
            className="tc-empty-jump"
            onClick={() => {
              const upcoming = monthsWithTournaments(tournaments);
              if (upcoming.length > 0) {
                setYear(upcoming[0].year);
                setMonth(upcoming[0].month);
                setSelectedDay(null);
              }
            }}
          >
            Jump to next tournament →
          </button>
        </div>
      )}

      {/* Selected day event list */}
      {selectedDay !== null && selectedTournaments.length > 0 && (
        <div className="tc-event-panel">
          <div className="tc-event-panel-header">
            <span>{MONTH_NAMES[month]} {selectedDay}</span>
            <button
              className="tc-event-panel-close"
              onClick={() => setSelectedDay(null)}
              aria-label="Close"
            >
              ×
            </button>
          </div>
          <ul className="tc-event-list">
            {selectedTournaments.map((t) => (
              <li
                key={t.id}
                className="tc-event-item"
                onClick={() => navigate(`/tournaments/${t.id}`)}
                role="button"
                tabIndex={0}
                onKeyDown={(e) => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); navigate(`/tournaments/${t.id}`); } }}
              >
                <div className="tc-event-name">{t.title}</div>
                <div className="tc-event-meta">
                  {t.startDate === t.endDate ? t.startDate : `${t.startDate} – ${t.endDate}`}
                  {t.location && ` · ${t.location}`}
                </div>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
};

export default TournamentCalendar;
