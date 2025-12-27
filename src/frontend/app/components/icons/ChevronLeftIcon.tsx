import React from "react";

interface ChevronLeftIconProps {
  className?: string;
}

const ChevronLeftIcon: React.FC<ChevronLeftIconProps> = ({ className = "" }) => {
  return (
    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24" className={className}>
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
    </svg>
  );
};

export default ChevronLeftIcon;
