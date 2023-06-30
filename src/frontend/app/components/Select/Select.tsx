import React from "react";
import Select, { Props } from "react-select";

export type SelectOption = {
  label: string;
  value: string;
};

const SelectInput = (props: Props<SelectOption>) => {
  return <Select {...props} />;
};

export default SelectInput;
