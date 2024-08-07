import React from "react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  LabelList,
  ResponsiveContainer,
  XAxis,
  YAxis,
} from "recharts";

type ChartData = {
  type: string;
  head?: number;
  assistant?: number;
  snitch?: number;
};

type BarConfig = {
  dataKey: string;
  fill: string;
};

interface StatBarChartProps {
  chartData: ChartData[];
  barConfig: BarConfig[];
  maxData: number;
}

const StatBarChart = (props: StatBarChartProps) => {
  const { chartData, barConfig, maxData } = props;

  const renderBar = (bar: BarConfig) => (
    <Bar key={bar.dataKey} {...bar} minPointSize={5}>
      {
        // https://github.com/recharts/recharts/issues/1265#issuecomment-822297208
        // hiding undefined entries
        chartData.map((entry, index) => {
          return <Cell key={`cell-${index}`}
            style={{display: (entry[bar.dataKey] === 0 || entry[bar.dataKey] > 0) ? '' : 'none'}}/>
        })
      }
      <LabelList dataKey="type" position="centerTop" angle={270} />
    </Bar>
  );

  return (
    <ResponsiveContainer height="100%" width="80%">
      <BarChart
        width={250}
        height={250}
        data={chartData}
        barSize={60}
        barCategoryGap="5%"
        margin={{ top: 20, bottom: 5, right: 2, left: -20 }}
      >
        <CartesianGrid vertical={false} />
        <XAxis dataKey="type" tick={false} />
        <YAxis type="number" domain={[0, maxData]} />
        {barConfig.map(renderBar)}
      </BarChart>
    </ResponsiveContainer>
  );
};

export default StatBarChart;
