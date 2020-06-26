import React, { useState, useRef, useEffect } from 'react'
import PropTypes from 'prop-types'
import {
  Editor, EditorState, convertFromHTML, ContentState, convertToRaw
} from 'draft-js'
import draftToHtml from 'draftjs-to-html'
import StylingToolbar from './StylingToolbar'
import { colorStyleMap } from '../../constants'

const RichTextEditor = (props) => {
  const { value, onChange, name } = props
  let initialState
  if (value.length) {
    const blocksFromHTML = convertFromHTML(value);
    const contentState = ContentState.createFromBlockArray(
      blocksFromHTML.contentBlocks,
      blocksFromHTML.entityMap,
    )
    initialState = EditorState.createWithContent(contentState)
  } else {
    initialState = EditorState.createEmpty()
  }
  const [editorState, setEditorState] = useState(initialState)

  const editor = useRef(null)

  function focusEditor() {
    editor.current.focus()
  }

  useEffect(() => {
    focusEditor()
  }, [])

  function handleEditorChange(newEditorState) {
    const content = editorState.getCurrentContent()
    const convertedContent = convertToRaw(content)
    const htmlMarkup = draftToHtml(convertedContent)

    setEditorState(newEditorState)

    if (onChange) onChange({}, { name, value: `${htmlMarkup}` })
  }

  return (
    <div>
      <StylingToolbar editorState={editorState} handleEditorChange={handleEditorChange} />
      <div role="textbox" onClick={focusEditor} onKeyPress={focusEditor} tabIndex={0}>
        <Editor
          customStyleMap={colorStyleMap}
          ref={editor}
          editorState={editorState}
          onChange={handleEditorChange}
        />
      </div>
    </div>
  )
}

RichTextEditor.propTypes = {
  onChange: PropTypes.func.isRequired,
  value: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired
}

export default RichTextEditor
