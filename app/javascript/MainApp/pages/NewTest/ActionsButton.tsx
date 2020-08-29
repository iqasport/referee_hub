import { faCaretDown } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import React from 'react'

import DropdownMenu from '../../components/DropdownMenu';

interface ActionsButtonProps {
  onEditClick: () => void;
  onImportClick: () => void;
  onDeleteClick: () => void;
  onExportClick: () => void;
}

const ActionsButton = (props: ActionsButtonProps) => {
  const { onEditClick, onImportClick, onDeleteClick, onExportClick } = props;

  const renderTrigger = (onClick: () => void) => {
    return (
      <button onClick={onClick} className="flex items-center green-button-outline z-1 relative">
        Actions
        <FontAwesomeIcon icon={faCaretDown} className="ml-4" />
      </button>
    )
  }

  const items = [
    {
      content: 'Import Questions',
      onClick: onImportClick,
    },
    {
      content: 'Export Questions',
      onClick: onExportClick,
    },
    {
      content: 'Edit',
      onClick: onEditClick,
    },
    {
      content: 'Delete',
      onClick: onDeleteClick,
    },
  ]

  return <DropdownMenu renderTrigger={renderTrigger} items={items} />
}

export default ActionsButton