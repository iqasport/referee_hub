import axios from 'axios'
import React from 'react';
import { useHistory } from 'react-router-dom';

import DropdownMenu, { ItemConfig } from '../DropdownMenu/DropdownMenu';

interface AvatarProps {
  firstName: string;
  lastName: string;
  roles: string[];
  userId: string;
  ownedNgbId: number;
}

const Avatar = (props: AvatarProps) => {
  const { firstName, lastName, roles, userId, ownedNgbId } = props

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

  const handleInviteClick = () => {
    window.location.href = `${window.location.origin}/invitation`
  }

  const handleRefProfileClick = () => {
    history.push(`/referees/${userId}`)
  }

  const handleNgbProfileClick = () => {
    history.push(`/national_governing_bodies/${ownedNgbId}`)
  }

  const renderTrigger = onClick => {
    const firstLetter = firstName ? firstName[0] : 'A'
    const lastLetter = lastName ? lastName[0] : 'R'

    return (
      <button onClick={onClick} className="avatar" type="button">
        {`${firstLetter}${lastLetter}`}
      </button>
    )
  }

  const home: ItemConfig = {
    content: 'Home',
    onClick: handleHomeClick,
  }
  const refereeProfile: ItemConfig = {
    content: 'Referee Profile',
    onClick: handleRefProfileClick
  }
  const ngbProfile: ItemConfig = {
    content: 'NGB Profile',
    onClick: handleNgbProfileClick
  }
  const invite: ItemConfig = {
    content: 'Invite NGB Admin',
    onClick: handleInviteClick
  }
  const logout: ItemConfig = {
    content: 'Logout',
    onClick: handleLogoutClick
  }

  const items: ItemConfig[] = [home]

  if (roles.length > 1) {
    const isNgbAndIqa = roles.includes('ngb_admin') && roles.includes('iqa_admin')
    const isRefereeAndNgb = roles.includes('ngb_admin') && roles.includes('referee')
    const isNgbAndRefereeAndIqa = roles.includes('ngb_admin')
      && roles.includes('iqa_admin')
      && roles.includes('referee')

    if (isNgbAndRefereeAndIqa) {
      items.push(...[ngbProfile, refereeProfile, invite])
    } else if (isNgbAndIqa) {
      items.push(...[ngbProfile, invite])
    } else if (isRefereeAndNgb) {
      items.push(...[refereeProfile])
    }
  } else if (roles.includes('iqa_admin')) {
    items.push(...[invite])
  }

  items.push(logout)

  return <DropdownMenu renderTrigger={renderTrigger} items={items} />
}

export default Avatar;
