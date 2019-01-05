import React, { Component, Fragment } from 'react'
import {
  Segment,
  Form,
  Header,
  Message,
  List,
  Button,
  Modal
} from 'semantic-ui-react'
import axios from 'axios'
import { Map } from 'immutable'
import { camelCase } from 'lodash'
import { DateTime } from 'luxon'
import ContentSegment from './ContentSegment'
import TestResultsTable from './TestResultsTable'

const filterByType = (dataToFilter, type) => {
  const filteredData = dataToFilter.filter(dataItem => dataItem.type === type)
  return (
    filteredData.map(({ id, attributes }) => {
      const newMap = Map(attributes)
      return newMap.mapKeys(key => camelCase(key)).set('id', id)
    })
  )
}

class RefereeDiagnostic extends Component {
  state = {
    referee: null,
    searchQuery: '',
    searchError: null,
    modalOpen: false,
    modalAction: '',
    actionError: null
  }

  get modalConfig() {
    const paymentContent = 'Before clicking confirm, has this referee provided evidence they have completed the'
      + " necessary transaction? If so clicking confirm will update this referee's payment details giving them access"
      + ' to the head referee test.'

    const renewContent = ''

    return {
      payment: {
        onConfirm: this.handleUpdatePayment,
        onCancel: this.handleCloseModal,
        header: 'Confirm Payment Update',
        content: paymentContent,
        errorMessage: "There was an issue updating this Referee's payment details."
      },
      renew: {
        onConfirm: this.handleRenewCertifications,
        onCancel: this.handleCloseModal,
        header: 'Confirm Certification Renewal',
        content: renewContent,
        errorMessage: "There was an issue updating this Referee's certifications."
      }
    }
  }

  handleApiResponse = ({ data }) => {
    const { data: { attributes, id }, included } = data
    const nationalGoverningBodies = filterByType(included, 'national_governing_body')
    const certifications = filterByType(included, 'referee_certification')
    const testResults = filterByType(included, 'test_result')
    const testAttempts = filterByType(included, 'test_attempt')

    this.setState({
      referee: {
        firstName: attributes.first_name,
        lastName: attributes.last_name,
        pronouns: attributes.pronouns,
        submittedPaymentAt: attributes.submitted_payment_at,
        refereeId: id,
        nationalGoverningBodies,
        certifications,
        testResults,
        testAttempts
      }
    })
  }

  handleInputChange = (_event, { value }) => this.setState({ searchQuery: value })

  handleSearchSubmit = () => {
    const { searchQuery } = this.state

    this.setState({ referee: null, searchError: null })

    axios.post('/api/v1/admin/search', { referee_search: searchQuery })
      .then(this.handleApiResponse)
      .catch(({ response }) => {
        const { data: errorData } = response

        this.setState({ searchError: errorData.error })
      })
  }

  handleCloseModal = () => this.setState({ modalOpen: false, modalAction: '' })

  handleOpenModal = modalAction => this.setState({ modalOpen: true, modalAction })

  handleRenewCertifications = () => {
    const { referee } = this.state
    const { certifications } = referee

    certifications.forEach((cert) => {
      const { id } = cert.toJS()
      axios
        .patch(`/api/v1/referee_certifications/${id}`, {
          needs_renewal_at: DateTime.local().toString()
        })
        .then(() => {
          this.handleCloseModal()
        })
    })
  }

  handleUpdatePayment = () => {
    const { referee } = this.state

    const apiData = { referee_id: referee.refereeId, submitted_payment_at: DateTime.local().toString() }
    axios.patch('/api/v1/admin/update-payment', apiData)
      .then(this.handleApiResponse)
      .then(this.handleCloseModal)
      .catch(({ response }) => {
        const { data: errorData } = response

        this.setState({ actionError: errorData.error })
      })
  }

  renderListItem = (label, itemContent) => (
    <List.Item>
      <List.Content>
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
    const updatePaymentLink = this.renderModalLink('payment', 'Confirm Payment')
    const renewCerts = this.renderModalLink('renew', 'Renew Certifications')

    return (
      <Fragment>
        <div style={{ display: 'flex' }}>
          <List divided verticalAlign="middle" style={{ flex: 1, marginRight: '3%' }}>
            {referee.pronouns && this.renderListItem('Pronouns:', referee.pronouns)}
            {this.renderListItem('Submitted Head Ref Payment At:', paymentDate)}
            {ngbNames && this.renderListItem('National Governing Bodies:', ngbNames)}
          </List>
          <Segment style={{ flex: 2, marginTop: 0 }}>
            <List horizontal verticalAlign="middle" style={{ display: 'flex', justifyContent: 'space-evenly' }}>
              {this.renderListItem('Update Payment Details', updatePaymentLink)}
              {this.renderListItem('Update Certifications', 'Coming soon!')}
              {this.renderListItem('Force Certification Renewal', renewCerts)}
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

  renderModalLink = (action, linkContent) => {
    const handleClick = () => this.handleOpenModal(action)
    return (
      <div style={{ margin: '10px 0' }}>
        <Button as="a" onClick={handleClick}>{linkContent}</Button>
      </div>
    )
  }

  renderModal = () => {
    const { modalOpen, modalAction, actionError } = this.state
    if (!modalOpen) return null

    const {
      onConfirm, onCancel, header, content, errorMessage
    } = this.modalConfig[modalAction]

    return (
      <Modal open={modalOpen} size="mini">
        <Modal.Header>{header}</Modal.Header>
        {!!actionError && (
          <Message negative>
            <Message.Header>{errorMessage}</Message.Header>
            {actionError}
          </Message>
        )}
        <Modal.Content>{content}</Modal.Content>
        <Modal.Actions>
          <Button secondary onClick={onCancel}>Cancel</Button>
          <Button primary onClick={onConfirm}>Confirm</Button>
        </Modal.Actions>
      </Modal>
    )
  }

  renderTestResults = () => {
    const { referee } = this.state
    if (!referee) return null

    const { testResults } = referee
    if (testResults.length < 1) return null

    const headerContent = 'Test Result Details'
    const segmentContent = <TestResultsTable testResults={testResults.map(result => result.toJS())} />

    return (
      <Fragment>
        <ContentSegment headerContent={headerContent} segmentContent={segmentContent} />
      </Fragment>
    )
  }

  render() {
    const { searchQuery, searchError } = this.state

    return (
      <Segment>
        <Header as="h1" textAlign="center">Referee Diagnostic</Header>
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
        {this.renderModal()}
        {this.renderTestResults()}
      </Segment>
    )
  }
}

export default RefereeDiagnostic
