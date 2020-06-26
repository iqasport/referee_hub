import { faCaretLeft, faEnvelopeOpenText, faRoute, faUpload, IconDefinition } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import React, { useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux';
import { useHistory } from 'react-router-dom';

import { importTeams } from '../../modules/team/teams';
import { RootState } from '../../rootReducer';
import FinishStep from './FinishStep';
import MapStep, { HeadersMap, REQUIRED_HEADERS } from './MapStep';
import StepDescriptions from './StepDescriptions';
import UploadStep from './UploadStep';

type StepConfig = {
  title: string;
  icon: IconDefinition
}

const stepTextMap: { [stepCount: number]: StepConfig } = {
  1: {
    icon: faUpload,
    title: 'Upload',
  },
  2: {
    icon: faRoute,
    title: 'Map',
  },
  3: {
    icon: faEnvelopeOpenText,
    title: 'Finish'
  },
}

const defaultHeadersMap: HeadersMap = REQUIRED_HEADERS.reduce((acc, value) => {
  acc[value] = value
  return acc
}, {})

const ImportWizard = () => {
  const [stepCount, setStepCount] = useState(1)
  const [uploadedFile, setUploadedFile] = useState<File>()
  const [mappedData, setMappedData] = useState<HeadersMap>(defaultHeadersMap)

  const { meta, error } = useSelector((state: RootState) => state.teams, shallowEqual);
  const history = useHistory();
  const dispatch = useDispatch();
  
  const isFinalStep = stepCount === 3;
  const buttonText = isFinalStep ? 'Done' : 'Next';
  const isDisabled = stepCount === 1 && !uploadedFile
  const currentStepConfig = stepTextMap[stepCount]

  const goForward = () => setStepCount(stepCount + 1)
  
  const handleHomeClick = () => history.goBack()
  const handleButtonClick = () => {
    if (isFinalStep) {
      handleHomeClick()
    } else if (stepCount === 2) {
      dispatch(importTeams(uploadedFile, mappedData))
      goForward()
    } else {
      goForward()
    }
  }
  const handleFileUpload = (selectedFile: File) => setUploadedFile(selectedFile)

  const renderStepContent = (): JSX.Element | null => {
    switch(stepCount) {
      case 1:
        return <UploadStep onFileUpload={handleFileUpload} uploadedFile={uploadedFile} />
      case 2:
        return <MapStep uploadedFile={uploadedFile} onMappingUpdate={setMappedData} mappedData={mappedData} />
      case 3:
        return <FinishStep meta={meta} error={error} />
      default:
        return null
    }
  }

  return (
    <div className="w-full px-10 py-4 flex flex-col h-screen items-center">
      <div className="justify-start w-full">
        <button className="py-4 px-8 flex items-center text-xl" onClick={handleHomeClick}>
          <FontAwesomeIcon icon={faCaretLeft} className="mr-2" />
          Home
        </button>
      </div>
      <h1 className="font-extrabold text-3xl w-full pl-32">Import</h1>
      <div className="lg:block xl:block hidden">
        <StepDescriptions currentStep={stepCount} scopes={['team']} />
      </div>
      <div className="rounded-lg bg-green w-3/4 flex justify-between py-4 px-12 text-navy-blue mb-4">
        <h3 className="text-xl font-bold flex items-center">
          {currentStepConfig.title}
          <FontAwesomeIcon icon={currentStepConfig.icon} className="ml-4" />
        </h3>
        <p className="uppercase">{`Step ${stepCount}/3`}</p>
      </div>
      <div className="rounded border border-gray-400 w-3/4">
        {renderStepContent()}
      </div>
      <div className="justify-end w-full flex mt-8">
        <button className="green-button-outline" onClick={handleButtonClick} disabled={isDisabled}>{buttonText}</button>
      </div>
    </div>
  )
}

export default ImportWizard
