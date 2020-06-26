/* eslint-disable react/no-danger */
import React, { useState } from 'react'
import PropTypes from 'prop-types'
import { Form, Checkbox } from 'semantic-ui-react';
import { isEmpty } from 'lodash'
import RichTextEditor from '../RichTextEditor'
import AnswerButtons from './AnswerButtons'

const Answer = (props) => {
  const {
    isEditable, descriptionValue, onChange, isEditing
  } = props

  const input = isEditable && (
    <RichTextEditor
      value={descriptionValue}
      onChange={onChange}
      name="Description"
    />
  )
  // eslint-disable-next-line react/no-danger
  const renderedText = (
    <div
      style={{ textAlign: 'left' }}
      dangerouslySetInnerHTML={{ __html: descriptionValue }}
    />
  )

  return (
    <div style={{ marginLeft: '10px' }}>
      {isEditing ? input : renderedText}
    </div>
  )
}

Answer.propTypes = {
  isEditable: PropTypes.bool.isRequired,
  descriptionValue: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired,
  isEditing: PropTypes.bool.isRequired,
}

const AnswerContainer = (props) => {
  const {
    values: { id: answerId, description }, isEditable, onDelete, onSave, onCorrectChange, isCorrect
  } = props

  // set state
  const [updatedDescription, setUpdatedDescription] = useState('')
  const [updatedCorrect, setUpdatedCorrect] = useState(false)
  const [isEditing, setIsEditing] = useState(false)

  // set function variables
  const descriptionValue = updatedDescription || description
  const isCheckboxDisabled = !isEditable ? false : !isEditing
  const isSaveDisabled = isEmpty(updatedDescription) && !updatedCorrect
  const isDeletable = !/null/.test(answerId) && !isEditing

  // set handler functions
  const handleEditClick = () => setIsEditing(true)
  const handleEditCancel = () => setIsEditing(false)
  const handleDescriptionChange = (_e, { value }) => setUpdatedDescription(value)
  const handleCorrectChange = () => {
    if (isEditable) setUpdatedCorrect(true)
    onCorrectChange(answerId)
  }
  const handleSave = () => {
    handleEditCancel()
    if (onSave) onSave({ id: answerId, description: descriptionValue })
  }
  const handleDeleteClick = () => { if (onDelete) onDelete(answerId) }

  // render buttons
  const buttons = (
    <AnswerButtons
      isDisabled={isSaveDisabled}
      isDeletable={isDeletable}
      isEditing={isEditing}
      onEditClick={handleEditClick}
      onDeleteClick={handleDeleteClick}
      onEditCloseClick={handleEditCancel}
      onSaveClick={handleSave}
    />
  )

  const answer = (
    <Answer
      isEditable={isEditable}
      descriptionValue={descriptionValue}
      onChange={handleDescriptionChange}
      isEditing={isEditing}
    />
  )

  return (
    <div style={{ display: 'flex', alignItems: 'center', margin: '15px 0' }}>
      <Form.Field
        className="answer-checkbox"
        control={Checkbox}
        disabled={isCheckboxDisabled}
        checked={isCorrect}
        onClick={handleCorrectChange}
        data-testid="checkbox"
      />
      {answer}
      {isEditable && buttons}
    </div>
  )
}

AnswerContainer.propTypes = {
  values: PropTypes.shape({
    description: PropTypes.string,
    id: PropTypes.string
  }).isRequired,
  isCorrect: PropTypes.bool.isRequired,
  onSave: PropTypes.func,
  onCorrectChange: PropTypes.func.isRequired,
  onDelete: PropTypes.func,
  isEditable: PropTypes.bool
}

AnswerContainer.defaultProps = {
  isEditable: true,
  onSave: () => {},
  onDelete: () => {}
}

export default AnswerContainer
