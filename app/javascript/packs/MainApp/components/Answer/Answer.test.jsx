import React from 'react'
import { screen, render, fireEvent } from '@testing-library/react'
import Answer from '.'

jest.mock('../RichTextEditor', () => ({ onChange }) => (
  <input data-testid="text-editor" onChange={event => onChange(event, { value: event.target.value })} />
))

describe('Answer', () => {
  const defaultProps = {
    values: {
      description: 'Yes',
      id: 'answer-1'
    },
    isCorrect: true,
    onSave: jest.fn(),
    onCorrectChange: jest.fn(),
    onDelete: jest.fn(),
    isEditable: true
  }


  test('it is rendered', () => {
    render(<Answer {...defaultProps} />)

    expect(screen.getByText(defaultProps.values.description)).toBeDefined()
  })

  test('it is editable', () => {
    render(<Answer {...defaultProps} />)

    fireEvent.click(screen.getByTestId('edit'))

    expect(screen.getByTestId('save')).toBeDefined()
  })

  test('it calls the onSave callback on save click', () => {
    render(<Answer {...defaultProps} />)

    fireEvent.click(screen.getByTestId('edit'))
    fireEvent.change(screen.getByTestId('text-editor'), { target: { value: 'a' } })

    const saveButton = screen.getByTestId('save')
    expect(saveButton).toBeDefined()
    fireEvent.click(saveButton)

    expect(defaultProps.onSave).toHaveBeenCalled()
  })

  test('it changes button state on close', () => {
    render(<Answer {...defaultProps} />)

    fireEvent.click(screen.getByTestId('edit'))
    expect(screen.getByTestId('save')).toBeDefined()
    expect(screen.getByTestId('close')).toBeDefined()

    fireEvent.click(screen.getByTestId('close'))
    expect(screen.queryByTestId('save')).toBeNull()
    expect(screen.queryByTestId('close')).toBeNull()
  })

  test('it is deletable', () => {
    render(<Answer {...defaultProps} />)

    fireEvent.click(screen.getByTestId('delete'))

    expect(defaultProps.onDelete).toHaveBeenCalled()
  })

  test('it is selectable', () => {
    render(<Answer {...defaultProps} />)

    fireEvent.click(screen.getByTestId('edit'))
    fireEvent.click(screen.getByTestId('checkbox'))

    expect(defaultProps.onCorrectChange).toHaveBeenCalled()
  })

  describe('when not editable', () => {
    const nonEditableProps = { ...defaultProps, isEditable: false }

    test('it does not render the edit button', () => {
      render(<Answer {...nonEditableProps} />)

      expect(screen.queryByTestId('edit')).toBeNull()
      expect(screen.queryByTestId('delete')).toBeNull()
    })
  })
})
