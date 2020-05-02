import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import { DateTime } from 'luxon'
import {
  Header, Message, Tab, Segment, Loader, Button
} from 'semantic-ui-react'
import RefereeProfileEdit from '../components/RefereeProfileEdit'
import ProfileContent from '../components/ProfileContent'
import CertificationContent from '../components/CertificationContent'

class OldRefereeProfile extends Component {
  static propTypes = {
    match: PropTypes.shape({
      params: PropTypes.object
    }).isRequired,
    history: PropTypes.shape({
      push: PropTypes.func
    }).isRequired
  }

  state = {
    httpStatus: 0,
    httpStatusText: '',
    referee: {
      firstName: '',
      lastName: '',
      bio: '',
      email: '',
      showPronouns: false,
      pronouns: '',
      nationalGoverningBodies: [],
      certifications: [],
      testAttempts: [],
      testResults: [],
      isEditable: false,
      submittedPaymentAt: null,
    },
    paymentError: false,
    paymentSuccess: false,
    paymentCancel: false,
    changedFirstName: null,
    changedLastName: null,
    changedNGBs: null,
    changedBio: null,
    changedShowPronouns: null,
    changedPronouns: null,
    policyAccepted: false
  }

  componentDidMount() {
    axios
      .get(this.currentRefereeApiRoute)
      .then(this.setComponentStateFromBackendData)
      .catch(this.setErrorStateFromBackendData)
  }

  get currentRefereeApiRoute() {
    const { match: { params } } = this.props

    return `/api/v1/referees/${params.id}`
  }

  get refereeEditValues() {
    const {
      referee, changedFirstName, changedLastName, changedNGBs, changedBio, changedShowPronouns, changedPronouns
    } = this.state

    return {
      firstName: changedFirstName || referee.firstName,
      lastName: changedLastName || referee.lastName,
      nationalGoverningBodies: changedNGBs || referee.nationalGoverningBodies,
      bio: changedBio || referee.bio,
      showPronouns: changedShowPronouns || referee.showPronouns,
      pronouns: changedPronouns || referee.pronouns
    }
  }

  setComponentStateFromBackendData = ({ status, statusText, data }) => {
    const { data: { attributes }, included } = data
    const certifications = included
      .filter(({ type }) => type === 'certification')
      .map(certification => certification.attributes) || []
    const nationalGoverningBodies = included
      .filter(({ type }) => type === 'nationalGoverningBody')
      .map(nationalGoverningBody => ({
        id: nationalGoverningBody.id,
        name: nationalGoverningBody.attributes.name,
        website: nationalGoverningBody.attributes.website
      })) || []
    const testAttempts = included
      .filter(({ type }) => type === 'testAttempt')
      .map(testAttempt => testAttempt.attributes) || []
    const testResults = included
      .filter(({ type }) => type === 'testResult')
      .map(testResult => ({
        duration: testResult.attributes.duration,
        minimumPassPercentage: testResult.attributes.minimumPassPercentage,
        passed: testResult.attributes.passed,
        percentage: testResult.attributes.percentage,
        pointsAvailable: testResult.attributes.pointsAvailable,
        pointsScored: testResult.attributes.pointsScored,
        timeFinished: testResult.attributes.timeFinished,
        timeStarted: testResult.attributes.timeStarted,
        testLevel: testResult.attributes.testLevel
      })) || []

    this.setState({
      httpStatus: status,
      httpStatusText: statusText,
      referee: {
        firstName: attributes.firstName,
        lastName: attributes.lastName,
        bio: attributes.bio,
        email: attributes.email,
        showPronouns: attributes.showPronouns,
        pronouns: attributes.pronouns,
        nationalGoverningBodies,
        certifications,
        testAttempts,
        testResults,
        isEditable: attributes.isEditable,
        submittedPaymentAt: attributes.submittedPaymentAt,
      },
      policyAccepted: !attributes.hasPendingPolicies
    })
  }

  setErrorStateFromBackendData = (error) => {
    const { status, statusText } = error.response || {
      status: 500,
      statusText: 'Error'
    }

    this.setState({
      httpStatus: status,
      httpStatusText: statusText
    })
  }

  handleRouteChange = (newRoute) => {
    const { history } = this.props

    history.push(newRoute)
  }

  handleSubmit = () => {
    const {
      referee: {
        firstName,
        lastName,
        bio,
        showPronouns,
        pronouns,
        nationalGoverningBodies
      },
      changedFirstName,
      changedLastName,
      changedNGBs,
      changedBio,
      changedShowPronouns,
      changedPronouns
    } = this.state

    const ngbState = changedNGBs
      ? changedNGBs.map(ngbId => Number(ngbId))
      : nationalGoverningBodies.map(ngb => Number(ngb.id))

    axios
      .patch(this.currentRefereeApiRoute, {
        first_name: changedFirstName || firstName,
        last_name: changedLastName || lastName,
        bio: changedBio || bio,
        show_pronouns: changedShowPronouns || showPronouns,
        pronouns: changedPronouns || pronouns,
        national_governing_body_ids: ngbState
      })
      .then(this.setComponentStateFromBackendData)
      .catch(this.setErrorStateFromBackendData)
  };

  handlePaymentSuccess = (payment) => {
    const { paid } = payment

    if (paid) {
      this.setState({ paymentSuccess: true, paymentError: false, paymentCancel: false })
      axios
        .patch(this.currentRefereeApiRoute, {
          submitted_payment_at: DateTime.local().toString()
        })
        .then(this.setComponentStateFromBackendData)
        .catch(this.setErrorStateFromBackendData)
    }
  }

  handlePaymentError = () => {
    this.setState({ paymentError: true, paymentCancel: false, paymentSuccess: false })
  }

  handlePaymentCancel = () => {
    this.setState({ paymentCancel: true, paymentError: false, paymentSuccess: false })
  }

  renderPaymentMessage = () => {
    const { paymentError, paymentSuccess, paymentCancel } = this.state
    if (!paymentError && !paymentSuccess && !paymentCancel) return null

    const successMessage = 'Your payment was successful.'
    const errorMessage = 'There was an issue with your payment, please try again.'
    const cancelMessage = 'Your payment was cancelled.'

    let messageProps
    if (paymentError) {
      messageProps = { error: true }
    } else if (paymentSuccess) {
      messageProps = { positive: true }
    } else if (paymentCancel) {
      messageProps = { warning: true }
    }

    return (
      <Message {...messageProps} size="small" onDismiss={this.clearPaymentState}>
        <p>
          {paymentSuccess && successMessage}
          {paymentError && errorMessage}
          {paymentCancel && cancelMessage}
        </p>
      </Message>
    )
  }

  clearPaymentState = () => {
    this.setState({ paymentError: false, paymentSuccess: false, paymentCancel: false })
  }

  handleInputChange = (stateKey, value) => {
    this.setState({ [stateKey]: value })
  }

  handleAcceptPolicy = () => {
    const { match: { params } } = this.props
    axios.post(`/api/v1/users/${params.id}/accept_policies`, { id: params.id })
      .then(() => {
        this.setState({ policyAccepted: true })
      })
  }

  handleRejectPolicy = () => {
    const { match: { params } } = this.props
    axios.post(`/api/v1/users/${params.id}/accept_policies`)
  }

  renderProfileContent = () => {
    const { httpStatus, httpStatusText, referee } = this.state

    let content
    if (httpStatus !== 200) {
      content = <h1>{httpStatusText}</h1>
    } else {
      content = <ProfileContent referee={referee} />
    }

    return (
      <Tab.Pane>
        {!httpStatus && <Loader active />}
        {content}
      </Tab.Pane>
    )
  }

  renderCertificationContent = () => {
    const {
      referee: {
        certifications, isEditable, submittedPaymentAt, testResults, testAttempts
      }
    } = this.state
    const { match: { params } } = this.props

    return (
      <Tab.Pane>
        <CertificationContent
          refereeId={params.id}
          isEditable={isEditable}
          hasPaid={!!submittedPaymentAt}
          testResults={testResults}
          testAttempts={testAttempts}
          refCertifications={certifications}
          onSuccess={this.handlePaymentSuccess}
          onError={this.handlePaymentError}
          onCancel={this.handlePaymentCancel}
          onRouteChange={this.handleRouteChange}
        />
      </Tab.Pane>
    )
  }

  renderPronouns = () => {
    const { referee: { showPronouns, pronouns } } = this.state
    if (!showPronouns) return null

    return <Fragment>{pronouns}</Fragment>
  }

  renderAcceptPolicy = () => {
    const { policyAccepted, referee: { isEditable } } = this.state
    if (!isEditable) return null
    if (policyAccepted) return null

    return (
      <Message warning>
        <Message.Header>Privacy Policy</Message.Header>
        <p>
          {'We have updated our '}
          <a target="_blank" rel="noopener noreferrer" href="https://www.iqareferees.org/privacy">Privacy Policy</a>
          , please review and accept no later than June 1st, 2020.
        </p>
        <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
          <Button style={{ margin: '0 20px' }} content="Reject" color="red" onClick={this.handleRejectPolicy} />
          <Button content="Accept" primary onClick={this.handleAcceptPolicy} />
        </div>
      </Message>
    )
  }

  render() {
    const { referee } = this.state

    const refHeader = (referee.firstName || referee.lastName) && `${referee.firstName} ${referee.lastName}`
    const panes = [
      { menuItem: 'Profile', render: this.renderProfileContent },
      { menuItem: 'Certifications', render: this.renderCertificationContent }
    ]

    return (
      <Segment>
        {this.renderAcceptPolicy()}
        {this.renderPaymentMessage()}
        <Header as="h1" textAlign="center">
          {refHeader || 'Anonymous Referee'}
          <Header sub>{this.renderPronouns()}</Header>
          {
            referee.isEditable
            && (
              <RefereeProfileEdit
                values={this.refereeEditValues}
                onSubmit={this.handleSubmit}
                onChange={this.handleInputChange}
              />
            )
          }
        </Header>
        <Tab panes={panes} />
      </Segment>
    )
  }
}

export default OldRefereeProfile