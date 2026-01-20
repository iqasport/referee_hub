import React from "react";

interface BackIconProps {
  className?: string;
}

const BackIcon: React.FC<BackIconProps> = ({ className = "" }) => {
  return (
    <svg
      className={className}
      width="20"
      height="20"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      style={{ filter: "drop-shadow(0 1px 2px rgba(0, 0, 0, 0.5))" }}
    >
      <path d="M19 12H5M12 19l-7-7 7-7" />
    </svg>
  );
};

export default BackIcon;
