import React from "react";

type StatusType = "pending" | "approved" | "declined" | "rejected" | string;

interface StatusBadgeProps {
  status: StatusType;
  className?: string;
}

const getStatusStyle = (status: StatusType) => {
  switch (status.toLowerCase()) {
    case "pending":
      return { backgroundColor: "#fef3c7", color: "#92400e" };
    case "approved":
      return { backgroundColor: "#d1fae5", color: "#065f46" };
    case "declined":
    case "rejected":
      return { backgroundColor: "#fee2e2", color: "#991b1b" };
    default:
      return { backgroundColor: "#f3f4f6", color: "#374151" };
  }
};

const StatusBadge: React.FC<StatusBadgeProps> = ({ status, className = "" }) => {
  const style = getStatusStyle(status);
  const displayStatus = status.charAt(0).toUpperCase() + status.slice(1).toLowerCase();

  return (
    <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${className}`} style={style}>
      {displayStatus}
    </span>
  );
};

export default StatusBadge;
