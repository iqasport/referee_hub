import { faEllipsisV } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import React from 'react'

import DropdownMenu from '../DropdownMenu'

interface ActionDropdownProps {
  testId: string;
  onEditClick: (testId: string) => void;
  onDeleteClick: (testId: string) => void;
  onActiveToggle: (testId: string) => void;
}

const ActionDropdown = (props: ActionDropdownProps) => {
  const { testId, onEditClick, onDeleteClick, onActiveToggle } = props

  const renderTrigger = (onClick: () => void) => {
    return (
      <button className="relative text-navy-blue" onClick={onClick}>
        <FontAwesomeIcon icon={faEllipsisV} />
      </button>
    )
  }

  const handleEditOpen = () => onEditClick(testId)
  const handleDeleteOpen = () => onDeleteClick(testId)
  const handleActiveToggle = () => onActiveToggle(testId)

  const items = [
    {
      content: 'Toggle Active',
      onClick: handleActiveToggle,
    },
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
