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

export const CERT_LINKS = (snitchAvailable, assAvailable, headAvailable) => ({
  english: [
    {
      title: 'Snitch Referee Written Test 2018–20',
      link: 'https://www.classmarker.com/online-test/start/?quiz=4q95bafa6c1b2a6a',
      color: 'yellow',
      enabled: snitchAvailable
    },
    {
      title: 'Assistant Referee Written Test 2018–20',
      link: 'https://www.classmarker.com/online-test/start/?quiz=gyv5babf1bd8146f',
      color: 'blue',
      enabled: assAvailable
    },
    {
      title: 'Head Referee Written Test 2018–20',
      link: 'https://www.classmarker.com/online-test/start/?quiz=tyg5baff2b2c128c',
      color: 'green',
      enabled: headAvailable
    },

  ],
  català: [
    {
      title: 'Examen escrit d’àrbitre/a d’esnitx 2018–20',
      link: 'https://www.classmarker.com/online-test/start/?quiz=x6r5c759d0fa5e30',
      color: 'yellow',
      enabled: snitchAvailable
    },
    {
      title: 'Examen escrit d’àrbitre/a assistent 2018–20',
      link: 'https://www.classmarker.com/online-test/start/?quiz=6e45c75953075a03',
      color: 'blue',
      enabled: assAvailable
    },
    {
      title: 'Examen escrit d’àrbitre/a principal 2018–20',
      link: 'https://www.classmarker.com/online-test/start/?quiz=age5ca75fca28524',
      color: 'green',
      enabled: headAvailable
    }
  ],
  français: [
    {
      title: 'Test écrit d’arbitre de vif d’or 2018–20',
      link: 'https://www.classmarker.com/online-test/start/?quiz=g6e5c759e2b640cb',
      color: 'yellow',
      enabled: snitchAvailable
    },
    {
      title: 'Test écrit d’arbitre assistant 2018–20',
      link: 'https://www.classmarker.com/online-test/start/?quiz=ddq5c759def81996',
      color: 'blue',
      enabled: assAvailable
    },
    {
      title: 'Test écrit d’arbitre principal 2018–20',
      link: 'https://www.classmarker.com/online-test/start/?quiz=9qy5ca75f0501760',
      color: 'green',
      enabled: headAvailable
    }
  ]
})

export const OLD_CERT_LINKS = {
  snitch: {
    title: 'Snitch Referee Written Test 2016-18',
    links: {
      en: 'https://www.classmarker.com/online-test/start/?quiz=crx5bb21de04a997'
    },
    color: 'yellow'
  },
  assistant: {
    title: 'Assistant Referee Written Test 2016-18',
    links: {
      en: 'https://www.classmarker.com/online-test/start/?quiz=tgr5bb21e1c149dc'
    },
    color: 'blue'
  },
  head: {
    title: 'Head Referee Written Test 2016-18',
    links: {
      en: 'https://www.classmarker.com/online-test/start/?quiz=9xb5bb21e53ea15f'
    },
    color: 'green'
  }
}

export const NEW_TESTS_ENABLED = true
