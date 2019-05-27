import React, { Component } from 'react'
import { Form, Message } from 'semantic-ui-react'
import PropTypes from 'prop-types'
import { TEST_FORM_CONFIG } from '../constants'

class TestEditForm extends Component {
  state = {
    inputError: null
  }

  handleChange = (_e, changeProps) => {
    const { onChange, onChangeKey } = this.props
    const { name, value } = changeProps
    if (onChange) onChange(`${onChangeKey}${name}`, value)
  }

  renderField = fieldConfig => <Form.Field key={fieldConfig.name} onChange={this.handleChange} {...fieldConfig} />

  render() {
    const { inputError } = this.state
    const { values } = this.props
    const hasError = !!inputError

    return (
      <div style={{ padding: '20px' }}>
        <Form>
          {hasError && <Message error content={inputError} />}
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
  onChangeKey: PropTypes.string.isRequired
}

export default TestEditForm
