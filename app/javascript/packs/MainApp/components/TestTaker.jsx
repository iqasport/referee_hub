/* eslint-disable react/no-danger */
import React, { Component } from 'react'
import PropTypes from 'prop-types'
import { Divider } from 'semantic-ui-react'

import Answer from './Answer'

class TestTaker extends Component {
  static propTypes = {
    currentQuestion: PropTypes.shape({
      id: PropTypes.string,
      description: PropTypes.string,
      answers: PropTypes.arrayOf(
        PropTypes.shape({
          id: PropTypes.string,
          description: PropTypes.string
        })
      ),
      selectedAnswer: PropTypes.string
    }).isRequired,
    onAnswerSelect: PropTypes.func.isRequired
  }

  state = {}

  handleAnswerChange = (answerId) => {
    const { onAnswerSelect } = this.props

    onAnswerSelect(answerId)
  }

  renderAnswer = (answer) => {
    const { currentQuestion: { selectedAnswer } } = this.props
    const isSelected = selectedAnswer === answer.id

    return (
      <Answer
        key={answer.id}
        isCorrect={isSelected}
        onCorrectChange={this.handleAnswerChange}
        isEditable={false}
        values={{
          id: answer.id,
          description: answer.description
        }}
      />
    )
  }

  render() {
    const { currentQuestion } = this.props

    return (
      <div>
        <div dangerouslySetInnerHTML={{ __html: currentQuestion.description }} />
        <Divider />
        <div style={{ width: '50%', margin: '0 auto' }}>
          {currentQuestion.answers.map(this.renderAnswer)}
        </div>
      </div>
    )
  }
}

export default TestTaker
