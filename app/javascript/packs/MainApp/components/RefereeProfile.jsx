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
    firstName: '',
    lastName: '',
    bio: '',
    email: '',
    pronouns: '',
    nationalGoverningBodies: [],
    certifications: []
  };

  componentDidMount() {
    const { match: { params } } = this.props;

    axios.get(`/api/v1/referees/${params.id}`)
      .then(({ data: { data: { attributes }, included } }) => {
        const certifications = included
          .filter(({ type }) => type === 'certification')
          .map(certification => certification.attributes);
        const nationalGoverningBodies = included
          .filter(({ type }) => type === 'national_governing_body')
          .map(nationalGoverningBody => nationalGoverningBody.attributes);
        this.setState({
          firstName: attributes.first_name,
          lastName: attributes.last_name,
          bio: attributes.bio,
          email: attributes.email,
          pronouns: attributes.pronouns,
          nationalGoverningBodies,
          certifications
        })
      })
  }

  render() {
    const {
      firstName,
      lastName,
      bio,
      email,
      pronouns,
      nationalGoverningBodies,
      certifications
    } = this.state;

    return (
      <div>
        <h1>{`${firstName} ${lastName}`}</h1>
        <dl>
          {pronouns
            && (
              <Fragment>
                <dt>Pronouns:</dt>
                <dd>{pronouns}</dd>
              </Fragment>
            )
          }
          <dt>
            National Governing
            {nationalGoverningBodies.count > 1 ? ' Bodies' : ' Body'}
            :
          </dt>
          {
            nationalGoverningBodies
              .map(
                nationalGoverningBody => (
                  <dd key={nationalGoverningBody.name}>
                    <a href={nationalGoverningBody.website}>
                      {nationalGoverningBody.name}
                    </a>
                  </dd>)
              )
          }
          <dt>Email:</dt>
          <dd>
            <a href={`mailto:${email}`}>{email}</a>
          </dd>
        </dl>
        <h2>Certifications</h2>
        <dl>
          <dt>Snitch Referee</dt>
          <dd>{certifications.some(({ level }) => level === 'snitch') ? 'YES' : 'NO'}</dd>
          <dt>Assistant Referee</dt>
          <dd>{certifications.some(({ level }) => level === 'assistant') ? 'YES' : 'NO'}</dd>
          <dt>Head Referee Written</dt>
          <dd>{certifications.some(({ level }) => level === 'head') ? 'YES' : 'NO'}</dd>
          <dt>Head Referee Field</dt>
          <dd>{certifications.some(({ level }) => level === 'field') ? 'YES' : 'NO'}</dd>
        </dl>
        <h2>Bio</h2>
        <p>{bio}</p>
      </div>
    )
  }
}

export default RefereeProfile
