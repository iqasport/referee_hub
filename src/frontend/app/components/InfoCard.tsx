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
    <div className=" rounded-lg border border-gray-200 p-4 shadow-sm" style={{backgroundColor:"#f7fafc"}}>
      <div className="flex items-center gap-2 mb-2">
        <span className={`flex-shrink-0 ${iconColor}`} style={{ width: '1.25rem', height: '1.25rem', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>{icon}</span>
        <p className="text-xs text-gray-600 font-semibold">{label}</p>
      </div>
      <p className="text-sm font-semibold text-gray-900">{value}</p>
    </div>
  );
};

export default InfoCard;
