import React, { useState, useMemo } from "react";
import { useNavigationParams } from "../../utils/navigationUtils";
import { 
  useGetTeamDetailsQuery,
  useAddTeamManagerToTeamMutation,
  useDeleteTeamPlayerMutation 
} from "../../store/serviceApi";
import { getErrorString } from "../../utils/errorUtils";
import TeamEditModal from "../../components/modals/TeamEditModal/TeamEditModal";

const TeamManagement = () => {
  const { teamId } = useNavigationParams<"teamId">();
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isAddManagerModalOpen, setIsAddManagerModalOpen] = useState(false);
  const [managerEmail, setManagerEmail] = useState("");
  const [addManagerError, setAddManagerError] = useState("");
  const [addManagerSuccess, setAddManagerSuccess] = useState("");
  
  const { data: team, error: teamError, isLoading } = useGetTeamDetailsQuery(
    { teamId: teamId! },
    { skip: !teamId }
  );

  const [addTeamManager, { isLoading: isAddingManager }] = useAddTeamManagerToTeamMutation();
  const [deleteTeamPlayer, { isLoading: isRemovingPlayer }] = useDeleteTeamPlayerMutation();

  const handleAddManager = async () => {
    if (!teamId) return;
    
    setAddManagerError("");
    setAddManagerSuccess("");

    // Simple email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(managerEmail)) {
      setAddManagerError("Please enter a valid email address");
      return;
    }

    try {
      await addTeamManager({ teamId, email: managerEmail }).unwrap();
      setAddManagerSuccess("Manager added successfully!");
      setManagerEmail("");
      // Close modal after 2 seconds
      setTimeout(() => {
        setIsAddManagerModalOpen(false);
        setAddManagerSuccess("");
      }, 2000);
    } catch (error: any) {
      setAddManagerError(error?.data || "Failed to add manager. Please try again.");
    }
  };

  const handleRemovePlayer = async (playerId: string, playerName: string) => {
    if (!teamId) return;
    
    if (!confirm(`Remove ${playerName} from team?`)) {
      return;
    }

    try {
      await deleteTeamPlayer({ teamId, playerId }).unwrap();
    } catch (error: any) {
      alert(error?.data || "Failed to remove player. Please try again.");
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
      logoUrl: team.logoUrl,
      description: team.description,
      contactEmail: team.contactEmail,
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
          {team.logoUrl && (
            <img
              src={team.logoUrl}
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
      {isAddManagerModalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-md">
            <h3 className="text-xl font-bold mb-4">Add Team Manager</h3>
            
            <div className="mb-4">
              <label className="block text-sm font-medium mb-2">Email Address</label>
              <input
                type="email"
                className="w-full px-3 py-2 border border-gray-300 rounded"
                value={managerEmail}
                onChange={(e) => setManagerEmail(e.target.value)}
                placeholder="manager@example.com"
              />
            </div>

            {addManagerError && (
              <div className="mb-4 text-red-600 text-sm">{addManagerError}</div>
            )}

            {addManagerSuccess && (
              <div className="mb-4 text-green-600 text-sm">{addManagerSuccess}</div>
            )}

            <div className="flex justify-end space-x-3">
              <button
                className="px-4 py-2 bg-gray-300 rounded hover:bg-gray-400"
                onClick={() => {
                  setIsAddManagerModalOpen(false);
                  setManagerEmail("");
                  setAddManagerError("");
                  setAddManagerSuccess("");
                }}
                disabled={isAddingManager}
              >
                Cancel
              </button>
              <button
                className="px-4 py-2 bg-green text-white rounded hover:bg-green-700 disabled:opacity-50"
                onClick={handleAddManager}
                disabled={isAddingManager || !managerEmail}
              >
                {isAddingManager ? "Adding..." : "Add Manager"}
              </button>
            </div>
          </div>
        </div>
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

      {/* Pending Invites Section */}
      <div className="bg-gray-100 rounded-lg p-6">
        <h2 className="text-2xl font-semibold mb-4 border-b-2 border-green pb-2">
          Pending Invitations
        </h2>
        {/* FUTURE Phase 3: Show pending invites when invite-player endpoint is implemented */}
        <p className="text-gray-500">No pending invitations</p>
      </div>
    </div>
  );
};

export default TeamManagement;
