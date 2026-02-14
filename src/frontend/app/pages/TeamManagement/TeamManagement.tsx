import React, { useState } from "react";
import { useNavigationParams } from "../../utils/navigationUtils";
import { useGetTeamDetailsQuery } from "../../store/serviceApi";
import { getErrorString } from "../../utils/errorUtils";
import TeamEditModal from "../../components/modals/TeamEditModal/TeamEditModal";

const TeamManagement = () => {
  const { teamId } = useNavigationParams<"teamId">();
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  
  // For now, use the regular team details endpoint
  // TODO: Switch to GET /api/v2/teams/{teamId}/management once Swagger is regenerated
  const { data: team, error: teamError, isLoading } = useGetTeamDetailsQuery(
    { teamId: teamId! },
    { skip: !teamId }
  );

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
          ngbId={team.ngbId}
          teamId={team.teamId}
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
                <button
                  className="text-red-600 hover:text-red-800 text-sm"
                  onClick={() => {
                    if (confirm(`Remove ${manager.name} as manager?`)) {
                      // TODO: Implement remove manager
                      alert("Remove manager functionality coming soon!");
                    }
                  }}
                >
                  Remove
                </button>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-gray-500">No managers assigned</p>
        )}
        <button
          className="mt-4 text-green font-semibold hover:underline"
          onClick={() => {
            // TODO: Implement add manager
            alert("Add manager functionality coming soon!");
          }}
        >
          + Add Manager
        </button>
      </div>

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
                        className="text-blue-600 hover:underline mr-3"
                        onClick={() => {
                          if (confirm(`Promote ${member.name} to team manager?`)) {
                            // TODO: Implement make manager
                            alert("Make manager functionality coming soon!");
                          }
                        }}
                      >
                        Make Manager
                      </button>
                      <button
                        className="text-red-600 hover:underline"
                        onClick={() => {
                          if (confirm(`Remove ${member.name} from team?`)) {
                            // TODO: Implement remove player
                            alert("Remove player functionality coming soon!");
                          }
                        }}
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
        <button
          className="mt-4 text-green font-semibold hover:underline"
          onClick={() => {
            // TODO: Implement invite player
            const email = prompt("Enter player email to invite:");
            if (email) {
              alert(`Invite player functionality coming soon! Email: ${email}`);
            }
          }}
        >
          + Invite Player
        </button>
      </div>

      {/* Pending Invites Section */}
      <div className="bg-gray-100 rounded-lg p-6">
        <h2 className="text-2xl font-semibold mb-4 border-b-2 border-green pb-2">
          Pending Invitations
        </h2>
        {/* TODO: Show pending invites when backend is implemented */}
        <p className="text-gray-500">No pending invitations</p>
      </div>
    </div>
  );
};

export default TeamManagement;
