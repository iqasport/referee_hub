import { faCaretDown } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

import DropdownMenu from "../../components/DropdownMenu";

interface ActionsButtonProps {
  onEditClick: () => void;
  onImportClick: () => void;
  onExportClick: () => void;
  onCreateTeamClick: () => void;
}

const ActionsButton = (props: ActionsButtonProps) => {
  const { onEditClick, onImportClick, onExportClick, onCreateTeamClick } = props;

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
