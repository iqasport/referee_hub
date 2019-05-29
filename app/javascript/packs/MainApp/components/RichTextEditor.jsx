import React, { Component } from 'react'
import ReactQuill from 'react-quill'
import PropTypes from 'prop-types'

const modules = {
  toolbar: [
    ['bold', 'italic', 'underline', 'strike'], // toggled buttons
    ['blockquote', 'code-block'],
    [{ list: 'ordered' }, { list: 'bullet' }],
    [{ script: 'sub' }, { script: 'super' }], // superscript/subscript
    [{ indent: '-1' }, { indent: '+1' }], // outdent/indent
    [{ direction: 'rtl' }, { direction: 'ltr' }], // text direction
    [{ size: [] }],
    [{ color: [] }, { background: [] }], // dropdown with defaults from theme
  ]
}

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

  handleChange = (content) => {
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
        modules={modules}
      />
    )
  }
}

export default RichTextEditor
