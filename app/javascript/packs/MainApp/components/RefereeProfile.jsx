import React, { Component } from 'react'
import axios from 'axios'

class RefereeProfile extends Component {
  state = {
    firstName: '',
    lastName: '',
    bio: '',
    showPronouns: null,
    pronouns: '',
    email: ''
  }

  componentDidMount() {
    const { params } = this.props.match

    axios.get(`/api/v1/referees/${params.id}`)
      .then(({ data }) => {
        const { attributes } = data.data
        this.setState({
          firstName: attributes.first_name,
          lastName: attributes.last_name,
          bio: attributes.bio,
          showPronouns: attributes.show_pronouns,
          pronouns: attributes.pronouns,
          email: attributes.email
        })
      })
  }

  render() {
    const { firstName, lastName, bio, email } = this.state

    return (
      <div>
        <h1>Referee Profile</h1>
        <h3>
          {`Name: ${firstName} ${lastName}`}
        </h3>
        <h3>
          {`Bio: ${bio}`}
        </h3>
        <h3>
          {`Email: ${email}`}
        </h3>
      </div>
    )
  }
}

export default RefereeProfile
