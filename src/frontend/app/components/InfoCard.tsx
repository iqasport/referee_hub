import React from "react";

interface InfoCardProps {
  icon: React.ReactNode;
  label: string;
  value: string | React.ReactNode;
  iconColor?: string;
}

const InfoCard: React.FC<InfoCardProps> = ({
  icon,
  label,
  value,
  iconColor = "text-green-600",
}) => {
  return (
    <div className="bg-white rounded-lg border border-gray-200 p-4 shadow-sm">
      <div className="flex items-center gap-2 mb-2">
        <span className={`w-5 h-5 ${iconColor}`}>{icon}</span>
        <p className="text-xs text-gray-600 font-semibold">{label}</p>
      </div>
      <p className="text-sm font-semibold text-gray-900">{value}</p>
    </div>
  );
};

export default InfoCard;
