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
    certifications: [],
    isEditable: false,
    edit: false
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
      certifications,
      isEditable: attributes.is_editable
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

  startEditMode() {
    this.setState({
      edit: true
    })
  }

  changeInput(event) {
    this.setState({
      [event.target.id]: event.target.value
    })
  }

  changeCheckbox(event) {
    this.setState({
      [event.target.id]: event.target.checked
    })
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
      nationalGoverningBodies,
      isEditable,
      edit
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

    if (edit) {
      return (
        <form method="post">
          <h1>
            <input id="firstName" type="text" value={firstName} placeholder="First names" onChange={this.changeInput.bind(this)} />
            {' '}
            <input id="lastName" type="text" value={lastName} placeholder="Last name" onChange={this.changeInput.bind(this)} />
            {' (Wow, that’s you!)'}
          </h1>
          <dl>
            <dt>
              Pronouns:
            </dt>
            <dd>
              <input id="pronouns" type="text" value={pronouns} placeholder="Your pronouns" onChange={this.changeInput.bind(this)} />
              <label htmlFor="showPronouns">
                <input id="showPronouns" type="checkbox" checked={showPronouns} onChange={this.changeCheckbox.bind(this)} />
                Show my pronouns on my referee profile
              </label>
            </dd>
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
              <input id="email" type="text" value={email} placeholder="Email address" onChange={this.changeInput.bind(this)} />
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
            <textarea id="bio" value={bio} placeholder="Your bio" onChange={this.changeInput.bind(this)} />
          </p>
          <button type="submit">Save</button>
        </form>
      )
    }

    return (
      <Fragment>
        <h1>
          {`${firstName} ${lastName}`}
          {isEditable && ' (Wow, that’s you!)'}
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
        {isEditable && <button type="button" onClick={this.startEditMode.bind(this)}>Edit</button>}
      </Fragment>
    )
  }
}

export default RefereeProfile
