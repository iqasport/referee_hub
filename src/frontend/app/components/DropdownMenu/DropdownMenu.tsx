import React, { useState } from "react";

export type ItemConfig = {
  content: string;
  onClick: () => void;
};

interface DropdownMenuProps {
  renderTrigger: (onClick: () => void) => React.ReactNode;
  items: ItemConfig[];
}

const DropdownMenu = (props: DropdownMenuProps) => {
  const { renderTrigger, items } = props;
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);

  const handleClose = () => setIsDropdownOpen(false);
  const handleToggle = () => setIsDropdownOpen(!isDropdownOpen);

  const renderItems = (item: ItemConfig) => {
    const handleClick = () => {
      handleClose();
      item.onClick();
    };

    return (
      <li
        key={item.content}
        className="block px-4 py-2 text-black hover:bg-gray-300 text-left"
        onClick={handleClick}
      >
        <button type="button" className="appearance-none" onClick={handleClick}>
          {item.content}
        </button>
      </li>
    );
  };

  return (
    <div className="relative">
      {renderTrigger(handleToggle)}
      {isDropdownOpen && (
        <button
          onClick={handleClose}
          type="button"
          tabIndex={-1}
          className="fixed inset-0 h-full w-full cursor-default"
          // This button covers the whole screen to hide the dropdown menu on click
          // The dropdown can be obstructed and the container it's hooked to needs to scroll down
          // This event handler will allow us to apply scroll event underneath the button
          onWheel={event => {
            (event.target as HTMLElement).style.pointerEvents = 'none'; // Temporarily disable pointer events
            
            // Trigger the wheel event on the element underneath the button
            const underlyingElement = document.elementFromPoint(event.clientX, event.clientY);
            if (underlyingElement) {
              const wheelEvent = new WheelEvent('wheel', {
                deltaX: event.deltaX,
                deltaY: event.deltaY,
                clientX: event.clientX,
                clientY: event.clientY,
                bubbles: true,
                cancelable: true,
              });
              underlyingElement.dispatchEvent(wheelEvent);
            }
          
            // Re-enable pointer events on the button after a short delay
            setTimeout(() => {
              (event.target as HTMLElement).style.pointerEvents = 'auto';
            }, 0);
          }}
        />
      )}
      {isDropdownOpen && (
        <div
          className="bg-white rounded py-2 min-w-32 mt-1 shadow-lg absolute right-0 z-3"
          style={{ minWidth: "max-content" }}
        >
          <ul>{items.map(renderItems)}</ul>
        </div>
      )}
    </div>
  );
};

export default DropdownMenu;
