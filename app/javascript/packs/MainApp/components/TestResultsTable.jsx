import React, { Component } from 'react'
import PropTypes from 'prop-types'
import { Table, Icon } from 'semantic-ui-react'
import { capitalize, uniqueId } from 'lodash'

function valueStringFormatter(value, modifier) {
  return value ? `${value}${modifier}` : 'N/A'
}

class TestResultsTable extends Component {
  static propTypes ={
    testResults: PropTypes.arrayOf(
      PropTypes.shape({
        duration: PropTypes.string,
        minimumPassPercentage: PropTypes.number,
        passed: PropTypes.bool,
        percentage: PropTypes.number,
        pointsAvailable: PropTypes.number,
        pointsScored: PropTypes.number,
        timeFinished: PropTypes.string,
        timeStarted: PropTypes.string,
        testLevel: PropTypes.string
      })
    ).isRequired
  }

  renderTableHeader = () => (
    <Table.Header>
      <Table.Row>
        <Table.HeaderCell>
          Test Type
        </Table.HeaderCell>
        <Table.HeaderCell>
          Total Test Duration
        </Table.HeaderCell>
        <Table.HeaderCell>
          Passed
        </Table.HeaderCell>
        <Table.HeaderCell>
          Minimum Pass Percentage
        </Table.HeaderCell>
        <Table.HeaderCell>
          Correct Answer Percentage
        </Table.HeaderCell>
        <Table.HeaderCell>
          Total Points Scored
        </Table.HeaderCell>
        <Table.HeaderCell>
          Total Points Available
        </Table.HeaderCell>
      </Table.Row>
    </Table.Header>
  )

  renderTableRow = (testResult) => {
    const {
      testLevel,
      duration,
      minimumPassPercentage,
      passed,
      percentage,
      pointsAvailable,
      pointsScored
    } = testResult

    const passedIcon = <Icon name="checkmark" color="green" size="large" />
    const failedIcon = <Icon name="times" color="red" size="large" />

    return (
      <Table.Row key={uniqueId(`${testLevel}_`)}>
        <Table.Cell>
          {capitalize(testLevel)}
        </Table.Cell>
        <Table.Cell>
          {valueStringFormatter(duration, '')}
        </Table.Cell>
        <Table.Cell>
          {passed ? passedIcon : failedIcon}
        </Table.Cell>
        <Table.Cell>
          {valueStringFormatter(minimumPassPercentage, '%')}
        </Table.Cell>
        <Table.Cell>
          {valueStringFormatter(percentage, '%')}
        </Table.Cell>
        <Table.Cell>
          {valueStringFormatter(pointsScored, ' pts')}
        </Table.Cell>
        <Table.Cell>
          {valueStringFormatter(pointsAvailable, ' pts')}
        </Table.Cell>
      </Table.Row>
    )
  }

  render() {
    const { testResults } = this.props

    return (
      <Table>
        {this.renderTableHeader()}
        <Table.Body>
          {testResults.map(this.renderTableRow)}
        </Table.Body>
      </Table>
    )
  }
}

export default TestResultsTable
