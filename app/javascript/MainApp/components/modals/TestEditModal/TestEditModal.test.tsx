import userEvent from '@testing-library/user-event'
import React from 'react'

import factories from '../../../factories'
import {
  fireEvent,
  mockedStore,
  render,
  screen,
} from "../../../utils/test-utils";

import TestEditModal from './TestEditModal'

describe('TestEditModal', () => {
  const languages = factories.language.buildList(5)
  const certifications = factories.certification.buildList(4)
  const defaultStore = {
    certifications: {
      certifications
    },
    languages: {
      languages
    },
    test: {
      certification: {},
      test: {},
    }
  }
  const mockStore = mockedStore(defaultStore)
  const defaultProps = {
    open: true,
    showClose: true
  }

  it('renders an empty form', async () => {
    render(<TestEditModal {...defaultProps} />, mockStore)

    screen.getByText('New Test')
  });

  it('renders the languages', () => {
    render(<TestEditModal {...defaultProps} />, mockStore)

    const selectElement = screen.getByPlaceholderText('Select the language')
    fireEvent.click(selectElement)
    languages.forEach((lang) => {
      screen.getAllByText(`${lang.attributes.longName} - ${lang.attributes.shortRegion}`)
    })
  })

  describe('with a testId', () => {
    const singleTest = factories.test.build()
    const editMockStore = mockedStore({
      ...defaultStore,
      test: {
        certification: certifications[0],
        test: singleTest
      }
    })
    const editProps = {
      ...defaultProps,
      testId: singleTest.id
    }

    it('renders the edit form', () => {
      render(<TestEditModal {...editProps} />, editMockStore);

      screen.getByText('Edit Test')
    })

    it('handles input changes', () => {
      render(<TestEditModal {...editProps} />, editMockStore)

      const descInput = screen.getByText(singleTest.attributes.description)
      const newDesc = 'A new description'
      userEvent.clear(descInput)
      userEvent.type(descInput, newDesc)

      expect(descInput).toHaveValue(newDesc)
    })

    it('handles dropdown changes', () => {
      render(<TestEditModal {...editProps} />, editMockStore);

      const certElement = screen.getByPlaceholderText('Select the level')
      userEvent.selectOptions(certElement, ['snitch'])

      expect(certElement).toHaveDisplayValue('Snitch')
    })

    it('enables the submit button after a change', () => {
      render(<TestEditModal {...editProps} />, editMockStore);

      const submitButton = screen.getByText('Done')
      expect(submitButton).toBeDisabled()

      const descInput = screen.getByText(singleTest.attributes.description);
      userEvent.type(descInput, 'a change');

      expect(submitButton).toBeEnabled()
    })
  })
})
