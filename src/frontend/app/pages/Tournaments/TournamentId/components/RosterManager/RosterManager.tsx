import React, { useState, useCallback, useMemo, useEffect } from "react";
import RosterColumn, { RosterMember } from "./RosterColumn";
import AddMemberModal from "./AddMemberModal";
import {
  useGetTeamMembersQuery,
  useUpdateParticipantRosterMutation,
  useGetParticipantsQuery,
  TeamMemberViewModel,
  RosterPlayerModel,
  RosterStaffModel,
} from "../../../../../store/serviceApi";

export interface RosterData {
  players: RosterMember[];
  coaches: RosterMember[];
  staff: RosterMember[];
}

export interface RosterManagerProps {
  tournamentId: string;
  teamId: string;
  teamName: string;
  ngb: string;
  initialRoster?: RosterData;
  onRosterSaved?: () => void;
  disabled?: boolean;
}

type ColumnType = "players" | "coaches" | "staff";

const RosterManager: React.FC<RosterManagerProps> = ({
  tournamentId,
  teamId,
  teamName,
  ngb,
  initialRoster,
  onRosterSaved,
  disabled = false,
}) => {
  // Roster state
  const [roster, setRoster] = useState<RosterData>({
    players: [],
    coaches: [],
    staff: [],
  });

  // Modal state
  const [modalState, setModalState] = useState<{
    isOpen: boolean;
    columnType: ColumnType;
  }>({
    isOpen: false,
    columnType: "players",
  });

  // Track if there are unsaved changes
  const [hasChanges, setHasChanges] = useState(false);

  // Track if we've loaded initial data
  const [initialLoaded, setInitialLoaded] = useState(false);

  // Fetch participants to get existing roster data
  const { data: participantsData } = useGetParticipantsQuery({
    tournamentId,
  });

  // Load initial roster from participants data
  useEffect(() => {
    if (participantsData && !initialLoaded) {
      const teamParticipant = participantsData.find((p) => p.teamId === teamId);
      if (teamParticipant) {
        setRoster({
          players: (teamParticipant.players || []).map((p) => ({
            userId: p.userId || "",
            userName: p.userName || "Unknown",
            number: p.number || "",
            gender: p.gender || "",
          })),
          coaches: (teamParticipant.coaches || []).map((c) => ({
            userId: c.userId || "",
            userName: c.userName || "Unknown",
          })),
          staff: (teamParticipant.staff || []).map((s) => ({
            userId: s.userId || "",
            userName: s.userName || "Unknown",
          })),
        });
        setInitialLoaded(true);
      }
    }
  }, [participantsData, teamId, initialLoaded]);

  // Fetch team members
  const { data: teamMembersData, isLoading: isLoadingMembers } = useGetTeamMembersQuery({
    ngb,
    teamId,
    skipPaging: true,
  });

  // Update roster mutation
  const [updateRoster, { isLoading: isSaving }] = useUpdateParticipantRosterMutation();

  // Convert team members to the format expected by the modal
  const availableTeamMembers = useMemo(() => {
    if (!teamMembersData?.items) return [];
    return teamMembersData.items.map((member: TeamMemberViewModel) => ({
      userId: member.userId || "",
      name: member.name || "Unknown",
    }));
  }, [teamMembersData]);

  // Get all existing member IDs across all columns
  const existingMemberIds = useMemo(() => {
    const ids = new Set<string>();
    roster.players.forEach((m) => ids.add(m.userId));
    roster.coaches.forEach((m) => ids.add(m.userId));
    roster.staff.forEach((m) => ids.add(m.userId));
    return ids;
  }, [roster]);

  // Handle opening the add member modal
  const handleOpenAddModal = useCallback((columnType: ColumnType) => {
    setModalState({ isOpen: true, columnType });
  }, []);

  // Handle closing the modal
  const handleCloseModal = useCallback(() => {
    setModalState((prev) => ({ ...prev, isOpen: false }));
  }, []);

  // Handle adding a member to a column
  const handleAddMember = useCallback(
    (
      member: { userId: string; name: string },
      number?: string,
      gender?: string
    ) => {
      const newMember: RosterMember = {
        userId: member.userId,
        userName: member.name,
        number: number || "",
        gender: gender || "",
      };

      setRoster((prev) => ({
        ...prev,
        [modalState.columnType]: [...prev[modalState.columnType], newMember],
      }));
      setHasChanges(true);
    },
    [modalState.columnType]
  );

  // Handle removing a member from a column
  const handleRemoveMember = useCallback((columnType: ColumnType, userId: string) => {
    setRoster((prev) => ({
      ...prev,
      [columnType]: prev[columnType].filter((m) => m.userId !== userId),
    }));
    setHasChanges(true);
  }, []);

  // Handle updating player number/gender
  const handleUpdateMember = useCallback(
    (userId: string, field: "number" | "gender", value: string) => {
      setRoster((prev) => ({
        ...prev,
        players: prev.players.map((player) =>
          player.userId === userId ? { ...player, [field]: value } : player
        ),
      }));
      setHasChanges(true);
    },
    []
  );

  // Handle saving the roster
  const handleSaveRoster = async () => {
    try {
      // Convert roster to API format
      const players: RosterPlayerModel[] = roster.players.map((p) => ({
        userId: p.userId,
        number: p.number || null,
        gender: p.gender || null,
      }));

      const coaches: RosterStaffModel[] = roster.coaches.map((c) => ({
        userId: c.userId,
      }));

      const staff: RosterStaffModel[] = roster.staff.map((s) => ({
        userId: s.userId,
      }));

      await updateRoster({
        tournamentId,
        teamId,
        updateRosterModel: {
          players,
          coaches,
          staff,
        },
      }).unwrap();

      setHasChanges(false);
      onRosterSaved?.();
      alert("Roster saved successfully!");
    } catch (error) {
      console.error("Failed to save roster:", error);
      alert("Failed to save roster. Please try again.");
    }
  };

  // Get modal title based on column type
  const getModalTitle = () => {
    switch (modalState.columnType) {
      case "players":
        return "Add Player";
      case "coaches":
        return "Add Coach";
      case "staff":
        return "Add Staff Member";
      default:
        return "Add Member";
    }
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
      {/* Header */}
      <div className="bg-gray-50 px-6 py-4 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-lg font-semibold text-gray-900">Team Roster</h2>
            <p className="text-sm text-gray-600 mt-1">
              Manage the roster for <span className="font-medium">{teamName}</span>
            </p>
          </div>
          <button
            type="button"
            onClick={handleSaveRoster}
            disabled={!hasChanges || isSaving || disabled}
            className="px-4 py-2 text-sm font-bold"
            style={{
              borderRadius: 4,
              minWidth: 110,
              background: hasChanges && !isSaving && !disabled ? '#16a34a' : '#e5e7eb',
              border: hasChanges && !isSaving && !disabled ? '1px solid #15803d' : '1px solid #d1d5db',
              color: hasChanges && !isSaving && !disabled ? '#fff' : '#6b7280',
              cursor: !hasChanges || isSaving || disabled ? 'not-allowed' : 'pointer',
              opacity: isSaving ? 0.7 : 1,
              transition: 'background 0.2s, color 0.2s',
            }}
          >
            {isSaving ? "Saving..." : hasChanges ? "Save Roster" : "Saved"}
          </button>
        </div>
        {hasChanges && (
          <p className="text-xs text-amber-600 mt-2">
            * You have unsaved changes. Click &quot;Save Roster&quot; to save your changes.
          </p>
        )}
      </div>

      {/* Roster Columns */}
      <div className="p-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <RosterColumn
            title="Players"
            type="players"
            members={roster.players}
            onAddClick={() => handleOpenAddModal("players")}
            onRemove={(userId) => handleRemoveMember("players", userId)}
            onUpdateMember={handleUpdateMember}
            showNumberAndGender={true}
            disabled={disabled}
          />
          <RosterColumn
            title="Coaches"
            type="coaches"
            members={roster.coaches}
            onAddClick={() => handleOpenAddModal("coaches")}
            onRemove={(userId) => handleRemoveMember("coaches", userId)}
            disabled={disabled}
          />
          <RosterColumn
            title="Staff"
            type="staff"
            members={roster.staff}
            onAddClick={() => handleOpenAddModal("staff")}
            onRemove={(userId) => handleRemoveMember("staff", userId)}
            disabled={disabled}
          />
        </div>

        {/* Summary */}
        <div className="mt-6 pt-4 border-t border-gray-200">
          <div className="flex flex-wrap gap-4 text-sm text-gray-600">
            <span>
              <span className="font-medium text-blue-600">{roster.players.length}</span> Players
            </span>
            <span>
              <span className="font-medium text-green-600">{roster.coaches.length}</span> Coaches
            </span>
            <span>
              <span className="font-medium text-purple-600">{roster.staff.length}</span> Staff
            </span>
            <span className="ml-auto">
              Total: <span className="font-medium">
                {roster.players.length + roster.coaches.length + roster.staff.length}
              </span> members
            </span>
          </div>
        </div>
      </div>

      {/* Add Member Modal */}
      <AddMemberModal
        isOpen={modalState.isOpen}
        onClose={handleCloseModal}
        title={getModalTitle()}
        columnType={modalState.columnType}
        availableMembers={availableTeamMembers}
        existingMemberIds={existingMemberIds}
        onAddMember={handleAddMember}
        isLoading={isLoadingMembers}
      />
    </div>
  );
};

export default RosterManager;
