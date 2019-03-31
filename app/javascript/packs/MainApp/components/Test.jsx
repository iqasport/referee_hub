/* eslint-disable camelcase */
import React, { Component, Fragment } from 'react'
import axios from 'axios'
import PropTypes from 'prop-types'
import {
  Segment, Tab, Header, Message
} from 'semantic-ui-react'
import { isEmpty } from 'lodash'

class Test extends Component {
  static propTypes = {
    match: PropTypes.shape({
      params: PropTypes.object
    }).isRequired
  }

  state = {
    test: {
      active: false,
      description: '',
      language: '',
      level: '',
      minimumPassPercentage: 0,
      name: '',
      negativeFeedback: '',
      positiveFeedback: '',
      timeLimit: 0,
      id: ''
    },
    httpStatus: 0,
    httpStatusText: ''
  }

  componentDidMount() {
    const { match: { params } } = this.props

    axios.get(`/api/v1/tests/${params.id}`)
      .then(({ status, statusText, data }) => {
        const { data: { id, attributes } } = data
        const {
          active,
          description,
          language,
          level,
          minimum_pass_percentage,
          name,
          negative_feedback,
          positive_feedback,
          time_limit
        } = attributes

        const test = {
          active,
          description,
          language,
          level,
          minimumPassPercentage: minimum_pass_percentage,
          name,
          negativeFeedback: negative_feedback,
          positiveFeedback: positive_feedback,
          timeLimit: time_limit,
          id
        }

        this.setState({ test, httpStatus: status, httpStatusText: statusText })
      })
      .catch((error) => {
        const { status, statusText } = error.response || {
          status: 500,
          statusText: 'Error'
        }

        this.setState({ test: {}, httpStatus: status, httpStatusText: statusText })
      })
  }

  renderDetailPane = () => {
    const { test, httpStatus, httpStatusText } = this.state
    const isError = httpStatus !== 200

    return (
      <Tab.Pane>
        <Fragment>
          {isError && <Message error header={httpStatus} content={httpStatusText} />}
          <p>{test.description}</p>
        </Fragment>
      </Tab.Pane>
    )
  }

  render() {
    const { test } = this.state
    const panes = [
      { menuItem: 'Details', render: this.renderDetailPane },
      { menuItem: 'Questions', render: this.renderQuestionPane }
    ]

    return (
      <Segment>
        <Header as="h1" textAlign="center">
          {!isEmpty(test) ? test.name : ''}
        </Header>
        <Tab panes={panes} />
      </Segment>
    )
  }
}

export default Test
