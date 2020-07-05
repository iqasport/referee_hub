import classnames from 'classnames'
import { capitalize } from 'lodash';
import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux';

import { UpdateTestRequest } from 'MainApp/apis/test';
import { createTest, getTest, updateTest } from 'MainApp/modules/test/test';
import { RootState } from 'MainApp/rootReducer';
import Modal, { ModalProps, ModalSize } from '../Modal/Modal';

const REQUIRED_FIELDS = [
  'name',
  'description',
  'language',
  'minimumPassPercentage',
  'testableQuestionCount',
  'timeLimit',
  'positiveFeedback',
  'negativeFeedback'
]
const initialNewTest: UpdateTestRequest = {
  description: '',
  language: '',
  level: null,
  minimumPassPercentage: 0,
  name: '',
  negativeFeedback: '',
  positiveFeedback: '',
  testableQuestionCount: 0,
  timeLimit: 0,
}

const validateInput = (test: UpdateTestRequest): string[] => {
  return Object.keys(test).filter((dataKey: string) => {
    if (REQUIRED_FIELDS.includes(dataKey) && !test[dataKey]) {
      return true
    }
    return false
  })
}

interface TestEditModalProps extends Omit<ModalProps, 'size'> {
  testId?: string;
}

const TestEditModal = (props: TestEditModalProps) => {
  const { testId, onClose } = props

  const [errors, setErrors] = useState<string[]>()
  const [hasChangedTest, setHasChangedTest] = useState(false)
  const [newTest, setNewTest] = useState<UpdateTestRequest>(initialNewTest)
  const { test } = useSelector((state: RootState) => state.test, shallowEqual)
  const dispatch = useDispatch()

  const formType = testId ? 'Edit' : 'New'
  const hasError = (dataKey: string): boolean => errors?.includes(dataKey)

  useEffect(() => {
    if (testId) {
      dispatch(getTest(testId))
    }
  }, [testId, dispatch])

  useEffect(() => {
    if (test) {
      setNewTest({ ...test.attributes })
    }
  }, [test])

  const handleSubmit = () => {
    const validationErrors = validateInput(newTest)
    if (validationErrors.length) {
      setErrors(validationErrors)
      return null
    }

    if (testId) {
      dispatch(updateTest(testId, {...newTest}))
    } else {
      dispatch(createTest({...newTest}))
    }

    setHasChangedTest(false)
    onClose()
  }

  const handleChange = (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = event.target

    if (!hasChangedTest) setHasChangedTest(true)
    setNewTest({ ...newTest, [name]: value })
  }

  const handleClose = () => {
    setErrors(null)
    setNewTest(initialNewTest)
    onClose()
  }

  const renderError = (attr: string) => {
    return hasError(attr) && <span className="text-red-500 text-sm">Cannot be blank</span>
  }

  return (
    <Modal {...props} onClose={handleClose} size={ModalSize.Large}>
      <h2 className="text-center text-xl font-semibold my-8">{`${formType} Test`}</h2>
      <form>
        <label className="block">
          <span className="text-gray-700">Name</span>
          <input
            className={classnames("form-input mt-1 block w-full", {'border border-red-500': hasError('name')})}
            placeholder="Snitch Referee Test"
            name="name"
            onChange={handleChange}
            value={newTest.name}
          />
          {renderError('name')}
        </label>
        <label className="block mt-8">
          <span className="text-gray-700">Description</span>
          <textarea
            className={
              classnames(
                "form-textarea mt-1 block w-full",
                { 'border border-red-500': hasError('description') }
              )
            }
            placeholder="What should referees know about this test before taking it?"
            name="description"
            onChange={handleChange}
            value={newTest.description || ''}
          />
          {renderError('description')}
        </label>
        <label className="block my-8">
          <span className="text-gray-700">Positive Feedback</span>
          <textarea
            className={
              classnames(
                "form-textarea mt-1 block w-full",
                { 'border border-red-500': hasError('positiveFeedback') }
              )
            }
            placeholder="Provide feedback after a passed test"
            name="positiveFeedback"
            onChange={handleChange}
            value={newTest.positiveFeedback || ''}
          />
          {renderError('positiveFeedback')}
        </label>
        <label className="block">
          <span className="text-gray-700">Negative Feedback</span>
          <textarea
            className={
              classnames(
                "form-textarea mt-1 block w-full",
                { 'border border-red-500': hasError('negativeFeedback') }
              )
            }
            placeholder="Provide feedback after a failed test"
            name="negativeFeedback"
            onChange={handleChange}
            value={newTest.negativeFeedback || ''}
          />
          {renderError('negativeFeedback')}
        </label>
        <div className="flex w-full my-8">
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">Minimum Pass Percentage</span>
            <input
              type="number"
              min="0"
              max="100"
              className={
                classnames(
                  "form-input mt-1 block w-full",
                  {'border border-red-500': hasError('minimumPassPercentage')}
                )
              }
              name="minimumPassPercentage"
              onChange={handleChange}
              value={newTest.minimumPassPercentage}
            />
            {renderError('minimumPassPercentage')}
          </label>
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">Question Count</span>
            <input
              type="number"
              min="1"
              className={
                classnames(
                  "form-input mt-1 block w-full",
                  { 'border border-red-500': hasError('testableQuestionCount') }
                )
              }
              name="testableQuestionCount"
              onChange={handleChange}
              value={newTest.testableQuestionCount}
            />
            {renderError('testableQuestionCount')}
          </label>
          <label className="w-1/3 mr-4">
            <span className="text-gray-700">Time Limit</span>
            <input
              type="number"
              min="1"
              max="120"
              className={
                classnames(
                  "form-input mt-1 block w-full",
                  { 'border border-red-500': hasError('timeLimit') }
                )
              }
              name="timeLimit"
              onChange={handleChange}
              value={newTest.timeLimit}
            />
            {renderError('timeLimit')}
          </label>
        </div>
        <div className="w-full text-center">
          <button
            type="button"
            className={
              classnames(
                "uppercase text-xl py-4 px-8 rounded-lg bg-green text-white",
                {'opacity-50 cursor-default': !hasChangedTest}
              )
            }
            onClick={handleSubmit}
            disabled={!hasChangedTest}
          >
            Done
          </button>
        </div>
      </form>
    </Modal>
  )
}

export default TestEditModal
