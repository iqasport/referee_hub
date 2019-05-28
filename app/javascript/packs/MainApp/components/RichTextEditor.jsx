import React, { Component } from 'react'
import ReactQuill from 'react-quill'
import PropTypes from 'prop-types'

class RichTextEditor extends Component {
  static propTypes = {
    onChange: PropTypes.func.isRequired,
    value: PropTypes.string.isRequired,
    // eslint-disable-next-line react/require-default-props
    placeholder: PropTypes.string,
    name: PropTypes.string.isRequired
  }

  constructor(props) {
    super(props)

    this.state = {
      editorValue: props.value
    }
  }

  handleChange = (content, delta, source, editor) => {
    const { onChange, name } = this.props
    this.setState({ editorValue: content })

    if (onChange) onChange({}, { name, value: content })
  }

  render() {
    const { editorValue } = this.state
    const { placeholder } = this.props

    return (
      <ReactQuill
        value={editorValue}
        onChange={this.handleChange}
        placeholder={placeholder}
        theme={null}
      />
    )
  }
}

export default RichTextEditor
