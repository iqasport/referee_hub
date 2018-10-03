import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import { Header } from 'semantic-ui-react'
import RefereeTable from './RefereeTable'

const getRefereeName = (firstName, lastName) => {
  if (!firstName && !lastName) {
    return 'Anonymous Referee'
  }

  if (firstName && !lastName) {
    return firstName
  }

  if (!firstName && lastName) {
    return lastName
  }

  return `${firstName} ${lastName}`
}

class Referees extends Component {
  static propTypes = {
    history: PropTypes.shape({
      push: PropTypes.func
    }).isRequired
  }

  state = {
    referees: [],
    nationalGoverningBodies: new Map(),
    nameSearch: '',
    nationalGoverningBodySearch: [],
    certificationSearch: []
  }

  componentDidMount() {
    this.handleSearch()
  }

  componentDidUpdate(prevProps, prevState) {
    const { nameSearch: oldName, nationalGoverningBodySearch: oldNGB, certificationSearch: oldCert } = this.state
    const { nameSearch: newName, nationalGoverningBodySearch: newNGB, certificationSearch: newCert } = prevState

    const nameSearchChanged = oldName !== newName
    const ngbFilterChanged = oldNGB.length !== newNGB.length
    const certFilterChanged = oldCert !== newCert

    if (nameSearchChanged || ngbFilterChanged || certFilterChanged) this.handleSearch()
  }

  setStateFromBackendData = ({ data: { data, included } }) => {
    const certifications = new Map()
    const nationalGoverningBodies = new Map()
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
        name: getRefereeName(attributes.first_name, attributes.last_name),
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
  }

  get dropdownOptions() {
    const { nationalGoverningBodies } = this.state
    return Array.from(nationalGoverningBodies).map(([id, name]) => ({
      value: id,
      text: name
    }))
  }

  hasRefereeName = ({ attributes }) => attributes.first_name || attributes.last_name

  handleRefereeClick = (refId) => {
    const { history } = this.props
    history.push(`/referees/${refId}`)
  }

  handleNameSearchChange = (e, { value }) => {
    this.setState({
      nameSearch: value
    })
  }

  handleNationalGoverningBodySearchChange = (e, { value }) => {
    this.setState({
      nationalGoverningBodySearch: value
    })
  }

  handleCertificationToggleChange = value => () => {
    this.setState(({ certificationSearch }) => {
      const checked = certificationSearch.includes(value)
      let newSearch
      if (checked) {
        newSearch = certificationSearch.filter(certification => certification !== value)
      } else {
        newSearch = certificationSearch.concat([value])
      }

      return {
        certificationSearch: newSearch
      }
    })
  }

  handleSearch = () => {
    const { nameSearch, nationalGoverningBodySearch, certificationSearch } = this.state
    const params = {}
    if (nameSearch.trim()) {
      params.q = nameSearch
    }
    if (nationalGoverningBodySearch.length) {
      params.national_governing_bodies = nationalGoverningBodySearch
    }
    if (certificationSearch.length) {
      params.certifications = certificationSearch
    }

    axios
      .get('/api/v1/referees', { params })
      .then(this.setStateFromBackendData)
  }

  render() {
    const {
      referees,
      nameSearch,
      nationalGoverningBodySearch,
      certificationSearch
    } = this.state

    return (
      <Fragment>
        <Header as="h1">
          Registered Referees
        </Header>
        <RefereeTable
          referees={referees}
          nameSearch={nameSearch}
          nationalGoverningBodySearch={nationalGoverningBodySearch}
          certificationSearch={certificationSearch}
          onNameChange={this.handleNameSearchChange}
          onNGBChange={this.handleNationalGoverningBodySearchChange}
          onCertificationChange={this.handleCertificationToggleChange}
          onRefereeClick={this.handleRefereeClick}
          dropdownOptions={this.dropdownOptions}
        />
      </Fragment>
    )
  }
}

export default Referees
