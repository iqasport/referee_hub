import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import {
  Form, Header, Label, Message, Segment
} from 'semantic-ui-react'

class RefereeProfileEdit extends Component {
  static propTypes = {
    defaultValues: PropTypes.shape({
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
    onSubmit: PropTypes.func.isRequired
  };

  constructor(props) {
    super(props);

    const {
      defaultValues: {
        firstName,
        lastName,
        bio,
        email,
        showPronouns,
        pronouns,
        nationalGoverningBodies
      }
    } = props;

    this.state = {
      firstName,
      lastName,
      bio,
      email,
      showPronouns,
      pronouns,
      nationalGoverningBodies,
      validationErrors: {},
      availableNationalGoverningBodies: []
    }
  }

  componentDidMount() {
    axios
      .get('/api/v1/national_governing_bodies')
      .then(({ data: { data } }) => {
        this.setState({
          availableNationalGoverningBodies: data.map(nationalGoverningBody => ({
            id: nationalGoverningBody.id,
            name: nationalGoverningBody.attributes.name,
            website: nationalGoverningBody.attributes.website
          }))
        })
      });
  }

  getNationalGoverningBodyJsx(nationalGoverningBody) {
    const { nationalGoverningBodies, validationErrors } = this.state;

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
  }

  changeInput = (event) => {
    this.setState({
      [event.target.id]: event.target.value
    })
  };

  changeCheckbox = (event) => {
    this.setState({
      [event.target.id]: event.target.checked
    })
  };

  changeNationalGoverningBodyCheckbox = (id, event) => {
    const { checked } = event.target;

    this.setState((oldState) => {
      const {
        nationalGoverningBodies,
        availableNationalGoverningBodies
      } = oldState;

      let newList = nationalGoverningBodies;
      if (checked) {
        nationalGoverningBodies.push(
          availableNationalGoverningBodies.filter(
            nationalGoverningBody => nationalGoverningBody.id === id
          )[0]
        )
      } else {
        newList = nationalGoverningBodies.filter(
          nationalGoverningBody => nationalGoverningBody.id !== id
        )
      }

      return {
        nationalGoverningBodies: newList,
        validationErrors: {
          noNationalGoverningBody: !newList.length
        }
      }
    })
  };

  submit = () => {
    const {
      firstName,
      lastName,
      bio,
      showPronouns,
      pronouns,
      nationalGoverningBodies
    } = this.state;
    const { onSubmit } = this.props;

    if (!nationalGoverningBodies.length) {
      this.setState({
        validationErrors: {
          noNationalGoverningBody: true
        }
      });

      return
    }

    onSubmit({
      firstName,
      lastName,
      bio,
      showPronouns,
      pronouns,
      nationalGoverningBodies
    });

    this.setState({
      validationErrors: {}
    })
  };

  render() {
    const {
      firstName,
      lastName,
      bio,
      email,
      showPronouns,
      pronouns,
      validationErrors,
      availableNationalGoverningBodies
    } = this.state;

    return (
      <Fragment>
        <Header as="h1">
          {`${firstName} ${lastName}  (Wow, that’s you!)`}
        </Header>
        <Form error={validationErrors.noNationalGoverningBody} onSubmit={this.submit}>
          <Segment attached="top">
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
                this.getNationalGoverningBodyJsx.bind(this)
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
      </Fragment>
    )
  }
}

export default RefereeProfileEdit
