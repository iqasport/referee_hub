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
    <div style={{ backgroundColor: '#fff', borderRadius: '0.75rem', boxShadow: '0 1px 2px 0 rgba(0, 0, 0, 0.05)', border: '1px solid #e5e7eb', overflow: 'hidden' }}>
      {/* Header */}
      <div style={{ backgroundColor: '#f9fafb', padding: '1rem 1.5rem', borderBottom: '1px solid #e5e7eb' }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: '1rem' }}>
          <div>
            <h2 style={{ fontSize: '1.125rem', fontWeight: '600', color: '#111827' }}>Team Roster</h2>
            <p style={{ fontSize: '0.875rem', color: '#4b5563', marginTop: '0.25rem' }}>
              Manage the roster for <span style={{ fontWeight: '500' }}>{teamName}</span>
            </p>
          </div>
          <button
            type="button"
            onClick={handleSaveRoster}
            disabled={!hasChanges || isSaving || disabled}
            style={{
              padding: '0.5rem 1rem',
              fontSize: '0.875rem',
              fontWeight: 'bold',
              borderRadius: '4px',
              minWidth: '110px',
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
          <p style={{ fontSize: '0.75rem', color: '#d97706', marginTop: '0.5rem' }}>
            * You have unsaved changes. Click &quot;Save Roster&quot; to save your changes.
          </p>
        )}
      </div>

      {/* Roster Columns */}
      <div style={{ padding: '1.25rem' }}>
        <div style={{ display: 'grid', gridTemplateColumns: window.innerWidth >= 1024 ? 'repeat(3, 1fr)' : '1fr', gap: '0.875rem' }}>
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
        <div style={{ marginTop: '1.5rem', paddingTop: '1rem', borderTop: '1px solid #e5e7eb' }}>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '1rem', fontSize: '0.875rem', color: '#4b5563' }}>
            <span>
              <span style={{ fontWeight: '500', color: '#2563eb' }}>{roster.players.length}</span> Players
            </span>
            <span>
              <span style={{ fontWeight: '500', color: '#16a34a' }}>{roster.coaches.length}</span> Coaches
            </span>
            <span>
              <span style={{ fontWeight: '500', color: '#9333ea' }}>{roster.staff.length}</span> Staff
            </span>
            <span style={{ marginLeft: 'auto' }}>
              Total: <span style={{ fontWeight: '500' }}>
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
