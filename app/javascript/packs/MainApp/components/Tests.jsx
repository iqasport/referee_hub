/* eslint-disable camelcase */
import React, { Component } from 'react'
import PropTypes from 'prop-types'
import {
  Segment, Header, Table, Icon, Button, Modal, Message
} from 'semantic-ui-react'
import axios from 'axios'
import { capitalize } from 'lodash'
import TestEditForm from './TestEditForm';

const sanitizeTestData = (test) => {
  const { id, attributes } = test
  const {
    active,
    description,
    language,
    level,
    minimum_pass_percentage,
    name,
    negative_feedback,
    positive_feedback,
    time_limit
  } = attributes

  return {
    active,
    description,
    language,
    level: capitalize(level),
    minimumPassPercentage: minimum_pass_percentage,
    name,
    negativeFeedback: negative_feedback,
    positiveFeedback: positive_feedback,
    timeLimit: time_limit,
    id
  }
}

class Tests extends Component {
  static propTypes = {
    history: PropTypes.shape({
      push: PropTypes.func
    }).isRequired
  }

  state = {
    tests: [],
    createModalOpen: false,
    testName: '',
    testDescription: '',
    testLanguage: '',
    testLevel: '',
    testMinimumPassPercentage: '',
    testNegativeFeedback: '',
    testPositiveFeedback: '',
    testTimeLimit: '',
    httpStatus: null,
    httpStatusText: null,
    deleteId: null,
    deleteModalOpen: false,
  }

  componentDidMount() {
    this.fetchTests()
  }

  get createValues() {
    const {
      testName,
      testDescription,
      testLanguage,
      testLevel,
      testMinimumPassPercentage,
      testNegativeFeedback,
      testPositiveFeedback,
      testTimeLimit,
    } = this.state

    return {
      name: testName,
      description: testDescription,
      language: testLanguage,
      level: testLevel,
      minimumPassPercentage: testMinimumPassPercentage,
      negativeFeedback: testNegativeFeedback,
      positiveFeedback: testPositiveFeedback,
      timeLimit: testTimeLimit,
    }
  }

  fetchTests = () => {
    axios.get('/api/v1/tests')
      .then(({ data }) => {
        const { data: testData } = data

        const sanitizedData = testData.map(sanitizeTestData)
        this.setState({ tests: sanitizedData })
      }).catch(this.setDataFromError)
  }

  setDataFromResponse = ({ status, statusText, data }) => {
    const { data: createdTestData } = data

    const sanitizedTest = sanitizeTestData(createdTestData)

    this.setState(prevState => ({
      tests: [sanitizedTest, ...prevState.tests],
      httpStatus: status,
      httpStatusText: statusText
    }))
  }

  setDataFromError = (error) => {
    const { status, statusText } = error.response || {
      status: 500,
      statusText: 'Error'
    }

    this.setState({ httpStatus: status, httpStatusText: statusText })
  }

  handleTestClick = id => () => {
    const { history } = this.props

    history.push(`/admin/tests/${id}`)
  }

  handleOpenCreateModal = () => this.setState({ createModalOpen: true })

  handleCloseModal = () => this.setState({ createModalOpen: false, deleteModalOpen: false })

  handleCreateChange = (key, value) => this.setState({ [`${key}`]: value })

  handleOpenDeleteModal = id => () => this.setState({ deleteId: id, deleteModalOpen: true })

  handleCreateTest = () => {
    const {
      testName,
      testDescription,
      testLanguage,
      testLevel,
      testMinimumPassPercentage,
      testNegativeFeedback,
      testPositiveFeedback,
      testTimeLimit,
    } = this.state

    axios.post('/api/v1/tests', {
      name: testName,
      description: testDescription,
      language: testLanguage,
      level: testLevel,
      minimum_pass_percentage: testMinimumPassPercentage,
      negative_feedback: testNegativeFeedback,
      positive_feedback: testPositiveFeedback,
      time_limit: testTimeLimit
    })
      .then(this.setDataFromResponse)
      .then(this.handleCloseModal)
      .catch(this.setDataFromError)
  }

  handleDeleteTest = () => {
    const { deleteId } = this.state

    axios.delete(`/api/v1/tests/${deleteId}`)
      .then(() => this.setState({ deleteId: null }))
      .then(this.fetchTests())
      .catch(this.setDataFromError)
  }

  renderHeader = () => (
    <Table.Header>
      <Table.Row>
        <Table.HeaderCell>
          Active
        </Table.HeaderCell>
        <Table.HeaderCell>
          Name
        </Table.HeaderCell>
        <Table.HeaderCell>
          Level
        </Table.HeaderCell>
        <Table.HeaderCell>
          Language
        </Table.HeaderCell>
        <Table.HeaderCell>
          Delete?
        </Table.HeaderCell>
        <Table.HeaderCell>
          View More
        </Table.HeaderCell>
      </Table.Row>
    </Table.Header>
  )

  renderTableRow = (test) => {
    const {
      active, name, level, language, id
    } = test
    const activeIcon = <Icon name="checkmark" color="green" size="large" />
    const inactiveIcon = <Icon name="times" color="red" size="large" />

    return (
      <Table.Row key={id}>
        <Table.Cell>
          {active ? activeIcon : inactiveIcon}
        </Table.Cell>
        <Table.Cell>
          {name}
        </Table.Cell>
        <Table.Cell>
          {level}
        </Table.Cell>
        <Table.Cell>
          {language}
        </Table.Cell>
        <Table.Cell>
          <Button icon="trash alternate" negative onClick={this.handleOpenDeleteModal(id)} />
        </Table.Cell>
        <Table.Cell>
          <Button icon="chevron right" basic onClick={this.handleTestClick(id)} />
        </Table.Cell>
      </Table.Row>
    )
  }

  renderEmptyTable = () => (
    <Table.Row>
      <Table.Cell textAlign="center">
        No tests found. To create a test use the Add New Test button.
      </Table.Cell>
    </Table.Row>
  )

  renderModal = () => {
    const { createModalOpen, deleteModalOpen } = this.state
    const content = <TestEditForm values={this.createValues} onChange={this.handleCreateChange} onChangeKey="test" />
    const deleteContent = "This action cannot be undone, if you don't want to delete this test but want to make it" +
      " unavailable please deactivate this test on it's detail view"
    const createModalProps = {
      header: 'Create Test',
      content,
      actions: [
        { key: 'cancel', content: 'Cancel', onClick: this.handleCloseModal },
        {
          key: 'submit',
          content: 'Submit',
          onClick: this.handleCreateTest,
          primary: true
        }
      ],
      size: 'large',
      scrolling: true
    }
    const deleteModalProps = {
      header: 'Delete Test?',
      content: deleteContent,
      actions: [
        { key: 'cancel', content: 'Cancel', onClick: this.handleCloseModal },
        {
          key: 'submit',
          content: 'Delete',
          onClick: this.handleDeleteTest,
          negative: true
        }
      ],
      size: 'mini'
    }

    const modalProps = createModalOpen ? createModalProps : deleteModalProps
    const isOpen = createModalOpen || deleteModalOpen
    return <Modal open={isOpen} {...modalProps} />
  }

  render() {
    const { tests, httpStatus, httpStatusText } = this.state
    const isError = httpStatus >= 400

    return (
      <Segment>
        <Header as="h1" textAlign="center">
          Test Administration
          <div>
            <Button content="Add New Test" onClick={this.handleOpenCreateModal} />
          </div>
          {isError && <Message error header={httpStatus} content={httpStatusText} />}
        </Header>
        <Table>
          {this.renderHeader()}
          <Table.Body>
            {tests.length > 0 ? tests.map(this.renderTableRow) : this.renderEmptyTable()}
          </Table.Body>
        </Table>
        {this.renderModal()}
      </Segment>
    )
  }
}

export default Tests
