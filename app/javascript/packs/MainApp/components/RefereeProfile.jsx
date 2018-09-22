import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'

class RefereeProfile extends Component {
  static propTypes = {
    match: PropTypes.shape({
      params: PropTypes.object
    }).isRequired
  };

  state = {
    httpStatus: 0,
    httpStatusText: '',
    firstName: '',
    lastName: '',
    bio: '',
    email: '',
    showPronouns: false,
    pronouns: '',
    nationalGoverningBodies: [],
    certifications: []
  };

  componentDidMount() {
    const { match: { params } } = this.props;

    axios.get(`/api/v1/referees/${params.id}`)
      .then(this.setComponentStateFromBackendData.bind(this))
      .catch(this.setErrorStateFromBackendData.bind(this))
  }

  setComponentStateFromBackendData({ status, statusText, data }) {
    const { data: { attributes }, included } = data;
    const certifications = included
      .filter(({ type }) => type === 'certification')
      .map(certification => certification.attributes);
    const nationalGoverningBodies = included
      .filter(({ type }) => type === 'national_governing_body')
      .map(nationalGoverningBody => nationalGoverningBody.attributes);
    this.setState({
      httpStatus: status,
      httpStatusText: statusText,
      firstName: attributes.first_name,
      lastName: attributes.last_name,
      bio: attributes.bio,
      email: attributes.email,
      showPronouns: attributes.show_pronouns,
      pronouns: attributes.pronouns,
      nationalGoverningBodies,
      certifications
    })
  }

  setErrorStateFromBackendData(error) {
    const { status, statusText } = error.response || {
      status: 500,
      statusText: 'Error'
    };

    this.setState({
      httpStatus: status,
      httpStatusText: statusText
    });
  }

  getNationalGoverningBodyJsx = nationalGoverningBody => (
    <dd key={nationalGoverningBody.name}>
      <a href={nationalGoverningBody.website}>
        {nationalGoverningBody.name}
      </a>
    </dd>
  );

  hasPassedTest(level) {
    const { certifications } = this.state;

    return certifications.some(({ level: certificationLevel }) => certificationLevel === level)
  }

  render() {
    const {
      httpStatus,
      httpStatusText,
      firstName,
      lastName,
      bio,
      email,
      showPronouns,
      pronouns,
      nationalGoverningBodies
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
        <h1>
          {`${firstName} ${lastName}`}
        </h1>
        <dl>
          {showPronouns
            && (
              <Fragment>
                <dt>
                  Pronouns:
                </dt>
                <dd>
                  {pronouns}
                </dd>
              </Fragment>
            )
          }
          <dt>
            National Governing
            {nationalGoverningBodies.count > 1 ? ' Bodies' : ' Body'}
            :
          </dt>
          {
            nationalGoverningBodies.map(this.getNationalGoverningBodyJsx)
          }
          <dt>
            Email:
          </dt>
          <dd>
            <a href={`mailto:${email}`}>
              {email}
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
        <h2>
          Bio
        </h2>
        <p>
          {bio}
        </p>
      </div>
    )
  }
}

export default RefereeProfile
