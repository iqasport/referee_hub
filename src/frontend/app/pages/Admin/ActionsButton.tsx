import { faCaretDown } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

import DropdownMenu from "../../components/DropdownMenu";

interface ActionsButtonProps {
  onTestClick: () => void;
  onImportClick?: () => void;
  onNgbClick?: () => void;
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
    onNgbClick
      ? {
          content: "Create NGB",
          onClick: onNgbClick,
        }
      : null,
    {
      content: "Create Test",
      onClick: onTestClick,
    },
    onImportClick
      ? {
          content: "Import NGBs",
          onClick: onImportClick,
        }
      : null,
  ].filter((item): item is { content: string; onClick: () => void } => item !== null);

  return <DropdownMenu renderTrigger={renderTrigger} items={items} />;
};

export default ActionsButton;
