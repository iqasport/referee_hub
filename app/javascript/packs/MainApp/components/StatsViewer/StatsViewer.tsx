import { Interval } from 'luxon'
import React, { useState } from 'react'

import { IncludedAttributes } from '../../schemas/getNationalGoverningBodySchema'
import { toDateTime } from '../../utils/dateUtils'
import RefereeStats from '../RefereeStats'

const sortByTimestamp = (stats: IncludedAttributes[]): IncludedAttributes[] => {
  return stats.sort((a, b) => {
    const aEndTime = toDateTime(a.end)
    const aStartTime = toDateTime(a.start)
    const bEndTime = toDateTime(b.end)
    const aInterval = Interval.fromDateTimes(aStartTime, aEndTime)

    if (aInterval.isBefore(bEndTime)) {
      return -1
    }

    if (aInterval.isAfter(bEndTime)) {
      return 1
    }

    return 0
  })
}

interface StatsViewerProps {
  stats: IncludedAttributes[]
}

enum SelectedStat {
  Referee = 'referee',
  TeamStatus = 'team_status',
  TeamType = 'team_type',
}

const StatsViewer = (props: StatsViewerProps) => {
  const [selectedStat, setSelectedStat] = useState<SelectedStat>()
  const orderedStats = sortByTimestamp(props.stats)
  const currentStat = orderedStats[0]
  
  const handleStatClick = (type: SelectedStat) => () => {
    if (selectedStat !== type) {
      setSelectedStat(type)
    } else {
      setSelectedStat(null)
    }
  }
  
  const renderCurrentStats = () => {
    const refereeSelected = selectedStat && selectedStat === SelectedStat.Referee
    // const teamStatusSelected = selectedStat === SelectedStat.TeamStatus
    // const teamTypeSelected = selectedStat === SelectedStat.TeamType

    return (
      <>
        { (!selectedStat || refereeSelected) && 
          <RefereeStats
            headCount={currentStat.headRefereesCount}
            assistantCount={currentStat.assistantRefereesCount}
            snitchCount={currentStat.snitchRefereesCount}
            uncertifiedCount={currentStat.uncertifiedCount}
            total={currentStat.totalRefereesCount}
            onClick={handleStatClick(SelectedStat.Referee)}
            showFull={refereeSelected}
            stats={orderedStats}
          />
        }
      </>
    )
  }

  return (
    <div className="w-full rounded-lg bg-gray-300 flex justify-between py-8 px-4">
      {renderCurrentStats()}
    </div>
  )
}

export default StatsViewer
