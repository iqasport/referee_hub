import React, { useState, useMemo } from "react";
import { faChevronLeft, faChevronRight } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { TournamentData } from "./TournamentsSection";
import { useNavigate } from "../../../utils/navigationUtils";

interface TournamentCalendarProps {
  tournaments: TournamentData[];
}

const DAY_NAMES = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

const MONTH_NAMES = [
  "January", "February", "March", "April", "May", "June",
  "July", "August", "September", "October", "November", "December",
];

function isoToLocalDate(iso: string): Date {
  // Parse YYYY-MM-DD as local date (avoids UTC-vs-local off-by-one)
  const [y, m, d] = iso.split("-").map(Number);
  return new Date(y, m - 1, d);
}

const TournamentCalendar: React.FC<TournamentCalendarProps> = ({ tournaments }) => {
  const navigate = useNavigate();
  const today = new Date();
  const [year, setYear] = useState(today.getFullYear());
  const [month, setMonth] = useState(today.getMonth()); // 0-based
  const [selectedDay, setSelectedDay] = useState<number | null>(null);

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

  // Map day-of-month → tournaments active on that day
  const tournamentsByDay = useMemo(() => {
    const map = new Map<number, TournamentData[]>();

    tournaments.forEach((t) => {
      if (!t.startDate || !t.endDate) return;

      const start = isoToLocalDate(t.startDate);
      const end = isoToLocalDate(t.endDate);

      // Iterate every day of this month
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
  // Day-of-week for the 1st of this month (0 = Sun)
  const firstDayOfWeek = new Date(year, month, 1).getDay();

  // Total cells = leading blanks + days in month, rounded up to full weeks
  const totalCells = Math.ceil((firstDayOfWeek + daysInMonth) / 7) * 7;

  const isToday = (day: number) =>
    today.getFullYear() === year && today.getMonth() === month && today.getDate() === day;

  const selectedTournaments = selectedDay !== null ? (tournamentsByDay.get(selectedDay) ?? []) : [];

  return (
    <div className="tournament-calendar">
      {/* Header: month navigation */}
      <div className="tc-header">
        <button className="tc-nav-btn" onClick={prevMonth} aria-label="Previous month">
          <FontAwesomeIcon icon={faChevronLeft} />
        </button>
        <h2 className="tc-month-title">{MONTH_NAMES[month]} {year}</h2>
        <button className="tc-nav-btn" onClick={nextMonth} aria-label="Next month">
          <FontAwesomeIcon icon={faChevronRight} />
        </button>
      </div>

      {/* Day-of-week row */}
      <div className="tc-grid">
        {DAY_NAMES.map((name) => (
          <div key={name} className="tc-day-name">{name}</div>
        ))}

        {/* Calendar cells */}
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
                    <span className="tc-dot-row">
                      {(tournamentsByDay.get(day) ?? []).slice(0, 3).map((t) => (
                        <span key={t.id} className="tc-dot" />
                      ))}
                      {(tournamentsByDay.get(day)?.length ?? 0) > 3 && (
                        <span className="tc-dot-overflow">+{(tournamentsByDay.get(day)?.length ?? 0) - 3}</span>
                      )}
                    </span>
                  )}
                </>
              )}
            </div>
          );
        })}
      </div>

      {/* Selected day popover / event list */}
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
