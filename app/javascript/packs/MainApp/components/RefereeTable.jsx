import React, { Component } from 'react'
import PropTypes from 'prop-types'
import {
  Table,
  Checkbox,
  Input,
  Dropdown,
  Label,
  Icon
} from 'semantic-ui-react'

const certificationArray = ['snitch', 'assistant', 'head', 'field']
const refereePropTypes = {
  id: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  certifications: PropTypes.arrayOf(PropTypes.string).isRequired,
  nationalGoverningBodies: PropTypes.arrayOf(PropTypes.string).isRequired,
  isCurrentReferee: PropTypes.bool
}

const TableRow = (props) => {
  const { referee, onRefereeClick } = props
  const {
    id,
    name,
    certifications,
    nationalGoverningBodies,
    isCurrentReferee
  } = referee
  const checkmarkIcon = <Icon name="checkmark" color="green" size="large" />

  const renderCertificationCell = certType => (
    <Table.Cell textAlign="center" key={certType}>
      {certifications.includes(certType) && checkmarkIcon}
    </Table.Cell>
  )

  const nameIcon = isCurrentReferee ? <Label ribbon color="blue">You</Label> : <Icon name="zoom" />

  return (
    <Table.Row onClick={() => onRefereeClick(id)} style={{ cursor: 'pointer' }}>
      <Table.Cell>
        {nameIcon}
        {' '}
        {name}
      </Table.Cell>
      <Table.Cell>{nationalGoverningBodies.join(', ')}</Table.Cell>
      {certificationArray.map(renderCertificationCell)}
    </Table.Row>
  )
}

TableRow.propTypes = {
  referee: PropTypes.shape(refereePropTypes).isRequired,
  onRefereeClick: PropTypes.func.isRequired
}

class RefereeTable extends Component {
  static propTypes = {
    certificationSearch: PropTypes.arrayOf(PropTypes.string).isRequired,
    nameSearch: PropTypes.string.isRequired,
    nationalGoverningBodySearch: PropTypes.arrayOf(PropTypes.string).isRequired,
    onNGBChange: PropTypes.func.isRequired,
    onNameChange: PropTypes.func.isRequired,
    onCertificationChange: PropTypes.func.isRequired,
    referees: PropTypes.arrayOf(PropTypes.shape(refereePropTypes)).isRequired,
    onRefereeClick: PropTypes.func.isRequired,
    dropdownOptions: PropTypes.arrayOf(PropTypes.shape({
      value: PropTypes.string,
      text: PropTypes.string
    })).isRequired
  }

  renderCertificationFilter = (certType) => {
    const { certificationSearch, onCertificationChange } = this.props

    return (
      <Table.HeaderCell key={certType}>
        <Checkbox
          toggle
          checked={certificationSearch.includes(certType)}
          value={certType}
          onChange={onCertificationChange(certType)}
        />
      </Table.HeaderCell>
    )
  }

  renderColumnHeader = () => (
    <Table.Row>
      <Table.HeaderCell>Name</Table.HeaderCell>
      <Table.HeaderCell>NGBs</Table.HeaderCell>
      <Table.HeaderCell>Snitch</Table.HeaderCell>
      <Table.HeaderCell>Assistant</Table.HeaderCell>
      <Table.HeaderCell>Head</Table.HeaderCell>
      <Table.HeaderCell>Field</Table.HeaderCell>
    </Table.Row>
  )

  renderSearchHeader = () => {
    const {
      nameSearch,
      onNameChange,
      nationalGoverningBodySearch,
      onNGBChange,
      dropdownOptions
    } = this.props

    return (
      <Table.Row>
        <Table.HeaderCell>
          <Input
            value={nameSearch}
            placeholder="Search by name …"
            onChange={onNameChange}
          />
        </Table.HeaderCell>
        <Table.HeaderCell>
          <Dropdown
            multiple
            search
            selection
            value={nationalGoverningBodySearch}
            placeholder="Search by NGB …"
            options={dropdownOptions}
            onChange={onNGBChange}
          />
        </Table.HeaderCell>
        {certificationArray.map(this.renderCertificationFilter)}
      </Table.Row>
    )
  }

  renderRefereeTableRow = (referee) => {
    const { onRefereeClick } = this.props

    return <TableRow key={referee.id} referee={referee} onRefereeClick={onRefereeClick} />
  }

  renderEmptyTable = () => (
    <Table.Row>
      <Table.Cell textAlign="center">
        No referees found.
      </Table.Cell>
    </Table.Row>
  )

  render() {
    const { referees } = this.props

    return (
      <Table selectable>
        <Table.Header>
          {this.renderColumnHeader()}
          {this.renderSearchHeader()}
        </Table.Header>
        <Table.Body>
          {referees.length > 0 ? referees.map(this.renderRefereeTableRow) : this.renderEmptyTable()}
        </Table.Body>
      </Table>
    )
  }
}

export default RefereeTable
