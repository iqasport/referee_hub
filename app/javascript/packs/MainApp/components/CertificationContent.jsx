import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import { Label } from 'semantic-ui-react'
import { capitalize } from 'lodash'
import { DateTime, Interval } from 'luxon'
import PaypalButton from './PaypalButton'
import ContentSegment from './ContentSegment'
import TestResultsTable from './TestResultsTable'

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

const oldCertificationLinkConfig = {
  snitch: {
    title: 'Snitch Referee Written Test',
    link: 'https://www.classmarker.com/online-test/start/?quiz=crx5bb21de04a997',
    color: 'yellow'
  },
  assistant: {
    title: 'Assistant Referee Written Test',
    link: 'https://www.classmarker.com/online-test/start/?quiz=tgr5bb21e1c149dc',
    color: 'blue'
  },
  head: {
    title: 'Head Referee Written Test',
    link: 'https://www.classmarker.com/online-test/start/?quiz=9xb5bb21e53ea15f',
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
    shouldTakeOldTests: PropTypes.bool.isRequired,
    refCertifications: PropTypes.arrayOf(
      PropTypes.shape({
        id: PropTypes.string,
        level: PropTypes.string
      })
    ).isRequired,
    onSuccess: PropTypes.func.isRequired,
    onError: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,
    testResults: PropTypes.arrayOf(PropTypes.shape({
      duration: PropTypes.string,
      minimumPassPercentage: PropTypes.number,
      passed: PropTypes.bool,
      percentage: PropTypes.number,
      pointsAvailable: PropTypes.number,
      pointsScored: PropTypes.number,
      timeFinished: PropTypes.string,
      timeStarted: PropTypes.string,
      testLevel: PropTypes.string
    })).isRequired,
    testAttempts: PropTypes.arrayOf(
      PropTypes.shape({
        next_attempt_at: PropTypes.string,
        test_level: PropTypes.string
      })
    ).isRequired
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

  isInCoolDownPeriod = (certType) => {
    const { testAttempts } = this.props

    const matchingTestAttempt = testAttempts.filter(testAttempt => testAttempt.test_level === certType)

    if (matchingTestAttempt.length > 0) {
      const nextAttemptAt = DateTime.local(matchingTestAttempt[0].next_attempt_at)
      const currentTime = DateTime.local()

      return nextAttemptAt < currentTime
    }

    return false
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
    if (!this.hasAssistantCert) return null
    if (!this.hasSnitchCert) return null
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
    const { isEditable, hasPaid, shouldTakeOldTests } = this.props
    if (!isEditable) return null

    const canTakeSnitchTest = !this.hasSnitchCert && !this.hasHeadCert && !this.isInCoolDownPeriod('snitch')
    const canTakeAssistantTest = !this.hasAssistantCert && !this.hasHeadCert && !this.isInCoolDownPeriod('assistant')
    const canTakeHeadTest = hasPaid && this.hasSnitchCert && this.hasAssistantCert && !this.isInCoolDownPeriod('head')
    const certificationConfig = shouldTakeOldTests ? oldCertificationLinkConfig : certificationLinkConfig

    const headerContent = 'Available Written Tests'
    const segmentContent = (
      <Fragment>
        {canTakeSnitchTest && this.certificationLink(certificationConfig.snitch)}
        {canTakeAssistantTest && this.certificationLink(certificationConfig.assistant)}
        {canTakeHeadTest && this.certificationLink(certificationConfig.head)}
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

  renderTestResults = () => {
    const { testResults, isEditable } = this.props
    if (!isEditable) return null
    if (testResults.length < 1) return null

    const headerContent = 'Test Result Details'
    const segmentContent = <TestResultsTable testResults={testResults} />

    return (
      <Fragment>
        <ContentSegment headerContent={headerContent} segmentContent={segmentContent} />
      </Fragment>
    )
  }

  render() {
    return (
      <Fragment>
        {this.renderPaypalButton()}
        {this.renderCertificationLinks()}
        {this.renderTestResults()}
        {this.renderCompletedCertifications()}
      </Fragment>
    )
  }
}

export default CertificationContent
