import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import axios from 'axios'
import { capitalize, groupBy } from 'lodash'
import { Segment, Label, Message } from 'semantic-ui-react'

import ContentSegment from './ContentSegment'
import {
  hasHeadCert,
  canTakeSnitchTest,
  canTakeAssistantTest,
  canTakeHeadTest,
} from '../utils'

const testColor = (testLevel) => {
  switch (testLevel) {
    case 'snitch':
      return 'yellow'
    case 'assistant':
      return 'blue'
    case 'head':
      return 'green'
    default:
      return 'blue'
  }
}

const allLinksDisabled = linkArray => linkArray.filter(link => link.enabled).length === 0

class CertificationLinks extends Component {
  static propTypes = {
    hasPaid: PropTypes.bool.isRequired,
    refereeId: PropTypes.string.isRequired,
    refCertifications: PropTypes.arrayOf(
      PropTypes.shape({
        id: PropTypes.string,
        level: PropTypes.string
      })
    ).isRequired,
    levelsThatNeedRenewal: PropTypes.arrayOf(
      PropTypes.shape({
        id: PropTypes.string,
        level: PropTypes.string,
      })
    ).isRequired,
    testAttempts: PropTypes.arrayOf(
      PropTypes.shape({
        next_attempt_at: PropTypes.string,
        test_level: PropTypes.string
      })
    ).isRequired,
    onRouteChange: PropTypes.func.isRequired,
  }

  state = {
    testLinks: []
  }

  componentDidMount() {
    axios.get('/api/v1/tests', { params: { active_only: true } })
      .then(({ data }) => {
        const { data: testData } = data

        if (testData) {
          const testLinks = testData.map(this.buildTestLink)
          this.setState({ testLinks })
        }
      })
  }

  get canTakeSnitchTest() {
    const { levelsThatNeedRenewal, testAttempts, refCertifications } = this.props
    return canTakeSnitchTest(levelsThatNeedRenewal, testAttempts, refCertifications)
  }

  get canTakeAssistantTest() {
    const { levelsThatNeedRenewal, testAttempts, refCertifications } = this.props
    return canTakeAssistantTest(levelsThatNeedRenewal, testAttempts, refCertifications)
  }

  get canTakeHeadTest() {
    const {
      levelsThatNeedRenewal, testAttempts, refCertifications, hasPaid
    } = this.props
    return canTakeHeadTest(hasPaid, levelsThatNeedRenewal, testAttempts, refCertifications)
  }

  get certificationConfig() {
    const { testLinks } = this.state

    const groupedLinks = groupBy(testLinks, 'language')
    return groupedLinks
  }

  isTestEnabled = (testLevel) => {
    switch (testLevel) {
      case 'snitch':
        return this.canTakeSnitchTest
      case 'assistant':
        return this.canTakeAssistantTest
      case 'head':
        return this.canTakeHeadTest
      default:
        return true
    }
  }

  buildTestLink = ({ id, attributes }) => {
    const { refereeId } = this.props
    const color = testColor(attributes.level)
    const enabled = this.isTestEnabled(attributes.level)

    return {
      link: `/referees/${refereeId}/tests/${id}`,
      title: attributes.name,
      language: attributes.language,
      color,
      enabled
    }
  }

  handleTestClick = link => () => {
    const { onRouteChange } = this.props

    onRouteChange(link)
  }

  renderLink = (test) => {
    const {
      link, title, enabled, color
    } = test
    if (!enabled) return null

    const labelProps = {
      key: title,
      size: 'big',
      as: 'button',
      onClick: this.handleTestClick(link),
      content: title,
      color,
      style: {
        cursor: 'pointer'
      }
    }

    return <Label {...labelProps} />
  }

  renderLanguageSection = (section) => {
    if (allLinksDisabled(section[1])) return null
    // We're doing this here to keep the links appearing in a consistent order.
    // Otherwise the api can return these items inconsistently resulting in an unexpected ux.
    const snitchLink = section[1].find(test => test.color === 'yellow')
    const assLink = section[1].find(test => test.color === 'blue')
    const headLink = section[1].find(test => test.color === 'green')

    return (
      <Segment padded key={section[0]}>
        <Label attached="top">{capitalize(section[0])}</Label>
        {snitchLink && this.renderLink(snitchLink)}
        {assLink && this.renderLink(assLink)}
        {headLink && this.renderLink(headLink)}
      </Segment>
    )
  }

  renderTests = () => (
    <Fragment>
      {Object.entries(this.certificationConfig).map(this.renderLanguageSection)}
      <Message info>
        Please note that you have to wait 24 hours after a failed test to be allowed to retry (72 hours for the head
        referee test), even if the link is still visible. Every passed and failed test attempt will be recorded,
        even if the testing tool fails to properly report the attempt back to us on time. In rare cases this may
        take up to 72 hours. Our apologies if this happens.
      </Message>
    </Fragment>
  )

  renderCoolDownNotice = () => (
    <p>
      One or more tests are unavailable at the moment, please check back after the 24 hour
      (for Snitch and Assistant) or 72 hour (for Head) cool down period.
    </p>
  )

  render() {
    const { refCertifications, levelsThatNeedRenewal } = this.props
    if (hasHeadCert(refCertifications, levelsThatNeedRenewal)) return null

    const canTakeSnitch = this.canTakeSnitchTest
    const canTakeAssistant = this.canTakeAssistantTest
    const canTakeHead = this.canTakeHeadTest

    const anyTestsAvailable = [canTakeSnitch, canTakeAssistant, canTakeHead].some(testStatus => testStatus)

    const headerContent = 'Available Written Tests'
    const segmentContent = anyTestsAvailable ? this.renderTests() : this.renderCoolDownNotice()

    return <ContentSegment segmentContent={segmentContent} headerContent={headerContent} />
  }
}

export default CertificationLinks
