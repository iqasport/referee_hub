import { faEnvelopeOpenText, faRoute, faUpload } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import classnames from 'classnames'
import React from 'react'

interface StepDescriptionProps {
  currentStep: number;
  scopes: string[];
}

const StepDescriptions = (props: StepDescriptionProps) => {
  const { scopes, currentStep } = props
  const scopeString = scopes.length > 1 ? scopes.join(' or ') : scopes[0]
  const isStepActive = (stepNum: number): boolean => currentStep === stepNum;

  return (
    <div className="relative my-12 mx-auto">
      <div className="step-connector" />
      <div className="flex w-full z-1">
        <div className={classnames("w-1/3 flex flex-col items-center text-gray-400 mx-12", { ["text-navy-blue"]: isStepActive(1) })}>
          <h3 className={classnames("uppercase font-bold text-gray-400", { ["text-navy-blue"]: isStepActive(1) })}>Step 1</h3>
          <div className={classnames("step-circle", { ["border-navy-blue"]: isStepActive(1) })}>
            <FontAwesomeIcon icon={faUpload} />
          </div>
          <p>
            <span className="font-bold">Upload</span>
            {` your CSV file of ${scopeString} data.`}
          </p>
        </div>
        <div className={classnames("w-1/3 flex flex-col items-center text-gray-400 mx-12", { ["text-navy-blue"]: isStepActive(2) })}>
          <h3 className={classnames("uppercase font-bold text-gray-400", { ["text-navy-blue"]: isStepActive(2) })}>Step 2</h3>
          <div className={classnames("step-circle", { ["border-navy-blue"]: isStepActive(2) })}>
            <FontAwesomeIcon icon={faRoute} />
          </div>
          <p>
            <span className="font-bold">Map</span>
            {` your custom headers to the required headers`}
          </p>
        </div>
        <div className={classnames("w-1/3 flex flex-col items-center text-gray-400 mx-12", { ["text-navy-blue"]: isStepActive(3) })}>
          <h3 className={classnames("uppercase font-bold text-gray-400", { ["text-navy-blue"]: isStepActive(3) })}>Step 3</h3>
          <div className={classnames("step-circle", { ["border-navy-blue"]: isStepActive(3) })}>
            <FontAwesomeIcon icon={faEnvelopeOpenText} />
          </div>
          <p>
            <span className="font-bold">Review</span>
            {` the results of your import`}
          </p>
        </div>
      </div>
    </div>
  )
}

export default StepDescriptions
