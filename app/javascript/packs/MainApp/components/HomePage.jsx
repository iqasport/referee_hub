import React, { Component } from 'react'
import PropTypes from 'prop-types'
import { Label, Segment, Divider } from 'semantic-ui-react'

class HomePage extends Component {
  static propTypes = {
    history: PropTypes.shape({
      push: PropTypes.func
    }).isRequired
  }

  handleRefereeListClick = () => {
    const { history } = this.props
    history.push('/referees')
  }

  handleSignupClick = () => {
    /* eslint-disable */
    window.location.assign(`${window.location.origin}/sign_up`)
  }

  render() {
    return (
      <Segment textAlign='center'>
        <h1>Welcome to the Referee Hub</h1>
        <p>
          The home for all International Quidditch Referees. As a referee you'll be able to register with the IQA, pay for, and take written tests to become certified. Soon you'll also be able to find resources to cultivate your skills as a quidditch referee.
        </p>
        <p>
          If you're not a referee you do not have to register, but you can search for and view referees based on their certifications, what National Governing Body they are affiliated with, and their name.
        </p>
        <Divider />
        <h3>Ready to get started?</h3>
        <Label circular as="a" size="big" onClick={this.handleRefereeListClick} content="View all Referees" />
        <Label circular as="a" size="big" onClick={this.handleSignupClick} content="Register as a Referee" />
      </Segment>
    )
  }
}

export default HomePage
