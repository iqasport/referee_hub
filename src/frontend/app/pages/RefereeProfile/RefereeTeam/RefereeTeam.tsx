import React, { useEffect, useState } from "react";
import { InputActionMeta, MultiValue, ActionMeta } from "react-select";

import Select, { SelectOption } from "../../../components/Select/Select";
import { RefereeLocationOptions } from "../RefereeLocation/RefereeLocation";
import { NgbTeamViewModel, NgbViewModel, RefereeViewModel, useGetNgbTeamsQuery, useGetNationalTeamsQuery } from "../../../store/serviceApi";

export type RefereeTeamOptions = Pick<RefereeViewModel, "playingTeam" | "coachingTeam" | "nationalTeam">

interface RefereeTeamProps {
  teams: RefereeTeamOptions;
  locations: RefereeLocationOptions;
  isEditing: boolean;
  onChange: (newTeams: RefereeTeamOptions) => void;
}

const RefereeTeam = (props: RefereeTeamProps) => {
  const { isEditing, onChange, teams, locations } = props;
  const isDisabled = !locations.primaryNgb && !locations.secondaryNgb;
  
  const { data: primaryNgbTeams, error: getPrimaryNgbTeamsError } = useGetNgbTeamsQuery({ ngb: locations.primaryNgb, skipPaging: true }, {skip: !locations.primaryNgb});
  const { data: secondaryNgbTeams, error: getSecondaryNgbTeamsError } = useGetNgbTeamsQuery({ ngb: locations.secondaryNgb, skipPaging: true }, {skip: !locations.secondaryNgb});
  const { data: allNationalTeamsData, error: getNationalTeamsError } = useGetNationalTeamsQuery({ skipPaging: true });
  
  const ngbTeams = (primaryNgbTeams?.items ?? []).concat(secondaryNgbTeams?.items ?? []);
  const allTeams = ngbTeams.concat(allNationalTeamsData?.items ?? []);
  
  // Filter teams by type: regular teams (excluding national) and national teams
  const regularTeams = ngbTeams.filter(team => team.groupAffiliation !== "national");
  const nationalTeams = allNationalTeamsData?.items ?? [];
  
  const [filteredPlayingTeams, setFilteredPlayingTeams] = useState<NgbTeamViewModel[]>([]);
  const [filteredCoachingTeams, setFilteredCoachingTeams] = useState<NgbTeamViewModel[]>([]);
  const [filteredNationalTeams, setFilteredNationalTeams] = useState<NgbTeamViewModel[]>([]);

  // update local state after teams are loaded
  useEffect(() => {
    const allRegularTeams = (primaryNgbTeams?.items ?? []).concat(secondaryNgbTeams?.items ?? []).filter(team => team.groupAffiliation !== "national");
    const allNationalTeamsFromApi = allNationalTeamsData?.items ?? [];
    setFilteredPlayingTeams(allRegularTeams);
    setFilteredCoachingTeams(allRegularTeams);
    setFilteredNationalTeams(allNationalTeamsFromApi);
  }, [primaryNgbTeams, secondaryNgbTeams, allNationalTeamsData]);

  const hasType = (type: keyof RefereeTeamOptions): boolean => {
    return !!teams[type];
  };

  const getTeamName = (type: keyof RefereeTeamOptions): string => {
    return allTeams.filter(team => team.teamId === teams[type]?.id)[0]?.name || teams[type]?.name;
  };

  const getSelectedTeam = (type: keyof RefereeTeamOptions): SelectOption => {
    if (!hasType(type)) return null;
    return ({
      value: teams[type]?.id.toString(),
      label: getTeamName(type),
    });
  };

  const handleSelect = (type: keyof RefereeTeamOptions, option: { value: string; label: string }) => {
    const newTeamId = option?.value;
    const isBlank = newTeamId === "-1" || !newTeamId;

    onChange({...teams, [type]: isBlank ? null : { id: newTeamId }})
  };

  const handleSearch = (teamType: 'playing' | 'coaching' | 'national', searchValue: string) => {
    const sourceTeams = teamType === 'national' ? nationalTeams : regularTeams;
    const filteredResults = searchValue 
      ? sourceTeams.filter(team => {
          const teamName = team.name ?? '';
          return teamName.toLowerCase().includes(searchValue.toLowerCase());
        })
      : sourceTeams;
    
    if (teamType === 'playing') {
      setFilteredPlayingTeams(filteredResults);
    } else if (teamType === 'coaching') {
      setFilteredCoachingTeams(filteredResults);
    } else {
      setFilteredNationalTeams(filteredResults);
    }
  };

  const handleInputChange = (teamType: 'playing' | 'coaching' | 'national') => (newValue: string, actionMeta: InputActionMeta) => {
    if (actionMeta.action === "input-change") {
      handleSearch(teamType, newValue);
    }
  };

  const handleChange = (type: keyof RefereeTeamOptions) => (
    value: SelectOption | MultiValue<SelectOption>,
    action: ActionMeta<SelectOption>
  ) => {
    switch (action.action) {
      case "clear":
        handleSelect(type, { value: "-1", label: "" });
        break;
      case "select-option":
        handleSelect(type, value as SelectOption); // cast only works while isMulti={false} below
        break;
    }
  };

  const renderDropdown = (type: keyof RefereeTeamOptions, teamType: 'playing' | 'coaching' | 'national') => {
    const options = teamType === 'playing' ? filteredPlayingTeams 
      : teamType === 'coaching' ? filteredCoachingTeams 
      : filteredNationalTeams;
    
    return (
      <label>
        {isDisabled && "Please select a Primary NGB to select a team"}
        <Select
          isClearable
          isMulti={false}
          isDisabled={isDisabled}
          onInputChange={handleInputChange(teamType)}
          onChange={handleChange(type)}
          options={options.map((team) => ({ value: team.teamId?.toString() ?? '', label: team.name ?? '' }))}
          value={getSelectedTeam(type)}
        />
      </label>
    );
  };
  const emptyTeam = "Team not selected";

  const renderTeamName = (type: keyof RefereeTeamOptions): JSX.Element => {
    if (!hasType(type)) {
      return <span>{emptyTeam}</span>;
    }

    const teamName = getTeamName(type);

    // Always display the name as plain text (privacy)
    return <span>{teamName}</span>;
  };

  return (
    <div className="flex flex-col w-1/2 p-4">
      <div className="w-full mb-4">
        <h4 className="text-sm mb-2">Playing Team</h4>
        {!isEditing && (
          <p className="font-bold">
            {renderTeamName("playingTeam")}
          </p>
        )}
        {isEditing && renderDropdown("playingTeam", "playing")}
      </div>
      <div className="w-full mb-4">
        <h4 className="text-sm mb-2">Coaching Team</h4>
        {!isEditing && (
          <p className="font-bold">
            {renderTeamName("coachingTeam")}
          </p>
        )}
        {isEditing && renderDropdown("coachingTeam", "coaching")}
      </div>
      <div className="w-full">
        <h4 className="text-sm mb-2">National Team</h4>
        {!isEditing && (
          <p className="font-bold">
            {renderTeamName("nationalTeam")}
          </p>
        )}
        {isEditing && renderDropdown("nationalTeam", "national")}
      </div>
    </div>
  );
};

export default RefereeTeam;
