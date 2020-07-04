import classnames from 'classnames'
import React, { useEffect, useState } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux';

import { UpdateTestRequest } from 'MainApp/apis/test';
import { createTest, getTest, updateTest } from 'MainApp/modules/test/test';
import { RootState } from 'MainApp/rootReducer';
import Modal, { ModalProps, ModalSize } from '../Modal/Modal';

const REQUIRED_FIELDS = ['name']
const initialNewTest: UpdateTestRequest = {
  active: false,
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

  const handleChange = (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = event.target

    if (!hasChangedTest) setHasChangedTest(true)
    setNewTest({ ...newTest, [name]: value })
  }

  return (
    <Modal {...props} size={ModalSize.Large}>
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
          {hasError('name') && <span className="text-red-500">Name cannot be blank</span>}
        </label>
      </form>
    </Modal>
  )
}

export default TestEditModal
