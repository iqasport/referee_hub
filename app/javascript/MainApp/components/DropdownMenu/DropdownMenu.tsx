import React, { useState } from 'react'

export type ItemConfig = {
  content: string;
  onClick: () => void;
}

interface DropdownMenuProps {
  renderTrigger: (onClick: () => void) => void;
  items: ItemConfig[];
}

const DropdownMenu = (props: DropdownMenuProps) => {
  const { renderTrigger, items } = props
  const [isDropdownOpen, setIsDropdownOpen] = useState(false)

  const handleClose = () => setIsDropdownOpen(false)
  const handleToggle = () => setIsDropdownOpen(!isDropdownOpen)

  const renderItems = (item: ItemConfig) => {
    const handleClick = () => {
      handleClose()
      item.onClick()
    }

    return (
      <li key={item.content} className="block px-4 py-2 text-black hover:bg-gray-300 text-left" onClick={handleClick}>
        <button type="button" className="appearance-none" onClick={handleClick}>
          {item.content}
        </button>
      </li>
    )
  }

  return (
    <div className="relative">
      {renderTrigger(handleToggle)}
      {isDropdownOpen && (
        <button
          onClick={handleClose}
          type="button"
          tabIndex={-1}
          className="fixed inset-0 h-full w-full cursor-default"
        />
      )}
      {isDropdownOpen && (
        <div
          className="bg-white rounded py-2 min-w-32 mt-1 shadow-lg absolute right-0 z-1"
          style={{ minWidth: 'max-content' }}
        >
          <ul>
            {items.map(renderItems)}
          </ul>
        </div>
      )}
    </div>
  )
}

export default DropdownMenu
