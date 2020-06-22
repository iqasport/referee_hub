import axios from 'axios'
import PropTypes from 'prop-types'
import React from 'react';
import { useHistory } from 'react-router-dom';

import DropdownMenu from '../DropdownMenu'

interface AvatarProps {
  firstName: string;
  lastName: string;
}

const Avatar = (props: AvatarProps) => {
  const { firstName, lastName } = props
  if (!firstName || !lastName) return null

  const history = useHistory()

  const handleHomeClick = () => {
    history.push('/')
  }

  const handleLogoutClick = () => {
    const token = document.getElementsByName('csrf-token')[0].getAttribute('content')
    axios.defaults.headers.common['X-CSRF-Token'] = token
    axios.defaults.headers.common.Accept = 'application/json'
    axios.delete('/sign_out').then(() => {
      window.location.href = `${window.location.origin}/sign_in`;
    })
  }

  const renderTrigger = onClick => (
    <button onClick={onClick} className="avatar" type="button">
      {`${firstName[0]}${lastName[0]}`}
    </button>
  )

  const items = [
    {
      content: 'Home',
      onClick: handleHomeClick,
    },
    {
      content: 'Logout',
      onClick: handleLogoutClick
    }
  ]

  return <DropdownMenu renderTrigger={renderTrigger} items={items} />
}

export default Avatar;
