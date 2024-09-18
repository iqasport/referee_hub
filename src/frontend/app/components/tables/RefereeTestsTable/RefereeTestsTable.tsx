import { capitalize, startCase } from "lodash";
import React from "react";

import { getVersion } from "../../../utils/certUtils";
import Table, { CellConfig } from "../Table/Table";
import { Certification, CertificationLevel, RefereeEligibilityResult, RefereeTestAvailableViewModel, useGetAvailableTestsQuery, useGetCurrentRefereeQuery } from "../../../store/serviceApi";
import { getErrorString } from "../../../utils/errorUtils";
import { faBan, faCircleCheck, faClock } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Loader from "../../Loader/Loader";
import { useNavigate } from "../../../utils/navigationUtils";

const HEADER_CELLS = ["title", "level", "rulebook", "language"];

interface RefereeTestsTableProps {
  refId: string;
}

const RefereeTestsTable = (props: RefereeTestsTableProps) => {
  const { refId } = props;
  const navigate = useNavigate();

  const { currentData: referee } = useGetCurrentRefereeQuery();
  const { currentData: tests, isLoading, error: getTestsError } = useGetAvailableTestsQuery();

  const compareByLevel = (a: RefereeTestAvailableViewModel, b: RefereeTestAvailableViewModel): number => {
    const levelValue: Record<CertificationLevel, number> = {
      scorekeeper: 0,
      assistant: 1,
      snitch: 2,
      head: 3,
      field: 4,
    };
    return levelValue[a.level] - levelValue[b.level];
  }
  const compareByEligibility = (a: RefereeTestAvailableViewModel, b: RefereeTestAvailableViewModel): number => {
    // TODO: add weights to eligibility status (e.g. cooldown before others)
    if (a.isRefereeEligible === b.isRefereeEligible) return 0;
    if (a.isRefereeEligible) return -1;
    return 1;
  }
  const compareByLanguage = (a: RefereeTestAvailableViewModel, b: RefereeTestAvailableViewModel): number => {
    const primaryLanguage = (() => {
      switch (referee?.primaryNgb) {
        case "FRA": return "fr";
        case "DEU": return "de";
        case "PRT": return "pt";
        case "ITA": return "it";
        case "AQC": return "ca";
        case "NLD": return "nl";
        case "TUR": return "tr";
        default: return "en";
      }
    })();
    const aLang = a.language.indexOf("-") > 0 ? a.language.substring(0, a.language.indexOf("-")) : a.language;
    const bLang = b.language.indexOf("-") > 0 ? b.language.substring(0, a.language.indexOf("-")) : b.language;

    if (aLang === bLang) return 0;
    if (aLang === primaryLanguage) return -1;
    if (bLang === primaryLanguage) return 1;
    if (aLang === "en") return -1;
    if (bLang === "en") return 1;
    return aLang < bLang ? -1 : 1;
  }

  // We want to have the eligible tests on top, ineligible on bottom
  // Within that we want tests grouped by language (primary language first, then english, then rest)
  // Within that we want tests sorted by level
  const sortedTests = !tests
    ? undefined
    : [...tests].sort(compareByLevel).sort(compareByLanguage).sort(compareByEligibility);

  const handleRowClick = (id: string) => {
    navigate(`/referees/${refId}/tests/${id}`);
  };

  const renderEmpty = () => {
    return <h2>No available tests for this referee</h2>;
  };
  
  const renderTestEligibilityIcon = (eligibilityResult: RefereeEligibilityResult) => {
    if (!eligibilityResult || eligibilityResult === "Eligible") return <></>;
    else if (eligibilityResult === "RefereeAlreadyCertified") return <FontAwesomeIcon icon={faCircleCheck} className="mr-2" />;
    else if (eligibilityResult === "InCooldownPeriod") return <FontAwesomeIcon icon={faClock} className="mr-2" />;
    else return <FontAwesomeIcon icon={faBan} className="mr-2" />;
  }

  const rowConfig: CellConfig<RefereeTestAvailableViewModel>[] = [
    {
      cellRenderer: (item) => {
        return (
          <div className="flex flex-col">
            <span>
              {renderTestEligibilityIcon(item.refereeEligibilityResult)}
              {item.title}
            </span>
            <em className="text-xs text-gray-600">{startCase(item.refereeEligibilityResult)}</em>
          </div>
        );
      },
      dataKey: "name",
    },
    {
      cellRenderer: (item) => {
        return capitalize(item.level);
      },
      dataKey: "level",
    },
    {
      cellRenderer: (item) => {
        return getVersion(item.awardedCertifications[item.awardedCertifications.length-1].version);
      },
      dataKey: "certificationVersion",
    },
    {
      cellRenderer: (item) => {
        return item.language;
      },
      dataKey: "language",
    },
  ];

  if (getTestsError) return <p style={{color: "red"}}>{getErrorString(getTestsError)}</p>;
  if (!tests) return <Loader />;

  return (
    <>
      <h2 className="text-navy-blue text-2xl font-semibold my-4">{`Available Tests(${tests.filter(t => t.isRefereeEligible).length})`}</h2>
      <Table
        items={sortedTests}
        isLoading={isLoading}
        headerCells={HEADER_CELLS}
        onRowClick={handleRowClick}
        emptyRenderer={renderEmpty}
        rowConfig={rowConfig}
        isHeightRestricted={false}
        getId={t => t.testId}
        disabled={t => !t.isRefereeEligible}
      />
    </>
  );
};

export default RefereeTestsTable;
