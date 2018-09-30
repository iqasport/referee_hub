import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import { DateTime } from 'luxon'
import { Header, Message } from 'semantic-ui-react'
import RefereeProfileEdit from './RefereeProfileEdit'
import PaypalButton from './PaypalButton'

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
      isEditable: false,
      submittedPaymentAt: null
    },
    paymentError: false,
    paymentSuccess: false,
    paymentCancel: false
  };

  componentDidMount() {
    axios
      .get(this.currentRefereeApiRoute)
      .then(this.setComponentStateFromBackendData)
      .catch(this.setErrorStateFromBackendData)
  }

  get currentRefereeApiRoute() {
    const { match: { params } } = this.props;

    return `/api/v1/referees/${params.id}`
  }

  setComponentStateFromBackendData = ({ status, statusText, data }) => {
    const { data: { attributes }, included } = data;
    const certifications = included
      .filter(({ type }) => type === 'certification')
      .map(certification => certification.attributes);
    const nationalGoverningBodies = included
      .filter(({ type }) => type === 'national_governing_body')
      .map(nationalGoverningBody => ({
        id: nationalGoverningBody.id,
        name: nationalGoverningBody.attributes.name,
        website: nationalGoverningBody.attributes.website
      }));
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
        isEditable: attributes.is_editable,
        submittedPaymentAt: attributes.submitted_payment_at
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

  getNationalGoverningBodyJsx = nationalGoverningBody => (
    <dd key={nationalGoverningBody.name}>
      <a href={nationalGoverningBody.website}>
        {nationalGoverningBody.name}
      </a>
    </dd>
  );

  change = (property, value) => {
    this.setState((state) => {
      const {
        referee: {
          firstName,
          lastName,
          bio,
          email,
          showPronouns,
          pronouns,
          nationalGoverningBodies,
          certifications,
          isEditable
        }
      } = state;

      return {
        referee: {
          firstName: property === 'firstName' ? value : firstName,
          lastName: property === 'lastName' ? value : lastName,
          bio: property === 'bio' ? value : bio,
          email,
          pronouns: property === 'pronouns' ? value : pronouns,
          showPronouns: property === 'showPronouns' ? value : showPronouns,
          nationalGoverningBodies: property === 'nationalGoverningBodies' ? value : nationalGoverningBodies,
          certifications,
          isEditable
        }
      }
    })
  };

  save = () => {
    const {
      referee: {
        firstName,
        lastName,
        bio,
        showPronouns,
        pronouns,
        nationalGoverningBodies
      }
    } = this.state;

    axios
      .patch(this.currentRefereeApiRoute, {
        first_name: firstName,
        last_name: lastName,
        bio,
        show_pronouns: showPronouns,
        pronouns,
        national_governing_body_ids: nationalGoverningBodies.map(
          nationalGoverningBody => Number(nationalGoverningBody.id)
        )
      })
      .then(this.setComponentStateFromBackendData)
      .catch(this.setErrorStateFromBackendData);
  };

  hasPassedTest = (level) => {
    const { referee: { certifications } } = this.state

    return certifications.some(({ level: certificationLevel }) => certificationLevel === level)
  }

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

  render() {
    const {
      httpStatus,
      httpStatusText,
      referee
    } = this.state;

    if (!httpStatus) {
      return (
        <h1>
          Loading referee profile
        </h1>
      )
    }

    if (httpStatus !== 200) {
      return (
        <h1>
          {httpStatusText}
        </h1>
      )
    }

    return (
      <div>
        {this.renderPaymentMessage()}
        <Header as="h1">
          {
            (
              (referee.firstName || referee.lastName)
              && `${referee.firstName} ${referee.lastName}`
            ) || 'Anonymous Referee'
          }
        </Header>
        {
          referee.isEditable && (
            <Message info>
              <Message.Header>
                Hey! This is your referee profile!
              </Message.Header>
              <p>
                You can copy this page’s link to tournament staff to show them your referee
                certifications. Feel free to edit your profile!
                {
                  !referee.firstName && !referee.lastName
                  && (
                    ' We suggest setting your name and your NGB first. Click on the edit button to'
                    + ' start.'
                  )
                }
              </p>
              <RefereeProfileEdit values={referee} onChange={this.change} onSubmit={this.save} />
            </Message>
          )
        }
        {
          referee.isEditable && !referee.submittedPaymentAt
          && (
            <PaypalButton
              onSuccess={this.handlePaymentSuccess}
              onError={this.handlePaymentError}
              onCancel={this.handlePaymentCancel}
            />
          )
        }
        <dl>
          {referee.showPronouns
            && (
              <Fragment>
                <dt>
                  Pronouns:
                </dt>
                <dd>
                  {referee.pronouns}
                </dd>
              </Fragment>
            )
          }
          <dt>
            National Governing
            {referee.nationalGoverningBodies.count > 1 ? ' Bodies' : ' Body'}
            :
          </dt>
          {referee.nationalGoverningBodies.map(this.getNationalGoverningBodyJsx)}
          {
            !referee.nationalGoverningBodies.length
            && (
              <dd>
                Unknown
              </dd>
            )
          }
          <dt>
            Email:
          </dt>
          <dd>
            <a href={`mailto:${referee.email}`}>
              {referee.email}
            </a>
          </dd>
        </dl>
        <h2>
          Certifications
        </h2>
        <dl>
          <dt>
            Snitch Referee
          </dt>
          <dd>
            {this.hasPassedTest('snitch') ? '✓' : '✗'}
          </dd>
          <dt>
            Assistant Referee
          </dt>
          <dd>
            {this.hasPassedTest('assistant') ? '✓' : '✗'}
          </dd>
          <dt>
            Head Referee Written
          </dt>
          <dd>
            {this.hasPassedTest('head') ? '✓' : '✗'}
          </dd>
          <dt>
            Head Referee Field
          </dt>
          <dd>
            {this.hasPassedTest('field') ? '✓' : '✗'}
          </dd>
        </dl>
        {
          referee.bio
          && (
            <Fragment>
              <Header as="h2">
                Bio
              </Header>
              <p>
                {referee.bio}
              </p>
            </Fragment>
          )
        }
      </div>
    )
  }
}

export default RefereeProfile
