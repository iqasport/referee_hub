import React, { useEffect, useState } from "react";
import { InputActionMeta, MultiValue, ActionMeta } from "react-select";

import Select, { SelectOption } from "../../../components/Select/Select";
import { RefereeLocationOptions } from "../RefereeLocation/RefereeLocation";
import { NgbTeamViewModel, NgbViewModel, RefereeViewModel, useGetNgbTeamsQuery } from "../../../store/serviceApi";

export type RefereeTeamOptions = Pick<RefereeViewModel, "playingTeam" | "coachingTeam">

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
  const allTeams = (primaryNgbTeams?.items ?? []).concat(secondaryNgbTeams?.items ?? []);
  const [filteredTeams, setFilteredTeams] = useState<NgbTeamViewModel[]>([]);

  // update local state after teams are loaded
  // can't use allTeams here as it's changing on each rerender
  useEffect(() => setFilteredTeams((primaryNgbTeams?.items ?? []).concat(secondaryNgbTeams?.items ?? [])), [primaryNgbTeams, secondaryNgbTeams]);

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

  const handleSearch = (searchValue: string) => {
    if (!searchValue) setFilteredTeams(allTeams);
    setFilteredTeams(allTeams.filter(team => team.name.toLowerCase().includes(searchValue.toLowerCase())));
  };

  const handleInputChange = (newValue: string, actionMeta: InputActionMeta) => {
    if (actionMeta.action === "input-change") {
      handleSearch(newValue);
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

  const renderDropdown = (type: keyof RefereeTeamOptions) => {
    return (
      <label>
        {isDisabled && "Please select a Primary NGB to select a team"}
        <Select
          isClearable
          isMulti={false}
          isDisabled={isDisabled}
          onInputChange={handleInputChange}
          onChange={handleChange(type)}
          options={filteredTeams.map((team) => ({ value: team.teamId.toString(), label: team.name }))}
          value={getSelectedTeam(type)}
        />
      </label>
    );
  };
  const emptyTeam = "Team not selected";

  return (
    <div className="flex flex-col w-1/2 p-4">
      <div className="w-full mb-4">
        <h4 className="text-sm mb-2">Playing Team</h4>
        {!isEditing && (
          <p className="font-bold">
            {hasType("playingTeam") ? getTeamName("playingTeam") : emptyTeam}
          </p>
        )}
        {isEditing && renderDropdown("playingTeam")}
      </div>
      <div className="w-full">
        <h4 className="text-sm mb-2">Coaching Team</h4>
        {!isEditing && (
          <p className="font-bold">
            {hasType("coachingTeam") ? getTeamName("coachingTeam") : emptyTeam}
          </p>
        )}
        {isEditing && renderDropdown("coachingTeam")}
      </div>
    </div>
  );
};

export default RefereeTeam;
