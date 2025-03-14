import {
  faCaretLeft,
  faEnvelopeOpenText,
  faRoute,
  faUpload,
  IconDefinition,
} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React, { useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";

import { importNgbs } from "../../modules/nationalGoverningBody/nationalGoverningBodies";
import { importTestQuestions } from "../../modules/question/questions";
import { importTeams } from "../../modules/team/teams";
import { RootState } from "../../rootReducer";

import FinishStep from "./FinishStep";
import MapStep, { HeadersMap, requiredHeaders } from "./MapStep";
import StepDescriptions from "./StepDescriptions";
import UploadStep from "./UploadStep";
import { AppDispatch } from "../../store";
import { useNavigate, useNavigationParams } from "../../utils/navigationUtils";
import { useImportTestQuestionsMutation } from "../../store/serviceApi";

type StepConfig = {
  title: string;
  icon: IconDefinition;
};

const stepTextMap: { [stepCount: number]: StepConfig } = {
  1: {
    icon: faUpload,
    title: "Upload",
  },
  2: {
    icon: faRoute,
    title: "Map",
  },
  3: {
    icon: faEnvelopeOpenText,
    title: "Finish",
  },
};

const defaultHeadersMap = (scope: string): HeadersMap =>
  requiredHeaders[scope].reduce((acc, value) => {
    acc[value] = value;
    return acc;
  }, {});

const ImportWizard = () => {
  const { importScope, scopeId } = useNavigationParams<"importScope" | "scopeId">();

  const [stepCount, setStepCount] = useState(1);
  const [uploadedFile, setUploadedFile] = useState<File>();
  const [mappedData, setMappedData] = useState<HeadersMap>(defaultHeadersMap(importScope));

  const { meta, error } = useSelector((state: RootState) => state.teams, shallowEqual);
  const { meta: questionMeta, error: questionError } = useSelector(
    (state: RootState) => state.questions,
    shallowEqual
  );
  const { meta: ngbMeta, error: ngbError } = useSelector(
    (state: RootState) => state.nationalGoverningBodies,
    shallowEqual
  );
  const navigate = useNavigate();
  const dispatch = useDispatch<AppDispatch>();

  const [importTestQuestions, {error: questionError2, isLoading: areQuestionsLoading}] = useImportTestQuestionsMutation();
  const questionMeta2 = areQuestionsLoading ? null : {total: "All"};

  const isFinalStep = stepCount === 3;
  const buttonText = isFinalStep ? "Done" : "Next";
  const isDisabled = stepCount === 1 && !uploadedFile;
  const currentStepConfig = stepTextMap[stepCount];
  const dataType = importScope === "test" ? "questions" : `${importScope}s`;
  const goForward = () => setStepCount(stepCount + 1);

  const handleHomeClick = () => navigate(-1);
  const handleButtonClick = () => {
    if (isFinalStep) {
      handleHomeClick();
    } else if (stepCount === 2) {
      if (importScope === "team") {
        dispatch(importTeams(uploadedFile, mappedData, scopeId));
      } else if (importScope === "test") {
        importTestQuestions({ testId: scopeId, testQuestions: uploadedFile });
      } else if (importScope === "ngb") {
        dispatch(importNgbs(uploadedFile, mappedData));
      }
      goForward();
    } else {
      goForward();
    }
  };
  const handleFileUpload = (selectedFile: File) => setUploadedFile(selectedFile);

  const renderStepContent = (): JSX.Element | null => {
    const finishedMeta = meta || questionMeta || ngbMeta || questionMeta2;
    const finishedError = error || questionError || ngbError || questionError2;
    switch (stepCount) {
      case 1:
        return <UploadStep onFileUpload={handleFileUpload} uploadedFile={uploadedFile} />;
      case 2:
        return (
          <MapStep
            uploadedFile={uploadedFile}
            onMappingUpdate={setMappedData}
            mappedData={mappedData}
            scope={importScope}
          />
        );
      case 3:
        return <FinishStep meta={finishedMeta} error={finishedError} dataType={dataType} />;
      default:
        return null;
    }
  };

  return (
    <div className="w-full px-10 py-4 flex flex-col items-center">
      <div className="justify-start w-full">
        <button className="py-4 px-8 flex items-center text-xl" onClick={handleHomeClick}>
          <FontAwesomeIcon icon={faCaretLeft} className="mr-2" />
          Home
        </button>
      </div>
      <h1 className="font-extrabold text-3xl w-full pl-32">Import</h1>
      <div className="lg:block xl:block hidden">
        <StepDescriptions currentStep={stepCount} scope={importScope} />
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
        <div className="justify-center mb-8 w-full flex">
          <button
            className="green-button-outline"
            onClick={handleButtonClick}
            disabled={isDisabled}
          >
            {buttonText}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ImportWizard;
