import React from "react";

interface EmptyStateProps {
  message: string;
  icon?: React.ReactNode;
  className?: string;
}

const EmptyState: React.FC<EmptyStateProps> = ({ message, icon, className = "" }) => {
  return (
    <div className={`text-center text-gray-600 py-8 ${className}`}>
      {icon && <div className="mb-2 flex justify-center">{icon}</div>}
      <p>{message}</p>
    </div>
  );
};

export default EmptyState;
