import React, { useEffect, useState } from "react";

import RefereeHeader from "./RefereeHeader";
import RefereeLocation from "./RefereeLocation";
import RefereeTeam from "./RefereeTeam";
import { capitalize } from "lodash";
import {
  useGetRefereeQuery,
  useUpdateCurrentRefereeMutation,
  useGetUserDataQuery,
  useUpdateCurrentUserDataMutation,
  useGetUpcomingTournamentsQuery,
  useGetTestAttemptsQuery,
  UserDataViewModel,
  TestAttemptViewModelRead,
} from "../../store/serviceApi";
import { RefereeLocationOptions } from "./RefereeLocation/RefereeLocation";
import { RefereeTeamOptions } from "./RefereeTeam/RefereeTeam";
import { getErrorString } from "../../utils/errorUtils";
import { useNavigate, useNavigationParams } from "../../utils/navigationUtils";
import Toggle from "../../components/Toggle";

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
        <input className="form-input flex-1" type="text" value={pronouns ?? ""} onChange={onPronounsChange} placeholder="Pronouns" />
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
          <textarea className="bg-gray-100 rounded p-2 text-sm w-full" style={{ resize: "vertical", minHeight: "4rem" }}
            onChange={handleStringChange("bio")} value={userData.bio ?? ""} placeholder="Tell us about yourself…" />
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
  const [isEditing, setIsEditing] = useState(false);

  const { currentData: referee } = useGetRefereeQuery({ userId: refereeId });
  const [editableReferee, setReferee] = useState<RefereeLocationOptions & RefereeTeamOptions>(referee);
  const [updateReferee, { error: updateRefereeError }] = useUpdateCurrentRefereeMutation();

  const handleChange = (newState: RefereeLocationOptions | RefereeTeamOptions) => {
    setReferee({ ...editableReferee, ...newState });
  };

  const buttonClick = () => {
    if (isEditing) {
      setIsEditing(false);
      updateReferee({ refereeUpdateViewModel: editableReferee });
    } else {
      setIsEditing(true);
    }
  };

  return (
    <div className="card card-mb">
      <div className="flex items-center justify-between mb-4">
        <h3 className="card-title" style={{ marginBottom: 0 }}>
          Player Details
        </h3>
        {refereeId === "me" && (
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
            primaryNgb: editableReferee?.primaryNgb,
            secondaryNgb: editableReferee?.secondaryNgb,
          }}
          isEditing={isEditing}
          onChange={handleChange}
        />
        <RefereeTeam
          teams={{
            coachingTeam: editableReferee?.coachingTeam,
            playingTeam: editableReferee?.playingTeam,
            nationalTeam: editableReferee?.nationalTeam,
          }}
          locations={{
            primaryNgb: editableReferee?.primaryNgb,
            secondaryNgb: editableReferee?.secondaryNgb,
          }}
          isEditing={isEditing}
          onChange={handleChange}
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
  const { data: tournaments, isLoading } = useGetUpcomingTournamentsQuery();

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
              {(tournament.place || tournament.city || tournament.country) && (
                <div style={{ fontSize: "0.8125rem", color: "#6b7280" }}>
                  {[tournament.place, tournament.city, tournament.country].filter(Boolean).join(", ")}
                </div>
              )}
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

  const { currentData: referee, error: refereeGetError } = useGetRefereeQuery({ userId: refereeId });
  const { data: userData } = useGetUserDataQuery({ userId: refereeId });
  const [updateUser, { error: updateUserError }] = useUpdateCurrentUserDataMutation();

  const [editableUser, setEditableUser] = useState<UserDataViewModel>(userData ?? {});
  const [isEditingDetails, setIsEditingDetails] = useState(false);

  useEffect(() => {
    if (userData) setEditableUser(userData);
  }, [userData]);

  if (refereeGetError) return <p style={{ color: "red" }}>{getErrorString(refereeGetError)}</p>;
  if (!referee) return null;

  const isEditable = refereeId === "me";

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
            {isEditable && <UpcomingEvents />}
          </div>

          {/* Column 2: Basic Details + Certification History */}
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
            {isEditable && <CertificationHistory />}
          </div>
        </div>
      </div>
    </section>
  );
};

export default RefereeProfile;

