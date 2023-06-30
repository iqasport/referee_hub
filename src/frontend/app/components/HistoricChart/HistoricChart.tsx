import React, { useState } from "react";
import { CartesianGrid, Legend, Line, LineChart, Tooltip, XAxis, YAxis } from "recharts";

import { IncludedAttributes } from "../../schemas/getNationalGoverningBodySchema";

interface ChartData extends IncludedAttributes {
  month: string;
}

interface LineConfig {
  name: string;
  dataKey: string;
  stroke: string;
}

interface HistoricChartProps {
  maxData: number;
  chartData: ChartData[];
  lineConfig: LineConfig[];
}

const HistoricChart = (props: HistoricChartProps) => {
  const { maxData, chartData, lineConfig } = props;
  const [activeCert, setActiveCert] = useState<string>();

  const getOpacity = (dataKey: string): number => {
    if (!activeCert) return 1;
    if (activeCert !== dataKey) return 0.2;

    return 1;
  };

  const handleMouseEnter = (o) => {
    const { dataKey } = o;

    setActiveCert(dataKey);
  };

  const handleMouseLeave = () => setActiveCert(null);

  const renderLine = (line: LineConfig) => {
    return (
      <Line
        {...line}
        key={line.dataKey}
        type="linear"
        dot={{ strokeWidth: 3 }}
        strokeOpacity={getOpacity(line.dataKey)}
      />
    );
  };

  return (
    <div className="w-2/3">
      <LineChart width={500} height={250} data={chartData} margin={{ top: 20 }}>
        <CartesianGrid vertical={false} />
        <XAxis dataKey="month" padding={{ left: 15, right: 15 }} />
        <YAxis type="number" domain={[0, maxData]} />
        <Tooltip />
        <Legend
          layout="vertical"
          verticalAlign="middle"
          wrapperStyle={{ top: 20, right: -120 }}
          onMouseEnter={handleMouseEnter}
          onMouseLeave={handleMouseLeave}
        />
        {lineConfig.map(renderLine)}
      </LineChart>
    </div>
  );
};

export default HistoricChart;
