import React, { Component } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import { Input, Checkbox } from 'semantic-ui-react'

class Referees extends Component {
  static propTypes = {
    history: PropTypes.shape({
      push: PropTypes.func
    }).isRequired
  }

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
    const { history } = this.props
    history.push(`/referees/${itemId}`)
  }

  renderListItem = (item) => {
    const { attributes, id } = item

    return (
      <li>
        <h2>
          {attributes.email}
          <button type="button" onClick={() => this.handleRefClick(id)}>View Ref</button>
        </h2>
      </li>
    )
  }

  handleSearchChange = (e, { value }) => {
    axios.get('/api/v1/referees', { params: { q: value } })
      .then(({ data }) => {
        this.setState({ referees: data.data })
      })
  }

  handleCheckboxChange = (e, { checked, value }) => {
    axios.get('/api/v1/referees', { params: { filter_by: { certifications: [value] } } })
      .then(({ data }) => {
        this.setState({ referees: data.data })
      })
  }

  render() {
    const { referees } = this.state

    return (
      <div>
        <h1>Referee List View</h1>
        <Input placeholder="Search..." onChange={this.handleSearchChange} />
        <Checkbox label="Snitch Certification" value="snitch" onChange={this.handleCheckboxChange} />
        <ul>
          {referees.map(this.renderListItem)}
        </ul>
      </div>
    )
  }
}

export default Referees
