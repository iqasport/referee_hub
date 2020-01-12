import React, { useState } from 'react'
import PropTypes from 'prop-types'
import { Header } from 'semantic-ui-react'
import useInterval from '../../hooks'

export const formatTime = time => (time < 10 ? `0${time}` : time)

export const getDisplayColor = (limit, minutes) => {
  const goodLimit = limit - Math.round(limit * 0.3)

  if (minutes < goodLimit) return 'grey'
  if (minutes < limit) return 'yellow'
  if (minutes >= limit) return 'red'

  return 'grey'
}

const Counter = (props) => {
  const [minutes, setMinutes] = useState(0)
  const [seconds, setSeconds] = useState(0)
  const { timeLimit, onTimeLimitMet } = props
  const displayMinute = formatTime(minutes)
  const displaySecond = formatTime(seconds)

  const handleTick = () => {
    if (minutes === timeLimit - 1 && seconds === 59) {
      onTimeLimitMet()
    }

    if (seconds < 59) {
      setSeconds(prevSeconds => prevSeconds + 1)
    } else {
      setMinutes(prevMinutes => prevMinutes + 1)
      setSeconds(0)
    }
  }

  useInterval(handleTick, 1000)

  return (
    <Header
      as="h3"
      data-testid="counter"
      color={getDisplayColor(timeLimit, minutes)}
    >
      {`${displayMinute}:${displaySecond}`}
    </Header>
  )
}

Counter.propTypes = {
  timeLimit: PropTypes.number.isRequired,
  onTimeLimitMet: PropTypes.func.isRequired
}

export default Counter
