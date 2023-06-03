import { faEllipsisV } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

import DropdownMenu from "../../DropdownMenu";

interface ActionDropdownProps {
  ngbId: string;
  onEditClick: (teamId: string) => void;
}

const ActionDropdown = (props: ActionDropdownProps) => {
  const { ngbId, onEditClick } = props;

  const renderTrigger = (onClick: () => void) => {
    return (
      <button className="relative text-navy-blue" onClick={onClick}>
        <FontAwesomeIcon icon={faEllipsisV} />
      </button>
    );
  };

  const handleEditOpen = () => onEditClick(ngbId);

  const items = [
    {
      content: "Edit",
      onClick: handleEditOpen,
    },
  ];

  return <DropdownMenu renderTrigger={renderTrigger} items={items} />;
};

export default ActionDropdown;
