import React from 'react'
import { VictoryBar, VictoryChart } from 'victory'

const getColor = (category: string) => {
  switch(category) {
    case 'head':
      return '#286E20'
    case 'assistant':
      return '#72BA6C'
    case 'snitch':
      return '#C1EDBC'
    case 'uncertified':
      return '#DFDFDF';
    default:
      return '#DFDFDF';
  }
}

interface CurrentRefereeStatsProps {
  assistantCount: number;
  snitchCount: number;
  headCount: number;
  total: number;
  uncertifiedCount: number;
}

const CurrentRefereeStats = (props: CurrentRefereeStatsProps) => {
  const chartData = [
    { x: 'head', y: props.headCount },
    { x: 'assistant', y: props.assistantCount },
    { x: 'snitch', y: props.snitchCount },
    { x: 'uncertified', y: props.uncertifiedCount }
  ]
  const categories = ['head', 'assistant', 'snitch', 'uncertified']

  return (
    <div className="w-1/3">
      <h3 className="text-green-darker text-xl font-bold mb-4">Referee Certifications</h3>
      <div className="bg-white rounded-lg hover:shadow-md cursor-pointer">
        <VictoryChart domainPadding={25} domain={{ y: [0, props.total] }} height={400}>
          <VictoryBar 
            data={chartData} 
            categories={{ x: categories }} 
            style={{
              data: { fill: ({ datum }) => getColor(datum.x) }
            }}
            cornerRadius={{ topLeft: 6, topRight: 6 }}
          />
        </VictoryChart>
      </div>
    </div>
  )
}

export default CurrentRefereeStats
