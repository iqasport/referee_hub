import { faUser } from "@fortawesome/free-regular-svg-icons";
import { faMapPin } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { capitalize } from "lodash";
import React, { useState } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";

import { getRefereeCertVersion } from "../../../utils/certUtils";
import { toDateTime } from "../../../utils/dateUtils";
import HeaderImage from "./HeaderImage";
import { Certification, CertificationLevel, CertificationVersion, useGetUserAvatarQuery, useGetUserDataQuery } from "../../../store/serviceApi";
import { useNavigationParams } from "../../../utils/navigationUtils";

/** Rank used to pick the "highest level" certification for the header. */
const LEVEL_RANK: Record<CertificationLevel, number> = {
  scorekeeper: 0,
  assistant: 1,
  snitch: 2,
  head: 3,
  field: 4,
};

/** Rank used to pick the "most recent version" certification for the header. */
const VERSION_RANK: Record<CertificationVersion, number> = {
  eighteen: 0,
  twenty: 1,
  twentytwo: 2,
  twentyfour: 3,
};

/**
 * Returns at most two representative certifications to display in the header:
 * - the one with the highest level (e.g. Head)
 * - the one with the most recent rulebook version (e.g. 2024)
 * Duplicates (same level+version) are collapsed to a single badge.
 */
function pickHeaderCerts(certs: Certification[]): Certification[] {
  if (!certs?.length) return [];

  const byLevel = [...certs].sort(
    (a, b) => (LEVEL_RANK[b.level] ?? -1) - (LEVEL_RANK[a.level] ?? -1)
  )[0];

  const byVersion = [...certs].sort(
    (a, b) => (VERSION_RANK[b.version] ?? -1) - (VERSION_RANK[a.version] ?? -1)
  )[0];

  // Deduplicate when max-level and most-recent-version happen to be the same cert.
  const seen = new Set<string>();
  return [byLevel, byVersion].filter((cert) => {
    const key = `${cert.level}-${cert.version}`;
    if (seen.has(key)) return false;
    seen.add(key);
    return true;
  });
}

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

  const [showTooltip, setShowTooltip] = useState(false);

  const headerCerts = pickHeaderCerts(certifications ?? []);

  // Certs that are not shown as explicit badges (all beyond the max-2 selection).
  const displayedKeys = new Set(headerCerts.map((c) => `${c.level}-${c.version}`));
  const hiddenCerts = (certifications ?? []).filter(
    (c) => !displayedKeys.has(`${c.level}-${c.version}`)
  );

  const renderCertLabel = (cert: Certification) =>
    `${capitalize(cert.level === "snitch" ? "flag" : cert.level)} (${getRefereeCertVersion(cert)})`;

  const renderCertifications = () => {
    if (!headerCerts.length && !onTakeTests) return null;

    return (
      <div style={{ display: "flex", alignItems: "center", flexWrap: "wrap", gap: "0.5rem", marginTop: "0.5rem" }}>
        {headerCerts.map((cert) => (
          <span
            key={`${cert.level}-${cert.version}`}
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
            {renderCertLabel(cert)}
          </span>
        ))}

        {/* Overflow badge — shows remaining cert count with a hover tooltip */}
        {hiddenCerts.length > 0 && (
          <span
            style={{ position: "relative", display: "inline-flex" }}
            onMouseEnter={() => setShowTooltip(true)}
            onMouseLeave={() => setShowTooltip(false)}
          >
            <span
              style={{
                background: "#e5e7eb",
                color: "#374151",
                border: "1px solid #d1d5db",
                padding: "0.2rem 0.6rem",
                borderRadius: "9999px",
                fontSize: "0.75rem",
                fontWeight: 600,
                cursor: "default",
              }}
              aria-label={`${hiddenCerts.length} more certifications`}
            >
              +{hiddenCerts.length}
            </span>

            {showTooltip && (
              <div
                role="tooltip"
                style={{
                  position: "absolute",
                  bottom: "calc(100% + 6px)",
                  left: "50%",
                  transform: "translateX(-50%)",
                  background: "#1f2937",
                  color: "#f9fafb",
                  borderRadius: "0.375rem",
                  padding: "0.4rem 0.6rem",
                  fontSize: "0.75rem",
                  whiteSpace: "nowrap",
                  zIndex: 10,
                  pointerEvents: "none",
                  boxShadow: "0 2px 8px rgba(0,0,0,0.25)",
                }}
              >
                {hiddenCerts.map((cert) => (
                  <div key={`${cert.level}-${cert.version}`}>{renderCertLabel(cert)}</div>
                ))}
              </div>
            )}
          </span>
        )}

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

  const renderBio = () => (
    <ReactMarkdown
      remarkPlugins={[remarkGfm]}
      components={{
        // eslint-disable-next-line react/prop-types
        a: ({ href, children }) => (
          <a
            href={href}
            target="_blank"
            rel="noopener noreferrer"
            className="text-blue-600 underline"
          >
            {children}
          </a>
        ),
      }}
    >
      {user.bio ?? ''}
    </ReactMarkdown>
  );

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
          <div
            style={{
              marginTop: "0.75rem",
              fontSize: "1rem",
              color: "#374151",
              maxWidth: "48rem",
            }}
          >
            {renderBio()}
          </div>
        )}
      </div>
    </div>
  );
};

export default RefereeHeader;

