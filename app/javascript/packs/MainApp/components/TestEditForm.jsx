import React, { Component } from 'react'
import { Form, Message } from 'semantic-ui-react'
import PropTypes from 'prop-types'
import { isEmpty } from 'lodash'
import { TEST_FORM_CONFIG } from '../constants'

class TestEditForm extends Component {
  state = {
    inputError: {}
  }

  handleChange = (_e, changeProps) => {
    const { inputError } = this.state
    const { onChange, onChangeKey, onError } = this.props
    const { name, value } = changeProps
    const intValue = parseInt(value, 2)

    if (name === 'TestableQuestionCount' && (intValue < 1 || isEmpty(value))) {
      this.setState({ inputError: { [name]: 'Question count must be greater than 0' } })
      onError(true)
    }
    if (name === 'TestableQuestionCount' && intValue > 0 && !isEmpty(inputError)) {
      this.setState({ inputError: {} })
      onError(false)
    }

    if (onChange) onChange(`${onChangeKey}${name}`, value)
  }

  renderField = (fieldConfig) => {
    const { inputError } = this.state
    const hasError = !!inputError[fieldConfig.name]
    return <Form.Field key={fieldConfig.name} error={hasError} onChange={this.handleChange} {...fieldConfig} />
  }

  render() {
    const { inputError } = this.state
    const { values } = this.props
    const hasError = !isEmpty(inputError)
    const errorMessage = hasError && Object.values(inputError).join(', ')

    return (
      <div style={{ padding: '20px' }}>
        {hasError && <Message error visibile content={errorMessage} />}
        <Form>
          {TEST_FORM_CONFIG(values).map(this.renderField)}
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
    minimumPassPercentage: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
    name: PropTypes.string,
    negativeFeedback: PropTypes.string,
    positiveFeedback: PropTypes.string,
    timeLimit: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  }).isRequired,
  onChangeKey: PropTypes.string.isRequired,
  onError: PropTypes.func.isRequired
}

export default TestEditForm
