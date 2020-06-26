import React from 'react'
import { render } from '../../utils/test-utils'

import Avatar from './Avatar'

describe('Avatar', () => {
  const defaultProps = {
    firstName: 'Alexis',
    lastName: 'Rocks'
  }

  test('it is rendered', () => {
    const { getByText } = render(<Avatar {...defaultProps} />)

    expect(getByText('AR')).toBeDefined()
  })
})
