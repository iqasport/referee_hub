import React from "react";
import ActionButtonPair from "../../../components/ActionButtonPair";
import { TournamentInviteViewModel } from "../../../store/serviceApi";

interface SidebarProps {
  isTournamentManager: boolean;
  isAnonymous: boolean;
  isRegistrationClosed: boolean;
  pendingInvitesForUser: TournamentInviteViewModel[];
  approvedTeamsForUser: any[];
  respondingTo: string | null;
  onRespondToInvite: (participantId: string, approved: boolean) => void;
  onEdit: () => void;
  onOpenRegistrations: () => void;
  onInviteTeams: () => void;
  onAddManager: () => void;
  onDelete: () => void;
  onContactOrganizer: () => void;
  onRegister: () => void;
  onManageRosters: () => void;
  totalPlayerCount: number;
  invitesCount: number;
  isPrivate: boolean;
}

export const TournamentDetailsSidebar: React.FC<SidebarProps> = ({
  isTournamentManager,
  isAnonymous,
  isRegistrationClosed,
  pendingInvitesForUser,
  approvedTeamsForUser,
  respondingTo,
  onRespondToInvite,
  onEdit,
  onOpenRegistrations,
  onInviteTeams,
  onAddManager,
  onDelete,
  onContactOrganizer,
  onRegister,
  onManageRosters,
  totalPlayerCount,
  invitesCount,
  isPrivate,
}) => {
  if (isTournamentManager) {
    return (
      <>
        {/* Manager Tools Card */}
        <div className="card card-highlighted card-mb card-sticky">
          <h3 className="card-title">Manager Tools</h3>
          <p className="card-description">
            You are the manager of this tournament. Use the tools below to manage the tournament.
          </p>
          <button onClick={onEdit} className="btn btn-primary btn-full-width btn-with-icon card-mb">
            <svg className="btn-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
              />
            </svg>
            Edit Tournament Details
          </button>
          <button onClick={onOpenRegistrations} className="btn btn-secondary btn-full-width card-mb">
            View Team Registrations ({invitesCount})
          </button>
          <button onClick={onInviteTeams} className="btn btn-secondary btn-full-width card-mb">
            Invite Teams
          </button>
          <button onClick={onAddManager} className="btn btn-secondary btn-full-width">
            Add Tournament Manager
          </button>
          <button onClick={onDelete} className="btn btn-danger btn-full-width" style={{ marginTop: "0.75rem" }}>
            Delete Tournament
          </button>
        </div>

        {/* Tournament Stats Card */}
        <div className="card">
          <h3 className="card-title">Tournament Stats</h3>
          <div className="stats-list">
            <div className="stats-item">
              <span className="stats-label">Teams Registered</span>
              <span className="stats-value">{invitesCount}</span>
            </div>
            <div className="stats-item">
              <span className="stats-label">Players Registered</span>
              <span className="stats-value">{totalPlayerCount}</span>
            </div>
            <div className="stats-item">
              <span className="stats-label">Private Tournament</span>
              <span className="stats-value">{isPrivate ? "Yes" : "No"}</span>
            </div>
          </div>
        </div>
      </>
    );
  }

  if (isAnonymous) {
    return (
      <div className="card card-highlighted card-mb card-sticky">
        <h3 className="card-title">Sign In To Participate</h3>
        <p className="card-description">
          This tournament is visible publicly. Sign in to register teams, manage rosters, or contact
          organizers.
        </p>
        <a className="btn btn-primary btn-full-width" href="/sign_in">
          Sign In
        </a>
      </div>
    );
  }

  return (
    <>
      {pendingInvitesForUser.length > 0 && (
        <div className="card card-highlighted card-mb">
          <h3 className="card-title">You&apos;re Invited!</h3>
          <p className="card-description">
            The tournament organizer has invited your team(s) to participate.
          </p>
          <div className="invite-list">
            {pendingInvitesForUser.map((invite) => (
              <div key={invite.participantId} className="invite-item">
                <p className="invite-team-name">{invite.participantName}</p>
                <ActionButtonPair
                  onAccept={() => onRespondToInvite(invite.participantId || "", true)}
                  onDecline={() => onRespondToInvite(invite.participantId || "", false)}
                  isLoading={respondingTo === invite.participantId}
                  size="sm"
                />
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Register Now / Manage Rosters Card */}
      <div className="card card-mb">
        {isRegistrationClosed ? (
          <>
            <h3 className="card-title">Registration Closed</h3>
            <p className="card-description">
              Registration for this tournament is now closed. Contact the organizers if you have
              questions.
            </p>
            <div className="mt-4 p-3 bg-gray-100 rounded-md border border-gray-300 text-center">
              <svg
                className="mx-auto h-12 w-12 text-gray-400 mb-2"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
                />
              </svg>
              <p className="text-sm font-medium text-gray-700">Registration is now closed</p>
            </div>
          </>
        ) : approvedTeamsForUser.length > 0 ? (
          <>
            <h3 className="card-title">You&apos;re Registered!</h3>
            <p className="card-description">
              Your team is registered for this tournament. Manage your roster below.
            </p>
            <button onClick={onManageRosters} className="btn btn-primary btn-full-width card-mb">
              Manage Your Rosters
            </button>
            <button onClick={onRegister} className="btn btn-outline btn-full-width">
              Register Another Team
            </button>
          </>
        ) : (
          <>
            <h3 className="card-title">Register Now</h3>
            <p className="card-description">
              Secure your spot in this exciting tournament. Limited slots available!
            </p>
            <button onClick={onRegister} className="btn btn-primary btn-full-width">
              Register for Tournament
            </button>
          </>
        )}
      </div>

      {/* Need Help Card */}
      <div className="card">
        <h3 className="card-title">Need Help?</h3>
        <p className="card-description">
          Have questions about this tournament? Contact the organizers.
        </p>
        <button onClick={onContactOrganizer} className="btn btn-outline btn-full-width">
          Contact Organizer
        </button>
      </div>
    </>
  );
};
