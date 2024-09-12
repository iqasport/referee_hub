import React from "react";

import { NgbViewModel, RefereeViewModel, useGetNgbsQuery } from "../../../store/serviceApi";

export type RefereeLocationOptions = Pick<RefereeViewModel, "primaryNgb" | "secondaryNgb">

interface LocationProps {
  locations: RefereeLocationOptions;
  isEditing: boolean;
  onChange: (newLocations: RefereeLocationOptions) => void;
}

const RefereeLocation = (props: LocationProps) => {
  const { locations, isEditing, onChange } = props;
  const { data: allNgbs, error: getNgbsError } = useGetNgbsQuery({ skipPaging: true });

  const hasType = (type: keyof RefereeLocationOptions): boolean => {
    return !!locations[type];
  };

  const getNgbName = (type: keyof RefereeLocationOptions): string => {
    return allNgbs.items.filter(ngb => ngb.countryCode === locations[type])[0]?.name;
  };

  const getSelectedNgb = (type: keyof RefereeLocationOptions) => {
    return locations[type];
  };

  const handleChange = (type: keyof RefereeLocationOptions) => (event: React.ChangeEvent<HTMLSelectElement>) => {
    const newNGB = event.target.value;
    const isBlank = newNGB === "-1";

    onChange({...locations, [type]: isBlank ? null : newNGB})
  };

  const renderOption = (ngb: NgbViewModel) => (
    <option key={ngb.countryCode} value={ngb.countryCode}>
      {ngb.name}
    </option>
  );
  const otherLocation = (type: keyof RefereeLocationOptions): keyof RefereeLocationOptions => type === "primaryNgb" ? "secondaryNgb" : "primaryNgb";
  const renderDropdown = (type: keyof RefereeLocationOptions) => {
    return (
      <select
        className="form-select block mt-1"
        onChange={handleChange(type)}
        value={getSelectedNgb(type) ?? ""}
      >
        {type === "secondaryNgb" && <option value="-1">None</option>}
        {allNgbs.items.map(renderOption).filter(o => !getSelectedNgb(otherLocation(type)) || o.key != getSelectedNgb(otherLocation(type)))}
      </select>
    );
  };
  const emptyNgb = "National Governing Body not selected";

  if (!allNgbs) return <></>;

  return (
    <div className="flex flex-col w-1/2 p-4">
      <div className="w-full mb-4">
        <h4 className="text-sm mb-2">Primary NGB</h4>
        {!isEditing && (
          <p className="font-bold">{hasType("primaryNgb") ? getNgbName("primaryNgb") : emptyNgb}</p>
        )}
        {isEditing && renderDropdown("primaryNgb")}
      </div>
      <div className="w-full">
        <h4 className="text-sm mb-2">Secondary NGB</h4>
        {!isEditing && (
          <p className="font-bold">{hasType("secondaryNgb") ? getNgbName("secondaryNgb") : emptyNgb}</p>
        )}
        {isEditing && renderDropdown("secondaryNgb")}
      </div>
    </div>
  );
};

export default RefereeLocation;
