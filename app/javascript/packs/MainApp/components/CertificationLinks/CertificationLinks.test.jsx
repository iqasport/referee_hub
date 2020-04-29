import React from 'react'
import { DateTime } from 'luxon'

import { render, screen } from '../../utils/test-utils';
import CertificationLinks from '.'

describe('CertificationLinks', () => {
  const defaultProps = {
    hasPaid: true,
    refereeId: '1',
    refCertifications: [{ id: '1', level: 'snitch' }],
    levelsThatNeedRenewal: [],
    testAttempts: [{ next_attempt_at: DateTime.local().toString(), test_level: 'snitch' }],
    onRouteChange: jest.fn()
  }

  test('it renders', () => {
    render(<CertificationLinks {...defaultProps} />)

    expect(screen.getByText('Available Written Tests')).toBeDefined()
  })
})
