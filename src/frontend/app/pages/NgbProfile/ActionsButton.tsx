import { faCaretDown } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

import DropdownMenu from "../../components/DropdownMenu";

interface ActionsButtonProps {
  onEditClick: () => void;
  onImportClick: () => void;
  onExportClick: () => void;
  onTeamClick: () => void;
}

const ActionsButton = (props: ActionsButtonProps) => {
  const { onEditClick, onImportClick, onExportClick, onTeamClick } = props;

  const renderTrigger = (onClick: () => void) => {
    return (
      <button onClick={onClick} className="flex items-center green-button-outline z-1 relative">
        Actions
        <FontAwesomeIcon icon={faCaretDown} className="ml-4" />
      </button>
    );
  };

  const items = [
    {
      content: "Edit",
      onClick: onEditClick,
    },
    {
      content: "Create Team",
      onClick: onTeamClick,
    },
    {
      content: "Import",
      onClick: onImportClick,
    },
    {
      content: "Export",
      onClick: onExportClick,
    },
  ];

  return <DropdownMenu renderTrigger={renderTrigger} items={items} />;
};

export default ActionsButton;
