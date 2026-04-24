import { faEllipsisV } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

import DropdownMenu from "../../DropdownMenu";

interface ActionDropdownProps {
  userId: string;
  onRenameClick: (userId: string) => void;
}

const ActionDropdown = ({ userId, onRenameClick }: ActionDropdownProps) => {
  const renderTrigger = (onClick: () => void) => {
    return (
      <button className="relative text-navy-blue" onClick={onClick} type="button">
        <FontAwesomeIcon icon={faEllipsisV} />
      </button>
    );
  };

  const items = [
    {
      content: "Rename",
      onClick: () => onRenameClick(userId),
    },
  ];

  return <DropdownMenu renderTrigger={renderTrigger} items={items} />;
};

export default ActionDropdown;
