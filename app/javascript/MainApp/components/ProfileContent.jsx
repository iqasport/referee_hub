import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import {
  Flag,
  Label
} from 'semantic-ui-react'
import ContentSegment from './ContentSegment'

const getFlagForNGB = (name) => {
  if (/(ireland|catalonia)/i.test(name)) {
    return null
  }

  let flagName = name.toLowerCase()
  if (/(republic of korea)/i.test(name)) {
    flagName = 'south korea'
  }

  return <Flag name={flagName} />
}

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
      isEditable: PropTypes.bool,
      gettingStartedDismissedAt: PropTypes.string,
    }).isRequired,
  }

  renderNGBItem = ({ id, name, website }) => (
    <Label color="blue" size="large" key={id} as="a" href={website} target="_blank" rel="noopener noreferrer">
      {getFlagForNGB(name)}
      {name}
    </Label>
  )

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
        {this.renderNationalGoverningBodies()}
        {this.renderBio()}
      </Fragment>
    )
  }
}

export default ProfileContent
