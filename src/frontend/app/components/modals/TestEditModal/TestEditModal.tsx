import classnames from "classnames";
import { capitalize } from "lodash";
import React, { useEffect, useState } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";

import LanguageDropdown from "../../../components/LanguageDropdown";
import Toggle from "../../../components/Toggle";

import Modal, { ModalProps, ModalSize } from "../Modal/Modal";
import { Certification, TestViewModel, useCreateNewTestMutation, useEditTestMutation, useGetAllTestsQuery, useGetLanguagesQuery } from "../../../store/serviceApi";

const REQUIRED_TEST_FIELDS = [
  "title",
  "description",
  "passPercentage",
  "questionsCount",
  "timeLimit",
  "positiveFeedback",
  "negativeFeedback",
  "language",
];
const REQUIRED_CERT_FIELDS = ["version", "level"];
const LEVEL_OPTIONS = ["snitch", "assistant", "head", "field", "scorekeeper"];
const VERSION_OPTIONS = ["eighteen", "twenty", "twentytwo", "twentyfour"];

const initialNewTest: TestViewModel = {
  awardedCertification: null,
  description: "",
  passPercentage: 0,
  title: "",
  negativeFeedback: "",
  positiveFeedback: "",
  recertification: false,
  questionsCount: 0,
  timeLimit: 0,
  language: "en-US",
};
const initialCertification: Certification = {
  level: null,
  version: "twentyfour",
};

const validateInput = (test: TestViewModel, cert: Certification): string[] => {
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
  const [newTest, setNewTest] = useState<TestViewModel>(initialNewTest);
  const [newCert, setNewCert] = useState<Certification>(initialCertification);

  const { data: tests } = useGetAllTestsQuery();
  const { data: languages } = useGetLanguagesQuery();
  const [createTest] = useCreateNewTestMutation();
  const [updateTest] = useEditTestMutation();

  const test = tests.filter(t => t.testId === testId)?.[0];
  const formType = testId ? "Edit" : "New";
  const hasError = (dataKey: string): boolean => errors?.includes(dataKey);

  useEffect(() => {
    if (test && testId) {
      setNewTest({ ...test });
      setNewCert({ level: test.awardedCertification?.level, version: test.awardedCertification?.version });
    }
  }, [test]);

  const handleSubmit = () => {
    const validationErrors = validateInput(newTest, newCert);
    if (validationErrors.length) {
      setErrors(validationErrors);
      return null;
    }

    const testToSend: TestViewModel = {
      ...newTest,
      ...newCert,
      awardedCertification: newCert,
    };

    if (testId) {
      updateTest({ testId, testViewModel: testToSend });
    } else {
      createTest({ testViewModel: testToSend });
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
          <span className="text-gray-700">Title</span>
          <input
            type="text"
            aria-label="title"
            className={classnames("form-input mt-1 block w-full", {
              "border border-red-500": hasError("title"),
            })}
            placeholder="Snitch Referee Test"
            name="title"
            onChange={handleChange}
            value={newTest.title}
          />
          {renderError("title")}
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
              name="language"
              languages={languages || []}
              hasError={hasError("language")}
              onChange={handleChange}
              value={newTest?.language || ""}
            />
            {renderError("newLanguageId")}
          </label>
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">Level</span>
            <select
              className={classnames("form-select mt-1 block w-full", {
                "border border-red-500": hasError("level"),
              })}
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
                "border border-red-500": hasError("passPercentage"),
              })}
              name="passPercentage"
              onChange={handleChange}
              value={newTest.passPercentage}
            />
            {renderError("passPercentage")}
          </label>
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">Question Count</span>
            <input
              type="number"
              min="1"
              className={classnames("form-input mt-1 block w-full", {
                "border border-red-500": hasError("questionsCount"),
              })}
              name="questionsCount"
              onChange={handleChange}
              value={newTest.questionsCount}
            />
            {renderError("questionsCount")}
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
