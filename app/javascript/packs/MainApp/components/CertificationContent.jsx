import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import { Label } from 'semantic-ui-react'
import { capitalize } from 'lodash'
import PaypalButton from './PaypalButton'
import ContentSegment from './ContentSegment'

const certificationLinkConfig = {
  snitch: {
    title: 'Snitch Referee Written Test',
    link: 'https://www.classmarker.com/online-test/start/?quiz=4q95bafa6c1b2a6a',
    color: 'yellow'
  },
  assistant: {
    title: 'Assistant Referee Written Test',
    link: 'https://www.classmarker.com/online-test/start/?quiz=gyv5babf1bd8146f',
    color: 'blue'
  },
  head: {
    title: 'Head Referee Written Test',
    link: 'https://www.classmarker.com/online-test/start/?quiz=tyg5baff2b2c128c',
    color: 'green'
  }
}

const hasPassedTest = (level, certifications) => {
  return certifications.some(({ level: certificationLevel }) => certificationLevel === level)
}

class CertificationContent extends Component {
  static propTypes = {
    refereeId: PropTypes.string.isRequired,
    isEditable: PropTypes.bool.isRequired,
    hasPaid: PropTypes.bool.isRequired,
    refCertifications: PropTypes.arrayOf(
      PropTypes.shape({
        id: PropTypes.string,
        level: PropTypes.string
      })
    ).isRequired,
    onSuccess: PropTypes.func.isRequired,
    onError: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired
  }

  get hasSnitchCert() {
    const { refCertifications } = this.props

    return hasPassedTest('snitch', refCertifications)
  }

  get hasAssistantCert() {
    const { refCertifications } = this.props

    return hasPassedTest('assistant', refCertifications)
  }

  get hasHeadCert() {
    const { refCertifications } = this.props

    return hasPassedTest('head', refCertifications)
  }

  certificationLink = ({ title, link, color }) => {
    const { refereeId } = this.props

    const fullLink = `${link}&cm_user_id=${refereeId}`
    return (
      <Label
        color={color}
        size="big"
        as="a"
        href={fullLink}
        target="_blank"
        rel="noopener noreferrer"
        content={title}
      />
    )
  }

  renderPaypalButton = () => {
    const {
      onSuccess,
      onError,
      onCancel,
      isEditable,
      hasPaid
    } = this.props
    if (!isEditable) return null
    if (!this.hasAssistantCert && !this.hasSnitchCert) return null
    if (hasPaid) return null

    const headerContent = 'Purchase Head Referee Written Test'
    const segmentContent = <PaypalButton onSuccess={onSuccess} onError={onError} onCancel={onCancel} />

    return <ContentSegment headerContent={headerContent} segmentContent={segmentContent} />
  }

  renderCertification = ({ level }) => {
    const labelContent = `${capitalize(level)} Referee`
    return <Label content={labelContent} size="big" key={level} color="green" />
  }

  renderCertificationLinks = () => {
    const { isEditable, hasPaid } = this.props
    if (!isEditable) return null

    const canTakeSnitchTest = !this.hasSnitchCert && !this.hasHeadCert
    const canTakeAssisstantTest = !this.hasAssistantCert && !this.hasHeadCert
    const canTakeHeadTest = hasPaid && this.hasSnitchCert && this.hasAssistantCert

    const headerContent = 'Available Written Tests'
    const segmentContent = (
      <Fragment>
        {canTakeSnitchTest && this.certificationLink(certificationLinkConfig.snitch)}
        {canTakeAssisstantTest && this.certificationLink(certificationLinkConfig.assistant)}
        {canTakeHeadTest && this.certificationLink(certificationLinkConfig.head)}
      </Fragment>
    )

    return <ContentSegment segmentContent={segmentContent} headerContent={headerContent} />
  }

  renderCompletedCertifications = () => {
    const { refCertifications } = this.props

    const headerContent = 'Completed Certifications'
    let segmentContent

    if (refCertifications.length > 0) {
      segmentContent = refCertifications.map(this.renderCertification)
    } else {
      segmentContent = 'This referee has not finished any certifications'
    }

    return <ContentSegment headerContent={headerContent} segmentContent={segmentContent} />
  }

  render() {
    return (
      <Fragment>
        {this.renderPaypalButton()}
        {this.renderCertificationLinks()}
        {this.renderCompletedCertifications()}
      </Fragment>
    )
  }
}

export default CertificationContent
