import { faCaretDown } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

import DropdownMenu from "../../components/DropdownMenu";

interface ActionsButtonProps {
  onTestClick: () => void;
  onImportClick: () => void;
  onNgbClick: () => void;
}

const ActionsButton = (props: ActionsButtonProps) => {
  const { onTestClick, onImportClick, onNgbClick } = props;

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
      content: "Create NGB",
      onClick: onNgbClick,
    },
    {
      content: "Create Test",
      onClick: onTestClick,
    },
    {
      content: "Import NGBs",
      onClick: onImportClick,
    },
  ];

  return <DropdownMenu renderTrigger={renderTrigger} items={items} />;
};

export default ActionsButton;
