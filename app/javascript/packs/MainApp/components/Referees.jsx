import React, { Component } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import { Header, Pagination, Segment } from 'semantic-ui-react'
import RefereeTable from './RefereeTable'

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
    certificationSearch: [],
    total: null,
    page: null
  }

  componentDidMount() {
    axios
      .get('api/v1/national_governing_bodies')
      .then(this.setAllNGBs)
      .then(this.handleSearch)
  }

  componentDidUpdate(_prevProps, prevState) {
    const { nameSearch: oldName, nationalGoverningBodySearch: oldNGB, certificationSearch: oldCert } = this.state
    const { nameSearch: newName, nationalGoverningBodySearch: newNGB, certificationSearch: newCert } = prevState

    const nameSearchChanged = oldName !== newName
    const ngbFilterChanged = oldNGB.length !== newNGB.length
    const certFilterChanged = oldCert !== newCert

    if (nameSearchChanged || ngbFilterChanged || certFilterChanged) this.handleSearch()
  }

  setStateFromBackendData = ({ data: { data, included, meta } }) => {
    const { nationalGoverningBodies } = this.state
    const certifications = new Map()

    included.forEach((include) => {
      if (include.type === 'referee_certification') {
        certifications.set(include.id, include.attributes.level)
      }
    })

    const referees = data
      .filter(this.hasRefereeName)
      .map(({ id, attributes, relationships }) => ({
        id,
        name: `${attributes.first_name} ${attributes.last_name}`.trim(),
        certifications: relationships.referee_certifications.data.map(
          certification => certifications.get(certification.id)
        ),
        nationalGoverningBodies: relationships.national_governing_bodies.data.map(
          nationalGoverningBody => nationalGoverningBodies.find(ngb => ngb.id === nationalGoverningBody.id)
        ),
        isCurrentReferee: attributes.is_editable
      }));

    this.setState({
      referees,
      nationalGoverningBodies,
      total: meta.total,
      page: meta.page
    })
  }

  setAllNGBs = ({ data: { data } }) => {
    this.setState({
      nationalGoverningBodies: data.map(nationalGoverningBody => ({
        id: nationalGoverningBody.id,
        name: nationalGoverningBody.attributes.name
      }))
    })
  }

  get dropdownOptions() {
    const { nationalGoverningBodies } = this.state
    return Array.from(nationalGoverningBodies).map(ngb => ({
      value: ngb.id,
      text: ngb.name
    }))
  }

  get totalPages() {
    const { total } = this.state

    return Math.round(total / 25) + 1
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
    const params = this.searchParams()

    axios
      .get('/api/v1/referees', { params })
      .then(this.setStateFromBackendData)
  }

  handlePageChange = (e, { activePage }) => {
    const searchParams = this.searchParams()

    const params = {
      page: activePage,
      ...searchParams
    }

    axios
      .get('/api/v1/referees', { params })
      .then(this.setStateFromBackendData)
  }

  searchParams = () => {
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

    return params
  }

  render() {
    const {
      referees,
      nameSearch,
      nationalGoverningBodySearch,
      certificationSearch,
      page
    } = this.state

    return (
      <Segment>
        <Header as="h1">
          Registered Referees
          <Pagination
            activePage={page}
            totalPages={this.totalPages}
            firstItem={null}
            lastItem={null}
            floated="right"
            color="blue"
            pointing
            secondary
            onPageChange={this.handlePageChange}
          />
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
      </Segment>
    )
  }
}

export default Referees
