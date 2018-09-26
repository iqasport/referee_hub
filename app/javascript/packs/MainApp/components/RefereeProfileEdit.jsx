import React, { Component } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import {
  Button, Form, Header, Icon, Label, Message, Modal, Segment
} from 'semantic-ui-react'

class RefereeProfileEdit extends Component {
  static propTypes = {
    values: PropTypes.shape({
      firstName: PropTypes.string,
      lastName: PropTypes.string,
      bio: PropTypes.string,
      email: PropTypes.string,
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
      .then(this.fetchAvailableNationalGoverningBodies);
  }

  fetchAvailableNationalGoverningBodies = ({ data: { data } }) => {
    this.setState({
      availableNationalGoverningBodies: data.map(nationalGoverningBody => ({
        id: nationalGoverningBody.id,
        name: nationalGoverningBody.attributes.name,
        website: nationalGoverningBody.attributes.website
      }))
    })
  };

  getNationalGoverningBodyJsx = (nationalGoverningBody) => {
    const { values: { nationalGoverningBodies } } = this.props;
    const { validationErrors } = this.state;

    return (
      <Form.Checkbox
        id={`nationalGoverningBody[${nationalGoverningBody.id}]`}
        label={nationalGoverningBody.name}
        key={nationalGoverningBody.id}
        checked={nationalGoverningBodies.some(ngb => ngb.id === nationalGoverningBody.id)}
        onChange={this.changeNationalGoverningBodyCheckbox.bind(this, nationalGoverningBody.id)}
        error={validationErrors.noNationalGoverningBody}
      />
    )
  };

  openForm = () => {
    this.setState({
      open: true
    })
  };

  changeInput = (event) => {
    const { onChange } = this.props;

    onChange(event.target.id, event.target.value)
  };

  changeCheckbox = (event) => {
    const { onChange } = this.props;

    onChange(event.target.id, event.target.checked)
  };

  changeNationalGoverningBodyCheckbox = (id, event) => {
    const { checked } = event.target;
    const {
      availableNationalGoverningBodies
    } = this.state;
    const {
      values: { nationalGoverningBodies },
      onChange
    } = this.props;

    let newList;
    if (checked) {
      newList = nationalGoverningBodies.concat(
        availableNationalGoverningBodies.filter(
          nationalGoverningBody => nationalGoverningBody.id === id
        )
      )
    } else {
      newList = nationalGoverningBodies.filter(
        nationalGoverningBody => nationalGoverningBody.id !== id
      )
    }

    this.setState({
      validationErrors: {
        noNationalGoverningBody: !newList.length
      }
    });

    onChange('nationalGoverningBodies', newList)
  };

  submit = () => {
    const {
      values: { nationalGoverningBodies }
    } = this.props;
    const { onSubmit } = this.props;

    if (!nationalGoverningBodies.length) {
      this.setState({
        validationErrors: {
          noNationalGoverningBody: true
        }
      });

      return
    }

    onSubmit();

    this.setState({
      validationErrors: {},
      open: false
    })
  };

  render() {
    const {
      values: {
        firstName,
        lastName,
        bio,
        email,
        showPronouns,
        pronouns
      }
    } = this.props;
    const {
      open,
      validationErrors,
      availableNationalGoverningBodies
    } = this.state;

    return (
      <Modal
        open={open}
        trigger={(
          <Button onClick={this.openForm}>
            <Icon name="edit" />
            Edit
          </Button>
        )}
      >
        <Form error={validationErrors.noNationalGoverningBody} onSubmit={this.submit}>
          <Segment attached="top">
            <Header>
              <Icon name="edit outline" />
              Edit Referee Profile
            </Header>
          </Segment>
          <Segment attached>
            <Form.Group widths="equal">
              <Form.Input
                fluid
                id="firstName"
                label="First names"
                value={firstName}
                placeholder="First names"
                onChange={this.changeInput}
              />
              <Form.Input
                fluid
                id="lastName"
                label="Last name"
                value={lastName}
                placeholder="Last name"
                onChange={this.changeInput}
              />
            </Form.Group>
          </Segment>
          <Segment attached>
            <Form.Input
              id="pronouns"
              label="Pronouns"
              value={pronouns}
              placeholder="Your pronouns"
              onChange={this.changeInput}
            />
            <Form.Checkbox
              toggle
              id="showPronouns"
              label="Show my pronouns on my referee profile"
              checked={showPronouns}
              onChange={this.changeCheckbox}
            />
          </Segment>
          <Segment attached>
            <Form.Group inline>
              <Form.Input
                id="email"
                label="Email"
                value={email}
                placeholder="Email address"
                disabled
              />
              <Label pointing="left">
                Please contact us if you need to change your email address.
              </Label>
            </Form.Group>
          </Segment>
          <Segment attached>
            <Form.Group inline>
              <label>National Governing Bodies</label>
              {availableNationalGoverningBodies.map(
                this.getNationalGoverningBodyJsx
              )}
            </Form.Group>
            <Message
              error
              header="No National Governing Body Selected"
              content="You must belong to at least one national governing body"
            />
          </Segment>
          <Segment attached>
            <Form.TextArea
              id="bio"
              label="Bio"
              value={bio}
              autoHeight
              placeholder="Tell us more about you!"
              onChange={this.changeInput}
            />
          </Segment>
          <Form.Button primary attached="bottom" onClick={this.submit}>
            Save
          </Form.Button>
        </Form>
      </Modal>
    )
  }
}

export default RefereeProfileEdit
