import React, { useState } from 'react'

import useInterval from '../../hooks'

export const formatTime = (time: number): string | number => (time < 10 ? `0${time}` : time)

export const getDisplayColor = (limit: number, minutes: number): string => {
  const goodLimit = limit - Math.round(limit * 0.3)

  if (minutes < goodLimit) return 'grey'
  if (minutes < limit) return 'yellow'
  if (minutes >= limit) return 'red'

  return 'grey'
}

interface CounterProps {
  timeLimit: number;
  onTimeLimitMet: (minutes: number, seconds: number) => void;
  setCurrentTime: (currentTime: {minutes: number; seconds: number}) => void;
}

const Counter = (props: CounterProps) => {
  const [minutes, setMinutes] = useState(0)
  const [seconds, setSeconds] = useState(0)
  const { timeLimit, onTimeLimitMet, setCurrentTime } = props
  const displayMinute = formatTime(minutes)
  const displaySecond = formatTime(seconds)

  const handleTick = () => {
    if (minutes === timeLimit - 1 && seconds === 59) {
      onTimeLimitMet(minutes, seconds)
    }

    if (seconds < 59) {
      setSeconds(prevSeconds => prevSeconds + 1)
    } else {
      setMinutes(prevMinutes => prevMinutes + 1)
      setSeconds(0)
    }

    setCurrentTime({ minutes, seconds })
  }

  useInterval(handleTick, 1000)

  return (
    <h3
      data-testid="counter"
      className={`font-bold ${getDisplayColor(timeLimit, minutes)}`}
    >
      {`${displayMinute}:${displaySecond}`}
    </h3>
  )
}

export default Counter
