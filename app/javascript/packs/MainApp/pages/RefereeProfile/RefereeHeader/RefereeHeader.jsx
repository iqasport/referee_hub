import React from 'react'
import PropTypes from 'prop-types'
import capitalize from 'lodash'

const RefereeHeader = (props) => {
  const { referee } = props
  const refName = () => {
    if (referee.firstName || referee.lastName) {
      return `${referee.firstName} ${referee.lastName}`
    }

    return 'Anonymous Referee'
  }

  const renderPronouns = () => {
    const { referee: { showPronouns, pronouns } } = props
    if (!showPronouns) return null

    return pronouns
  }

  const renderCertifications = () => {
    const { referee: { certifications } } = props;

    return certifications.map(certification => (
      <div key={certification.level} className="bg-green py-2 px-6 rounded mr-5">
        {`${capitalize(certification.level)}`}
      </div>
    ))
  }

  return (
    <>
      <div className="flex items-center mb-8">
        <div className="flex-shrink mr-20">
          <h1 className="text-4xl">{refName()}</h1>
        </div>
        <div className="flex items-center justify-around">
          {renderCertifications()}
        </div>
      </div>
      <div className="flex mb-8">
        <h2 className="text-l">{renderPronouns()}</h2>
      </div>
      <div className="text-2xl mb-8 h-24">
        {referee.bio}
      </div>
    </>
  )
}

RefereeHeader.propTypes = {
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
        website: PropTypes.string,
      })
    ),
    certifications: PropTypes.arrayOf(
      PropTypes.shape({
        id: PropTypes.string,
        level: PropTypes.string
      })
    ),
    isEditable: PropTypes.bool,
    gettingStartedDismissedAt: PropTypes.string,
  }).isRequired,
};

export default RefereeHeader
