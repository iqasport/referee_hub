import axios from 'axios'
import PropTypes from 'prop-types'
import React, { useState } from 'react';
import { useHistory } from 'react-router-dom';

const Avatar = (props) => {
  const { firstName, lastName } = props
  if (!firstName || !lastName) return null

  const [isDropdownOpen, setIsDropdownOpen] = useState(false)
  const history = useHistory()

  const handleOpen = () => setIsDropdownOpen(true)
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
    <div tabIndex={0} className="avatar" role="button" onClick={handleToggle} onKeyPress={handleOpen}>
      <span>{`${firstName[0]}${lastName[0]}`}</span>
      <div className={`avatar-dropdown ${isDropdownOpen && 'dropdown-visible'}`}>
        <ul>
          <li>
            <button type="button" className="appearance-none" onClick={handleHomeClick}>
              Home
            </button>
          </li>
          <li>
            <button onClick={handleLogoutClick} type="button">
              Logout
            </button>
          </li>
        </ul>
      </div>
    </div>
  );
}

Avatar.propTypes = {
  firstName: PropTypes.string.isRequired,
  lastName: PropTypes.string.isRequired
}
export default Avatar;
