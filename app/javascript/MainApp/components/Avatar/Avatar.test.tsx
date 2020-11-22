import React from 'react'

import { render, screen } from '../../utils/test-utils'

import Avatar from './'

describe('Avatar', () => {
  const defaultProps = {
    enabledFeatures: [],
    firstName: 'Quidditch',
    lastName: 'Rocks',
    ownedNgbId: 1234,
    roles: ['referee'],
    userId: '123',
  }

  test('it is rendered', () => {
    render(<Avatar {...defaultProps} />)

    screen.getByText('QR')
  })
})
