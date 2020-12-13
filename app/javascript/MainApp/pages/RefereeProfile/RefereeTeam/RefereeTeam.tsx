import { omitBy } from "lodash";
import React, { useEffect } from "react";
import { shallowEqual, useDispatch, useSelector } from 'react-redux';

import { AssociationData } from "MainApp/apis/referee";
import { GetTeamsFilter } from 'MainApp/apis/team';
import { getTeams, updateFilters } from "MainApp/modules/team/teams";
import { RootState } from "MainApp/rootReducer";
import { AssociationType, IncludedAttributes } from "MainApp/schemas/getRefereeSchema";
import { Datum } from 'MainApp/schemas/getTeamsSchema';

interface RefereeTeamProps {
  teams: IncludedAttributes[];
  locations: IncludedAttributes[];
  isEditing: boolean;
  onChange: (value: AssociationData, stateKey: string) => void;
  value: AssociationData;
  isDisabled: boolean;
}

const RefereeTeam = (props: RefereeTeamProps) => {
  const { isEditing, value, onChange, teams, locations, isDisabled } = props;
  const dispatch = useDispatch();
  const { teams: allTeams, filters } = useSelector((state: RootState) => state.teams, shallowEqual);

  useEffect(() => {
    if (isEditing) {
      const ngbIds = locations.map(loc => loc.nationalGoverningBodyId)
      const updatedFilters: GetTeamsFilter = {
        ...filters,
        nationalGoverningBodies: ngbIds
      }
      dispatch(updateFilters(updatedFilters))
      dispatch(getTeams(updatedFilters))
    }
  }, [isEditing]);

  const hasType = (type: AssociationType): boolean => {
    return (
      teams.filter((team) => team.associationType === type).length > 0
    );
  };

  const getTeamName = (type: AssociationType): string => {
    let teamId: number
    if (Object.values(value).includes(type)) {
      teamId = Number(Object.entries(value).find((association) => association[1] === type)[0])
    } else {
      teamId = teams.find((team) => team.associationType === type).teamId
    }

    if (isEditing) {
      return allTeams.find((team) => Number(team.id) === teamId)?.attributes.name
    } else {
      return teams.find((team) => team.associationType === type)?.name
    }
  };

  const getSelectedTeam = (type: AssociationType) => {
    if (Object.values(value).includes(type)) {
      return Object.entries(value).find((association) => association[1] === type)[0]
    }
    return teams.filter((team) => team.associationType === type)[0]
      ?.teamId;
  };

  const handleChange = (type: AssociationType) => (
    event: React.ChangeEvent<HTMLSelectElement>
  ) => {
    let updatedValue: AssociationData = { ...value };
    const newTeam = event.target.value;
    const isBlank = newTeam === "-1";
    const hasTypeInValue = Object.values(value).includes(type);

    if (hasTypeInValue && !isBlank) {
      const filtered = omitBy(
        value,
        (existingType: string) => existingType === type
      );
      updatedValue = Object.assign(filtered, { [event.target.value]: type });
    } else if (isBlank) {
      updatedValue = omitBy(
        value,
        (existingType: string) => existingType === type
      );
    } else {
      updatedValue[newTeam] = type;
    }

    onChange(updatedValue, "teamsData");
  };

  const renderOption = (team: Datum) => (
    <option key={team.id} value={team.id}>
      {team.attributes.name}
    </option>
  )

  const renderDropdown = (type: AssociationType) => {
    return (
      <label>
        {isDisabled && 'Please select a Primary NGB to select a team'}
        <select
          className="form-select block mt-1"
          onChange={handleChange(type)}
          value={getSelectedTeam(type)}
          disabled={isDisabled}
        >
          {type === AssociationType.Coach && <option value="-1">None</option>}
          {allTeams?.map(renderOption)}
        </select>
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
            {hasType(AssociationType.Player) ? getTeamName(AssociationType.Player) : emptyTeam}
          </p>
        )}
        {isEditing && renderDropdown(AssociationType.Player)}
      </div>
      <div className="w-full">
        <h4 className="text-sm mb-2">Coaching Team</h4>
        {!isEditing && (
          <p className="font-bold">
            {hasType(AssociationType.Coach) ? getTeamName(AssociationType.Coach) : emptyTeam}
          </p>
        )}
        {isEditing && renderDropdown(AssociationType.Coach)}
      </div>
    </div>
  );
};

export default RefereeTeam;
