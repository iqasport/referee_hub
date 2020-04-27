import { fireEvent, render } from '@testing-library/react'
import React from 'react'

import { UpdateRefereeRequest } from '../../../apis/referee';
import { DataAttributes, IncludedAttributes } from '../../../schemas/getRefereeSchema';
import RefereeHeader from './RefereeHeader'

describe('RefereeHeader', () => {
  const referee: DataAttributes = {
    avatarUrl: null,
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
  const updatedValues: UpdateRefereeRequest = {
    bio: referee.bio,
    exportName: referee.exportName,
    firstName: referee.firstName,
    lastName: referee.lastName,
    ngbData: {},
    pronouns: referee.pronouns,
    showPronouns: referee.showPronouns,
    submittedPaymentAt: referee.submittedPaymentAt,
    teamsData: {},
  }
  const defaultProps = {
    certifications,
    id: '123',
    isEditing: false,
    isSaveDisabled: false,
    onChange: jest.fn(),
    onEditClick: jest.fn(),
    onSubmit: jest.fn(),
    referee,
    updatedValues,
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

  test('it calls the edit function on edit button click', () => {
    const { getByText } = render(<RefereeHeader {...defaultProps} />)

    const editButton = getByText('Edit')
    fireEvent.click(editButton)

    expect(defaultProps.onEditClick).toHaveBeenCalled()
  })

  describe('while editing', () => {
    const editProps = {
      ...defaultProps,
      isEditing: true
    }

    test('it renders a save button', () => {
      const { getByText } = render(<RefereeHeader {...editProps} />)

      expect(getByText('Save Changes')).toBeDefined()
    })

    test('it does not render certifications', () => {
      const { queryByText } = render(<RefereeHeader {...editProps} />)

      expect(queryByText('Snitch')).toBeNull()
      expect(queryByText('Assistant')).toBeNull()
    })

    test('it renders an export name toggle', () => {
      const { getByLabelText } = render(<RefereeHeader {...editProps} />)

      expect(getByLabelText('Export Name?')).toBeDefined()
    })

    test('it renders pronoun editing', () => {
      const { getByLabelText, getAllByRole } = render(<RefereeHeader {...editProps} />)

      expect(getByLabelText('Show Pronouns?')).toBeDefined()
      expect(getAllByRole('textbox')[0].getAttribute('value')).toEqual(referee.pronouns)
    })

    test('it renders bio editing', () => {
      const { getAllByRole } = render(<RefereeHeader {...editProps} />)

      const bio = getAllByRole('textbox')[1]

      expect(bio.innerHTML).toEqual(referee.bio)
    })

    test('it fires the change event when a value has changed', () => {
      const { getByLabelText } = render(<RefereeHeader {...editProps} />)

      fireEvent.click(getByLabelText('Show Pronouns?'))

      expect(defaultProps.onChange).toHaveBeenCalledWith(false, "showPronouns")
    })

    describe('without a name', () => {
      const noNameProps = {
        ...editProps,
        referee: {
          ...referee,
          firstName: null,
          lastName: null
        },
        updatedValues: {
          ...updatedValues,
          firstName: null,
          lastName: null,
        }
      }

      test('it renders name inputs', () => {
        const { getAllByRole } = render(<RefereeHeader {...noNameProps} />)

        const allInputs = getAllByRole('textbox')
        const firstName = allInputs[0]
        const lastName = allInputs[1]

        expect(firstName.getAttribute('value')).toEqual("")
        expect(lastName.getAttribute('value')).toEqual("")
      })
    })
  })
})
