import { faUser } from "@fortawesome/free-regular-svg-icons";
import { faMapPin } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { capitalize } from "lodash";
import React from "react";

import { getRefereeCertVersion } from "../../../utils/certUtils";
import { toDateTime } from "../../../utils/dateUtils";
import HeaderImage from "./HeaderImage";
import { Certification, useGetUserAvatarQuery, useGetUserDataQuery } from "../../../store/serviceApi";
import { useNavigationParams } from "../../../utils/navigationUtils";

type HeaderProps = {
  name: string;
  certifications: Certification[];
  isEditable: boolean;
  onTakeTests?: () => void;
};

const RefereeHeader = (props: HeaderProps) => {
  const { refereeId } = useNavigationParams<"refereeId">();
  const { certifications, name, isEditable, onTakeTests } = props;

  const { data: userAvatar } = useGetUserAvatarQuery({ userId: refereeId });
  const { data: user } = useGetUserDataQuery({ userId: refereeId });

  const renderCertifications = () => {
    if (!certifications?.length && !onTakeTests) return null;

    return (
      <div style={{ display: "flex", alignItems: "center", flexWrap: "wrap", gap: "0.5rem", marginTop: "0.5rem" }}>
        {certifications?.map((certification) => (
          <span
            key={`${certification.level}-${certification.version}`}
            style={{
              background: "#dcfce7",
              color: "#166534",
              border: "1px solid #86efac",
              padding: "0.2rem 0.6rem",
              borderRadius: "9999px",
              fontSize: "0.75rem",
              fontWeight: 600,
            }}
          >
            {`${capitalize(certification.level === "snitch" ? "flag" : certification.level)} (${getRefereeCertVersion(certification)})`}
          </span>
        ))}
        {onTakeTests && (
          <button
            type="button"
            className="btn btn-primary"
            style={{ fontSize: "0.75rem", padding: "0.2rem 0.75rem" }}
            onClick={onTakeTests}
          >
            Take Tests
          </button>
        )}
      </div>
    );
  };

  if (!user) return null;

  const joinYear = toDateTime(user.createdAt).year;

  return (
    <div
      style={{
        display: "flex",
        alignItems: "flex-start",
        gap: "1.5rem",
        padding: "1.5rem 0",
        borderBottom: "1px solid #e5e7eb",
        marginBottom: "1.5rem",
        flexWrap: "wrap",
      }}
    >
      {/* Avatar */}
      <HeaderImage avatarUrl={userAvatar as string} id={refereeId} isEditable={isEditable} />

      {/* Name + meta */}
      <div style={{ flex: 1, minWidth: "200px" }}>
        <h1 style={{ fontSize: "2rem", fontWeight: 700, color: "#111827", margin: 0 }}>{name}</h1>

        <div style={{ display: "flex", gap: "1.5rem", marginTop: "0.5rem", flexWrap: "wrap" }}>
          {user.createdAt && (
            <span style={{ fontSize: "0.875rem", color: "#6b7280" }}>
              <FontAwesomeIcon icon={faMapPin} className="mr-1" />
              Joined {joinYear}
            </span>
          )}
          {user.showPronouns && user.pronouns && (
            <span style={{ fontSize: "0.875rem", color: "#6b7280" }}>
              <FontAwesomeIcon icon={faUser} className="mr-1" />
              {user.pronouns}
            </span>
          )}
        </div>

        {renderCertifications()}

        {user.bio && (
          <p
            style={{
              marginTop: "0.75rem",
              fontSize: "1rem",
              color: "#374151",
              maxWidth: "48rem",
            }}
          >
            {user.bio}
          </p>
        )}
      </div>
    </div>
  );
};

export default RefereeHeader;

