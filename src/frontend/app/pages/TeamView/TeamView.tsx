import React from "react";
import { Link } from "react-router-dom";
import { useNavigationParams } from "../../utils/navigationUtils";
import { useGetTeamDetailsQuery } from "../../store/serviceApi";
import { getErrorString } from "../../utils/errorUtils";
import { toDateTime } from "../../utils/dateUtils";

const TeamView = () => {
  const { teamId } = useNavigationParams<"teamId">();
  const { data: team, error: teamError, isLoading } = useGetTeamDetailsQuery(
    { teamId: teamId! },
    { skip: !teamId }
  );

  if (isLoading) {
    return (
      <div className="m-auto w-full my-10 px-4 xl:w-3/4 xl:px-0">
        <p>Loading team details...</p>
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

  // Use the backend-provided flag for whether current user is a manager
  const isTeamManager = team.isCurrentUserManager;

  return (
    <div className="m-auto w-full my-10 px-4 xl:w-3/4 xl:px-0">
      {/* Team Header */}
      <div className="flex items-center mb-8">
        {team.logoUrl && (
          <img
            src={team.logoUrl}
            alt={`${team.name} logo`}
            className="w-24 h-24 object-cover rounded mr-6"
          />
        )}
        <div className="flex-1">
          <h1 className="text-3xl font-bold mb-2">{team.name}</h1>
          <div className="text-gray-600">
            <p>
              {team.city}
              {team.state && `, ${team.state}`}, {team.country}
            </p>
            {team.contactEmail && (
              <p className="mt-1">
                <span className="font-semibold">Contact:</span>{" "}
                <a href={`mailto:${team.contactEmail}`} className="text-blue-600 hover:underline">
                  {team.contactEmail}
                </a>
              </p>
            )}
          </div>
        </div>
        {/* Manage Team Button for Team Managers */}
        {isTeamManager && (
          <div>
            <Link
              to={`/teams/${teamId}/manage`}
              className="bg-green text-white px-6 py-3 rounded-lg font-semibold hover:bg-green-700 transition inline-block"
            >
              Manage Team
            </Link>
          </div>
        )}
      </div>

      {/* Description */}
      {team.description && (
        <div className="bg-gray-100 rounded-lg p-4 mb-8">
          <h2 className="text-xl font-semibold mb-2">About</h2>
          <p className="whitespace-pre-wrap">{team.description}</p>
        </div>
      )}

      {/* Social Media */}
      {team.socialAccounts && team.socialAccounts.length > 0 && (
        <div className="bg-gray-100 rounded-lg p-4 mb-8">
          <h2 className="text-xl font-semibold mb-2">Social Media</h2>
          <div className="flex flex-wrap gap-2">
            {team.socialAccounts.map((account, index) => (
              <a
                key={index}
                href={account.url}
                target="_blank"
                rel="noopener noreferrer"
                className="text-blue-600 hover:underline"
              >
                {account.type}
              </a>
            ))}
          </div>
        </div>
      )}

      <div className="flex flex-col lg:flex-row gap-8">
        {/* Team Managers */}
        <div className="flex-1 bg-gray-100 rounded-lg p-4">
          <h2 className="text-xl font-semibold mb-4 border-b-2 border-green pb-2">
            Team Managers
          </h2>
          {team.managers && team.managers.length > 0 ? (
            <ul className="space-y-2">
              {team.managers.map((manager) => (
                <li key={manager.id} className="flex items-center">
                  <span className="font-medium">{manager.name}</span>
                  {manager.email && (
                    <span className="ml-2 text-gray-600 text-sm">({manager.email})</span>
                  )}
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-gray-500">No managers assigned</p>
          )}
        </div>

        {/* Team Members/Players */}
        <div className="flex-1 bg-gray-100 rounded-lg p-4">
          <h2 className="text-xl font-semibold mb-4 border-b-2 border-green pb-2">
            Team Members
          </h2>
          {team.members && team.members.length > 0 ? (
            <ul className="space-y-2">
              {team.members.map((member) => (
                <li key={member.userId} className="font-medium">
                  {member.name}
                  {team.groupAffiliation === "national" && member.primaryTeamName && (
                    <span className="ml-2 text-gray-600 text-sm">({member.primaryTeamName})</span>
                  )}
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-gray-500">No members</p>
          )}
        </div>
      </div>

      {/* Team Info */}
      <div className="mt-8 bg-gray-100 rounded-lg p-4">
        <h2 className="text-xl font-semibold mb-2">Team Information</h2>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <span className="font-semibold">Type:</span>{" "}
            <span className="capitalize">{team.groupAffiliation}</span>
          </div>
          <div>
            <span className="font-semibold">Status:</span>{" "}
            <span className="capitalize">{team.status}</span>
          </div>
          <div>
            <span className="font-semibold">Joined:</span>{" "}
            {toDateTime(team.joinedAt).toFormat("MMMM d, yyyy")}
          </div>
        </div>
      </div>
    </div>
  );
};

export default TeamView;
