import React, { Component } from 'react'
import PropTypes from 'prop-types'
import {
  Form, Input, Checkbox, Button
} from 'semantic-ui-react';
import { isEmpty } from 'lodash'

class Answer extends Component {
  state = {
    updatedDescription: '',
    updatedCorrect: false,
    isEditing: false
  }

  get descriptionValue() {
    const { values: { description } } = this.props
    const { updatedDescription } = this.state

    return updatedDescription || description
  }

  handleEditClick = () => this.setState({ isEditing: true })

  handleEditCancel = () => this.setState({ isEditing: false })

  handleDescriptionChange = (_e, { value }) => this.setState({ updatedDescription: value })

  handleCorrectChange = () => {
    const { onCorrectChange, values: { id } } = this.props

    this.setState({ updatedCorrect: true })
    if (onCorrectChange) onCorrectChange(id)
  }

  handleSave = () => {
    const { onSave, values: { id } } = this.props

    this.handleEditCancel()
    if (onSave) onSave({ id, description: this.descriptionValue })
  }

  handleDeleteClick = () => {
    const { onDelete, values: { id } } = this.props

    if (onDelete) onDelete(id)
  }

  renderButtons = () => {
    const { isEditing, updatedDescription, updatedCorrect } = this.state
    const { values: { id } } = this.props
    const buttonStyle = { margin: '0 10px' }
    const isDisabled = isEmpty(updatedDescription) && !updatedCorrect
    const isDeletable = !/null/.test(id) && !isEditing

    return (
      <div style={{ display: 'flex', justifyContent: 'flex-end', flex: '1' }}>
        {!isEditing && <Button style={buttonStyle} icon="edit" color="green" onClick={this.handleEditClick} />}
        {isDeletable && <Button style={buttonStyle} icon="trash alternate" negative onClick={this.handleDeleteClick} />}
        {isEditing && <Button style={buttonStyle} icon="close" onClick={this.handleEditCancel} />}
        {isEditing
          && (
            <Button style={buttonStyle} icon="checkmark" onClick={this.handleSave} primary disabled={isDisabled} />
          )
        }
      </div>
    )
  }

  render() {
    const { isCorrect } = this.props
    const { isEditing } = this.state

    return (
      <div style={{ display: 'flex', alignItems: 'center', margin: '15px 0' }}>
        <Form.Field
          className="answer-checkbox"
          control={Checkbox}
          disabled={!isEditing}
          checked={isCorrect}
          onClick={this.handleCorrectChange}
        />
        <Form.Field
          className="answer-input"
          fluid
          control={Input}
          disabled={!isEditing}
          value={this.descriptionValue}
          onChange={this.handleDescriptionChange}
        />
        {this.renderButtons()}
      </div>
    )
  }
}

Answer.propTypes = {
  values: PropTypes.shape({
    description: PropTypes.string,
    id: PropTypes.string
  }).isRequired,
  isCorrect: PropTypes.bool.isRequired,
  onSave: PropTypes.func.isRequired,
  onCorrectChange: PropTypes.func.isRequired,
  onDelete: PropTypes.func.isRequired
}

export default Answer
