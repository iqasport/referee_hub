import { faCaretRight } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { capitalize, difference } from "lodash";
import Papa from "papaparse";
import React, { useEffect, useState } from "react";

export type HeadersMap = {
  [original: string]: string;
};

const URL_REGEX = /url/;
const ANSWER_REGEX = /answer/;
const TEAM_REQUIRED_HEADERS = [
  "name",
  "city",
  "country",
  "state",
  "age_group",
  "status",
  "joined_at",
  "url",
];
const TEST_REQUIRED_HEADERS = [
  "description",
  "feedback",
  "points_available",
  "answer",
  "correct_answer",
];
const NGB_REQUIRED_HEADERS = [
  "name",
  "country",
  "region",
  "acronym",
  "player_count",
  "website",
  "membership_status",
  "url",
];

export const requiredHeaders: { [key: string]: string[] } = {
  ngb: NGB_REQUIRED_HEADERS,
  team: TEAM_REQUIRED_HEADERS,
  test: TEST_REQUIRED_HEADERS,
};

const findSelectedValue = (mappedValue: string): string => {
  if (mappedValue.match(URL_REGEX)) {
    return "url";
  } else if (mappedValue.match(ANSWER_REGEX)) {
    return "answer";
  } else {
    return mappedValue;
  }
};

interface MapStepProps {
  uploadedFile: File;
  mappedData: HeadersMap;
  onMappingUpdate: (mappedHeaders: HeadersMap) => void;
  scope: string;
}

const MapStep = (props: MapStepProps) => {
  const { uploadedFile, mappedData, onMappingUpdate, scope } = props;
  const [originalHeaders, setOriginalHeaders] = useState<string[]>();
  const [originalData, setOriginalData] = useState<string[]>();

  useEffect(() => {
    if (uploadedFile) {
      const reader = new FileReader();

      reader.readAsBinaryString(uploadedFile);
      reader.onloadend = () => {
        const parsed = Papa.parse<string[]>(reader.result.toString());
        setOriginalHeaders(parsed.data[0]);
        setOriginalData(parsed.data[1]);
      };
    }
  }, [uploadedFile]);

  const handleSelectMap = (columnName: string) => (event: React.ChangeEvent<HTMLSelectElement>) => {
    let mappedColumn = event.target.value;

    // remove the default data mapping so we don't have duplicates
    const mapClone = Object.assign({}, mappedData);
    delete mapClone[mappedColumn];

    const urls = Object.values(mapClone).filter((value) => value.match(/url_\d+/));
    const answers = Object.values(mapClone).filter((value) => value.match(/answer_\d+/));

    if (mappedColumn === "url") {
      mappedColumn = `url_${urls.length + 1}`;
    } else if (mappedColumn === "answer") {
      mappedColumn = `answer_${answers.length + 1}`;
    }

    onMappingUpdate({ ...mapClone, [columnName]: mappedColumn });
  };

  const renderColumn = (columnName: string) => {
    const columnIndex = originalHeaders.indexOf(columnName);
    const value = originalData && originalData[columnIndex];
    const mappedValue = mappedData[columnName] || "";
    const selectedValue = findSelectedValue(mappedValue);

    return (
      <div className="flex justify-between items-center mb-4" key={columnName}>
        <div key={columnName} className="w-1/4">
          <h3 className="font-bold text-lg text-navy-blue capitalize">{columnName}</h3>
          <p className="italic text-gray-600">{value}</p>
        </div>
        <div className="w-1/2 flex items-center justify-center">
          <div className="h-1 bg-navy-blue w-1/2" />
          <FontAwesomeIcon icon={faCaretRight} size="3x" className="text-navy-blue" />
        </div>
        <div className="w-1/4">
          <select
            className="form-select block mt-1 w-full"
            onChange={handleSelectMap(columnName)}
            value={selectedValue}
          >
            <option value="">Select</option>
            {requiredHeaders[scope].map((header) => {
              return (
                <option key={header} value={header} disabled={header === selectedValue}>
                  {header
                    .split("_")
                    .map((word) => capitalize(word))
                    .join(" ")}
                </option>
              );
            })}
          </select>
        </div>
      </div>
    );
  };

  const renderMapping = () => {
    const needsMapping = originalHeaders ? difference(originalHeaders, requiredHeaders[scope]) : [];
    if (!needsMapping.length)
      return <h1>All headers are correctly mapped, click Next to upload your csv</h1>;

    return (
      <>
        <div className="flex justify-between items-center mb-8">
          <h4 className="uppercase text-xl">Your Columns</h4>
          <h4 className="uppercase text-xl">IQA Columns</h4>
        </div>
        {needsMapping.map(renderColumn)}
      </>
    );
  };

  return <div className="w-1/2 mx-auto my-4 py-12">{renderMapping()}</div>;
};

export default MapStep;
