import { faEnvelopeOpenText, faRoute, faUpload } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import classnames from 'classnames'
import React from 'react'

const stepTemplateMap: { [scope: string]: string } = {
  'ngb': 'https://docs.google.com/spreadsheets/d/1Z5Wf-2CMYrXvk87gaglNRwZkO6tLuTolz6InhyJhpAQ/edit?usp=sharing',
  'team': 'https://docs.google.com/spreadsheets/d/17NOk3OjKls8y2gih5PTz5Avw7wJ0mY_x3Cl-hkFOcrA/edit?usp=sharing',
  'test': 'https://docs.google.com/spreadsheets/d/1TIGX1CpuSyaGJBwaZzVB1eb7zhw9BvjoWezgUaXbRKU/edit?usp=sharing'
}

interface StepDescriptionProps {
  currentStep: number;
  scope: string;
}

const StepDescriptions = (props: StepDescriptionProps) => {
  const { scope, currentStep } = props
  const isStepActive = (stepNum: number): boolean => currentStep === stepNum;

  return (
    <div className="relative my-12 mx-auto">
      <div className="step-connector" />
      <div className="flex w-full z-1">
        <div className={classnames("step-container", { ["step-active"]: isStepActive(1) })}>
          <h3 className={classnames("step-number", { ["step-active"]: isStepActive(1) })}>Step 1</h3>
          <div className={classnames("step-circle", { ["circle-active"]: isStepActive(1) })}>
            <FontAwesomeIcon icon={faUpload} />
          </div>
          <p>
            <span className="font-bold">Upload</span>
            {` your CSV file of ${scope} data.`}
          </p>
          <p>
            Need help? Download an example csv {' '}
            <a
              className="text-blue-darker hover:text-blue"
              target="_blank"
              rel="noopener noreferrer"
              href={stepTemplateMap[scope]}
            >
              here
            </a>
          </p>
        </div>
        <div className={classnames("step-container", { ["step-active"]: isStepActive(2) })}>
          <h3 className={classnames("step-number", { ["step-active"]: isStepActive(2) })}>Step 2</h3>
          <div className={classnames("step-circle", { ["circle-active"]: isStepActive(2) })}>
            <FontAwesomeIcon icon={faRoute} />
          </div>
          <p>
            <span className="font-bold">Map</span>
            {` your custom headers to the required headers`}
          </p>
        </div>
        <div className={classnames("step-container", { ["step-active"]: isStepActive(3) })}>
          <h3 className={classnames("step-number", { ["step-active"]: isStepActive(3) })}>Step 3</h3>
          <div className={classnames("step-circle", { ["circle-active"]: isStepActive(3) })}>
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
