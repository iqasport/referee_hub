/* eslint-disable camelcase */
import React, { Component } from 'react'
import PropTypes from 'prop-types'
import {
  Segment, Header, Table, Icon
} from 'semantic-ui-react'
import axios from 'axios'
import { capitalize } from 'lodash'

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
    tests: []
  }

  componentDidMount() {
    axios.get('/api/v1/tests')
      .then(({ data }) => {
        const { data: testData } = data

        const sanitizedData = testData.map(sanitizeTestData)
        this.setState({ tests: sanitizedData })
      })
  }

  handleTestClick = id => () => {
    const { history } = this.props

    history.push(`/admin/tests/${id}`)
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
      </Table.Row>
    </Table.Header>
  )

  renderTableRow = (test) => {
    const {
      active, name, level, language, id
    } = test
    const passedIcon = <Icon name="checkmark" color="green" size="large" />
    const failedIcon = <Icon name="times" color="red" size="large" />

    return (
      <Table.Row key={id} onClick={this.handleTestClick(id)} style={{ cursor: 'pointer' }}>
        <Table.Cell>
          {active ? passedIcon : failedIcon}
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
      </Table.Row>
    )
  }

  renderEmptyTable = () => (
    <Table.Row>
      <Table.Cell textAlign="center">
        No tests found.
      </Table.Cell>
    </Table.Row>
  )

  render() {
    const { tests } = this.state

    return (
      <Segment>
        <Header as="h1" textAlign="center">Test Administration</Header>
        <Table>
          {this.renderHeader()}
          <Table.Body>
            {tests.length > 0 ? tests.map(this.renderTableRow) : this.renderEmptyTable()}
          </Table.Body>
        </Table>
      </Segment>
    )
  }
}

export default Tests
