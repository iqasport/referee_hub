import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import {
  Message,
  Flag,
  Label
} from 'semantic-ui-react'
import ContentSegment from './ContentSegment'

class ProfileContent extends Component {
  static propTypes = {
    referee: PropTypes.shape({
      firstName: PropTypes.string,
      lastName: PropTypes.string,
      bio: PropTypes.string,
      showPronouns: PropTypes.bool,
      pronouns: PropTypes.string,
      nationalGoverningBodies: PropTypes.arrayOf(
        PropTypes.shape({
          id: PropTypes.string,
          name: PropTypes.string,
          website: PropTypes.string
        })
      ),
      isEditable: PropTypes.bool
    }).isRequired,
    onDismiss: PropTypes.func.isRequired
  }

  renderNGBItem = ({ id, name, website }) => {
    return (
      <Label color="blue" size="large" key={id} as="a" href={website} target="_blank" rel="noopener noreferrer">
        <Flag name={name.toLowerCase()} />
        {name}
      </Label>
    )
  }

  renderGettingStartedMessage = () => {
    const { referee, onDismiss } = this.props
    const {
      isEditable,
      firstName,
      lastName,
      gettingStartedDismissedAt
    } = referee
    if (!isEditable) return null
    if (gettingStartedDismissedAt) return null

    const notFirstAndLastName = !firstName && !lastName
    const getStarted = " Let's start by adding your name and NGB affiliation, click on the edit button to get started"
    const headerText = 'Welcome to your Referee Profile'

    return (
      <Message info onDismiss={onDismiss}>
        <Message.Header>{headerText}</Message.Header>
        <p>
          Here you can pay for and take your referee tests, give a little bit of background in your bio, and select any National Governing Bodies that you are affiliated with officially, or regularly officiate games.
          {notFirstAndLastName && getStarted}
        </p>
      </Message>
    )
  }

  renderNationalGoverningBodies = () => {
    const { referee: { nationalGoverningBodies } } = this.props

    let segmentContent
    if (nationalGoverningBodies.length > 0) {
      segmentContent = nationalGoverningBodies.map(this.renderNGBItem)
    } else {
      segmentContent = 'This referee has not set an affiliation with a National Governing Body'
    }
    const headerContent = 'National Governing Body Affiliation'

    return <ContentSegment headerContent={headerContent} segmentContent={segmentContent} />
  }

  renderBio = () => {
    const { referee: { bio } } = this.props
    const headerContent = 'Referee Bio'
    const segmentContent = bio || 'This referee has not set a bio'

    return <ContentSegment headerContent={headerContent} segmentContent={segmentContent} />
  }

  render() {
    return (
      <Fragment>
        {this.renderGettingStartedMessage()}
        {this.renderNationalGoverningBodies()}
        {this.renderBio()}
      </Fragment>
    )
  }
}

export default ProfileContent
