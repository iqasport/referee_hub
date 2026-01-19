import React from "react";

interface ActionButtonPairProps {
  onAccept: () => void;
  onDecline: () => void;
  isLoading?: boolean;
  acceptLabel?: string;
  declineLabel?: string;
  loadingLabel?: string;
  size?: "sm" | "md";
}

const ActionButtonPair: React.FC<ActionButtonPairProps> = ({
  onAccept,
  onDecline,
  isLoading = false,
  acceptLabel = "Accept",
  declineLabel = "Decline",
  loadingLabel = "...",
  size = "md",
}) => {
  const sizeClasses = size === "sm" ? "py-1.5 px-3 text-sm" : "py-2 px-4";

  return (
    <div className="flex gap-2">
      <button
        onClick={onAccept}
        disabled={isLoading}
        className={`flex-1 font-semibold rounded-lg transition-colors ${sizeClasses}`}
        style={{
          backgroundColor: "#16a34a",
          color: "white",
          opacity: isLoading ? 0.5 : 1,
          cursor: isLoading ? "not-allowed" : "pointer",
        }}
      >
        {isLoading ? loadingLabel : acceptLabel}
      </button>
      <button
        onClick={onDecline}
        disabled={isLoading}
        className={`flex-1 bg-red-600 hover:bg-red-700 text-white font-semibold rounded-lg transition-colors ${sizeClasses}`}
        style={{
          opacity: isLoading ? 0.5 : 1,
          cursor: isLoading ? "not-allowed" : "pointer",
        }}
      >
        {isLoading ? loadingLabel : declineLabel}
      </button>
    </div>
  );
};

export default ActionButtonPair;
