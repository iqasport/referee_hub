import React, { Component } from 'react'
import PropTypes from 'prop-types'
import { Header } from 'semantic-ui-react'

const formatTime = time => (time < 10 ? `0${time}` : time)

const getDisplayColor = (limit, minutes) => {
  const goodLimit = limit - Math.round(limit * 0.3)

  if (minutes < goodLimit) return 'grey'
  if (minutes < limit) return 'yellow'
  if (minutes >= limit) return 'red'

  return 'grey'
}

class Counter extends Component {
  static propTypes = {
    timeLimit: PropTypes.number.isRequired,
    onTimeLimitMet: PropTypes.func.isRequired
  }

  constructor(props) {
    super(props)

    this.state = {
      minutes: 0,
      seconds: 0
    }

    this.interval = setInterval(this.handleTick, 1000)
  }

  componentWillUnmount() {
    clearInterval(this.interval)
  }

  handleTick = () => {
    const { seconds, minutes } = this.state
    const { timeLimit, onTimeLimitMet } = this.props

    if (minutes === timeLimit - 1 && seconds === 59) {
      onTimeLimitMet()
    }

    if (seconds < 59) {
      this.setState(prevState => ({ seconds: prevState.seconds + 1 }))
    } else {
      this.setState(prevState => ({ minutes: prevState.minutes + 1, seconds: 0 }))
    }
  }

  render() {
    const { minutes, seconds } = this.state
    const { timeLimit } = this.props
    const displayMinute = formatTime(minutes)
    const displaySecond = formatTime(seconds)

    return (
      <Header as="h3" color={getDisplayColor(timeLimit, minutes)}>{`${displayMinute}:${displaySecond}`}</Header>
    )
  }
}

export default Counter
