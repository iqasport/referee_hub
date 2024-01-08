import classnames from "classnames";
import React from "react";

import { getMonths, toDateTime } from "../../utils/dateUtils";
import HistoricChart from "../HistoricChart";
import StatBarChart from "../StatBarChart";
import { INgbStatsContextRead } from "../../store/serviceApi";

interface TeamTypeProps {
  communityCount: number;
  universityCount: number;
  youthCount: number;
  total: number;
  onClick: () => void;
  showFull: boolean;
  stats: INgbStatsContextRead[];
}

const TeamTypeStats = (props: TeamTypeProps) => {
  const { total, communityCount, universityCount, youthCount, showFull, onClick, stats } = props;
  const historicChartData = getMonths().map((month) => {
    const foundStat = stats.find((stat) => toDateTime(stat.collectedAt).monthShort === month);
    if (!foundStat) {
      return { month, universityTeamsCount: 0, youthTeamsCount: 0, communityTeamsCount: 0 };
    }

    return { month, ...foundStat };
  });

  const chartData = [
    { type: "community", community: communityCount },
    { type: "university", university: universityCount },
    { type: "youth", youth: youthCount },
  ];

  const barConfig = [
    { dataKey: "community", fill: "#297EE2" },
    { dataKey: "university", fill: "#96C7FF" },
    { dataKey: "youth", fill: "#E3F0FF" },
  ];

  const lineConfig = [
    { name: "Community", dataKey: "communityTeamsCount", stroke: "#297EE2" },
    { name: "University", dataKey: "universityTeamsCount", stroke: "#96C7FF" },
    { name: "Youth", dataKey: "youthTeamsCount", stroke: "#E3F0FF" },
  ];

  const timeText = showFull ? "12 months" : "month";

  return (
    <div className={classnames({ ["w-full"]: showFull, ["w-full lg:w-1/3"]: !showFull })}>
      <h3 className="text-blue-darker text-md lg:text-lg font-bold mb-4">{`Team Type (last ${timeText})`}</h3>
      <div
        className="bg-white flex flex-row rounded-lg hover:shadow-md cursor-pointer mb-4 mr-4"
        onClick={onClick}
      >
        {!showFull && (
          <div className="w-full h-64 flex-1">
            <StatBarChart maxData={total} barConfig={barConfig} chartData={chartData} />
          </div>
        )}
        {showFull && (
          <HistoricChart chartData={historicChartData} maxData={total} lineConfig={lineConfig} />
        )}
      </div>
    </div>
  );
};

export default TeamTypeStats;
