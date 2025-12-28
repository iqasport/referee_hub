import React from "react";

interface UsersIconProps {
  className?: string;
}

const UsersIcon: React.FC<UsersIconProps> = ({ className = "" }) => {
  return (
    <svg className={className} fill="currentColor" viewBox="0 0 20 20">
      <path d="M9 6a3 3 0 11-6 0 3 3 0 016 0zM9 6a3 3 0 11-6 0 3 3 0 016 0zm12 0a3 3 0 11-6 0 3 3 0 016 0zm-5 9a3 3 0 11-6 0 3 3 0 016 0zm5-1a2 2 0 10-4 0v1h4v-1z" />
    </svg>
  );
};

export default UsersIcon;
