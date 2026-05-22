import React, { useState, useMemo } from "react";
import { useNavigationParams } from "../../utils/navigationUtils";
import { 
  useCreateTeamInviteMutation,
  useGetTeamManagementQuery,
  useRemovePlayerMutation,
  useRespondToPendingTeamInviteMutation,
  useRevokeTeamInviteMutation,
  useSetTeamAutoApprovePlayerRequestsMutation,
} from "../../store/serviceApi";
import { getErrorString } from "../../utils/errorUtils";
import TeamEditModal from "../../components/modals/TeamEditModal/TeamEditModal";
import AddManagerModal from "./AddManagerModal";
import ActionButtonPair from "../../components/ActionButtonPair";
import Toggle from "../../components/Toggle";

const TeamManagement = () => {
  const { teamId } = useNavigationParams<"teamId">();
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isAddManagerModalOpen, setIsAddManagerModalOpen] = useState(false);
  const [inviteEmail, setInviteEmail] = useState("");
  const [inviteError, setInviteError] = useState<string | null>(null);
  
  const { data: team, error: teamError, isLoading } = useGetTeamManagementQuery(
    { teamId: teamId! },
    { skip: !teamId, refetchOnMountOrArgChange: true }
  );

  const [createTeamInvite, { isLoading: isCreatingInvite }] = useCreateTeamInviteMutation();
  const [removePlayer, { isLoading: isRemovingPlayer }] = useRemovePlayerMutation();
  const [revokeTeamInvite, { isLoading: isRevokingInvite }] = useRevokeTeamInviteMutation();
  const [respondToPendingTeamInvite, { isLoading: isRespondingInvite }] = useRespondToPendingTeamInviteMutation();
  const [setTeamAutoApprovePlayerRequests, { isLoading: isUpdatingAutoApprove }] = useSetTeamAutoApprovePlayerRequestsMutation();

  const handleAutoApproveToggle = async (isEnabled: boolean) => {
    if (!teamId) return;
    try {
      await setTeamAutoApprovePlayerRequests({
        teamId,
        setTeamAutoApprovePlayerRequestsRequest: { isEnabled },
      }).unwrap();
    } catch (error: any) {
      alert(error?.data || "Failed to update auto-approve setting. Please try again.");
    }
  };

  const handleRemovePlayer = async (playerId: string, playerName: string) => {
    if (!teamId) return;
    
    if (!confirm(`Remove ${playerName} from team?`)) {
      return;
    }

    try {
      await removePlayer({ teamId, playerId }).unwrap();
    } catch (error: any) {
      alert(error?.data || "Failed to remove player. Please try again.");
    }
  };

  const handleCreateInvite = async () => {
    if (!teamId || !inviteEmail.trim()) {
      return;
    }

    setInviteError(null);

    try {
      await createTeamInvite({
        teamId,
        invitePlayerRequest: { email: inviteEmail.trim() },
      }).unwrap();
      setInviteEmail("");
    } catch (error: any) {
      setInviteError(error?.data || "Failed to create invite. Please try again.");
    }
  };

  const handleRevokeInvite = async (invitationId: string, email: string) => {
    if (!teamId) return;

    if (!confirm(`Revoke request for ${email}?`)) {
      return;
    }

    try {
      await revokeTeamInvite({ teamId, invitationId }).unwrap();
    } catch (error: any) {
      alert(error?.data || "Failed to revoke request. Please try again.");
    }
  };

  const handleRespondToPendingInvite = async (invitationId: string, approved: boolean) => {
    if (!teamId) return;

    try {
      await respondToPendingTeamInvite({
        teamId,
        invitationId,
        inviteResponseModel: { approved },
      }).unwrap();
    } catch (error: any) {
      alert(error?.data || "Failed to update player request. Please try again.");
    }
  };

  // Memoize the team object to prevent unnecessary re-renders and form resets
  const teamForModal = useMemo(() => {
    if (!team) return undefined;
    return {
      teamId: team.teamId,
      name: team.name,
      city: team.city,
      state: team.state,
      country: team.country,
      status: team.status,
      groupAffiliation: team.groupAffiliation,
      joinedAt: team.joinedAt,
      socialAccounts: team.socialAccounts,
      description: team.description,
      contactEmail: team.contactEmail,
      logoUri: team.logoUri,
    };
  }, [team]);

  if (isLoading) {
    return (
      <div className="m-auto w-full my-10 px-4 xl:w-3/4 xl:px-0">
        <p>Loading team management...</p>
      </div>
    );
  }

  if (teamError) {
    return (
      <div className="m-auto w-full my-10 px-4 xl:w-3/4 xl:px-0">
        <p className="text-red-500">Error: {getErrorString(teamError)}</p>
      </div>
    );
  }

  if (!team) {
    return (
      <div className="m-auto w-full my-10 px-4 xl:w-3/4 xl:px-0">
        <p>Team not found</p>
      </div>
    );
  }

  return (
    <div className="m-auto w-full my-10 px-4 xl:w-3/4 xl:px-0">
      {/* Team Header */}
      <div className="flex items-center justify-between mb-8">
        <div className="flex items-center">
          {team.logoUri && (
            <img
              src={team.logoUri}
              alt={`${team.name} logo`}
              className="w-20 h-20 object-cover rounded mr-4"
            />
          )}
          <div>
            <h1 className="text-3xl font-bold">{team.name}</h1>
            <p className="text-gray-600">
              {team.city}
              {team.state && `, ${team.state}`}, {team.country}
            </p>
          </div>
        </div>
        
        {/* Actions Button */}
        <div className="relative">
          <button
            className="bg-green text-white px-6 py-3 rounded-lg font-semibold hover:bg-green-700 transition"
            onClick={() => setIsEditModalOpen(true)}
          >
            Edit Team
          </button>
        </div>
      </div>

      {/* Edit Team Modal */}
      {isEditModalOpen && team && (
        <TeamEditModal
          open={isEditModalOpen}
          showClose={true}
          teamId={team.teamId}
          team={teamForModal}
          onClose={() => setIsEditModalOpen(false)}
        />
      )}

      {/* Team Managers Section */}
      <div className="bg-gray-100 rounded-lg p-6 mb-8">
        <h2 className="text-2xl font-semibold mb-4 border-b-2 border-green pb-2">
          Team Managers
        </h2>
        {team.managers && team.managers.length > 0 ? (
          <div className="space-y-2">
            {team.managers.map((manager) => (
              <div key={manager.id} className="flex items-center justify-between bg-white p-3 rounded">
                <div>
                  <span className="font-medium">{manager.name}</span>
                  {manager.email && (
                    <span className="ml-2 text-gray-600 text-sm">({manager.email})</span>
                  )}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-gray-500">No managers assigned</p>
        )}
        <button
          className="mt-4 text-green font-semibold hover:underline"
          onClick={() => setIsAddManagerModalOpen(true)}
        >
          + Add Manager
        </button>
      </div>

      {/* Add Manager Modal */}
      {isAddManagerModalOpen && teamId && (
        <AddManagerModal
          teamId={teamId}
          onClose={() => setIsAddManagerModalOpen(false)}
        />
      )}

      {/* Team Players/Members Section */}
      <div className="bg-gray-100 rounded-lg p-6 mb-8">
        <h2 className="text-2xl font-semibold mb-4 border-b-2 border-green pb-2">
          Team Members
        </h2>
        {team.members && team.members.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full bg-white rounded">
              <thead className="bg-gray-200">
                <tr>
                  <th className="px-4 py-2 text-left">Name</th>
                  {team.groupAffiliation === "national" && (
                    <th className="px-4 py-2 text-left">Primary Team</th>
                  )}
                  <th className="px-4 py-2 text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {team.members.map((member) => (
                  <tr key={member.userId} className="border-t">
                    <td className="px-4 py-3 font-medium">{member.name}</td>
                    {team.groupAffiliation === "national" && (
                      <td className="px-4 py-3 text-gray-600">
                        {member.primaryTeamName || "—"}
                      </td>
                    )}
                    <td className="px-4 py-3 text-right">
                      <button
                        className="text-red-600 hover:underline disabled:opacity-50"
                        onClick={() => handleRemovePlayer(member.userId, member.name)}
                        disabled={isRemovingPlayer}
                      >
                        Remove
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="text-gray-500">No members</p>
        )}
      </div>

      {/* Pending Requests Section */}
      <div className="bg-gray-100 rounded-lg p-6">
        {/* Auto-approve toggle */}
        <div className="mb-4 rounded border border-gray-300 bg-white p-4">
          <div className="flex items-center justify-between gap-4">
            <div className="flex items-center gap-2">
              <p className="font-semibold text-gray-900">Auto-approve incoming player requests</p>
              <span
                className="cursor-help text-gray-400 border border-gray-400 rounded-full w-4 h-4 inline-flex items-center justify-center text-xs font-bold"
                title="If this is on, incoming requests are approved automatically and all pending requests are approved."
                aria-label="Auto-approve help"
              >
                ?
              </span>
            </div>
            <div className="flex items-center">
              <Toggle
                name="autoApprovePlayerRequests"
                checked={Boolean(team.autoApprovePlayerRequests)}
                onChange={(e) => handleAutoApproveToggle(e.target.checked)}
              />
              {isUpdatingAutoApprove && <span className="ml-2 text-sm font-medium text-gray-700">Saving...</span>}
            </div>
          </div>
        </div>
        <div className="flex items-center justify-between mb-4 border-b-2 border-green pb-2 gap-4">
          <h2 className="text-2xl font-semibold">Pending Requests</h2>
          <div className="flex gap-2 w-full max-w-lg">
            <input
              type="email"
              className="flex-1 px-3 py-2 border border-gray-300 rounded"
              placeholder="player@example.com"
              value={inviteEmail}
              onChange={(event) => setInviteEmail(event.target.value)}
            />
            <button
              className="bg-green text-white px-4 py-2 rounded font-semibold hover:bg-green-700 disabled:opacity-50"
              onClick={handleCreateInvite}
              disabled={isCreatingInvite || !inviteEmail.trim()}
            >
              {isCreatingInvite ? "Inviting..." : "Invite Player"}
            </button>
          </div>
        </div>
        {inviteError && <p className="mb-4 text-sm text-red-600">{inviteError}</p>}
        {team.pendingInvites && team.pendingInvites.length > 0 ? (
          <div className="space-y-2">
            {team.pendingInvites.map((invite) => (
              <div key={invite.invitationId} className="bg-white p-3 rounded flex items-center justify-between gap-4">
                <div>
                  <p className="font-medium">{invite.email}</p>
                  <p className="text-sm text-gray-600">
                    {invite.requiresManagerDecision ? "Requested" : "Invited"} {invite.createdAt ? new Date(invite.createdAt).toLocaleDateString() : "recently"}
                    {invite.invitedByName ? ` by ${invite.invitedByName}` : ""}
                  </p>
                </div>
                {invite.requiresManagerDecision ? (
                  <ActionButtonPair
                    onAccept={() => handleRespondToPendingInvite(invite.invitationId, true)}
                    onDecline={() => handleRespondToPendingInvite(invite.invitationId, false)}
                    isLoading={isRespondingInvite}
                    size="sm"
                  />
                ) : (
                  <button
                    className="text-red-600 hover:underline disabled:opacity-50"
                    onClick={() => handleRevokeInvite(invite.invitationId, invite.email)}
                    disabled={isRevokingInvite}
                  >
                    Revoke
                  </button>
                )}
              </div>
            ))}
          </div>
        ) : (
          <p className="text-gray-500">No pending requests</p>
        )}
      </div>

      <div className="bg-gray-100 rounded-lg p-6 mt-8">
        <h2 className="text-2xl font-semibold mb-4 border-b-2 border-green pb-2">Player Activity</h2>
        {team.playerHistory && team.playerHistory.length > 0 ? (
          <div className="space-y-2">
            {team.playerHistory.map((activity, index) => (
              <div key={`${activity.createdAt || "unknown"}-${index}`} className="bg-white p-3 rounded">
                <p className="font-medium">
                  {activity.activityType === "inviteCreated" && (
                    activity.userId
                      ? `${activity.userName || activity.email || "A player"} requested to join`
                      : `Invite sent to ${activity.email || "unknown"}`
                  )}
                  {activity.activityType === "inviteRevoked" && `Invite revoked for ${activity.email || "unknown"}`}
                  {activity.activityType === "inviteAccepted" && `${activity.userName || activity.email || "A user"} joined team`}
                  {activity.activityType === "inviteDeclined" && (
                    activity.userId && activity.initiatorName && activity.userName && activity.initiatorName !== activity.userName
                      ? `Join request declined for ${activity.userName || activity.email || "a user"}`
                      : `${activity.userName || activity.email || "A user"} declined invitation`
                  )}
                  {activity.activityType === "playerRemoved" && `${activity.userName || activity.email || "A user"} removed from team`}
                </p>
                <p className="text-sm text-gray-600">
                  {activity.createdAt ? new Date(activity.createdAt).toLocaleString() : "Unknown time"}
                  {activity.initiatorName ? ` by ${activity.initiatorName}` : ""}
                </p>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-gray-500">No player activity yet</p>
        )}
      </div>
    </div>
  );
};

export default TeamManagement;
