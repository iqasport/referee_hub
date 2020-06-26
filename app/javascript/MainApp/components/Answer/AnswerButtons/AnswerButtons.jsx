import React from 'react'
import PropTypes from 'prop-types'
import { Button } from 'semantic-ui-react'

const AnswerButton = (props) => {
  const buttonStyle = { margin: '0 10px' }
  const {
    icon, color, onClick, testId
  } = props

  return (
    <Button
      style={buttonStyle}
      icon={icon}
      color={color}
      onClick={onClick}
      data-testid={testId}
    />
  )
}

AnswerButton.propTypes = {
  icon: PropTypes.string.isRequired,
  color: PropTypes.string,
  onClick: PropTypes.func.isRequired,
  testId: PropTypes.string.isRequired
}

AnswerButton.defaultProps = {
  color: null
}

const EditButton = (props) => {
  const { onClick } = props
  return (
    <AnswerButton
      icon="edit"
      color="green"
      onClick={onClick}
      testId="edit"
    />
  )
}

EditButton.propTypes = {
  onClick: PropTypes.func.isRequired
}

const DeleteButton = (props) => {
  const { onClick } = props

  return (
    <AnswerButton
      icon="trash alternate"
      color="red"
      onClick={onClick}
      testId="delete"
    />
  )
}

DeleteButton.propTypes = {
  onClick: PropTypes.func.isRequired
}

const CloseButton = (props) => {
  const { onClick } = props

  return (
    <AnswerButton
      icon="close"
      onClick={onClick}
      testId="close"
    />
  )
}

CloseButton.propTypes = {
  onClick: PropTypes.func.isRequired
}

const SaveButton = (props) => {
  const { onClick, isDisabled } = props

  return (
    <AnswerButton
      icon="checkmark"
      onClick={onClick}
      primary
      disabled={isDisabled}
      testId="save"
    />
  )
}

SaveButton.propTypes = {
  onClick: PropTypes.func.isRequired,
  isDisabled: PropTypes.bool.isRequired
}

const AnswerButtons = (props) => {
  const {
    isEditing, isDeletable, isDisabled, onEditClick, onDeleteClick, onEditCloseClick, onSaveClick
  } = props

  return (
    <div style={{ display: 'flex', justifyContent: 'flex-end', flex: '1' }}>
      {!isEditing && <EditButton onClick={onEditClick} />}
      {isDeletable && <DeleteButton onClick={onDeleteClick} />}
      {isEditing && <CloseButton onClick={onEditCloseClick} />}
      {isEditing && <SaveButton onClick={onSaveClick} isDisabled={isDisabled} />}
    </div>
  )
}

AnswerButtons.propTypes = {
  isDisabled: PropTypes.bool.isRequired,
  isDeletable: PropTypes.bool.isRequired,
  isEditing: PropTypes.bool.isRequired,
  onEditClick: PropTypes.func.isRequired,
  onDeleteClick: PropTypes.func.isRequired,
  onEditCloseClick: PropTypes.func.isRequired,
  onSaveClick: PropTypes.func.isRequired
}

export default AnswerButtons
