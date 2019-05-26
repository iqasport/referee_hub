import React, { Component } from 'react'
import {
  Form, Input, TextArea, Dropdown, Message, Container
} from 'semantic-ui-react'
import PropTypes from 'prop-types'
import { range } from 'lodash'

const levelDropdown = [
  { text: 'Snitch', value: 'snitch' },
  { text: 'Assistant', value: 'assistant' },
  { text: 'Head', value: 'value' }
]
const MAX_TIME = 120
const MIN_TIME = 10
const minuteArray = range(MIN_TIME, MAX_TIME)
const timeLimitDropdown = minuteArray.map(minute => ({
  text: minute,
  value: minute
}))

class TestEditForm extends Component {
  state = {
    inputError: null
  }

  // minimumPassPercentage = text input with num validation
  // language = text input
  // negativeFeedback = text area
  // positiveFeedback = text area
  // timeLimit = dropdown of integers up to 120
  handleChange = (_e, changeProps) => {
    const { onChange } = this.props
    const { name, value } = changeProps
    if (onChange) onChange(`updated${name}`, value)
  }

  render() {
    const { inputError } = this.state
    const { values } = this.props
    const descPlaceholder = !values.description ? "What should referee's know about this test before taking it?" : null
    const negFeedbackPlaceholder = !values.negativeFeedback ? 'Provide feedback after a failed test' : null
    const posFeedbackPlaceholder = !values.positiveFeedback ? 'Provide feedback after a passed test' : null
    const hasError = !!inputError

    return (
      <div style={{ padding: '20px' }}>
        <Form>
          {hasError && <Message error content={inputError} />}
          <Form.Field
            control={Input}
            label="Name"
            name="Name"
            value={values.name}
            onChange={this.handleChange}
          />
          <Form.Field
            control={TextArea}
            label="Description"
            name="Description"
            placeholder={descPlaceholder}
            value={values.description}
            onChange={this.handleChange}
          />
          <Form.Field
            control={Dropdown}
            label="Ceritification Level"
            name="Level"
            options={levelDropdown}
            value={values.level}
            onChange={this.handleChange}
          />
          <Form.Field
            control={Input}
            label="Minimum Pass Percentage"
            name="MinimumPassPercentage"
            value={values.minimumPassPercentage}
            onChange={this.handleChange}
            error={hasError}
          />
          <Form.Field
            control={Dropdown}
            label="Time Limit"
            name="TimeLimit"
            options={timeLimitDropdown}
            value={values.timeLimit}
            onChange={this.handleChange}
            scrolling="true"
          />
          <Form.Field
            control={TextArea}
            label="Positive Feedback"
            name="PositiveFeedback"
            placeholder={posFeedbackPlaceholder}
            value={values.positiveFeedback}
            onChange={this.handleChange}
          />
          <Form.Field
            control={TextArea}
            label="Negative Feedback"
            name="NegativeFeedback"
            placeholder={negFeedbackPlaceholder}
            value={values.negativeFeedback}
            onChange={this.handleChange}
          />
        </Form>
      </div>
    )
  }
}

TestEditForm.propTypes = {
  onChange: PropTypes.func.isRequired,
  values: PropTypes.shape({
    description: PropTypes.string,
    language: PropTypes.string,
    level: PropTypes.string,
    minimumPassPercentage: PropTypes.number,
    name: PropTypes.string,
    negativeFeedback: PropTypes.string,
    positiveFeedback: PropTypes.string,
    timeLimit: PropTypes.number,
  }).isRequired
}

export default TestEditForm
