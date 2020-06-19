import { faCaretDown } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import React, { useState } from 'react'

interface ActionsButtonProps {
  onEditClick: () => void;
  onImportClick: () => void;
  onExportClick: () => void;
  onTeamClick: () => void;
}

const ActionsButton = (props: ActionsButtonProps) => {
  const { onEditClick, onImportClick, onExportClick, onTeamClick } = props;
  const [isDropdownActive, setIsDropdownActive] = useState(false)
  
  const handleActionClick = () => setIsDropdownActive(!isDropdownActive)
  const handleActionClose = () => setIsDropdownActive(false)
  const handleEditClick = () => {
    handleActionClose()
    onEditClick()
  }
  const handleImportClick = () => {
    handleActionClose()
    onImportClick()
  }
  const handleExportClick = () => {
    handleActionClose()
    onExportClick()
  }
  const handleTeamClick = () => {
    handleActionClose()
    onTeamClick()
  }

  return (
    <div className="relative">
      <button onClick={handleActionClick} className="flex items-center green-button-outline z-1 relative">
        Actions
        <FontAwesomeIcon icon={faCaretDown} className="ml-4" />
      </button>
      {isDropdownActive && <button onClick={handleActionClose} tabIndex={-1} className="fixed inset-0 h-full w-full cursor-default" />}
      {isDropdownActive && (
        <div className="bg-white rounded py-2 w-32 mt-1 shadow-lg absolute right-0 z-1">
          <ul>
            <li className="block px-4 py-2 hover:bg-gray-300">
              <button type="button" onClick={handleEditClick}>
                Edit
              </button>
            </li>
            <li className="block px-4 py-2 hover:bg-gray-300">
              <button type="button" onClick={handleTeamClick}>
                Create Team
              </button>
            </li>
            <li className="block px-4 py-2 hover:bg-gray-300">
              <button onClick={handleImportClick} type="button">
                Import
              </button>
            </li>
            <li className="block px-4 py-2 hover:bg-gray-300">
              <button onClick={handleExportClick} type="button">
                Export
              </button>
            </li>
          </ul>
        </div>
      )}
    </div>
  )
}

export default ActionsButton
