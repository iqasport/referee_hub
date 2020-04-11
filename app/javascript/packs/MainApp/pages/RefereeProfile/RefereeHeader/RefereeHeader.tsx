import { capitalize } from 'lodash'
import React from 'react'

import { DataAttributes, IncludedAttributes } from '../../../schemas/getRefereeSchema';

type HeaderProps = {
  referee: DataAttributes;
  certifications: IncludedAttributes[]
}

const RefereeHeader = (props: HeaderProps) => {
  const { referee, certifications } = props
  const refName = () => {
    if (referee.firstName || referee.lastName) {
      return `${referee.firstName} ${referee.lastName}`
    }

    return 'Anonymous Referee'
  }

  const renderCertifications = () => {
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
        {referee.showPronouns && <h2 className="text-l">{referee.pronouns}</h2>}
      </div>
      <div className="text-2xl mb-8 h-24">
        {referee.bio}
      </div>
    </>
  )
}

export default RefereeHeader
