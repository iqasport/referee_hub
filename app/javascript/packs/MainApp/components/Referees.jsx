import React, { Component } from 'react'
import axios from 'axios';

class Referees extends Component {
  state = {
    referees: []
  }

  componentDidMount () {
    axios.get('/api/v1/referees')
      .then(({ data }) => {
        this.setState({ referees: data.data })
      })
  }

  renderListItem = (item) => {
    return (
      <li>
        <h2>{item.attributes.email}</h2>
      </li>
    )
  }

  render () {
    return (
      <div>
        <h1>Referee List View</h1>
        <ul>
          {this.state.referees.map(this.renderListItem)}
        </ul>
      </div>
    )
  }
}

export default Referees
