import { faEllipsisV } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import React from 'react'

import DropdownMenu from '../../DropdownMenu'

interface ActionDropdownProps {
  teamId: string;
  onEditClick: (teamId: string) => void;
  onDeleteClick: (teamId: string) => void;
}

const ActionDropdown = (props: ActionDropdownProps) => {
  const { teamId, onEditClick, onDeleteClick } = props

  const renderTrigger = (onClick: () => void) => {
    return (
      <button className="relative text-navy-blue" onClick={onClick}>
        <FontAwesomeIcon icon={faEllipsisV} />
      </button>
    )
  }

  const handleEditOpen = () => onEditClick(teamId)
  const handleDeleteOpen = () => onDeleteClick(teamId)

  const items = [
    {
      content: 'Edit',
      onClick: handleEditOpen
    },
    {
      content: 'Delete',
      onClick: handleDeleteOpen,
    }
  ]

  return <DropdownMenu renderTrigger={renderTrigger} items={items} />
}

export default ActionDropdown
