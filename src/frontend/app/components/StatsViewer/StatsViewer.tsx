import React, { useState } from "react";

import RefereeStats from "../RefereeStats";
import TeamStatusStats from "../TeamStatusStats";
import TeamTypeStats from "../TeamTypeStats";
import { INgbStatsContextRead } from "../../store/serviceApi";

interface StatsViewerProps {
  stats: INgbStatsContextRead[];
}

enum SelectedStat {
  Referee = "referee",
  TeamStatus = "team_status",
  TeamType = "team_type",
}

const StatsViewer = (props: StatsViewerProps) => {
  const [selectedStat, setSelectedStat] = useState<SelectedStat>();
  const orderedStats = props.stats;
  const currentStat = props.stats[0];

  const handleStatClick = (type: SelectedStat) => () => {
    if (selectedStat !== type) {
      setSelectedStat(type);
    } else {
      setSelectedStat(null);
    }
  };

  const renderCurrentStats = () => {
    const refereeSelected = selectedStat && selectedStat === SelectedStat.Referee;
    const teamStatusSelected = selectedStat && selectedStat === SelectedStat.TeamStatus;
    const teamTypeSelected = selectedStat && selectedStat === SelectedStat.TeamType;

    return (
      <>
        {(!selectedStat || refereeSelected) && (
          <RefereeStats
            headCount={currentStat?.headRefereesCount}
            assistantCount={currentStat?.assistantRefereesCount}
            snitchCount={currentStat?.flagRefereesCount}
            uncertifiedCount={currentStat?.uncertifiedRefereesCount}
            total={currentStat?.totalRefereesCount}
            onClick={handleStatClick(SelectedStat.Referee)}
            showFull={refereeSelected}
            stats={orderedStats}
          />
        )}
        {(!selectedStat || teamTypeSelected) && (
          <TeamTypeStats
            communityCount={currentStat?.communityTeamsCount}
            universityCount={currentStat?.universityTeamsCount}
            youthCount={currentStat?.youthTeamsCount}
            total={currentStat?.totalTeamsCount}
            onClick={handleStatClick(SelectedStat.TeamType)}
            showFull={teamTypeSelected}
            stats={orderedStats}
          />
        )}
        {(!selectedStat || teamStatusSelected) && (
          <TeamStatusStats
            competitiveCount={currentStat?.competitiveTeamsCount}
            developingCount={currentStat?.developingTeamsCount}
            inactiveCount={currentStat?.inactiveTeamsCount}
            total={currentStat?.totalTeamsCount}
            onClick={handleStatClick(SelectedStat.TeamStatus)}
            showFull={teamStatusSelected}
            stats={orderedStats}
          />
        )}
      </>
    );
  };

  return (
    <div className="w-full rounded-lg bg-gray-300 flex flex-col lg:flex-row justify-between py-8 px-4">
      {renderCurrentStats()}
    </div>
  );
};

export default StatsViewer;
