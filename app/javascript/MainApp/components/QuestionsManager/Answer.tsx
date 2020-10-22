import BraftEditor, { EditorState } from 'braft-editor'
import { isEqual } from 'lodash'
import React, { useState } from 'react'

import { Included } from 'MainApp/schemas/getQuestionsSchema'
import { convertToHtml } from 'MainApp/utils/editorUtils'

import RichTextEditor from '../RichTextEditor'

interface AnswerProps {
  answer: Included;
  isEditing: boolean;
  onDescriptionChange: (answerId: string, value: string) => void;
  onCorrectChange: (answerId: string, value: boolean) => void;
}

const Answer = (props: AnswerProps) => {
  const { answer, isEditing, onCorrectChange, onDescriptionChange } = props;

  const [descriptionState, setDescriptionState] = useState<EditorState>(
    BraftEditor.createEditorState(answer.attributes.description)
  )

  const handleCorrectChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const { checked } = event.currentTarget

    onCorrectChange(answer.id, checked)
  };

  const handleDescriptionChange = (editor: EditorState) => {
    setDescriptionState(editor)
    const descriptionString = editor.toHTML()
    if (!isEqual(descriptionString, answer.attributes.description)) {
      onDescriptionChange(answer.id, descriptionString)
    }
  }

  return (
    <div className="flex items-start flex-row w-full">
      {isEditing && (
        <input
          type="checkbox"
          className="form-checkbox mx-4"
          onChange={handleCorrectChange}
          checked={answer.attributes.correct}
        />
      )}
      {isEditing && (
        <RichTextEditor
          content={descriptionState}
          onChange={handleDescriptionChange}
        />
      )}
      {!isEditing && (
        <div
          dangerouslySetInnerHTML={{ __html: answer.attributes.description }}
        />
      )}
    </div>
  );
}

export default Answer
