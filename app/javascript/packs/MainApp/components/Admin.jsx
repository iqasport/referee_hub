import React, { Component } from 'react'
import PropTypes from 'prop-types'
import {
  Segment,
  Grid,
  Divider,
  Header,
  Icon,
  Button
} from 'semantic-ui-react'

class Admin extends Component {
  static propTypes = {
    history: PropTypes.shape({
      push: PropTypes.func
    }).isRequired
  }

  handleClickDiagnostic = () => {
    const { history } = this.props
    history.push('/admin/referee-diagnostic')
  }

  handleViewTests = () => {
    const { history } = this.props
    history.push('/admin/tests')
  }

  render() {
    return (
      <Segment>
        <Header as="h1" textAlign="center">
          Administrative Tools
        </Header>
        <Segment>
          <Grid columns={2} stackable textAlign="center">
            <Divider vertical>Or</Divider>

            <Grid.Row verticalAlign="middle">
              <Grid.Column textAlign="center">
                <Header icon>
                  <Icon name="search" />
                  Find a Referee
                </Header>
                <Button fluid color="green" content="Referee Diagnostic" onClick={this.handleClickDiagnostic} />
              </Grid.Column>

              <Grid.Column textAlign="center">
                <Header icon>
                  <Icon name="edit" />
                  View and Edit Active Tests (Coming soon!)
                </Header>
                <Button fluid content="View Tests" color="blue" onClick={this.handleViewTests} />
              </Grid.Column>
            </Grid.Row>
          </Grid>
        </Segment>
      </Segment>
    )
  }
}

export default Admin
