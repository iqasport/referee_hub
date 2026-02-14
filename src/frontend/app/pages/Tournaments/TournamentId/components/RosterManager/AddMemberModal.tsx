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
  onAddMembers: (members: TeamMember[]) => void;
  isLoading?: boolean;
}

const AddMemberModal: React.FC<AddMemberModalProps> = ({
  isOpen,
  onClose,
  title,
  columnType,
  availableMembers,
  existingMemberIds,
  onAddMembers,
  isLoading = false,
}) => {
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedMembers, setSelectedMembers] = useState<Set<string>>(new Set());

  // Filter available members who aren't already in any roster column
  const filteredMembers = useMemo(() => {
    return availableMembers
      .filter((member) => !existingMemberIds.has(member.userId))
      .filter(
        (member) =>
          searchQuery === "" || member.name.toLowerCase().includes(searchQuery.toLowerCase())
      );
  }, [availableMembers, existingMemberIds, searchQuery]);

  const handleToggleMember = (member: TeamMember) => {
    setSelectedMembers((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(member.userId)) {
        newSet.delete(member.userId);
      } else {
        newSet.add(member.userId);
      }
      return newSet;
    });
  };

  const handleAddMembers = () => {
    if (selectedMembers.size === 0) return;

    const membersToAdd = availableMembers.filter((m) => selectedMembers.has(m.userId));
    onAddMembers(membersToAdd);

    // Reset state
    setSelectedMembers(new Set());
    setSearchQuery("");
    onClose();
  };

  const handleClose = () => {
    setSelectedMembers(new Set());
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
                {filteredMembers.map((member) => {
                  const isSelected = selectedMembers.has(member.userId);
                  return (
                    <li key={member.userId}>
                      <button
                        type="button"
                        onClick={() => handleToggleMember(member)}
                        className={`w-full px-4 py-3 text-left hover:bg-gray-50 transition-colors flex items-center justify-between ${
                          isSelected ? "bg-blue-50 border-l-4 border-blue-500" : ""
                        }`}
                      >
                        <div className="flex items-center gap-3">
                          <input
                            type="checkbox"
                            checked={isSelected}
                            readOnly
                            className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                          />
                          <span className="font-medium text-gray-900">{member.name}</span>
                        </div>
                      </button>
                    </li>
                  );
                })}
              </ul>
            )}
          </div>

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
            <button
              type="button"
              onClick={handleAddMembers}
              disabled={selectedMembers.size === 0}
              className="px-4 py-2 text-sm font-bold text-white"
              style={{
                backgroundColor: selectedMembers.size === 0 ? "#9ca3af" : "#16a34a",
                borderRadius: 4,
                minWidth: 100,
                cursor: selectedMembers.size === 0 ? "not-allowed" : "pointer",
              }}
            >
              Add {selectedMembers.size > 0 ? `(${selectedMembers.size})` : ""}
            </button>
          </div>
        </DialogPanel>
      </div>
    </Dialog>
  );
};

export default AddMemberModal;
