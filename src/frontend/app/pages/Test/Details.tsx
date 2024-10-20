import { isBoolean, kebabCase } from "lodash";
import React from "react";
import { TestViewModel } from "../../store/serviceApi";

// the language field is excluded because it's a legacy attribute
const EXCLUDED_ATTRIBUTES = ["active", "updatedAt", "awardedCertification"];
export interface DetailsProps {
  test: TestViewModel;
  languages: string[];
}

const Details = (props: DetailsProps) => {
  const { test, languages } = props;
  const dataToRender = test ? test : [];
  const language = test?.language;

  const renderData = (entry: [string, string | boolean]) => {
    if (EXCLUDED_ATTRIBUTES.includes(entry[0])) return null;
    let labelText: string;
    let dataText: string;

    /*if (language && entry[0] === "newLanguageId") {
      const {
        attributes: { longName, shortRegion },
      } = language;
      const regionText = shortRegion ? ` - ${shortRegion}` : "";
      labelText = "Language";
      dataText = `${longName}${regionText}`;
    } else {*/
      labelText = kebabCase(entry[0]).split("-").join(" ");
      dataText = isBoolean(entry[1]) ? String(entry[1]) : entry[1];
    //}

    return (
      <div key={entry[0]} className="my-4">
        <label className="uppercase text-md font-hairline text-gray-400">{labelText}</label>
        <p>{dataText}</p>
      </div>
    );
  };

  return <div>{Object.entries(dataToRender).map(renderData)}</div>;
};

export default Details;
