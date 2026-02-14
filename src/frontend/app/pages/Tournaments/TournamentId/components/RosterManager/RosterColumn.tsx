import React from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus, faXmark } from "@fortawesome/free-solid-svg-icons";

export interface RosterMember {
  userId: string;
  userName: string;
  number?: string;
  gender?: string;
}

interface RosterColumnProps {
  title: string;
  type: "players" | "coaches" | "staff";
  members: RosterMember[];
  onAddClick: () => void;
  onRemove: (userId: string) => void;
  onUpdateMember?: (userId: string, field: "number" | "gender", value: string) => void;
  showNumberAndGender?: boolean;
  disabled?: boolean;
}

const RosterColumn: React.FC<RosterColumnProps> = ({
  title,
  type,
  members,
  onAddClick,
  onRemove,
  onUpdateMember,
  showNumberAndGender = false,
  disabled = false,
}) => {
  const getBgColor = () => {
    switch (type) {
      case "players":
        return "bg-blue-50 border-blue-200";
      case "coaches":
        return "bg-green-50 border-green-200";
      case "staff":
        return "bg-purple-50 border-purple-200";
      default:
        return "bg-gray-50 border-gray-200";
    }
  };

  const getHeaderColor = () => {
    switch (type) {
      case "players":
        return "bg-blue-100 text-blue-800";
      case "coaches":
        return "bg-green-100 text-green-800";
      case "staff":
        return "bg-purple-100 text-purple-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  return (
    <div className={`flex flex-col border rounded-lg overflow-hidden ${getBgColor()}`}>
      {/* Header */}
      <div className={`px-4 py-3 ${getHeaderColor()} flex justify-between items-center`}>
        <h3 className="font-semibold text-sm uppercase tracking-wide">
          {title} ({members.length})
        </h3>
        <button
          type="button"
          onClick={onAddClick}
          disabled={disabled}
          className={`rounded-full transition-colors ${
            disabled
              ? "bg-gray-300 text-gray-500 cursor-not-allowed"
              : "bg-white text-gray-700 hover:bg-gray-100 shadow-sm"
          }`}
          style={{ padding: "6px", width: "28px", height: "28px", display: "flex", alignItems: "center", justifyContent: "center", color: "#16a34a" }}
          title={`Add ${title.toLowerCase()}`}
        >
          <FontAwesomeIcon icon={faPlus} style={{ fontSize: "14px" }} />
        </button>
      </div>

      {/* Members List */}
      <div className="flex-1 p-3 overflow-y-auto" style={{ minHeight: "200px", maxHeight: "400px" }}>
        {members.length === 0 ? (
          <div className="text-center text-gray-500 text-sm py-8">
            No {title.toLowerCase()} added yet.
            <br />
            <span className="text-xs">Click + to add members</span>
          </div>
        ) : (
          members.map((member) => (
            <div
              key={member.userId}
              className="bg-white rounded-lg p-3 shadow-sm border border-gray-100 hover:shadow-md transition-shadow mb-2"
            >
              <div className="flex items-start justify-between">
                <div className="flex-1" style={{ minWidth: 0 }}>
                  <span className="font-medium text-gray-900 text-sm block" style={{ overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                    {member.userName}
                  </span>
                  
                  {/* Number and Gender fields for players */}
                  {showNumberAndGender && (
                    <div className="flex mt-2" style={{ gap: "8px" }}>
                      <div className="flex-1">
                        <label className="block text-xs text-gray-500 mb-1">Number</label>
                        <input
                          type="text"
                          value={member.number || ""}
                          onChange={(e) => onUpdateMember?.(member.userId, "number", e.target.value)}
                          disabled={disabled}
                          placeholder="#"
                          className="w-full px-2 py-1 text-sm border border-gray-300 rounded disabled:bg-gray-100"
                          maxLength={3}
                        />
                      </div>
                      <div className="flex-1">
                        <label className="block text-xs text-gray-500 mb-1">Observations</label>
                        <input
                          type="text"
                          value={member.gender || ""}
                          onChange={(e) => onUpdateMember?.(member.userId, "gender", e.target.value)}
                          disabled={disabled}
                          placeholder="Notes"
                          className="w-full px-2 py-1 text-sm border border-gray-300 rounded disabled:bg-gray-100"
                          maxLength={50}
                        />
                      </div>
                    </div>
                  )}
                </div>

                {/* Remove Button */}
                <button
                  type="button"
                  onClick={() => onRemove(member.userId)}
                  disabled={disabled}
                  className={`rounded transition-colors ${
                    disabled
                      ? "text-gray-400 cursor-not-allowed"
                      : "text-gray-400 hover:text-red-500 hover:bg-red-50"
                  }`}
                  style={{ padding: "4px", marginLeft: "8px" }}
                  title={`Remove ${member.userName}`}
                >
                  <FontAwesomeIcon icon={faXmark} style={{ fontSize: "12px" }} />
                </button>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default RosterColumn;
