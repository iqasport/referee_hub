import React from 'react'
import { act } from 'react-dom/test-utils'
import Counter from '.'
import { render, screen } from '../../utils/test-utils'
import { formatTime, getDisplayColor } from './Counter'

describe('Counter', () => {
  const defaultProps = {
    onTimeLimitMet: jest.fn(),
    timeLimit: 10,
  }

  beforeEach(() => { jest.useFakeTimers() })

  afterEach(() => { jest.clearAllTimers() })

  test('it renders a counter', () => {
    render(<Counter {...defaultProps} />)

    expect(screen.getByText('00:00')).toBeDefined()
  })

  test('it handles a second tick', () => {
    render(<Counter {...defaultProps} />)

    act(() => {
      jest.advanceTimersByTime(1000);
    })

    expect(screen.getByText('00:01')).toBeDefined()
  })
})

describe('utility functions', () => {
  describe('formatTime', () => {
    test('time is less than 10', () => {
      const time = 4
      const expectedTime = '04'
      const actual = formatTime(time)

      expect(actual).toEqual(expectedTime)
    })

    test('time is greater than 10', () => {
      const time = 15
      const expectedTime = 15
      const actual = formatTime(time)

      expect(actual).toEqual(expectedTime)
    })
  })

  describe('getDisplayColor', () => {
    const limit = 10

    test('it returns grey when in the goodLimit', () => {
      const minutes = 5
      const expectedColor = 'grey'
      const actual = getDisplayColor(limit, minutes)

      expect(actual).toEqual(expectedColor)
    })

    test('it returns yellow when in the warning limit', () => {
      const minutes = 8
      const expectedColor = 'yellow'
      const actual = getDisplayColor(limit, minutes)

      expect(actual).toEqual(expectedColor)
    })

    test('it returns red when at the limit', () => {
      const minutes = 10
      const expectedColor = 'red'
      const actual = getDisplayColor(limit, minutes)

      expect(actual).toEqual(expectedColor)
    })
  })
})
