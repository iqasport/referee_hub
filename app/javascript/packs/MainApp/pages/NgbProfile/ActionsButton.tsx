import React, { useState } from 'react'

interface ActionsButtonProps {
  onEditClick: () => void;
  onImportClick: () => void;
  onExportClick: () => void;
}

const ActionsButton = (props: ActionsButtonProps) => {
  const { onEditClick, onImportClick, onExportClick } = props;
  const [isDropdownActive, setIsDropdownActive] = useState(false)
  
  const handleActionClick = () => setIsDropdownActive(!isDropdownActive)
  const handleEditClick = () => {
    handleActionClick()
    onEditClick()
  }
  const handleImportClick = () => {
    handleActionClick()
    onImportClick()
  }
  const handleExportClick = () => {
    handleActionClick()
    onExportClick()
  }

  return (
    <>
      <button onClick={handleActionClick} className="rounded bg-white border-2 border-green text-green py-2 px-4 uppercase">Actions</button>
      <div className={`avatar-dropdown ${isDropdownActive && 'dropdown-visible'}`}>
        <ul>
          <li>
            <button type="button" className="appearance-none" onClick={handleEditClick}>
              Edit
            </button>
          </li>
          <li>
            <button onClick={handleImportClick} type="button">
              Import
            </button>
          </li>
          <li>
            <button onClick={handleExportClick} type="button">
              Export
            </button>
          </li>
        </ul>
      </div>
    </>
  )
}

export default ActionsButton
