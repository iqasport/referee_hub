import axios from 'axios'
import PropTypes from 'prop-types'
import React, { useState } from 'react';
import { useHistory } from 'react-router-dom';

const Avatar = (props) => {
  const { firstName, lastName } = props
  if (!firstName || !lastName) return null

  const [isDropdownOpen, setIsDropdownOpen] = useState(false)
  const history = useHistory()

  const handleClose = () => setIsDropdownOpen(false)
  const handleToggle = () => setIsDropdownOpen(!isDropdownOpen)

  const handleHomeClick = () => {
    handleClose()
    history.push('/')
  }

  const handleLogoutClick = () => {
    // eslint-disable-next-line no-undef
    const token = document.getElementsByName('csrf-token')[0].getAttribute('content')
    axios.defaults.headers.common['X-CSRF-Token'] = token
    // eslint-disable-next-line dot-notation
    axios.defaults.headers.common['Accept'] = 'application/json'
    axios.delete('/sign_out').then(() => {
      // eslint-disable-next-line no-undef
      window.location = `${window.location.origin}/sign_in`;
    })
  }

  return (
    <div className="relative">
      <button onClick={handleToggle} className="avatar" type="button">{`${firstName[0]}${lastName[0]}`}</button>
      {isDropdownOpen && (
        <button
          onClick={handleClose}
          type="button"
          tabIndex={-1}
          className="fixed inset-0 h-full w-full cursor-default"
        />
      )}
      {isDropdownOpen && (
        <div className="bg-white rounded py-2 w-32 mt-1 shadow-lg absolute right-0 z-1">
          <ul>
            <li className="block px-4 py-2 text-black hover:bg-gray-300 text-left">
              <button type="button" className="appearance-none" onClick={handleHomeClick}>
                Home
              </button>
            </li>
            <li className="block px-4 py-2 text-black hover:bg-gray-300 text-left">
              <button onClick={handleLogoutClick} type="button">
                Logout
              </button>
            </li>
          </ul>
        </div>
      )}
    </div>
  );
}

Avatar.propTypes = {
  firstName: PropTypes.string.isRequired,
  lastName: PropTypes.string.isRequired
}
export default Avatar;
