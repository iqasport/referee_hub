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
          The home for all international quidditch referees. Here you will find every referee qualified by the IQA based on their certifications, their National Governing Body affiliation, and their name.
        </p>
        <p>
          As a referee you're able to register and you’ll be able to immediately take written tests to become certified as snitch referee and/or assistant referee. If you passed both tests, you're free to pay for the written head referee test.Soon you'll also be able to find resources to cultivate your skills as a quidditch referee.
        </p>
        <Divider />
        <h3>Ready to get started?</h3>
        <Label
          circular
          as="a"
          size="huge"
          color="blue"
          onClick={this.handleRefereeListClick}
          content="View all Referees"
        />
        <Label
          circular
          as="a"
          size="huge"
          color="green"
          onClick={this.handleSignupClick}
          content="Register as a Referee"
        />
      </Segment>
    )
  }
}

export default HomePage
