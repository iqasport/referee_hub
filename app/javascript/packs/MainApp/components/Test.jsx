/* eslint-disable camelcase */
import React, { Component } from 'react'
import axios from 'axios'
import PropTypes from 'prop-types'
import {
  Segment, Tab, Header, Message, List, Radio, Button, Modal
} from 'semantic-ui-react'
import { isEmpty, capitalize } from 'lodash'
import ContentSegment from './ContentSegment'
import TestEditForm from './TestEditForm'
import QuestionManager from './QuestionManager'

class Test extends Component {
  static propTypes = {
    match: PropTypes.shape({
      params: PropTypes.object
    }).isRequired,
    history: PropTypes.shape({
      push: PropTypes.func
    }).isRequired
  }

  state = {
    test: {
      active: false,
      description: '',
      language: '',
      level: '',
      minimumPassPercentage: 0,
      name: '',
      negativeFeedback: '',
      positiveFeedback: '',
      timeLimit: 0,
      id: '',
      testableQuestionCount: 0
    },
    httpStatus: 0,
    httpStatusText: '',
    editModalOpen: false,
    confirmModalOpen: false,
    updatedName: null,
    updatedDescription: null,
    updatedLanguage: null,
    updatedLevel: null,
    updatedMinimumPassPercentage: null,
    updatedNegativeFeedback: null,
    updatedPositiveFeedback: null,
    updatedTimeLimit: null,
    updatedTestableQuestionCount: null,
    formHasError: false,
  }

  componentDidMount() {
    const { match: { params } } = this.props

    axios.get(`/api/v1/tests/${params.id}`)
      .then(this.setDataFromResponse)
      .catch(this.setDataFromError)
  }

  get testEditValues() {
    const {
      test,
      updatedName,
      updatedDescription,
      updatedLanguage,
      updatedLevel,
      updatedMinimumPassPercentage,
      updatedNegativeFeedback,
      updatedPositiveFeedback,
      updatedTimeLimit,
      updatedTestableQuestionCount
    } = this.state

    return {
      name: updatedName || test.name,
      description: updatedDescription || test.description,
      language: updatedLanguage || test.language,
      level: updatedLevel || test.level,
      minimumPassPercentage: updatedMinimumPassPercentage || test.minimumPassPercentage,
      negativeFeedback: updatedNegativeFeedback || test.negativeFeedback,
      positiveFeedback: updatedPositiveFeedback || test.positiveFeedback,
      timeLimit: updatedTimeLimit || test.timeLimit,
      testableQuestionCount: updatedTestableQuestionCount || test.testableQuestionCount,
    }
  }

  setDataFromResponse = ({ status, statusText, data }) => {
    const { data: { id, attributes } } = data
    const {
      active,
      description,
      language,
      level,
      minimum_pass_percentage,
      name,
      negative_feedback,
      positive_feedback,
      time_limit,
      testable_question_count,
    } = attributes

    const test = {
      active,
      description,
      language,
      level,
      minimumPassPercentage: minimum_pass_percentage,
      name,
      negativeFeedback: negative_feedback,
      positiveFeedback: positive_feedback,
      timeLimit: time_limit,
      id,
      testableQuestionCount: testable_question_count,
    }

    this.setState({ test, httpStatus: status, httpStatusText: statusText })
  }

  setDataFromError = (error) => {
    const { status, statusText } = error.response || {
      status: 500,
      statusText: 'Error'
    }

    this.setState({ test: {}, httpStatus: status, httpStatusText: statusText })
  }

  handleEditChange = (key, value) => this.setState({ [`${key}`]: value })

  // eslint-disable-next-line no-unused-vars
  handleActiveChange = (_e, { _value }) => {
    const { test: { id, active } } = this.state
    axios.patch(`/api/v1/tests/${id}`, { active: !active })
      .then(this.setDataFromResponse)
      .then(this.handleCloseModal)
      .catch(this.setDataFromError)
  }

  handleOpenEditModal = () => this.setState({ editModalOpen: true })

  handleOpenConfirmModal = () => this.setState({ confirmModalOpen: true })

  handleCloseModal = () => this.setState({ editModalOpen: false, confirmModalOpen: false })

  handleHasError = errorState => this.setState({ formHasError: errorState })

  handleSubmit = () => {
    const {
      test,
      updatedName,
      updatedDescription,
      updatedLanguage,
      updatedLevel,
      updatedMinimumPassPercentage,
      updatedNegativeFeedback,
      updatedPositiveFeedback,
      updatedTimeLimit,
      updatedTestableQuestionCount,
    } = this.state

    axios.patch(`/api/v1/tests/${test.id}`, {
      name: updatedName || test.name,
      description: updatedDescription || test.description,
      language: updatedLanguage || test.language,
      level: updatedLevel || test.level,
      minimum_pass_percentage: updatedMinimumPassPercentage || test.minimumPassPercentage,
      negative_feedback: updatedNegativeFeedback || test.negativeFeedback,
      positive_feedback: updatedPositiveFeedback || test.positiveFeedback,
      time_limit: updatedTimeLimit || test.timeLimit,
      testable_question_count: updatedTestableQuestionCount || test.testableQuestionCount,
    })
      .then(this.setDataFromResponse)
      .then(this.handleCloseModal)
      .catch(this.setDataFromError)
  }

  handleGoBack = () => {
    const { history } = this.props
    history.push('/admin/tests')
  }

  renderEditForm = () => (
    <TestEditForm
      values={this.testEditValues}
      onChange={this.handleEditChange}
      onChangeKey="updated"
      onError={this.handleHasError}
    />
  )

  renderModal = () => {
    const {
      editModalOpen, confirmModalOpen, test, formHasError
    } = this.state
    const isOpen = editModalOpen || confirmModalOpen
    const editModalProps = {
      header: `Edit ${test.name}`,
      content: this.renderEditForm(),
      actions: [
        { key: 'cancel', content: 'Cancel', onClick: this.handleCloseModal },
        {
          key: 'submit',
          content: 'Submit',
          onClick: this.handleSubmit,
          primary: true,
          disabled: formHasError
        }
      ],
      size: 'large',
      scrolling: true
    }

    const confirmModalProps = {
      header: 'Confirm Test Activation?',
      content: 'Once activated this test will be available to all referees.',
      actions: [
        { key: 'cancel', content: 'Cancel', onClick: this.handleCloseModal },
        {
          key: 'submit',
          content: 'Confirm',
          onClick: this.handleActiveChange,
          primary: true
        }
      ],
      size: 'mini'
    }

    const modalProps = editModalOpen ? editModalProps : confirmModalProps

    return <Modal open={isOpen} {...modalProps} />
  }

  renderEditButtons = () => {
    const { test } = this.state

    return (
      <div style={{ display: 'flex', justifyContent: 'space-around', alignItems: 'center' }}>
        <Radio
          toggle
          label="Active?"
          onChange={this.handleOpenConfirmModal}
          checked={test.active}
        />
        <Button content={`Edit ${test.name}`} onClick={this.handleOpenEditModal} />
      </div>
    )
  }

  renderDetails = () => {
    const { test } = this.state
    const nullFeedback = "Look's like there isn't any feedback for referees after they finish their test. "
      + 'Edit this test so referees know their next steps.'

    return (
      <div style={{ width: '50%' }}>
        <List divided relaxed>
          <List.Item header="Level" content={capitalize(test.level)} />
          <List.Item header="Language" content={capitalize(test.language)} />
          <List.Item header="Minimum Pass Percentage" content={test.minimumPassPercentage} />
          <List.Item header="Question Count" content={test.testableQuestionCount} />
          <List.Item
            header="Feedback After Test Passing"
            content={test.positiveFeedback ? test.positiveFeedback : nullFeedback}
          />
          <List.Item
            header="Feedback After Test Failure"
            content={test.negativeFeedback ? test.negativeFeedback : nullFeedback}
          />
          <List.Item header="Default Time Limit" content={test.timeLimit} />
        </List>
      </div>
    )
  }

  renderDetailPane = () => {
    const { test, httpStatus, httpStatusText } = this.state
    const isError = httpStatus !== 200

    return (
      <Tab.Pane>
        {isError && <Message error header={httpStatus} content={httpStatusText} />}
        {this.renderEditButtons()}
        <ContentSegment headerContent="Description" segmentContent={test.description} />
        <ContentSegment headerContent="Completion Details" segmentContent={this.renderDetails()} />
        {this.renderModal()}
      </Tab.Pane>
    )
  }

  renderQuestionPane = () => {
    const { test: { id } } = this.state
    return (
      <Tab.Pane>
        <QuestionManager testId={id} />
      </Tab.Pane>
    )
  }

  render() {
    const { test } = this.state
    const panes = [
      { menuItem: 'Details', render: this.renderDetailPane },
      { menuItem: 'Questions', render: this.renderQuestionPane }
    ]

    return (
      <Segment>
        <Header as="h1" textAlign="center" style={{ display: 'flex', alignItems: 'center' }}>
          <div style={{ marginRight: '20%' }}>
            <Button onClick={this.handleGoBack} content="Go Back to Tests" basic />
          </div>
          {!isEmpty(test) ? test.name : ''}
        </Header>
        <Tab panes={panes} />
      </Segment>
    )
  }
}

export default Test
