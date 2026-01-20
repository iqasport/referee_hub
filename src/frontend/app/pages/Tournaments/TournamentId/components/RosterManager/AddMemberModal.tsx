import React, { useState, useMemo } from "react";
import { Dialog, DialogPanel, DialogTitle } from "@headlessui/react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faMagnifyingGlass, faXmark } from "@fortawesome/free-solid-svg-icons";

interface TeamMember {
  userId: string;
  name: string;
}

interface AddMemberModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  columnType: "players" | "coaches" | "staff";
  availableMembers: TeamMember[];
  existingMemberIds: Set<string>;
  onAddMember: (member: TeamMember, number?: string, gender?: string) => void;
  isLoading?: boolean;
}

const AddMemberModal: React.FC<AddMemberModalProps> = ({
  isOpen,
  onClose,
  title,
  columnType,
  availableMembers,
  existingMemberIds,
  onAddMember,
  isLoading = false,
}) => {
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedMember, setSelectedMember] = useState<TeamMember | null>(null);
  const [playerNumber, setPlayerNumber] = useState("");
  const [playerGender, setPlayerGender] = useState("");

  // Filter available members who aren't already in any roster column
  const filteredMembers = useMemo(() => {
    return availableMembers
      .filter((member) => !existingMemberIds.has(member.userId))
      .filter(
        (member) =>
          searchQuery === "" || member.name.toLowerCase().includes(searchQuery.toLowerCase())
      );
  }, [availableMembers, existingMemberIds, searchQuery]);

  const handleSelectMember = (member: TeamMember) => {
    setSelectedMember(member);
    // Reset player fields when selecting new member
    if (columnType !== "players") {
      handleAddMember(member);
    }
  };

  const handleAddMember = (member: TeamMember = selectedMember!) => {
    if (!member) return;

    if (columnType === "players") {
      onAddMember(member, playerNumber, playerGender);
    } else {
      onAddMember(member);
    }

    // Reset state
    setSelectedMember(null);
    setPlayerNumber("");
    setPlayerGender("");
    setSearchQuery("");
    onClose();
  };

  const handleClose = () => {
    setSelectedMember(null);
    setPlayerNumber("");
    setPlayerGender("");
    setSearchQuery("");
    onClose();
  };

  const getHeaderColor = () => {
    switch (columnType) {
      case "players":
        return "text-blue-600";
      case "coaches":
        return "text-green-600";
      case "staff":
        return "text-purple-600";
      default:
        return "text-gray-600";
    }
  };

  return (
    <Dialog open={isOpen} as="div" className="relative z-50" onClose={handleClose}>
      <div
        className="fixed inset-0"
        style={{ backgroundColor: "rgba(0, 0, 0, 0.3)" }}
        aria-hidden="true"
      />
      <div className="fixed inset-0 flex items-center justify-center p-4 overflow-y-auto">
        <DialogPanel
          className="relative w-full rounded-lg bg-white p-6 shadow-xl"
          style={{ maxWidth: "28rem" }}
        >
          <button
            onClick={handleClose}
            className="absolute text-gray-400 hover:text-gray-600"
            style={{ top: "16px", right: "16px", padding: "4px" }}
          >
            <FontAwesomeIcon icon={faXmark} style={{ fontSize: "18px" }} />
          </button>

          <DialogTitle as="h3" className={`text-lg font-semibold mb-4 ${getHeaderColor()}`}>
            {title}
          </DialogTitle>

          {/* Search Input */}
          <div className="relative mb-4">
            <span
              className="absolute text-gray-400"
              style={{ left: "12px", top: "50%", transform: "translateY(-50%)" }}
            >
              <FontAwesomeIcon icon={faMagnifyingGlass} style={{ fontSize: "14px" }} />
            </span>
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Search team members..."
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          {/* Members List */}
          <div className="max-h-60 overflow-y-auto mb-4 border border-gray-200 rounded-lg">
            {isLoading ? (
              <div className="p-4 text-center text-gray-500">Loading team members...</div>
            ) : filteredMembers.length === 0 ? (
              <div className="p-4 text-center text-gray-500">
                {searchQuery ? "No matching members found" : "All team members have been assigned"}
              </div>
            ) : (
              <ul className="divide-y divide-gray-100">
                {filteredMembers.map((member) => (
                  <li key={member.userId}>
                    <button
                      type="button"
                      onClick={() => handleSelectMember(member)}
                      className={`w-full px-4 py-3 text-left hover:bg-gray-50 transition-colors flex items-center justify-between ${
                        selectedMember?.userId === member.userId
                          ? "bg-blue-50 border-l-4 border-blue-500"
                          : ""
                      }`}
                    >
                      <span className="font-medium text-gray-900">{member.name}</span>
                      {selectedMember?.userId === member.userId && (
                        <span className="text-xs text-blue-600 font-medium">Selected</span>
                      )}
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          {/* Player-specific fields */}
          {columnType === "players" && selectedMember && (
            <div className="bg-blue-50 rounded-lg p-4 mb-4">
              <p className="text-sm font-medium text-blue-800 mb-3">
                Adding: {selectedMember.name}
              </p>
              <div className="flex" style={{ gap: "12px" }}>
                <div className="flex-1">
                  <label className="block text-xs text-blue-700 mb-1 font-medium">
                    Jersey Number
                  </label>
                  <input
                    type="text"
                    value={playerNumber}
                    onChange={(e) => setPlayerNumber(e.target.value)}
                    placeholder="#"
                    maxLength={3}
                    className="w-full px-3 py-2 border border-blue-300 rounded-lg"
                  />
                </div>
                <div className="flex-1">
                  <label className="block text-xs text-blue-700 mb-1 font-medium">Gender</label>
                  <input
                    type="text"
                    value={playerGender}
                    onChange={(e) => setPlayerGender(e.target.value)}
                    placeholder="Enter gender"
                    className="w-full px-3 py-2 border border-blue-300 rounded-lg"
                  />
                </div>
              </div>
            </div>
          )}

          {/* Action Buttons */}
          <div className="flex justify-end" style={{ gap: 8 }}>
            <button
              type="button"
              onClick={handleClose}
              className="px-4 py-2 text-sm font-bold text-gray-700 bg-white border border-gray-300"
              style={{ borderRadius: 4 }}
            >
              Cancel
            </button>
            {columnType === "players" && selectedMember && (
              <button
                type="button"
                onClick={() => handleAddMember()}
                className="px-4 py-2 text-sm font-bold text-white bg-green-600"
                style={{ backgroundColor: "#16a34a", borderRadius: 4, minWidth: 100 }}
              >
                Add Player
              </button>
            )}
          </div>
        </DialogPanel>
      </div>
    </Dialog>
  );
};

export default AddMemberModal;
