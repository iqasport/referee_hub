import React, { useEffect, useState } from "react";

import RefereeHeader from "./RefereeHeader";
import RefereeLocation from "./RefereeLocation";
import RefereeTeam from "./RefereeTeam";
import {
  useGetRefereeQuery,
  useUpdateCurrentRefereeMutation,
  useGetUserDataQuery,
  useUpdateCurrentUserDataMutation,
  useGetUpcomingTournamentsQuery,
  UserDataViewModel,
} from "../../store/serviceApi";
import { RefereeLocationOptions } from "./RefereeLocation/RefereeLocation";
import { RefereeTeamOptions } from "./RefereeTeam/RefereeTeam";
import { getErrorString } from "../../utils/errorUtils";
import { useNavigate, useNavigationParams } from "../../utils/navigationUtils";
import Toggle from "../../components/Toggle";

// ──────────────────────────────────────────────────────────────────────────────
// Basic Details section (pronouns, bio + new private fields)
// ──────────────────────────────────────────────────────────────────────────────

interface BasicDetailsProps {
  userData: UserDataViewModel;
  isEditing: boolean;
  isEditable: boolean;
  onChange: (updated: Partial<UserDataViewModel>) => void;
  onEdit: () => void;
  onSave: () => void;
  onCancel: () => void;
}

const BasicDetails = ({
  userData,
  isEditing,
  isEditable,
  onChange,
  onEdit,
  onSave,
  onCancel,
}: BasicDetailsProps) => {
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
        <h3 className="card-title" style={{ marginBottom: 0 }}>
          Basic Details
        </h3>
        {isEditable && (
          <div className="flex gap-2">
            {isEditing ? (
              <>
                <button
                  type="reset"
                  className="btn btn-secondary"
                  onClick={onCancel}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                  onClick={onSave}
                >
                  Save
                </button>
              </>
            ) : (
              <button type="button" className="btn btn-primary" onClick={onEdit}>
                Edit
              </button>
            )}
          </div>
        )}
      </div>

      {/* Pronouns */}
      <div className="stats-item" style={isEditing ? { flexDirection: "column", alignItems: "flex-start", gap: "0.5rem" } : {}}>
        <span className="stats-label">Pronouns</span>
        {isEditing ? (
          <div className="flex items-center gap-3 w-full">
            <Toggle
              name="showPronouns"
              label="Show?"
              onChange={handleToggleChange("showPronouns")}
              checked={userData.showPronouns ?? false}
            />
            <input
              className="form-input flex-1"
              type="text"
              value={userData.pronouns ?? ""}
              onChange={handleStringChange("pronouns")}
              placeholder="Pronouns"
            />
          </div>
        ) : (
          <span className="stats-value">
            {userData.showPronouns && userData.pronouns ? userData.pronouns : "—"}
          </span>
        )}
      </div>

      {/* Bio / Comments */}
      <div className="stats-item" style={isEditing ? { flexDirection: "column", alignItems: "flex-start", gap: "0.5rem" } : {}}>
        <span className="stats-label">Bio</span>
        {isEditing ? (
          <textarea
            className="bg-gray-100 rounded p-2 text-sm w-full"
            style={{ resize: "vertical", minHeight: "4rem" }}
            onChange={handleStringChange("bio")}
            value={userData.bio ?? ""}
            placeholder="Tell us about yourself…"
          />
        ) : (
          <span className="stats-value">{userData.bio || "—"}</span>
        )}
      </div>

      {/* Private fields — visible only when isEditable (current user) */}
      {isEditable && (
        <>
          {/* Date of birth */}
          <div className="stats-item" style={isEditing ? { flexDirection: "column", alignItems: "flex-start", gap: "0.5rem" } : {}}>
            <span className="stats-label">Date of Birth</span>
            {isEditing ? (
              <input
                className="form-input"
                type="date"
                value={userData.dateOfBirth ?? ""}
                onChange={handleStringChange("dateOfBirth")}
              />
            ) : (
              <span className="stats-value">{userData.dateOfBirth || "—"}</span>
            )}
          </div>

          {/* Food restrictions / allergies */}
          <div className="stats-item" style={isEditing ? { flexDirection: "column", alignItems: "flex-start", gap: "0.5rem" } : {}}>
            <span className="stats-label">Food Restrictions / Allergies</span>
            {isEditing ? (
              <textarea
                className="bg-gray-100 rounded p-2 text-sm w-full"
                style={{ resize: "vertical", minHeight: "3rem" }}
                onChange={handleStringChange("foodRestrictions")}
                value={userData.foodRestrictions ?? ""}
                placeholder="Any food restrictions or allergies…"
              />
            ) : (
              <span className="stats-value">{userData.foodRestrictions || "—"}</span>
            )}
          </div>

          {/* Medical information */}
          <div className="stats-item" style={isEditing ? { flexDirection: "column", alignItems: "flex-start", gap: "0.5rem" } : {}}>
            <span className="stats-label">Medical Information</span>
            {isEditing ? (
              <textarea
                className="bg-gray-100 rounded p-2 text-sm w-full"
                style={{ resize: "vertical", minHeight: "3rem" }}
                onChange={handleStringChange("medicalInformation")}
                value={userData.medicalInformation ?? ""}
                placeholder="Relevant medical information…"
              />
            ) : (
              <span className="stats-value">{userData.medicalInformation || "—"}</span>
            )}
          </div>

          {/* Emergency contact */}
          <div className="stats-item" style={isEditing ? { flexDirection: "column", alignItems: "flex-start", gap: "0.5rem" } : {}}>
            <span className="stats-label">Emergency Contact</span>
            {isEditing ? (
              <input
                className="form-input"
                type="text"
                value={userData.emergencyContact ?? ""}
                onChange={handleStringChange("emergencyContact")}
                placeholder="Name and phone number…"
              />
            ) : (
              <span className="stats-value">{userData.emergencyContact || "—"}</span>
            )}
          </div>
        </>
      )}
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
// Upcoming Events section
// ──────────────────────────────────────────────────────────────────────────────

interface UpcomingEventsProps {
  refereeId: string;
}

const UpcomingEvents = ({ refereeId }: UpcomingEventsProps) => {
  const navigate = useNavigate();
  const { data: tournaments, isLoading } = useGetUpcomingTournamentsQuery(
    { userId: refereeId },
    { skip: !refereeId }
  );

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
              style={{ cursor: "pointer" }}
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

// ──────────────────────────────────────────────────────────────────────────────
// Main RefereeProfile page
// ──────────────────────────────────────────────────────────────────────────────

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
          {/* Column 1: Player Details (location + teams) */}
          <div>
            <PlayerDetails />
          </div>

          {/* Column 2: Basic Details + Upcoming Events */}
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
            {isEditable && <UpcomingEvents refereeId={refereeId} />}
          </div>
        </div>
      </div>
    </section>
  );
};

export default RefereeProfile;

