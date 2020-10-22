import BraftEditor, { ControlType, EditorState } from 'braft-editor'
import React from 'react'

import "braft-editor/dist/index.css";

interface RichTextEditorProps {
  onChange: (editorState: EditorState) => void;
  content: EditorState;
  placeholder?: string;
}

const RichTextEditor = (props: RichTextEditorProps) => {
  const { onChange, content } = props

  const controls: ControlType[] = [
    'bold',
    'italic',
    'underline',
    'link',
    'text-color'
  ]

  return (
    <div className="h-24 w-full">
      <BraftEditor
        onChange={onChange}
        value={content}
        controls={controls}
      />
    </div>
  );
}

export default RichTextEditor
