import classnames from "classnames";
import React from "react";

import { getMonths, toDateTime } from "../../utils/dateUtils";
import HistoricChart from "../HistoricChart";
import StatBarChart from "../StatBarChart";
import { INgbStatsContextRead } from "../../store/serviceApi";

interface RefereeStatsProps {
  assistantCount: number;
  snitchCount: number;
  headCount: number;
  total: number;
  uncertifiedCount: number;
  onClick: () => void;
  showFull: boolean;
  stats: INgbStatsContextRead[];
}

const RefereeStats = (props: RefereeStatsProps) => {
  const { total, headCount, assistantCount, snitchCount, showFull, onClick, stats } = props;
  const historicChartData = getMonths().map((month) => {
    const foundStat = stats.find((stat) => toDateTime(stat.collectedAt).monthShort === month);
    if (!foundStat) {
      return {
        month,
        assistantRefereesCount: 0,
        flagRefereesCount: 0,
        headRefereesCount: 0,
        uncertifiedCount: 0,
      };
    }

    return { month, ...foundStat };
  });

  const chartData = [
    { type: "head", head: headCount },
    { type: "assistant", assistant: assistantCount },
    { type: "flag", flag: snitchCount },
  ];

  const barConfig = [
    { dataKey: "head", fill: "#286E20" },
    { dataKey: "assistant", fill: "#72BA6C" },
    { dataKey: "flag", fill: "#C1EDBC" },
  ];

  const lineConfig = [
    { name: "Assistant", dataKey: "assistantRefereesCount", stroke: "#72BA6C" },
    { name: "Flag", dataKey: "flagRefereesCount", stroke: "#C1EDBC" },
    { name: "Head", dataKey: "headRefereesCount", stroke: "#286E20" },
  ];

  const timeText = showFull ? "12 months" : "month";

  return (
    <div className={classnames({ ["w-full"]: showFull, ["w-full lg:w-1/3"]: !showFull })}>
      <h3 className="text-green-darker text-md lg:text-lg font-bold mb-4">
        {`Referee Certifications (last ${timeText})`}
      </h3>
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

export default RefereeStats;
