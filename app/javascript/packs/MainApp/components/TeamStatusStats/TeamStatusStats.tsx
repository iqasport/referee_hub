import classnames from 'classnames'
import React from 'react';

import { IncludedAttributes } from 'schemas/getNationalGoverningBodySchema';
import { getMonths, toDateTime } from '../../utils/dateUtils';
import HistoricChart from '../HistoricChart';
import StatBarChart from '../StatBarChart';

interface TeamStatusProps {
  competitiveCount: number;
  developingCount: number;
  inactiveCount: number;
  total: number;
  onClick: () => void;
  showFull: boolean;
  stats: IncludedAttributes[]
}

const TeamStatusStats = (props: TeamStatusProps) => {
  const { total, competitiveCount, developingCount, inactiveCount, showFull, onClick, stats } = props
  const historicChartData = getMonths().map((month) => {
    const foundStat = stats.find((stat) => toDateTime(stat.endTime).monthShort === month)
    if (!foundStat) {
      return { month, developingTeamsCount: 0, inactiveTeamsCount: 0, competitiveTeamsCount: 0 }
    }

    return { month, ...foundStat }
  })

  const chartData = [
    { type: 'competitive', competitive: competitiveCount },
    { type: 'developing', developing: developingCount },
    { type: 'inactive', inactive: inactiveCount },
  ]

  const barConfig = [
    { dataKey: "competitive", fill: "#D49011" },
    { dataKey: "developing", fill: "#FFBE45" },
    { dataKey: "inactive", fill: "#FFE6B8" },
  ]

  const lineConfig = [
    { name: "Competitive", dataKey: "competitiveTeamsCount", stroke: "#D49011" },
    { name: "Developing", dataKey: "developingTeamsCount", stroke: "#FFBE45" },
    { name: "Inactive", dataKey: "inactiveTeamsCount", stroke: "#FFE6B8" },
  ]

  return (
    <div className={classnames({ ["w-full"]: showFull, ["w-1/3"]: !showFull })}>
      <h3 className="text-yellow-darker text-xl font-bold mb-4">Team Status</h3>
      <div className="bg-white flex flex-row rounded-lg hover:shadow-md cursor-pointer" onClick={onClick}>
        <div className="w-full h-64 flex-1">
          <StatBarChart maxData={total} barConfig={barConfig} chartData={chartData} />
        </div>
        {showFull && <HistoricChart chartData={historicChartData} maxData={total} lineConfig={lineConfig} />}
      </div>
    </div>
  )
}

export default TeamStatusStats;
