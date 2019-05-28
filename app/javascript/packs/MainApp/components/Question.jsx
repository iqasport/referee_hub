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

  renderReadOnlyView = () => {
    const { values: { description, feedback, pointsAvailable } } = this.props
    return (
      <Fragment>
        <Label content="Question:" />
        <div dangerouslySetInnerHTML={{ __html: description }} />
        <Divider />
        <Label content="Post Test Feedback" />
        <div dangerouslySetInnerHTML={{ __html: feedback }} />
        <Divider />
        <Label content="Points Available" />
        <div>
          {pointsAvailable}
        </div>
      </Fragment>
    )
  }

  renderEditView = () => {
    const { description, feedback, pointsAvailable } = this.editValues
    const questionPlaceholder = 'Rich text of the test question.'
    const feedbackPlaceholder = 'Information for Referees regarding this question, provided after finishing the test.'

    return (
      <Fragment>
        <Label content="Question:" />
        <RichTextEditor
          value={description}
          placeholder={questionPlaceholder}
          onChange={this.handleChange}
          name="Description"
        />
        <Divider />
        <Label content="Post Test Feedback" />
        <RichTextEditor
          value={feedback}
          placeholder={feedbackPlaceholder}
          onChange={this.handleChange}
          name="Feedback"
        />
        <Divider />
        <Label content="Points Available" />
        <Form.Field control={Input} value={pointsAvailable} onChange={this.handleChange} name="PointsAvailable" />
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
      <div>
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
    const { isEditing } = this.state

    return (
      <Segment>
        {isEditing ? this.renderEditView() : this.renderReadOnlyView()}
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
    pointsAvailable: PropTypes.number,
    id: PropTypes.string
  }).isRequired,
  onSave: PropTypes.func.isRequired,
  onDelete: PropTypes.func.isRequired
}

export default Question
