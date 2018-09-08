import React, { Component } from 'react'

class HomePage extends Component {
  handleClick = () => {
    this.props.history.push('/referees')
  }

  render () {
    return (
      <div>
        <h1 onClick={this.handleClick}>Welcome to Refs-R-Us</h1>
      </div>
    )
  }
}

export default HomePage
