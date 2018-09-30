import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import {
  Checkbox, Dropdown, Header, Icon, Input, Label, Table
} from 'semantic-ui-react'

class Referees extends Component {
  static propTypes = {
    history: PropTypes.shape({
      push: PropTypes.func
    }).isRequired
  };

  state = {
    referees: [],
    nationalGoverningBodies: new Map()
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
        ),
        isCurrentReferee: attributes.is_editable
      }));

    this.setState({
      referees,
      nationalGoverningBodies
    })
  };

  get dropdownOptions() {
    const { nationalGoverningBodies } = this.state;
    return Array.from(nationalGoverningBodies).map(([id, name]) => ({
      value: id,
      text: name
    }))
  }

  hasRefereeName = ({ attributes }) => attributes.first_name || attributes.last_name;

  handleRefClick = (itemId) => {
    const { history } = this.props;
    history.push(`/referees/${itemId}`)
  };

  renderRefereeTableRow = ({
    id, name, certifications, nationalGoverningBodies, isCurrentReferee
  }) => (
    <Table.Row onClick={() => this.handleRefClick(id)} key={id} style={{ cursor: 'pointer' }}>
      <Table.Cell>
        {
          (isCurrentReferee
          && <Label ribbon>You</Label>)
          || <Icon name="zoom" />
        }
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

  handleNameSearchChange = (e, { value }) => {
    axios.get('/api/v1/referees', { params: { q: value } })
      .then(this.setStateFromBackendData)
  };

  handleCertificationToggleChange = (e, { value }) => {
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
                Head
              </Table.HeaderCell>
              <Table.HeaderCell>
                Field
              </Table.HeaderCell>
            </Table.Row>
            <Table.Row>
              <Table.HeaderCell>
                <Input placeholder="Search …" onChange={this.handleNameSearchChange} />
              </Table.HeaderCell>
              <Table.HeaderCell>
                <Dropdown
                  search
                  selection
                  placeholder="Search …"
                  options={this.dropdownOptions}
                  onChange={this.handleNameSearchChange}
                />
              </Table.HeaderCell>
              <Table.HeaderCell textAlign="center">
                <Checkbox
                  toggle
                  defaultChecked
                  value="snitch"
                  onChange={this.handleCertificationToggleChange}
                />
              </Table.HeaderCell>
              <Table.HeaderCell textAlign="center">
                <Checkbox
                  toggle
                  defaultChecked
                  value="assistant"
                  onChange={this.handleCertificationToggleChange}
                />
              </Table.HeaderCell>
              <Table.HeaderCell textAlign="center">
                <Checkbox
                  toggle
                  defaultChecked
                  value="head"
                  onChange={this.handleCertificationToggleChange}
                />
              </Table.HeaderCell>
              <Table.HeaderCell textAlign="center">
                <Checkbox
                  toggle
                  defaultChecked
                  value="field"
                  onChange={this.handleCertificationToggleChange}
                />
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
