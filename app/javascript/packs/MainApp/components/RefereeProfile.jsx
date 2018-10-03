import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import { DateTime } from 'luxon'
import {
  Header, Message, Tab, Segment, Loader
} from 'semantic-ui-react'
import RefereeProfileEdit from './RefereeProfileEdit'
import ProfileContent from './ProfileContent'
import CertificationContent from './CertificationContent'

class RefereeProfile extends Component {
  static propTypes = {
    match: PropTypes.shape({
      params: PropTypes.object
    }).isRequired
  };

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
      submittedPaymentAt: null
    },
    paymentError: false,
    paymentSuccess: false,
    paymentCancel: false,
    changedFirstName: null,
    changedLastName: null,
    changedNGBs: null,
    changedBio: null,
    changedShowPronouns: null,
    changedPronouns: null
  };

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

  get refereeDisplayName() {
    const {
      referee: { firstName, lastName }
    } = this.state

    if (!firstName && !lastName) {
      return 'Anonymous Referee'
    }

    if (firstName && !lastName) {
      return firstName
    }

    if (!firstName && lastName) {
      return lastName
    }

    return `${firstName} ${lastName}`
  }

  setComponentStateFromBackendData = ({ status, statusText, data }) => {
    const { data: { attributes }, included } = data
    const certifications = included
      .filter(({ type }) => type === 'certification')
      .map(certification => certification.attributes)
    const nationalGoverningBodies = included
      .filter(({ type }) => type === 'national_governing_body')
      .map(nationalGoverningBody => ({
        id: nationalGoverningBody.id,
        name: nationalGoverningBody.attributes.name,
        website: nationalGoverningBody.attributes.website
      }))
    const testAttempts = included
      .filter(({ type }) => type === 'test_attempt')
      .map(testAttempt => testAttempt.attributes)
    const testResults = included
      .filter(({ type }) => type === 'test_result')
      .map(testResult => ({
        duration: testResult.attributes.duration,
        minimumPassPercentage: testResult.attributes.minimum_pass_percentage,
        passed: testResult.attributes.passed,
        percentage: testResult.attributes.percentage,
        pointsAvailable: testResult.attributes.points_available,
        pointsScored: testResult.attributes.points_scored,
        timeFinished: testResult.attributes.time_finished,
        timeStarted: testResult.attributes.time_started,
        testLevel: testResult.attributes.test_level
      }))

    this.setState({
      httpStatus: status,
      httpStatusText: statusText,
      referee: {
        firstName: attributes.first_name,
        lastName: attributes.last_name,
        bio: attributes.bio,
        email: attributes.email,
        showPronouns: attributes.show_pronouns,
        pronouns: attributes.pronouns,
        nationalGoverningBodies,
        certifications,
        testAttempts,
        testResults,
        isEditable: attributes.is_editable,
        submittedPaymentAt: attributes.submitted_payment_at,
        gettingStartedDismissedAt: attributes.getting_started_dismissed_at
      }
    })
  };

  setErrorStateFromBackendData = (error) => {
    const { status, statusText } = error.response || {
      status: 500,
      statusText: 'Error'
    };

    this.setState({
      httpStatus: status,
      httpStatusText: statusText
    });
  };

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

    const ngbs = changedNGBs || nationalGoverningBodies
    const ngbState = ngbs.map(ngbId => Number(ngbId))

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

  handleGetStartedDismiss = () => {
    axios
      .patch(this.currentRefereeApiRoute, {
        getting_started_dismissed_at: DateTime.local().toString()
      })
      .then(this.setComponentStateFromBackendData)
      .catch(this.setErrorStateFromBackendData)
  }

  handleInputChange = (stateKey, value) => {
    this.setState({ [stateKey]: value })
  }

  renderProfileContent = () => {
    const { httpStatus, httpStatusText, referee } = this.state

    let content
    if (httpStatus !== 200) {
      content = <h1>{httpStatusText}</h1>
    } else {
      content = <ProfileContent referee={referee} onDismiss={this.handleGetStartedDismiss} />
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
        certifications, isEditable, submittedPaymentAt, nationalGoverningBodies, testResults, testAttempts
      }
    } = this.state
    const { match: { params } } = this.props

    const shouldTakeOldTests = nationalGoverningBodies.some(ngb => /australia/i.test(ngb.name))

    return (
      <Tab.Pane>
        <CertificationContent
          refereeId={params.id}
          isEditable={isEditable}
          shouldTakeOldTests={shouldTakeOldTests}
          hasPaid={!!submittedPaymentAt}
          testResults={testResults}
          testAttempts={testAttempts}
          refCertifications={certifications}
          onSuccess={this.handlePaymentSuccess}
          onError={this.handlePaymentError}
          onCancel={this.handlePaymentCancel}
        />
      </Tab.Pane>
    )
  }

  renderPronouns = () => {
    const { referee: { showPronouns, pronouns } } = this.state
    if (!showPronouns) return null

    return <Fragment>{pronouns}</Fragment>
  }

  render() {
    const { referee } = this.state

    const panes = [
      { menuItem: 'Profile', render: this.renderProfileContent },
      { menuItem: 'Certifications', render: this.renderCertificationContent }
    ]

    return (
      <Segment>
        {this.renderPaymentMessage()}
        <Header as="h1" textAlign="center">
          {this.refereeDisplayName}
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

export default RefereeProfile
