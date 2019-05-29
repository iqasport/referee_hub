/* eslint-disable react/no-danger */
import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import {
  Segment, Label, Divider, Input, Button, Modal, Form
} from 'semantic-ui-react'
import RichTextEditor from './RichTextEditor'

const initialState = {
  isEditing: false,
  updatedDescription: null,
  updatedFeedback: null,
  updatedPointsAvailable: null,
  confirmModalOpen: false,
}

const labelProps = {
  horizontal: true,
  size: 'large',
  basic: true
}

class Question extends Component {
  state = initialState

  get editValues() {
    const { updatedDescription, updatedFeedback, updatedPointsAvailable } = this.state
    const {
      values: {
        description, feedback, pointsAvailable, id
      }
    } = this.props

    return {
      description: updatedDescription || description,
      feedback: updatedFeedback || feedback,
      pointsAvailable: updatedPointsAvailable || pointsAvailable,
      id
    }
  }

  handleChange = (_e, { name, value }) => {
    this.setState({ [`updated${name}`]: value })
  }

  handleSave = () => {
    const { onSave } = this.props
    if (onSave) onSave(this.editValues)
    this.setState(initialState)
  }

  handleDeleteQuestion = () => {
    const { onDelete, values: { id } } = this.props

    if (onDelete) onDelete(id)
    this.setState(initialState)
  }

  handleEditClick = () => this.setState({ isEditing: true })

  handleEditCancel = () => this.setState(initialState)

  handleDeleteClick = () => this.setState({ confirmModalOpen: true })

  handleCloseModal = () => this.setState({ confirmModalOpen: false })

  renderQuestion = () => {
    const { isEditing } = this.state
    const { values: { description, feedback, pointsAvailable } } = this.props
    const questionPlaceholder = 'Rich text of the test question.'
    const feedbackPlaceholder = 'Information for Referees regarding this question, provided after finishing the test.'

    return (
      <Fragment>
        <Label {...labelProps}>
          Question:
        </Label>
        <div style={{ margin: '2%' }}>
          {
            isEditing
              ? (
                <RichTextEditor
                  value={this.editValues.description}
                  placeholder={questionPlaceholder}
                  onChange={this.handleChange}
                  name="Description"
                />
              )
              : <div dangerouslySetInnerHTML={{ __html: description }} />
          }
        </div>
        <Divider />
        <Label {...labelProps}>
          Post Test Feedback:
        </Label>
        <div style={{ margin: '2%' }}>
          {
            isEditing
              ? (
                <RichTextEditor
                  value={this.editValues.feedback}
                  placeholder={feedbackPlaceholder}
                  onChange={this.handleChange}
                  name="Feedback"
                />
              )
              : <div dangerouslySetInnerHTML={{ __html: feedback }} />
          }
        </div>
        <Divider />
        <Label {...labelProps}>
          Points Available:
        </Label>
        {
          isEditing
            ? (
              <Form.Field
                control={Input}
                value={this.editValues.pointsAvailable}
                onChange={this.handleChange}
                name="PointsAvailable"
              />
            )
            : <span>{pointsAvailable}</span>
        }
      </Fragment>
    )
  }

  renderButtons = () => {
    const {
      isEditing, updatedDescription, updatedFeedback, updatedPointsAvailable
    } = this.state
    const { values: { id } } = this.props
    const isDeletable = !isEditing && id !== null
    const isDisabled = !updatedDescription && !updatedFeedback && !updatedPointsAvailable

    return (
      <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
        {!isEditing && <Button icon="edit" color="green" onClick={this.handleEditClick} />}
        {isDeletable && <Button icon="trash alternate" negative onClick={this.handleDeleteClick} />}
        {isEditing && <Button content="Cancel" onClick={this.handleEditCancel} />}
        {isEditing && <Button content="Save" onClick={this.handleSave} primary disabled={isDisabled} />}
      </div>
    )
  }

  renderModal = () => {
    const { confirmModalOpen } = this.state

    const modalProps = {
      header: 'Delete Question?',
      content: 'Once deleted you can not get this question back.',
      actions: [
        { key: 'cancel', content: 'Cancel', onClick: this.handleCloseModal },
        {
          key: 'submit',
          content: 'Confirm',
          onClick: this.handleDeleteQuestion,
          primary: true
        }
      ],
      size: 'mini'
    }

    return <Modal open={confirmModalOpen} {...modalProps} />
  }

  render() {
    return (
      <Segment>
        {this.renderQuestion()}
        {this.renderButtons()}
        {this.renderModal()}
      </Segment>
    )
  }
}

Question.propTypes = {
  values: PropTypes.shape({
    description: PropTypes.string,
    feedback: PropTypes.string,
    pointsAvailable: PropTypes.string,
    id: PropTypes.string
  }).isRequired,
  onSave: PropTypes.func.isRequired,
  onDelete: PropTypes.func.isRequired
}

export default Question
