import { Input, TextArea, Dropdown } from 'semantic-ui-react'
import { range } from 'lodash'

export const currencyConfig = [
  {
    total: '23',
    currency: 'AUD'
  },
  {
    total: '23',
    currency: 'CAD'
  },
  {
    total: '15',
    currency: 'EUR'
  },
  {
    total: '10',
    currency: 'GBP'
  },
  {
    total: '16',
    currency: 'USD'
  }
]

export const CERTIFICATION_LEVELS = {
  snitch: 'snitch',
  assistant: 'assistant',
  head: 'head',
  field: 'field'
}

const levelDropdown = [
  { text: 'Snitch', value: 'snitch' },
  { text: 'Assistant', value: 'assistant' },
  { text: 'Head', value: 'head' }
]
const MAX_TIME = 120
const MIN_TIME = 10
const minuteArray = range(MIN_TIME, MAX_TIME)
const timeLimitDropdown = minuteArray.map(minute => ({
  text: minute,
  value: minute
}))

export const TEST_FORM_CONFIG = (values) => {
  const {
    name,
    description,
    level,
    language,
    minimumPassPercentage,
    timeLimit,
    positiveFeedback,
    negativeFeedback,
    testableQuestionCount
  } = values

  const descPlaceholder = !description ? "What should referee's know about this test before taking it?" : null
  const negFeedbackPlaceholder = !negativeFeedback ? 'Provide feedback after a failed test' : null
  const posFeedbackPlaceholder = !positiveFeedback ? 'Provide feedback after a passed test' : null

  return [
    {
      control: Input,
      label: 'Name',
      name: 'Name',
      value: name,
    },
    {
      control: TextArea,
      label: 'Description',
      name: 'Description',
      value: description,
      placeholder: descPlaceholder,
    },
    {
      control: Input,
      label: 'Language',
      name: 'Language',
      value: language,
    },
    {
      control: Dropdown,
      label: 'Certification Level',
      name: 'Level',
      options: levelDropdown,
      value: level || undefined,
      defaultValue: !level ? 'snitch' : undefined,
    },
    {
      control: Input,
      label: 'Minimum Pass Percentage',
      name: 'MinimumPassPercentage',
      value: minimumPassPercentage,
    },
    {
      control: Input,
      label: 'Question Count',
      name: 'TestableQuestionCount',
      value: testableQuestionCount,
    },
    {
      control: Dropdown,
      label: 'Time Limit',
      name: 'TimeLimit',
      options: timeLimitDropdown,
      value: timeLimit || undefined,
      defaultValue: !timeLimit ? 18 : undefined,
      scrolling: 'true'
    },
    {
      control: TextArea,
      label: 'Positive Feedback',
      name: 'PositiveFeedback',
      value: positiveFeedback,
      placeholder: posFeedbackPlaceholder,
    },
    {
      control: TextArea,
      label: 'Negative Feedback',
      name: 'NegativeFeedback',
      value: negativeFeedback,
      placeholder: negFeedbackPlaceholder,
    },
  ]
}
