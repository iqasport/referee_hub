import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import {
  Checkbox, Header, Icon, Input, Table
} from 'semantic-ui-react'

class Referees extends Component {
  static propTypes = {
    history: PropTypes.shape({
      push: PropTypes.func
    }).isRequired
  };

  state = {
    referees: []
  };

  componentDidMount() {
    axios.get('/api/v1/referees')
      .then(this.setStateFromBackendData)
  }

  setStateFromBackendData = ({ data: { data, included } }) => {
    const certifications = new Map();
    const nationalGoverningBodies = new Map();
    included.forEach((include) => {
      if (include.type === 'certification') {
        certifications.set(include.id, include.attributes.level);
      } else if (include.type === 'national_governing_body') {
        nationalGoverningBodies.set(include.id, include.attributes.name);
      }
    });
    const referees = data
      .filter(this.hasRefereeName)
      .map(({ id, attributes, relationships }) => ({
        id,
        name: `${attributes.first_name} ${attributes.last_name}`.trim(),
        certifications: relationships.certifications.data.map(
          certification => certifications.get(certification.id)
        ),
        nationalGoverningBodies: relationships.national_governing_bodies.data.map(
          nationalGoverningBody => nationalGoverningBodies.get(nationalGoverningBody.id)
        )
      }));

    this.setState({ referees })
  };

  hasRefereeName = ({ attributes }) => attributes.first_name || attributes.last_name;

  handleRefClick = (itemId) => {
    const { history } = this.props;
    history.push(`/referees/${itemId}`)
  };

  renderRefereeTableRow = ({
    id, name, certifications, nationalGoverningBodies
  }) => (
    <Table.Row onClick={() => this.handleRefClick(id)} key={id} style={{ cursor: 'pointer' }}>
      <Table.Cell>
        <Icon name="zoom" />
        {' '}
        {name}
      </Table.Cell>
      <Table.Cell>
        {nationalGoverningBodies.join(', ')}
      </Table.Cell>
      <Table.Cell textAlign="center">
        {
          certifications.includes('snitch')
            && <Icon name="checkmark" color="green" size="large" />
        }
      </Table.Cell>
      <Table.Cell textAlign="center">
        {
          certifications.includes('assistant')
          && <Icon name="checkmark" color="green" size="large" />
        }
      </Table.Cell>
      <Table.Cell textAlign="center">
        {
          certifications.includes('head')
          && <Icon name="checkmark" color="green" size="large" />
        }
      </Table.Cell>
      <Table.Cell textAlign="center">
        {
          certifications.includes('field')
          && <Icon name="checkmark" color="green" size="large" />
        }
      </Table.Cell>
    </Table.Row>
  );

  handleSearchChange = (e, { value }) => {
    axios.get('/api/v1/referees', { params: { q: value } })
      .then(this.setStateFromBackendData)
  };

  handleCheckboxChange = (e, { value }) => {
    axios.get('/api/v1/referees', { params: { filter_by: { certifications: [value] } } })
      .then(this.setStateFromBackendData)
  };

  render() {
    const { referees } = this.state;

    return (
      <Fragment>
        <Header as="h1">
          Referee List View
        </Header>
        <Input placeholder="Search by name …" onChange={this.handleSearchChange} />
        <Checkbox label="Snitch Certification" value="snitch" onChange={this.handleCheckboxChange} />
        <Table selectable>
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell>
                Name
              </Table.HeaderCell>
              <Table.HeaderCell>
                NGBs
              </Table.HeaderCell>
              <Table.HeaderCell>
                Snitch
              </Table.HeaderCell>
              <Table.HeaderCell>
                Assistant
              </Table.HeaderCell>
              <Table.HeaderCell>
                Head Written
              </Table.HeaderCell>
              <Table.HeaderCell>
                Head Field
              </Table.HeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {referees.map(this.renderRefereeTableRow)}
          </Table.Body>
        </Table>
      </Fragment>
    )
  }
}

export default Referees
