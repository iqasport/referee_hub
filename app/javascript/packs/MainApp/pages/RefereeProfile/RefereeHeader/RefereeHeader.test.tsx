import { render } from '@testing-library/react'
import React from 'react'

import { DataAttributes, IncludedAttributes } from '../../../schemas/getRefereeSchema';
import RefereeHeader from './RefereeHeader'

describe('RefereeHeader', () => {
  const referee: DataAttributes = {
    bio: 'words',
    exportName: false,
    firstName: 'Build',
    hasPendingPolicies: false,
    isEditable: true,
    lastName: 'Stuff',
    pronouns: 'she/her',
    showPronouns: true,
    submittedPaymentAt: new Date(),
  }
  const certifications: IncludedAttributes[] = [
    {
      level: 'snitch',
    },
    {
      level: 'assistant'
    }
  ]
  const defaultProps = {
    certifications,
    referee,
  }

  test('it renders the referee name', () => {
    const { getByText } = render(<RefereeHeader {...defaultProps} />)
    const fullName = `${referee.firstName} ${referee.lastName}`
    expect(getByText(fullName)).toBeDefined()
  })

  test('it renders a default name if names not present', () => {
    const noName: DataAttributes = {
      ...referee,
      firstName: null,
      lastName: null
    }
    const noNameProps = {
      ...defaultProps,
      referee: noName,
    }
    const { getByText } = render(<RefereeHeader {...noNameProps} />)

    expect(getByText('Anonymous Referee')).toBeDefined()
  })

  test('it renders pronouns', () => {
    const { getByText } = render(<RefereeHeader {...defaultProps} />)

    expect(getByText(referee.bio)).toBeDefined()
  })

  test("it doesn't render pronouns when show pronouns is false", () => {
    const noPronounsProps = {
      ...defaultProps,
      referee: {
        ...referee,
        showPronouns: false,
      }
    }
    const { queryByText } = render(<RefereeHeader {...noPronounsProps} />)

    expect(queryByText(referee.pronouns)).toBeNull()
  })

  test('it renders certifications', () => {
    const { getByText } = render(<RefereeHeader {...defaultProps} />)

    expect(getByText('Snitch')).toBeDefined()
    expect(getByText('Assistant')).toBeDefined()
  })

  test('it renders the bio', () => {
    const { getByText } = render(<RefereeHeader {...defaultProps} />)

    expect(getByText(referee.bio)).toBeDefined()
  })
})
