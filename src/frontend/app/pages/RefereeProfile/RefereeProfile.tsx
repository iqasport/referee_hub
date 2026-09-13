import React, { useEffect, useRef, useState } from "react";

import RefereeHeader from "./RefereeHeader";
import RefereeLocation from "./RefereeLocation";
import RefereeTeam from "./RefereeTeam";
import { capitalize } from "lodash";
import * as serviceApiModule from "../../store/serviceApi";
import {
  useGetRefereeQuery,
  useGetCurrentRefereeQuery,
  useUpdateCurrentRefereeMutation,
  useGetUserDataQuery,
  useGetCurrentUserDataQuery,
  useUpdateCurrentUserDataMutation,
  useGetMyTeamInvitesQuery,
  useRespondToTeamInviteMutation,
  useCancelMyTeamInviteMutation,
  useGetMyUpcomingTournamentsQuery,
  useGetManagedTeamsQuery,
  useGetTestAttemptsQuery,
  UserDataViewModel,
  TestAttemptViewModelRead,
} from "../../store/serviceApi";
import { RefereeLocationOptions } from "./RefereeLocation/RefereeLocation";
import { RefereeTeamOptions } from "./RefereeTeam/RefereeTeam";
import { getErrorString } from "../../utils/errorUtils";
import { useNavigate, useNavigationParams } from "../../utils/navigationUtils";
import Toggle from "../../components/Toggle";
import ActionButtonPair from "../../components/ActionButtonPair";
import CustomAlert from "../../components/CustomAlert";
import { useAlert } from "../../hooks/useAlert";

// ──────────────────────────────────────────────────────────────────────────────
// Basic Details section (pronouns, bio)
// ──────────────────────────────────────────────────────────────────────────────

// Shared row wrapper — handles the stats-item container and label, then renders
// either the editContent (when editing) or a plain value span.
interface DetailsRowProps {
  label: string;
  isEditing: boolean;
  value: string | null | undefined;
  editContent: React.ReactNode;
}

const DetailsRow = ({ label, isEditing, value, editContent }: DetailsRowProps) => (
  <div
    className="stats-item"
    style={isEditing ? { flexDirection: "column", alignItems: "flex-start", gap: "0.5rem" } : {}}
  >
    <span className="stats-label">{label}</span>
    {isEditing ? editContent : <span className="stats-value">{value || "—"}</span>}
  </div>
);

// Special pronouns row — includes a visibility toggle alongside the text input.
interface PronounsRowProps {
  isEditing: boolean;
  pronouns: string | null | undefined;
  showPronouns: boolean | null | undefined;
  onPronounsChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onShowPronounsChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

const PronounsRow = ({ isEditing, pronouns, showPronouns, onPronounsChange, onShowPronounsChange }: PronounsRowProps) => (
  <DetailsRow
    label="Pronouns"
    isEditing={isEditing}
    value={showPronouns && pronouns ? pronouns : null}
    editContent={
      <div className="flex items-center gap-3 w-full">
        <Toggle name="showPronouns" label="Show?" onChange={onShowPronounsChange} checked={showPronouns ?? false} />
        <input
          id="referee-pronouns"
          name="pronouns"
          className="form-input flex-1"
          type="text"
          value={pronouns ?? ""}
          onChange={onPronounsChange}
          placeholder="Pronouns"
        />
      </div>
    }
  />
);

interface BasicDetailsProps {
  userData: UserDataViewModel;
  isEditing: boolean;
  isEditable: boolean;
  onChange: (updated: Partial<UserDataViewModel>) => void;
  onEdit: () => void;
  onSave: () => void;
  onCancel: () => void;
}

const BasicDetails = ({ userData, isEditing, isEditable, onChange, onEdit, onSave, onCancel }: BasicDetailsProps) => {
  const handleStringChange =
    (key: keyof UserDataViewModel) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) =>
      onChange({ [key]: e.currentTarget.value });

  const handleToggleChange =
    (key: keyof UserDataViewModel) => (e: React.ChangeEvent<HTMLInputElement>) =>
      onChange({ [key]: e.currentTarget.checked });

  return (
    <div className="card card-mb">
      <div className="flex items-center justify-between mb-4">
        <h3 className="card-title" style={{ marginBottom: 0 }}>Basic Details</h3>
        {isEditable && (
          <div className="flex gap-2">
            {isEditing ? (
              <>
                <button type="reset" className="btn btn-secondary" onClick={onCancel}>Cancel</button>
                <button type="submit" className="btn btn-primary" onClick={onSave}>Save</button>
              </>
            ) : (
              <button type="button" className="btn btn-primary" onClick={onEdit}>Edit</button>
            )}
          </div>
        )}
      </div>

      <PronounsRow
        isEditing={isEditing}
        pronouns={userData.pronouns}
        showPronouns={userData.showPronouns}
        onPronounsChange={handleStringChange("pronouns")}
        onShowPronounsChange={handleToggleChange("showPronouns")}
      />

      <DetailsRow
        label="Bio"
        isEditing={isEditing}
        value={userData.bio}
        editContent={
          <textarea
            id="referee-bio"
            name="bio"
            className="bg-gray-100 rounded p-2 text-sm w-full"
            style={{ resize: "vertical", minHeight: "4rem" }}
            onChange={handleStringChange("bio")}
            value={userData.bio ?? ""}
            placeholder="Tell us about yourself…"
          />
        }
      />

    </div>
  );
};

// ──────────────────────────────────────────────────────────────────────────────
// Player Details (location + teams) section
// ──────────────────────────────────────────────────────────────────────────────

const PlayerDetails = () => {
  const { refereeId } = useNavigationParams<"refereeId">();
  const isOwnProfile = refereeId === "me";
  const refereeQueryUserId = refereeId ?? "";
  const [isEditing, setIsEditing] = useState(false);
  const { data: managedTeams } = useGetManagedTeamsQuery(undefined, {
    skip: !isOwnProfile,
  });
  const { data: myInvites } = useGetMyTeamInvitesQuery(undefined, {
    skip: !isOwnProfile,
  });

  // A pending join request is one the player initiated themselves (canRespond === false)
  const pendingPlayingTeamId = myInvites?.find((i) => i.canRespond === false)?.teamId ?? null;

  const { currentData: currentReferee } = useGetCurrentRefereeQuery(undefined, {
    skip: !isOwnProfile,
  });
  const { currentData: viewedReferee } = useGetRefereeQuery(
    { userId: refereeQueryUserId },
    { skip: isOwnProfile || !refereeId },
  );
  const referee = isOwnProfile ? currentReferee : viewedReferee;
  const [editableReferee, setReferee] = useState<RefereeLocationOptions & RefereeTeamOptions>(referee);
  const editableRefereeRef = useRef<RefereeLocationOptions & RefereeTeamOptions>(editableReferee);
  const [updateReferee, { error: updateRefereeError }] = useUpdateCurrentRefereeMutation();

  const fallbackPrimaryNgb = editableReferee?.primaryNgb ?? referee?.primaryNgb ?? managedTeams?.[0]?.ngb ?? null;
  const fallbackPlayingTeam =
    editableReferee?.playingTeam ??
    referee?.playingTeam ??
    (managedTeams?.length === 1 && managedTeams[0]?.teamId
      ? { id: managedTeams[0].teamId, name: managedTeams[0].teamName ?? undefined }
      : null);

  useEffect(() => {
    if (!isEditing && referee) {
      setReferee(referee);
      editableRefereeRef.current = referee;
    }
  }, [referee, isEditing]);

  useEffect(() => {
    editableRefereeRef.current = editableReferee;
  }, [editableReferee]);

  const handleChange = (newState: RefereeLocationOptions | RefereeTeamOptions) => {
    setReferee((prev) => {
      const updated = { ...prev, ...newState };
      editableRefereeRef.current = updated;
      return updated;
    });
  };

  const buttonClick = async () => {
    if (isEditing) {
      const payload = editableRefereeRef.current ?? editableReferee ?? referee;
      if (!payload) {
        return;
      }

      try {
        await updateReferee({ refereeUpdateViewModel: payload }).unwrap();
        setIsEditing(false);
      } catch {
        // Keep editing mode active so the user can fix and resubmit.
      }
    } else {
      if (referee) {
        const prefilledReferee = {
          ...referee,
          primaryNgb: referee.primaryNgb ?? managedTeams?.[0]?.ngb ?? null,
          playingTeam: referee.playingTeam ?? (
            managedTeams?.length === 1 && managedTeams[0]?.teamId
              ? { id: managedTeams[0].teamId, name: managedTeams[0].teamName ?? undefined }
              : null
          ),
        };
        setReferee(prefilledReferee);
        editableRefereeRef.current = prefilledReferee;
      }
      setIsEditing(true);
    }
  };

  return (
    <div className="card card-mb">
      <div className="flex items-center justify-between mb-4">
        <h3 className="card-title" style={{ marginBottom: 0 }}>
          Player Details
        </h3>
        {isOwnProfile && (
          <button
            type="button"
            className="btn btn-primary"
            onClick={buttonClick}
          >
            {isEditing ? "Save" : "Edit"}
          </button>
        )}
      </div>

      {updateRefereeError && (
        <p style={{ color: "#dc2626", marginBottom: "0.5rem" }}>
          Error: {getErrorString(updateRefereeError)}
        </p>
      )}

      <div className="flex flex-col lg:flex-row">
        <RefereeLocation
          locations={{
            primaryNgb: fallbackPrimaryNgb,
            secondaryNgb: editableReferee?.secondaryNgb,
          }}
          isEditing={isEditing}
          onChange={handleChange}
        />
        <RefereeTeam
          teams={{
            coachingTeam: editableReferee?.coachingTeam,
            playingTeam: fallbackPlayingTeam,
            nationalTeam: editableReferee?.nationalTeam,
          }}
          locations={{
            primaryNgb: fallbackPrimaryNgb,
            secondaryNgb: editableReferee?.secondaryNgb,
          }}
          isEditing={isEditing}
          onChange={handleChange}
          pendingPlayingTeamId={pendingPlayingTeamId}
        />
      </div>
    </div>
  );
};

// ──────────────────────────────────────────────────────────────────────────────
// Upcoming Events section (only shown on own profile)
// ──────────────────────────────────────────────────────────────────────────────

const UpcomingEvents = () => {
  const navigate = useNavigate();
  const { data: tournaments, isLoading } = useGetMyUpcomingTournamentsQuery();

  const formatDate = (dateStr: string | undefined): string => {
    if (!dateStr) return "TBD";
    return new Date(dateStr).toLocaleDateString(undefined, {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  };

  return (
    <div className="card card-mb">
      <h3 className="card-title">Upcoming Events</h3>
      {isLoading && <p className="card-description">Loading…</p>}
      {!isLoading && (!tournaments || tournaments.length === 0) && (
        <p className="card-description">No upcoming tournaments found.</p>
      )}
      {!isLoading && tournaments && tournaments.length > 0 && (
        <div className="invite-list">
          {tournaments.map((tournament) => (
            <div
              key={tournament.id}
              className="invite-item"
              style={{ cursor: "pointer", flexDirection: "column", alignItems: "flex-start", gap: "0.25rem" }}
              onClick={() => navigate(`/tournaments/${tournament.id}`)}
            >
              <div className="invite-team-name">{tournament.name}</div>
              <div style={{ display: "flex", gap: "1rem", fontSize: "0.875rem", color: "#4b5563" }}>
                <span>{formatDate(tournament.startDate)}</span>
                {tournament.endDate && tournament.endDate !== tournament.startDate && (
                  <span>→ {formatDate(tournament.endDate)}</span>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

const TeamInvites = () => {
  const { data: invites, isLoading } = useGetMyTeamInvitesQuery();
  const [respondToTeamInvite] = useRespondToTeamInviteMutation();
  const [cancelMyTeamInvite] = useCancelMyTeamInviteMutation();
  const [respondingTo, setRespondingTo] = useState<string | null>(null);
  const [cancellingId, setCancellingId] = useState<string | null>(null);
  const { alertState, showAlert, hideAlert } = useAlert();

  const handleRespond = async (invitationId: string, approved: boolean) => {
    setRespondingTo(invitationId);
    try {
      await respondToTeamInvite({
        invitationId,
        inviteResponseModel: { approved },
      }).unwrap();
      showAlert(approved ? "Successfully accepted request." : "Request declined.", "success");
    } catch (error) {
      console.error("Failed to respond to team invite", error);
      showAlert("Failed to respond. Please try again.", "error");
    } finally {
      setRespondingTo(null);
    }
  };

  const handleCancel = async (invitationId: string) => {
    setCancellingId(invitationId);
    try {
      await cancelMyTeamInvite({ invitationId }).unwrap();
      showAlert("Join request cancelled.", "success");
    } catch (error) {
      console.error("Failed to cancel join request", error);
      showAlert("Failed to cancel request. Please try again.", "error");
    } finally {
      setCancellingId(null);
    }
  };

  return (
    <div className="card card-mb">
      {alertState.isVisible && (
        <CustomAlert message={alertState.message} type={alertState.type} onClose={hideAlert} />
      )}
      <h3 className="card-title">Team Requests</h3>
      {isLoading && <p className="card-description">Loading…</p>}
      {!isLoading && (!invites || invites.length === 0) && (
        <p className="card-description">No pending team requests.</p>
      )}
      {!isLoading && invites && invites.length > 0 && (
        <div className="invite-list">
          {invites.map((invite) => {
            if (!invite.invitationId) return null;

            return (
              <div key={invite.invitationId} className="invite-item" style={{ alignItems: "center" }}>
                <div style={{ display: "flex", alignItems: "center", gap: "0.75rem" }}>
                  <InviteTeamLogo teamName={invite.teamName} teamLogoUri={invite.teamLogoUri} />
                  <div>
                    <div className="invite-team-name">{invite.teamName || invite.teamId || "Team"}</div>
                    <div className="text-sm text-gray-600">
                      {invite.canRespond
                        ? (invite.invitedByName ? `Invited by ${invite.invitedByName}` : "Invited")
                        : (invite.invitedByName ? `Requested by ${invite.invitedByName}` : "Request pending")}
                      {invite.createdAt ? ` on ${new Date(invite.createdAt).toLocaleDateString()}` : ""}
                      {!invite.canRespond ? " - Waiting for team manager approval" : ""}
                    </div>
                  </div>
                </div>
                {invite.canRespond ? (
                  <ActionButtonPair
                    onAccept={() => handleRespond(invite.invitationId!, true)}
                    onDecline={() => handleRespond(invite.invitationId!, false)}
                    isLoading={respondingTo === invite.invitationId}
                    size="sm"
                  />
                ) : (
                  <button
                    type="button"
                    className="btn btn-secondary"
                    style={{ fontSize: "0.75rem", padding: "0.25rem 0.75rem" }}
                    disabled={cancellingId === invite.invitationId}
                    onClick={() => handleCancel(invite.invitationId!)}
                  >
                    {cancellingId === invite.invitationId ? "Cancelling…" : "Cancel request"}
                  </button>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};

const InviteTeamLogo = ({
  teamName,
  teamLogoUri,
}: {
  teamName?: string | null;
  teamLogoUri?: string | null;
}) => {
  if (!teamLogoUri) {
    return (
      <div
        className="rounded border border-gray-200 bg-gray-100"
        style={{ width: "2rem", height: "2rem" }}
        aria-hidden="true"
      />
    );
  }

  return (
    <img
      src={teamLogoUri}
      alt={`${teamName || "Team"} logo`}
      className="rounded border border-gray-200 object-cover"
      style={{ width: "2rem", height: "2rem" }}
    />
  );
};

type TeamTransferHistoryItem = {
  activityType?: string;
  createdAt?: string;
  teamId?: string;
  teamName?: string | null;
  teamLogoUri?: string | null;
};

type TeamHistoryQueryResult = {
  data?: TeamTransferHistoryItem[];
  isLoading?: boolean;
};

const TeamTransferHistory = ({ userId, isOwnProfile }: { userId?: string; isOwnProfile: boolean }) => {
  const myHistoryQueryHook = (serviceApiModule as {
    useGetMyTeamHistoryQuery?: (
      arg?: void,
      options?: { skip?: boolean }
    ) => TeamHistoryQueryResult;
  }).useGetMyTeamHistoryQuery;

  const otherHistoryQueryHook = (serviceApiModule as {
    useGetUserTeamHistoryQuery?: (
      arg: { userId: string },
      options?: { skip?: boolean }
    ) => TeamHistoryQueryResult;
  }).useGetUserTeamHistoryQuery;

  const myHistoryQuery = myHistoryQueryHook
    ? myHistoryQueryHook(undefined, { skip: !isOwnProfile })
    : { data: [], isLoading: false };

  const otherHistoryQuery = otherHistoryQueryHook
    ? otherHistoryQueryHook(
        { userId: userId || "" },
        { skip: isOwnProfile || !userId }
      )
    : { data: [], isLoading: false };

  const isLoading = isOwnProfile ? myHistoryQuery.isLoading : otherHistoryQuery.isLoading;
  const history = isOwnProfile ? (myHistoryQuery.data || []) : (otherHistoryQuery.data || []);

  const formatSummary = (activity: TeamTransferHistoryItem) => {
    const teamLabel = activity.teamName || activity.teamId || "Unknown team";

    if (activity.activityType === "inviteAccepted") {
      return `Joined ${teamLabel}`;
    }

    if (activity.activityType === "playerRemoved") {
      return `Left ${teamLabel}`;
    }

    return teamLabel;
  };

  return (
    <div className="card card-mb">
      <h3 className="card-title">Team Transfer History</h3>
      {isLoading && <p className="card-description">Loading…</p>}
      {!isLoading && (!history || history.length === 0) && (
        <p className="card-description">No transfer activity yet.</p>
      )}
      {!isLoading && history && history.length > 0 && (
        <div className="invite-list">
          {history.map((activity, index) => (
            <div
              key={`${activity.createdAt || "unknown"}-${activity.teamId || "team"}-${index}`}
              className="invite-item"
              style={{ justifyContent: "space-between", cursor: "default" }}
            >
              <div style={{ display: "flex", alignItems: "center", gap: "0.75rem" }}>
                <InviteTeamLogo
                  teamName={activity.teamName}
                  teamLogoUri={activity.teamLogoUri}
                />
                <div style={{ fontWeight: 500 }}>{formatSummary(activity)}</div>
              </div>
              <div style={{ fontSize: "0.875rem", color: "#4b5563" }}>
                {activity.createdAt ? new Date(activity.createdAt).toLocaleString() : "Unknown time"}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

// ──────────────────────────────────────────────────────────────────────────────
// Certification Attempt History section (only shown on own profile)
// ──────────────────────────────────────────────────────────────────────────────

const VERSION_LABELS: Record<string, string> = {
  eighteen: "18",
  twenty: "20",
  twentytwo: "22",
  twentyfour: "24",
};

const formatAttemptLevel = (attempt: TestAttemptViewModelRead): string => {
  const level = attempt.level === "snitch" ? "flag" : (attempt.level ?? "");
  const version = attempt.version ? ` (${VERSION_LABELS[attempt.version] ?? attempt.version})` : "";
  return capitalize(level) + version;
};

const CertificationHistory = () => {
  const { data: attempts, isLoading } = useGetTestAttemptsQuery();

  const sortedAttempts = attempts
    ? [...attempts].sort((a, b) => {
        const aDate = a.startedAt ? new Date(a.startedAt).getTime() : 0;
        const bDate = b.startedAt ? new Date(b.startedAt).getTime() : 0;
        return bDate - aDate;
      })
    : [];

  return (
    <div className="card card-mb">
      <h3 className="card-title">Certification History</h3>
      {isLoading && <p className="card-description">Loading…</p>}
      {!isLoading && sortedAttempts.length === 0 && (
        <p className="card-description">No test attempts yet.</p>
      )}
      {!isLoading && sortedAttempts.length > 0 && (
        <div>
          {sortedAttempts.map((attempt) => {
            const date = attempt.startedAt
              ? new Date(attempt.startedAt).toLocaleDateString(undefined, {
                  year: "numeric",
                  month: "short",
                  day: "numeric",
                })
              : "—";
            const score = attempt.score != null ? `${attempt.score}%` : "—";
            const isPassed = attempt.passed === true;
            const isFailed = attempt.passed === false;
            const isInProgress = attempt.isInProgress === true;

            return (
              <div
                key={attempt.attemptId}
                className="invite-item"
                style={{ justifyContent: "space-between", cursor: "default" }}
              >
                <div style={{ fontWeight: 500 }}>{formatAttemptLevel(attempt)}</div>
                <div style={{ display: "flex", gap: "1rem", alignItems: "center", fontSize: "0.875rem", color: "#4b5563" }}>
                  <span>{date}</span>
                  <span>{score}</span>
                  {isInProgress && (
                    <span style={{ background: "#e5e7eb", color: "#374151", borderRadius: "0.25rem", padding: "0.125rem 0.5rem", fontSize: "0.75rem", fontWeight: 600 }}>
                      In Progress
                    </span>
                  )}
                  {!isInProgress && isPassed && (
                    <span style={{ background: "#d1fae5", color: "#065f46", borderRadius: "0.25rem", padding: "0.125rem 0.5rem", fontSize: "0.75rem", fontWeight: 600 }}>
                      Passed
                    </span>
                  )}
                  {!isInProgress && isFailed && (
                    <span style={{ background: "#fee2e2", color: "#991b1b", borderRadius: "0.25rem", padding: "0.125rem 0.5rem", fontSize: "0.75rem", fontWeight: 600 }}>
                      Failed
                    </span>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};



const RefereeProfile = () => {
  const navigate = useNavigate();
  const { refereeId } = useNavigationParams<"refereeId">();
  const isOwnProfile = refereeId === "me";
  const refereeQueryUserId = refereeId ?? "";

  const { currentData: currentReferee, error: currentRefereeError } = useGetCurrentRefereeQuery(undefined, {
    skip: !isOwnProfile,
  });
  const { currentData: viewedReferee, error: viewedRefereeError } = useGetRefereeQuery(
    { userId: refereeQueryUserId },
    { skip: isOwnProfile || !refereeId },
  );

  const { data: currentUserData } = useGetCurrentUserDataQuery(undefined, {
    skip: !isOwnProfile,
  });
  const { data: viewedUserData } = useGetUserDataQuery(
    { userId: refereeQueryUserId },
    { skip: isOwnProfile || !refereeId },
  );

  const referee = isOwnProfile ? currentReferee : viewedReferee;
  const refereeGetError = isOwnProfile ? currentRefereeError : viewedRefereeError;
  const userData = isOwnProfile ? currentUserData : viewedUserData;
  const [updateUser, { error: updateUserError }] = useUpdateCurrentUserDataMutation();

  const [editableUser, setEditableUser] = useState<UserDataViewModel>(userData ?? {});
  const [isEditingDetails, setIsEditingDetails] = useState(false);

  useEffect(() => {
    if (userData) setEditableUser(userData);
  }, [userData]);

  if (refereeGetError) return <p style={{ color: "red" }}>{getErrorString(refereeGetError)}</p>;
  if (!referee) return null;

  const isEditable = isOwnProfile;

  const handleDetailsChange = (partial: Partial<UserDataViewModel>) =>
    setEditableUser((prev) => ({ ...prev, ...partial }));

  const handleDetailsSave = () => {
    setIsEditingDetails(false);
    updateUser({ userDataViewModel: editableUser });
  };

  const handleDetailsCancel = () => {
    setIsEditingDetails(false);
    setEditableUser(userData ?? {});
  };

  return (
    <section className="tournament-details-section">
      <div className="tournament-details-wrapper">
        {/* Page header — certifications badges + Take Tests button */}
        <RefereeHeader
          name={referee.name}
          certifications={referee.acquiredCertifications}
          isEditable={isEditable}
          onTakeTests={isEditable ? () => navigate("/referees/me/tests") : undefined}
        />

        {updateUserError && (
          <div style={{ color: "#dc2626", marginBottom: "1rem" }}>
            Error: {getErrorString(updateUserError)}
          </div>
        )}

        {/* Two-column grid */}
        <div className="tournament-details-grid" style={{ marginTop: "1.5rem" }}>
          {/* Column 1: Player Details + Upcoming Events */}
          <div>
            <PlayerDetails />
            {isOwnProfile && <UpcomingEvents />}
            <TeamTransferHistory userId={refereeQueryUserId} isOwnProfile={isOwnProfile} />
          </div>

          {/* Column 2: Basic Details + Team Requests + Certification History */}
          <div>
            {editableUser && (
              <BasicDetails
                userData={editableUser}
                isEditing={isEditingDetails}
                isEditable={isEditable}
                onChange={handleDetailsChange}
                onEdit={() => setIsEditingDetails(true)}
                onSave={handleDetailsSave}
                onCancel={handleDetailsCancel}
              />
            )}
            {isEditable && <TeamInvites />}
            {isEditable && <CertificationHistory />}
          </div>
        </div>
      </div>
    </section>
  );
};

export default RefereeProfile;

