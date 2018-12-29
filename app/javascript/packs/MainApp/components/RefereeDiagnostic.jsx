import React, { Component, Fragment } from 'react'
import {
  Segment,
  Form,
  Header,
  Message,
  List
} from 'semantic-ui-react'
import axios from 'axios'
import { Map } from 'immutable'
import { camelCase } from 'lodash'
import { DateTime } from 'luxon'
import ContentSegment from './ContentSegment'

const filterByType = (dataToFilter, type) => {
  const filteredData = dataToFilter.filter(dataItem => dataItem.type === type)
  return (
    filteredData.map(({ attributes }) => {
      const newMap = Map(attributes)
      return newMap.mapKeys(key => camelCase(key))
    })
  )
}

class RefereeDiagnostic extends Component {
  state = {
    referee: null,
    searchQuery: '',
    searchError: null
  }

  handleInputChange = (_event, { value }) => this.setState({ searchQuery: value })

  handleSearchSubmit = () => {
    const { searchQuery } = this.state

    this.setState({ referee: null, searchError: null })

    axios.post('/api/v1/admin/search', { referee_search: searchQuery })
      .then(({ data }) => {
        const { data: { attributes }, included } = data
        const nationalGoverningBodies = filterByType(included, 'national_governing_body')
        const certifications = filterByType(included, 'referee_certification')
        const testResults = filterByType(included, 'test_result')
        const testAttempts = filterByType(included, 'test_attempt')

        this.setState({
          referee: {
            firstName: attributes.first_name,
            lastName: attributes.last_name,
            email: attributes.email,
            pronouns: attributes.pronouns,
            submittedPaymentAt: attributes.submitted_payment_at,
            nationalGoverningBodies,
            certifications,
            testResults,
            testAttempts
          }
        })
      })
      .catch(({ response }) => {
        const { data: errorData } = response

        this.setState({ searchError: errorData.error })
      })
  }

  renderListItem = (label, itemContent, floatDirection = 'left') => (
    <List.Item>
      <List.Content style={{ textAlign: floatDirection }} floated={floatDirection}>
        <List.Header>{label}</List.Header>
        {itemContent}
      </List.Content>
    </List.Item>
  )

  renderRefereeDetails = () => {
    const { referee } = this.state
    const ngbNames = referee.nationalGoverningBodies.length > 0
      && referee.nationalGoverningBodies.map(ngb => ngb.get('name')).join(', ')
    const paymentDate = referee.submittedPaymentAt
      ? DateTime.fromSQL(referee.submittedPaymentAt.slice(0, -3).trim()).toLocaleString(DateTime.DATETIME_FULL)
      : 'N/A'

    return (
      <Fragment>
        <div style={{ display: 'flex' }}>
          <List divided verticalAlign="middle" style={{ flex: 1, marginRight: '3%' }}>
            {this.renderListItem('Email:', referee.email)}
            {referee.pronouns && this.renderListItem('Pronouns:', referee.pronouns)}
            {this.renderListItem('Submitted Head Ref Payment At:', paymentDate)}
            {ngbNames && this.renderListItem('National Governing Bodies:', ngbNames)}
          </List>
          <Segment style={{ flex: 1, marginTop: 0 }}>
            <List relaxed verticalAlign="middle">
              {this.renderListItem('Update Payment Details', 'This will be a link for updating payment', 'right')}
              {this.renderListItem('Update Certifications', 'This will be a link to update certs', 'right')}
              {this.renderListItem('Force Certification Renewal', 'This will be a link to renew certs', 'right')}
            </List>
          </Segment>
        </div>
      </Fragment>
    )
  }

  renderRefereeSegment = () => {
    const { referee } = this.state
    if (!referee) return null
    const headerContent = `${referee.firstName} ${referee.lastName}`

    return <ContentSegment headerContent={headerContent} segmentContent={this.renderRefereeDetails()} />
  }

  render() {
    const { searchQuery, searchError } = this.state

    return (
      <Segment>
        <Header as="h1" textAlign="center">Referee Diagnostic Tools</Header>
        <Form error={!!searchError}>
          <Form.Group>
            <Form.Input
              placeholder="Enter email to search"
              onChange={this.handleInputChange}
              value={searchQuery}
            />
            <Form.Button onClick={this.handleSearchSubmit} content="Search" />
          </Form.Group>
          {!!searchError && <Message error header="Referee Search Error" content={searchError} />}
        </Form>
        {this.renderRefereeSegment()}
      </Segment>
    )
  }
}

export default RefereeDiagnostic
