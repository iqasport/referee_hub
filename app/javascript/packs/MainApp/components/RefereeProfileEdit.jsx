import React, { Component } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import {
  Button, Form, Header, Icon, Message, Modal
} from 'semantic-ui-react'
import { isEmpty } from 'lodash'

class RefereeProfileEdit extends Component {
  static propTypes = {
    values: PropTypes.shape({
      firstName: PropTypes.string,
      lastName: PropTypes.string,
      bio: PropTypes.string,
      showPronouns: PropTypes.bool,
      nationalGoverningBodies: PropTypes.arrayOf(PropTypes.shape({
        id: PropTypes.string,
        name: PropTypes.string
      }))
    }).isRequired,
    onChange: PropTypes.func.isRequired,
    onSubmit: PropTypes.func.isRequired
  };

  state = {
    open: false,
    validationErrors: {},
    availableNationalGoverningBodies: []
  };

  componentDidMount() {
    axios
      .get('/api/v1/national_governing_bodies')
      .then(this.fetchAvailableNationalGoverningBodies)
  }

  get dropdownOptions() {
    const { availableNationalGoverningBodies } = this.state

    return availableNationalGoverningBodies.map(ngb => ({ value: ngb.id, text: ngb.name }))
  }

  fetchAvailableNationalGoverningBodies = ({ data: { data } }) => {
    this.setState({
      availableNationalGoverningBodies: data.map(nationalGoverningBody => ({
        id: nationalGoverningBody.id,
        name: nationalGoverningBody.attributes.name,
        website: nationalGoverningBody.attributes.website
      }))
    })
  }

  openForm = () => {
    this.setState({
      open: true
    })
  };

  changeInput = (event, { value }) => {
    const { onChange } = this.props
    onChange(event.target.id, value)
  };

  changeCheckbox = (event, { checked }) => {
    const { onChange } = this.props
    onChange(event.target.id, checked)
  }

  handleNGBChange = (_event, { value }) => {
    const { onChange } = this.props

    this.setState({
      validationErrors: {
        noNationalGoverningBody: !value.length
      }
    })
    onChange('changedNGBs', value)
  }

  handleSubmit = () => {
    const { onSubmit, values: { nationalGoverningBodies } } = this.props

    if (!nationalGoverningBodies.length) {
      this.setState({ validationErrors: { noNationalGoverningBody: true } })
      return
    }

    onSubmit()

    this.setState({ validationErrors: {}, open: false })
  }

  handleCancel = () => this.setState({ validationErrors: {}, open: false })

  render() {
    const {
      values: {
        firstName,
        lastName,
        bio,
        showPronouns,
        pronouns,
        nationalGoverningBodies
      }
    } = this.props
    const { open, validationErrors } = this.state

    const modalTrigger = (
      <Button onClick={this.openForm}>
        <Icon name="edit" />
        Edit
      </Button>
    )
    const hasNGBS = !isEmpty(nationalGoverningBodies)
    const initialNGBValues = hasNGBS ? nationalGoverningBodies.map(ngb => ngb.id) : null

    return (
      <Modal open={open} trigger={modalTrigger}>
        <Modal.Header>
          <Header>
            <Icon name="edit outline" />
            Edit Referee Profile
          </Header>
        </Modal.Header>
        <Modal.Content>
          <Form error={validationErrors.noNationalGoverningBody} onSubmit={this.handleSubmit}>
            <Form.Group widths="equal">
              <Form.Input
                fluid
                id="changedFirstName"
                label="First names"
                value={firstName}
                placeholder="First names"
                onChange={this.changeInput}
              />
              <Form.Input
                fluid
                id="changedLastName"
                label="Last name"
                value={lastName}
                placeholder="Last name"
                onChange={this.changeInput}
              />
            </Form.Group>
            <Form.Group inline>
              <Form.Input
                id="changedPronouns"
                label="Pronouns"
                value={pronouns}
                placeholder="Your pronouns"
                onChange={this.changeInput}
              />
              <Form.Checkbox
                toggle
                id="changedShowPronouns"
                label="Show my pronouns on my referee profile"
                checked={showPronouns}
                onChange={this.changeCheckbox}
              />
            </Form.Group>
            <Form.Group>
              <Form.Dropdown
                id="changedNGBs"
                style={{ width: '100%' }}
                multiple
                selection
                fluid
                label="National Governing Bodies"
                defaultValue={initialNGBValues}
                placeholder="Select National Governing Bodies ..."
                options={this.dropdownOptions}
                onChange={this.handleNGBChange}
                error={validationErrors.noNationalGoverningBody}
              />
            </Form.Group>
            <Message
              error
              header="No National Governing Body Selected"
              content="You must belong to at least one national governing body"
            />
            <Form.TextArea
              id="changedBio"
              label="Bio"
              value={bio}
              autoHeight
              placeholder="Tell us more about you!"
              onChange={this.changeInput}
            />
          </Form>
        </Modal.Content>
        <Modal.Actions>
          <Button secondary onClick={this.handleCancel}>
            Cancel
          </Button>
          <Button primary onClick={this.handleSubmit}>
            Save Profile
          </Button>
        </Modal.Actions>
      </Modal>
    )
  }
}

export default RefereeProfileEdit
