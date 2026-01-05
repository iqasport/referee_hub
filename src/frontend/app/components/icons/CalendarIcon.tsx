import React from "react";

interface CalendarIconProps {
  className?: string;
}

const CalendarIcon: React.FC<CalendarIconProps> = ({ className = "" }) => {
  return (
    <svg className={className} fill="currentColor" viewBox="0 0 20 20">
      <path d="M6 2a1 1 0 00-1 1v2H4a2 2 0 00-2 2v2h16V7a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v2H7V3a1 1 0 00-1-1zm0 5a2 2 0 002 2h8a2 2 0 002-2H6z" />
    </svg>
  );
};

export default CalendarIcon;
