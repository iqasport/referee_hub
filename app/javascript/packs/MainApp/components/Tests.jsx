import React, { Component } from 'react'
import { Segment, Header } from 'semantic-ui-react'
import axios from 'axios'

class Tests extends Component {
  state = {
    tests: []
  }

  componentDidMount() {
    axios.get('/api/v1/tests')
      .then(({ data }) => {
        const { data: testData } = data

        this.setState({ tests: testData })
      })
  }

  render() {
    const { tests } = this.state

    return (
      <Segment>
        <Header as="h1" textAlign="center">Test Administration</Header>
        <div>
          {tests.map(test => console.log(test))}
        </div>
      </Segment>
    )
  }
}

export default Tests

