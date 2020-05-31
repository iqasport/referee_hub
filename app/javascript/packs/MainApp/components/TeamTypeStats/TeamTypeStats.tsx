import classnames from 'classnames'
import React from 'react';

import { IncludedAttributes } from 'schemas/getNationalGoverningBodySchema';
import { getMonths, toDateTime } from '../../utils/dateUtils';
import HistoricChart from '../HistoricChart';
import StatBarChart from '../StatBarChart';

interface TeamTypeProps {
  communityCount: number;
  universityCount: number;
  youthCount: number;
  total: number;
  onClick: () => void;
  showFull: boolean;
  stats: IncludedAttributes[]
}

const TeamTypeStats = (props: TeamTypeProps) => {
  const { total, communityCount, universityCount, youthCount, showFull, onClick, stats } = props
  const historicChartData = getMonths().map((month) => {
    const foundStat = stats.find((stat) => toDateTime(stat.end).monthShort === month)
    if (!foundStat) {
      return { month, universityTeamsCount: 0, youthTeamsCount: 0, communityTeamsCount: 0 }
    }

    return { month, ...foundStat }
  })

  const chartData = [
    { type: 'community', community: communityCount },
    { type: 'university', university: universityCount },
    { type: 'youth', youth: youthCount },
  ]

  const barConfig = [
    { dataKey: "community", fill: "#297EE2" },
    { dataKey: "university", fill: "#96C7FF" },
    { dataKey: "youth", fill: "#E3F0FF" },
  ]

  const lineConfig = [
    { name: "Community", dataKey: "communityTeamsCount", stroke: "#297EE2" },
    { name: "University", dataKey: "universityTeamsCount", stroke: "#96C7FF" },
    { name: "Youth", dataKey: "youthTeamsCount", stroke: "#E3F0FF" },
  ]

  return (
    <div className={classnames({ ["w-full"]: showFull, ["w-1/3 mx-4"]: !showFull })}>
      <h3 className="text-blue-darker text-xl font-bold mb-4">Team Status</h3>
      <div className="bg-white flex flex-row rounded-lg hover:shadow-md cursor-pointer" onClick={onClick}>
        <div className="w-full h-64 flex-1">
          <StatBarChart maxData={total} barConfig={barConfig} chartData={chartData} />
        </div>
        {showFull && <HistoricChart chartData={historicChartData} maxData={total} lineConfig={lineConfig} />}
      </div>
    </div>
  )
}

export default TeamTypeStats;
