import React, { Fragment } from 'react'
import PropTypes from 'prop-types'
import { Segment, Divider, Header } from 'semantic-ui-react'

const ContentSegment = ({ headerContent, segmentContent }) => (
  <Fragment>
    <Header as="h3" content={headerContent} />
    <Divider fitted />
    <Segment basic>
      {segmentContent}
    </Segment>
  </Fragment>
)

ContentSegment.propTypes = {
  headerContent: PropTypes.string.isRequired,
  segmentContent: PropTypes.oneOfType([PropTypes.string, PropTypes.node]).isRequired
}

export default ContentSegment
