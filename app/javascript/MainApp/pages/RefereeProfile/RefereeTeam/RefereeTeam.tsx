import { omitBy } from "lodash";
import React, { useEffect } from "react";
import { shallowEqual, useDispatch, useSelector } from "react-redux";
import { InputActionMeta, ValueType, ActionMeta } from "react-select";

import { AssociationData } from "MainApp/apis/referee";
import { GetTeamsFilter } from "MainApp/apis/team";
import { getTeams, updateFilters } from "MainApp/modules/team/teams";
import { RootState } from "MainApp/rootReducer";
import { AssociationType, IncludedAttributes } from "MainApp/schemas/getRefereeSchema";
import Select, { SelectOption } from "MainApp/components/Select/Select";

interface RefereeTeamProps {
  teams: IncludedAttributes[];
  locations: IncludedAttributes[];
  isEditing: boolean;
  onChange: (value: AssociationData, stateKey: string) => void;
  associationValue: AssociationData;
  isDisabled: boolean;
}

const RefereeTeam = (props: RefereeTeamProps) => {
  const { isEditing, associationValue, onChange, teams, locations, isDisabled } = props;
  const dispatch = useDispatch();
  const { teams: allTeams, filters } = useSelector((state: RootState) => state.teams, shallowEqual);

  const updateTeams = (updatedFilters: GetTeamsFilter) => {
    dispatch(updateFilters(updatedFilters));
    dispatch(getTeams(updatedFilters));
  };

  useEffect(() => {
    if (isEditing) {
      const ngbIds = locations.map((loc) => loc.nationalGoverningBodyId);
      const updatedFilters: GetTeamsFilter = {
        ...filters,
        nationalGoverningBodies: ngbIds,
      };
      updateTeams(updatedFilters);
    }
  }, [isEditing]);

  const hasType = (type: AssociationType): boolean => {
    return teams.filter((team) => team.associationType === type).length > 0;
  };

  const getTeamName = (type: AssociationType): string => {
    if (isEditing) {
      let teamId: number;
      if (Object.values(associationValue).includes(type)) {
        teamId = Number(
          Object.entries(associationValue).find((association) => association[1] === type)[0]
        );
      } else {
        teamId = teams.find((team) => team.associationType === type).teamId;
      }
      return allTeams.find((team) => Number(team.id) === teamId)?.attributes.name;
    } else {
      return teams.find((team) => team.associationType === type)?.name;
    }
  };

  const getSelectedTeam = (type: AssociationType): SelectOption => {
    let foundTeam: IncludedAttributes;
    if (Object.values(associationValue).includes(type)) {
      const foundTeamId = Object.entries(associationValue).find(
        (association) => association[1] === type
      )[0];
      foundTeam = allTeams
        ?.filter((team) => team.id === foundTeamId)
        ?.map((t) => ({ teamId: Number(t.id), name: t.attributes.name }));
    } else {
      foundTeam = teams?.find((team) => team.associationType === type);
    }

    return foundTeam ? { value: foundTeam[0].teamId.toString(), label: foundTeam[0].name } : null;
  };

  const handleSelect = (type: AssociationType, option: { value: string; label: string }) => {
    let updatedValue: AssociationData = { ...associationValue };
    const newTeamId = option.value;
    const isBlank = newTeamId === "-1";
    const hasTypeInValue = Object.values(associationValue).includes(type);

    if (hasTypeInValue && !isBlank) {
      const filtered = omitBy(associationValue, (existingType: string) => existingType === type);
      updatedValue = Object.assign(filtered, { [newTeamId]: type });
    } else if (isBlank) {
      updatedValue = omitBy(associationValue, (existingType: string) => existingType === type);
    } else {
      updatedValue[newTeamId] = type;
    }

    const updatedFilters = { ...filters };
    delete updatedFilters.q;
    updateTeams(updatedFilters);
    onChange(updatedValue, "teamsData");
  };

  const handleSearch = (searchValue: string) => {
    updateTeams({ ...filters, q: searchValue });
  };

  const handleInputChange = (newValue: string, actionMeta: InputActionMeta) => {
    if (actionMeta.action === "input-change") {
      handleSearch(newValue);
    }
  };

  const handleChange = (type: AssociationType) => (
    value: ValueType<SelectOption, false>,
    action: ActionMeta<SelectOption>
  ) => {
    switch (action.action) {
      case "clear":
        handleSelect(type, { value: "-1", label: "" });
      case "select-option":
        handleSelect(type, value);
    }
  };

  const renderDropdown = (type: AssociationType) => {
    return (
      <label>
        {isDisabled && "Please select a Primary NGB to select a team"}
        <Select
          isClearable
          isMulti={false}
          isDisabled={isDisabled}
          onInputChange={handleInputChange}
          onChange={handleChange(type)}
          options={allTeams.map((t) => ({ value: t.id, label: t.attributes.name }))}
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
