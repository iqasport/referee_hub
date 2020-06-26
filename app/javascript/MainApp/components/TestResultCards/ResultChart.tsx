import React from 'react'
import { VictoryLabel, VictoryPie } from 'victory'

type ResultChartProps = {
  minimum: number;
  actual: number;
}

const ResultChart = (props: ResultChartProps) => {
  return (
    <svg viewBox="0 0 400 400" style={{height: '100%', width: '50%', left: 0, top: 0}}>
      <VictoryPie
        standalone={false}
        data={[{x: 1, y: props.actual}, {x: 2, y: 100 - props.actual}]} 
        innerRadius={120}
        width={400}
        height={400}
        padAngle={4}
        labels={() => null}
        style={{
          data: {
            fill: ({ datum }) => {
              const color = datum.y >= props.minimum ? '#72BA6B' : '#FAC55C'
              return datum.x === 1 ? color : "#D8D8D8";
            }
          }
        }}
      />
      <VictoryLabel 
        textAnchor="middle" 
        verticalAnchor="middle"
        x={200}
        y={200}
        text={`${props.actual}%`}
        style={{fontSize: '100px', fontWeight: 600}}
      />
    </svg>
  )
}

export default ResultChart;
