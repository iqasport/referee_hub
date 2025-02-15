import { faCaretDown } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

import DropdownMenu from "../../components/DropdownMenu";
import { ItemConfig } from "../../components/DropdownMenu/DropdownMenu";

interface ActionsButtonProps {
  onEditClick: () => void;
  onImportClick: () => void;
  onExportClick: () => void;
  onCreateTeamClick: () => void;
  onManageAdminsClick: () => void;
}

const ActionsButton = (props: ActionsButtonProps) => {
  const { onEditClick, onImportClick, onExportClick, onCreateTeamClick, onManageAdminsClick } = props;

  const renderTrigger = (onClick: () => void) => {
    return (
      <button onClick={onClick} className="flex items-center green-button-outline z-1 relative">
        Actions
        <FontAwesomeIcon icon={faCaretDown} className="ml-4" />
      </button>
    );
  };

  const items: ItemConfig[] = [
    {
      content: "Edit",
      onClick: onEditClick,
    },
    {
      content: "Manage admins",
      onClick: onManageAdminsClick,
    },
    {
      content: "Create Team",
      onClick: onCreateTeamClick,
    },
    /*{
      content: "Import",
      onClick: onImportClick,
    },*/ // TODO: unblock functionality once implemented
    {
      content: "Export",
      onClick: onExportClick,
    },
  ];

  return <DropdownMenu renderTrigger={renderTrigger} items={items} />;
};

export default ActionsButton;
