import classnames from "classnames";
import React, { FunctionComponent } from "react";

type DataLabelProps = {
  label: string;
  customClass?: string;
  children?: React.ReactNode;
};

const DataLabel: FunctionComponent<DataLabelProps> = (props) => {
  return (
    <label className={classnames("text-gray-600 font-light mt-8", props.customClass)}>
      {props.label}
      {props.children}
    </label>
  );
};

export default DataLabel;
