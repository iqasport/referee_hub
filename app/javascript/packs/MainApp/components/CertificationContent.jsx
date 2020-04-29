import React, { Component, Fragment } from 'react'
import PropTypes from 'prop-types'
import {
  Label,
  Modal,
  Button,
} from 'semantic-ui-react'
import { capitalize } from 'lodash'
import { DateTime } from 'luxon'
import axios from 'axios'

import PaypalButton from './PaypalButton'
import ContentSegment from './ContentSegment'
import TestResultsTable from './TestResultsTable'
import CertificationLinks from './CertificationLinks'
import { hasAssistantCert, hasSnitchCert } from '../utils/certUtils'

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
    onCancel: PropTypes.func.isRequired,
    onRouteChange: PropTypes.func.isRequired,
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
        nextAttemptAt: PropTypes.string,
        testLevel: PropTypes.string
      })
    ).isRequired
  }

  state = {
    levelsThatNeedRenewal: [],
    refCertificationDetails: [],
    renewConfirmOpen: false
  }

  componentDidMount() {
    const { isEditable } = this.props

    if (isEditable) {
      axios.get('/api/v1/referee_certifications')
        .then(({ data }) => {
          const refCertificationDetails = []
          const renewalLevels = []
          if (data && data.data) {
            data.data.forEach((refCertification) => {
              const { needsRenewalAt, level } = refCertification.attributes
              const refCertDetailData = { level, id: refCertification.id }

              refCertificationDetails.push(refCertDetailData)
              if (needsRenewalAt) { renewalLevels.push(refCertDetailData) }
            })
          }

          this.setState({ levelsThatNeedRenewal: renewalLevels, refCertificationDetails })
        })
    }
  }

  handleRenewalConfirm = () => {
    const { refCertificationDetails } = this.state

    refCertificationDetails.forEach(({ id }) => {
      axios
        .patch(`/api/v1/referee_certifications/${id}`, {
          needs_renewal_at: DateTime.local().toString()
        })
        .then(({ data }) => {
          const updatedCertification = data.data
          const { level } = updatedCertification.attributes

          this.setState((prevState) => {
            const levelsThatNeedRenewal = prevState.levelsThatNeedRenewal.push({ level, id: updatedCertification.id })
            return levelsThatNeedRenewal
          })
        })
    })

    this.handleRenewalConfirmClose()
  }

  handleRenewalConfirmOpen = () => this.setState({ renewConfirmOpen: true })

  handleRenewalConfirmClose = () => this.setState({ renewConfirmOpen: false })

  renderPaypalButton = () => {
    const {
      onSuccess,
      onError,
      onCancel,
      isEditable,
      hasPaid,
      refCertifications,
    } = this.props
    const { levelsThatNeedRenewal } = this.state

    if (!isEditable) return null
    if (!hasAssistantCert(refCertifications, levelsThatNeedRenewal)) return null
    if (!hasSnitchCert(refCertifications, levelsThatNeedRenewal)) return null
    if (hasPaid) return null

    const headerContent = 'Purchase Head Referee Written Test'
    const segmentContent = <PaypalButton onSuccess={onSuccess} onError={onError} onCancel={onCancel} />

    return <ContentSegment headerContent={headerContent} segmentContent={segmentContent} />
  }

  renderCertification = ({ level }) => {
    const { levelsThatNeedRenewal } = this.state
    if (levelsThatNeedRenewal.find(refCert => level === refCert.level)) return null

    const labelContent = `${capitalize(level)} Referee`

    return <Label content={labelContent} size="big" key={level} color="green" />
  }

  renderCertificationLinks = () => {
    const { levelsThatNeedRenewal } = this.state
    const {
      refereeId, testAttempts, isEditable, hasPaid, refCertifications, onRouteChange
    } = this.props
    if (!isEditable) return null

    const certificationProps = {
      refereeId,
      testAttempts,
      hasPaid,
      refCertifications,
      levelsThatNeedRenewal,
      onRouteChange
    }

    return <CertificationLinks {...certificationProps} />
  }

  renderRenewalButton = () => (
    <div style={{ flex: '1', display: 'flex', justifyContent: 'flex-end' }}>
      <Button negative content="Renew Certifications" onClick={this.handleRenewalConfirmOpen} />
    </div>
  )

  renderCompletedCertifications = () => {
    const { levelsThatNeedRenewal } = this.state
    const { refCertifications, isEditable } = this.props

    const shouldShowRenewalButton = !(levelsThatNeedRenewal.length > 0)
    const headerContent = 'Completed Certifications'
    let segmentContent

    if (refCertifications.length > 0) {
      segmentContent = (
        <div style={{ display: 'flex' }}>
          <div style={{ flex: '1' }}>
            {refCertifications.map(this.renderCertification)}
          </div>
          {isEditable && shouldShowRenewalButton && this.renderRenewalButton()}
        </div>
      )
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

  renderRenewalModal = () => {
    const { renewConfirmOpen } = this.state
    return (
      <Modal open={renewConfirmOpen} size="small">
        <Modal.Header>Confirm Certification Renewals</Modal.Header>
        <Modal.Content>
          <p>Are you sure you would like to renew your certifications?</p>

          <p>Clicking renew will invalidate all of your current certifications. This cannot be undone.</p>
        </Modal.Content>
        <Modal.Actions>
          <Button color="blue" onClick={this.handleRenewalConfirmClose} content="Cancel" />
          <Button color="red" onClick={this.handleRenewalConfirm} content="Renew" />
        </Modal.Actions>
      </Modal>
    )
  }

  render() {
    return (
      <Fragment>
        {this.renderPaypalButton()}
        {this.renderCertificationLinks()}
        {this.renderTestResults()}
        {this.renderCompletedCertifications()}
        {this.renderRenewalModal()}
      </Fragment>
    )
  }
}

export default CertificationContent
