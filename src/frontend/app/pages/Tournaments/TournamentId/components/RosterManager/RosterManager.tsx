import React, { useState, useCallback, useMemo, useEffect } from "react";
import RosterColumn, { RosterMember } from "./RosterColumn";
import PlayersTable from "./PlayersTable";
import AddMemberModal from "./AddMemberModal";
import CustomAlert from "../../../../../components/CustomAlert";
import { useAlert } from "../../../../../hooks/useAlert";
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

export interface TeamInfo {
  teamId: string;
  teamName: string;
  ngb: string;
}

export interface RosterManagerProps {
  tournamentId: string;
  teams: TeamInfo[];
  onRosterSaved?: () => void;
  disabled?: boolean;
}

type ColumnType = "players" | "coaches" | "staff";

const RosterManager: React.FC<RosterManagerProps> = ({
  tournamentId,
  teams,
  onRosterSaved,
  disabled = false,
}) => {
  // Alert state
  const { alertState, showAlert, hideAlert } = useAlert();

  // Selected team state
  const [selectedTeamId, setSelectedTeamId] = useState<string>(teams[0]?.teamId || "");
  
  // Get current team info
  const currentTeam = teams.find(t => t.teamId === selectedTeamId) || teams[0];

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
    if (participantsData && selectedTeamId) {
      const teamParticipant = participantsData.find((p) => p.teamId === selectedTeamId);
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
      } else {
        // No roster data for this team yet, reset to empty
        setRoster({
          players: [],
          coaches: [],
          staff: [],
        });
        setInitialLoaded(true);
      }
      setHasChanges(false);
    }
  }, [participantsData, selectedTeamId]);

  // Fetch team members
  const { data: teamMembersData, isLoading: isLoadingMembers } = useGetTeamMembersQuery({
    ngb: currentTeam?.ngb || "",
    teamId: selectedTeamId,
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
        teamId: selectedTeamId,
        updateRosterModel: {
          players,
          coaches,
          staff,
        },
      }).unwrap();

      setHasChanges(false);
      onRosterSaved?.();
      showAlert("Roster saved successfully!", "success");
    } catch (error) {
      console.error("Failed to save roster:", error);
      showAlert("Failed to save roster. Please try again.", "error");
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
    <>
      {alertState.isVisible && (
        <CustomAlert
          message={alertState.message}
          type={alertState.type}
          onClose={hideAlert}
        />
      )}
      <div style={{ backgroundColor: '#fff', borderRadius: '0.75rem', boxShadow: '0 1px 2px 0 rgba(0, 0, 0, 0.05)', border: '1px solid #e5e7eb', overflow: 'hidden' }}>
        {/* Header */}
      <div style={{ backgroundColor: '#f9fafb', padding: '1rem 1.5rem', borderBottom: '1px solid #e5e7eb' }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: '1rem' }}>
          <div>
            <h2 style={{ fontSize: '1.125rem', fontWeight: '600', color: '#111827' }}>Team Roster</h2>
            <div style={{ fontSize: '0.875rem', color: '#4b5563', marginTop: '0.25rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <span>Manage the roster for</span>
              {teams.length > 1 ? (
                <select
                  value={selectedTeamId}
                  onChange={(e) => setSelectedTeamId(e.target.value)}
                  disabled={disabled || hasChanges}
                  style={{
                    fontWeight: '500',
                    padding: '0.25rem 0.5rem',
                    borderRadius: '0.375rem',
                    border: '1px solid #d1d5db',
                    backgroundColor: '#fff',
                    cursor: hasChanges ? 'not-allowed' : 'pointer',
                    opacity: hasChanges ? 0.6 : 1,
                  }}
                  title={hasChanges ? 'Save or discard changes before switching teams' : 'Select a team'}
                >
                  {teams.map((team) => (
                    <option key={team.teamId} value={team.teamId}>
                      {team.teamName}
                    </option>
                  ))}
                </select>
              ) : (
                <span style={{ fontWeight: '500' }}>{currentTeam?.teamName}</span>
              )}
            </div>
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
              border: hasChanges && !isSaving && !disabled ? '1px solid #16a34a' : '1px solid #d1d5db',
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
          <PlayersTable
            members={roster.players}
            onAddClick={() => handleOpenAddModal("players")}
            onRemove={(userId) => handleRemoveMember("players", userId)}
            onUpdateMember={handleUpdateMember}
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
    </>
  );
};

export default RosterManager;
