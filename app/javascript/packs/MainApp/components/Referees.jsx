import React, { Component } from 'react'
import axios from 'axios';

class Referees extends Component {
  state = {
    referees: []
  }

  componentDidMount() {
    axios.get('/api/v1/referees')
      .then(({ data }) => {
        this.setState({ referees: data.data })
      })
  }

  handleRefClick = (itemId) => {
    this.props.history.push(`/referees/${itemId}`)
  }

  renderListItem = (item) => {
    const { attributes, id } = item

    return (
      <li>
        <h2 onClick={() => this.handleRefClick(id)}>{attributes.email}</h2>
      </li>
    )
  }

  render() {
    const { referees } = this.state

    return (
      <div>
        <h1>Referee List View</h1>
        <ul>
          {referees.map(this.renderListItem)}
        </ul>
      </div>
    )
  }
}

export default Referees
