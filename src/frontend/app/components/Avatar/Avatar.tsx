import axios from "axios";
import React from "react";

import DropdownMenu, { ItemConfig } from "../DropdownMenu/DropdownMenu";
import { useNavigate } from "../../utils/navigationUtils";
import { useGetManagedTeamsQuery } from "../../store/serviceApi";

interface AvatarProps {
  firstName: string;
  lastName: string;
  roles: string[];
  userId: string;
  ownedNgbId: number;
  enabledFeatures: string[];
}

const Avatar = (props: AvatarProps) => {
  const { firstName, lastName, roles, userId, ownedNgbId, enabledFeatures } = props;

  const navigate = useNavigate();
  const { data: managedTeams } = useGetManagedTeamsQuery();

  const handleHomeClick = () => {
    navigate("/");
  };

  const handleLogoutClick = () => {
    window.location.href = `${window.location.origin}/sign_out`;
  };

  const handleInviteClick = () => {
    window.location.href = `${window.location.origin}/invitation`;
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
      <button onClick={onClick} className="avatar" type="button">
        {`${firstLetter}${lastLetter}`}
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
  const invite: ItemConfig = {
    content: "Invite NGB Admin",
    onClick: handleInviteClick,
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
