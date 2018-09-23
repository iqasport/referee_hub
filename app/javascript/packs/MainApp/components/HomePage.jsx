import React, { Component } from 'react'
import PropTypes from 'prop-types'

class HomePage extends Component {
  static propTypes = {
    history: PropTypes.shape({
      push: PropTypes.func
    }).isRequired
  }

  handleClick = () => {
    const { history } = this.props
    history.push('/referees')
  }

  render() {
    return (
      <div className="ui segment">
        <h1>Welcome to Refs-R-Us</h1>
        <button type="button" onClick={this.handleClick}>Go to Ref list</button>
      </div>
    )
  }
}

export default HomePage
