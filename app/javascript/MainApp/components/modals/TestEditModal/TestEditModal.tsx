import classnames from "classnames";
import { capitalize } from "lodash";
import React, { useEffect, useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";

import { UpdateTestRequest } from "MainApp/apis/single_test";
import LanguageDropdown from "MainApp/components/LanguageDropdown";
import Toggle from "MainApp/components/Toggle";
import { getCertifications } from "MainApp/modules/certification/certifications";
import { getLanguages } from "MainApp/modules/language/languages";
import { createTest, getTest, updateTest } from "MainApp/modules/test/single_test";
import { RootState } from "MainApp/rootReducer";
import { TestLevel } from "MainApp/schemas/getTestSchema";

import Modal, { ModalProps, ModalSize } from "../Modal/Modal";

type UpdateCertification = {
  level: TestLevel;
  version: string;
};

const REQUIRED_TEST_FIELDS = [
  "name",
  "description",
  "minimumPassPercentage",
  "testableQuestionCount",
  "timeLimit",
  "positiveFeedback",
  "negativeFeedback",
  "newLanguageId",
];
const REQUIRED_CERT_FIELDS = ["version", "level"];
const LEVEL_OPTIONS = ["snitch", "assistant", "head", "field", "scorekeeper"];
const VERSION_OPTIONS = ["eighteen", "twenty", "twentytwo"];

const initialNewTest: UpdateTestRequest = {
  certificationId: null,
  description: "",
  level: null,
  minimumPassPercentage: 0,
  name: "",
  negativeFeedback: "",
  positiveFeedback: "",
  recertification: false,
  testableQuestionCount: 0,
  timeLimit: 0,
  newLanguageId: 0,
};
const initialCertification: UpdateCertification = {
  level: null,
  version: "",
};

const validateInput = (test: UpdateTestRequest, cert: UpdateCertification): string[] => {
  const testErrors = Object.keys(test).filter((dataKey: string) => {
    if (REQUIRED_TEST_FIELDS.includes(dataKey) && !test[dataKey]) {
      return true;
    }
    return false;
  });
  const certErrors = Object.keys(cert).filter((dataKey: string) => {
    if (REQUIRED_CERT_FIELDS.includes(dataKey) && !cert[dataKey]) {
      return true;
    }
    return false;
  });

  return testErrors.concat(certErrors);
};

interface TestEditModalProps extends Omit<ModalProps, "size"> {
  testId?: string;
  shouldUpdateTests?: boolean;
}

const TestEditModal = (props: TestEditModalProps) => {
  const { testId, onClose, shouldUpdateTests } = props;

  const [errors, setErrors] = useState<string[]>();
  const [hasChangedTest, setHasChangedTest] = useState(false);
  const [newTest, setNewTest] = useState<UpdateTestRequest>(initialNewTest);
  const [newCert, setNewCert] = useState<UpdateCertification>(initialCertification);
  const { test, certification } = useSelector((state: RootState) => state.test, shallowEqual);
  const { certifications } = useSelector((state: RootState) => state.certifications, shallowEqual);
  const { languages } = useSelector((state: RootState) => state.languages, shallowEqual);
  const dispatch = useDispatch();

  const formType = testId ? "Edit" : "New";
  const hasError = (dataKey: string): boolean => errors?.includes(dataKey);

  useEffect(() => {
    if (testId && testId !== test?.id) {
      dispatch(getTest(testId));
    }
  }, [testId, test, dispatch]);

  useEffect(() => {
    if (test && testId) {
      setNewTest({ ...test?.attributes });
      setNewCert({ level: certification?.level, version: certification?.version });
    }
  }, [test, certification]);

  useEffect(() => {
    if (!certifications?.length) {
      dispatch(getCertifications());
    }
    if (!languages?.length) {
      dispatch(getLanguages());
    }
  }, []);

  const handleSubmit = () => {
    const validationErrors = validateInput(newTest, newCert);
    if (validationErrors.length) {
      setErrors(validationErrors);
      return null;
    }

    const matchedCert = certifications.find(
      ({ attributes: { level, version } }) => level === newCert.level && version === newCert.version
    );
    const testToSend: UpdateTestRequest = {
      ...newTest,
      ...newCert,
      certificationId: parseInt(matchedCert?.id, 10),
    };

    if (testId) {
      dispatch(updateTest(testId, testToSend, shouldUpdateTests));
    } else {
      dispatch(createTest(testToSend));
    }

    setHasChangedTest(false);
    onClose();
  };

  const handleChange = (
    event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = event.target;

    if (!hasChangedTest) setHasChangedTest(true);
    if (Object.keys(newCert).includes(name)) {
      setNewCert({ ...newCert, [name]: value });
    } else {
      setNewTest({ ...newTest, [name]: value });
    }
  };

  const handleToggleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.currentTarget.checked;

    if (!hasChangedTest) setHasChangedTest(true);

    setNewTest({ ...newTest, recertification: value });
  };

  const handleClose = () => {
    setErrors(null);
    setNewTest(initialNewTest);
    onClose();
  };

  const renderError = (attr: string) => {
    return (
      hasError(attr) && (
        <span className="text-red-500 text-sm">{`${capitalize(attr)} Cannot be blank`}</span>
      )
    );
  };

  const renderOption = (value: string) => {
    return (
      <option data-testid={value} key={value} value={value}>
        {capitalize(value)}
      </option>
    );
  };

  return (
    <Modal {...props} onClose={handleClose} size={ModalSize.Large}>
      <h2 className="text-center text-xl font-semibold my-8">{`${formType} Test`}</h2>
      <form>
        <label className="block">
          <span className="text-gray-700">Name</span>
          <input
            type="text"
            aria-label="name"
            className={classnames("form-input mt-1 block w-full", {
              "border border-red-500": hasError("name"),
            })}
            placeholder="Snitch Referee Test"
            name="name"
            onChange={handleChange}
            value={newTest.name}
          />
          {renderError("name")}
        </label>
        <label className="block mt-8">
          <span className="text-gray-700">Description</span>
          <textarea
            aria-label="description"
            className={classnames("form-textarea mt-1 block w-full", {
              "border border-red-500": hasError("description"),
            })}
            placeholder="What should referees know about this test before taking it?"
            name="description"
            onChange={handleChange}
            value={newTest.description || ""}
          />
          {renderError("description")}
        </label>
        <label className="block my-8">
          <span className="text-gray-700">Positive Feedback</span>
          <textarea
            className={classnames("form-textarea mt-1 block w-full", {
              "border border-red-500": hasError("positiveFeedback"),
            })}
            placeholder="Provide feedback after a passed test"
            name="positiveFeedback"
            onChange={handleChange}
            value={newTest.positiveFeedback || ""}
          />
          {renderError("positiveFeedback")}
        </label>
        <label className="block">
          <span className="text-gray-700">Negative Feedback</span>
          <textarea
            className={classnames("form-textarea mt-1 block w-full", {
              "border border-red-500": hasError("negativeFeedback"),
            })}
            placeholder="Provide feedback after a failed test"
            name="negativeFeedback"
            onChange={handleChange}
            value={newTest.negativeFeedback || ""}
          />
          {renderError("negativeFeedback")}
        </label>
        <div className="flex w-full my-8">
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">Language</span>
            <LanguageDropdown
              name="newLanguageId"
              languages={languages}
              hasError={hasError("newLanguageId")}
              onChange={handleChange}
              value={newTest?.newLanguageId?.toString() || ""}
            />
            {renderError("newLanguageId")}
          </label>
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">Level</span>
            <select
              className={classnames("form-select mt-1 block w-full", {
                "border border-red-500": hasError("level"),
              })}
              placeholder="Select the level"
              name="level"
              onChange={handleChange}
              value={newCert.level || ""}
            >
              <option value="">Select the level</option>
              {LEVEL_OPTIONS.map(renderOption)}
            </select>
            {renderError("level")}
          </label>
          <label className="w-1/3">
            <span className="text-gray-700">Version</span>
            <select
              className={classnames("form-select mt-1 block w-full", {
                "border border-red-500": hasError("version"),
              })}
              name="version"
              onChange={handleChange}
              value={newCert.version}
              placeholder="Select rulebook version"
            >
              <option value="">Select rulebook version</option>
              {VERSION_OPTIONS.map(renderOption)}
            </select>
            {renderError("version")}
          </label>
        </div>
        <div className="flex w-full my-8">
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">Minimum Pass Percentage</span>
            <input
              type="number"
              min="0"
              max="100"
              className={classnames("form-input mt-1 block w-full", {
                "border border-red-500": hasError("minimumPassPercentage"),
              })}
              name="minimumPassPercentage"
              onChange={handleChange}
              value={newTest.minimumPassPercentage}
            />
            {renderError("minimumPassPercentage")}
          </label>
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">Question Count</span>
            <input
              type="number"
              min="1"
              className={classnames("form-input mt-1 block w-full", {
                "border border-red-500": hasError("testableQuestionCount"),
              })}
              name="testableQuestionCount"
              onChange={handleChange}
              value={newTest.testableQuestionCount}
            />
            {renderError("testableQuestionCount")}
          </label>
          <label className="w-1/3">
            <span className="text-gray-700">Time Limit</span>
            <input
              type="number"
              min="1"
              max="120"
              className={classnames("form-input mt-1 block w-full", {
                "border border-red-500": hasError("timeLimit"),
              })}
              name="timeLimit"
              onChange={handleChange}
              value={newTest.timeLimit}
            />
            {renderError("timeLimit")}
          </label>
        </div>
        <div className="w-full text-left">
          <Toggle
            onChange={handleToggleChange}
            name="recertification"
            label="Recertification Test?"
            checked={newTest.recertification}
          />
        </div>
        <div className="w-full text-center">
          <button
            type="button"
            className={classnames("uppercase text-xl py-4 px-8 rounded-lg bg-green text-white", {
              "opacity-50 cursor-default": !hasChangedTest,
            })}
            onClick={handleSubmit}
            disabled={!hasChangedTest}
          >
            Done
          </button>
        </div>
      </form>
    </Modal>
  );
};

export default TestEditModal;
