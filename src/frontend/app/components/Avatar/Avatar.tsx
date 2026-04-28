import { faBars } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

import DropdownMenu, { ItemConfig } from "../DropdownMenu/DropdownMenu";
import { useNavigate } from "../../utils/navigationUtils";
import { useGetCurrentUserAvatarQuery, useGetManagedTeamsQuery } from "../../store/serviceApi";

interface AvatarProps {
  firstName: string;
  lastName: string;
  roles: string[];
  userId: string;
  ownedNgbId: number;
  unreadNotifications: number;
  enabledFeatures: string[];
}

const Avatar = (props: AvatarProps) => {
  const { firstName, lastName, roles, userId, ownedNgbId, unreadNotifications, enabledFeatures } = props;

  const navigate = useNavigate();
  const { data: managedTeams } = useGetManagedTeamsQuery();
  const { data: avatarUrl } = useGetCurrentUserAvatarQuery();

  const handleHomeClick = () => {
    navigate("/");
  };

  const handleLogoutClick = () => {
    window.location.href = `${window.location.origin}/sign_out`;
  };

  const handleRefProfileClick = () => {
    navigate(`/referees/${userId}`);
  };

  const handleNgbProfileClick = () => {
    navigate(`/national_governing_bodies/${ownedNgbId}`);
  };

  const handleSettingsClick = () => {
    navigate("/settings");
  };

  const handleTournamentsClick = () => {
    navigate("/tournaments");
  };

  const renderTrigger = (onClick) => {
    const firstLetter = firstName ? firstName[0] : "A";
    const lastLetter = lastName ? lastName[0] : "R";

    return (
      <button onClick={onClick} className="flex items-center gap-2 cursor-pointer" type="button">
        <span className="avatar">
          {avatarUrl
            ? <img src={avatarUrl} alt={`Profile picture of ${firstName} ${lastName}`} className="rounded-full w-full h-full object-cover" />
            : `${firstLetter}${lastLetter}`}
          {unreadNotifications > 0 && (
            <span className="absolute -top-2 -right-2 bg-red-600 text-white text-xs font-bold rounded-full min-w-5 h-5 px-1 flex items-center justify-center">
              {unreadNotifications > 99 ? "99+" : unreadNotifications}
            </span>
          )}
        </span>
        <FontAwesomeIcon icon={faBars} className="text-white" />
      </button>
    );
  };

  const home: ItemConfig = {
    content: "Home",
    onClick: handleHomeClick,
  };
  const refereeProfile: ItemConfig = {
    content: "Referee Profile",
    onClick: handleRefProfileClick,
  };
  const ngbProfile: ItemConfig = {
    content: "NGB Profile",
    onClick: handleNgbProfileClick,
  };
  const logout: ItemConfig = {
    content: "Logout",
    onClick: handleLogoutClick,
  };
  const settings: ItemConfig = {
    content: "Settings",
    onClick: handleSettingsClick,
  };
  const tournaments: ItemConfig = {
    content: "Tournaments",
    onClick: handleTournamentsClick,
  };

  const items: ItemConfig[] = [home];

  if (enabledFeatures?.includes("i18n")) items.push(settings);

  if (roles.includes("NgbAdmin")) items.push(ngbProfile);
  if (roles.includes("Referee")) items.push(refereeProfile);
  //if (roles.includes("NgbAdmin") || roles.includes("IqaAdmin")) items.push(invite); // TODO: unblock once implemented

  // Add managed teams section
  if (managedTeams && managedTeams.length > 0) {
    managedTeams.forEach((team) => {
      const teamItem: ItemConfig = {
        content: `${team.teamName} (Team)`,
        onClick: () => navigate(`/teams/${team.teamId}/manage`),
      };
      items.push(teamItem);
    });
  }

  items.push(tournaments);
  items.push(logout);

  return <DropdownMenu renderTrigger={renderTrigger} items={items} />;
};

export default Avatar;
