import React, { Component } from 'react'
import PropTypes from 'prop-types'

class TestTaker extends Component {
  static propTypes = {
    currentQuestion: PropTypes.shape({
      id: PropTypes.string,
      description: PropTypes.string,
      answers: PropTypes.arrayOf(
        PropTypes.shape({
          id: PropTypes.string,
          description: PropTypes.string,
          selected: PropTypes.bool
        })
      )
    }).isRequired
  }

  state = {}

  render() {
    const { currentQuestion } = this.props

    return (
      <div>
        {currentQuestion.description}
        <ul>
          {currentQuestion.answers.map(answer => <li key={answer.id}>{answer.description}</li>)}
        </ul>
      </div>
    )
  }
}

export default TestTaker
