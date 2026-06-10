import { capitalize, startCase } from "lodash";
import React, { useEffect, useMemo, useState } from "react";

import { getVersion } from "../../../utils/certUtils";
import Table, { CellConfig } from "../Table/Table";
import { Certification, CertificationLevel, RefereeEligibilityResult, RefereeTestAvailableViewModel, useGetAvailableTestsQuery, useGetCurrentRefereeQuery } from "../../../store/serviceApi";
import { getErrorString } from "../../../utils/errorUtils";
import { faBan, faCircleCheck, faClock } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Loader from "../../Loader/Loader";
import { useNavigate } from "../../../utils/navigationUtils";

const HEADER_CELLS = ["title", "level", "rulebook", "language"];
const ALL_FILTER_VALUE = "*";
const RULEBOOK_ORDER: Record<string, number> = {
  eighteen: 0,
  twenty: 1,
  twentytwo: 2,
  twentyfour: 3,
};

interface RefereeTestsTableProps {
  refId: string;
}

const RefereeTestsTable = (props: RefereeTestsTableProps) => {
  const { refId } = props;
  const navigate = useNavigate();
  const [testTypeFilter, setTestTypeFilter] = useState<string>(ALL_FILTER_VALUE);
  const [languageFilter, setLanguageFilter] = useState<string>(ALL_FILTER_VALUE);
  const [rulebookFilter, setRulebookFilter] = useState<string>("");

  const { currentData: referee } = useGetCurrentRefereeQuery();
  const { currentData: tests, isLoading, error: getTestsError } = useGetAvailableTestsQuery();

  const getRulebookVersion = (test: RefereeTestAvailableViewModel): string => {
    const latestAwardedCertification = test.awardedCertifications[test.awardedCertifications.length - 1];
    return latestAwardedCertification?.version;
  };

  const formatLanguageCode = (languageCode: string): string => {
    if (!languageCode) return "";

    const normalizedLanguageCode = languageCode.replace("_", "-");
    const [language, region] = normalizedLanguageCode.split("-");

    try {
      const languageName = new Intl.DisplayNames(["en"], { type: "language" }).of(language) ?? language;
      if (!region) return capitalize(languageName);

      const regionName = new Intl.DisplayNames(["en"], { type: "region" }).of(region.toUpperCase()) ?? region.toUpperCase();
      return `${capitalize(languageName)} (${regionName})`;
    } catch {
      return normalizedLanguageCode;
    }
  };

  const compareByLevel = (a: RefereeTestAvailableViewModel, b: RefereeTestAvailableViewModel): number => {
    const levelValue: Record<CertificationLevel, number> = {
      scorekeeper: 0,
      flagrunner: 1,
      assistant: 2,
      snitch: 3,
      head: 4,
      field: 5,
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
        case "BRA": return "pt"; // we have mainly Brazilian Portuguese
        case "ITA": return "it";
        case "AQC": return "ca";
        case "NLD": return "nl";
        case "TUR": return "tr";
        case "AQE": return "es";
        case "ARG": return "es";
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

  const testTypeOptions = useMemo(() => {
    if (!tests) return [];

    const levels = Array.from(new Set(tests.map(t => t.level)));
    return levels.sort((a, b) => {
      const levelValue: Record<CertificationLevel, number> = {
        scorekeeper: 0,
        flagrunner: 1,
        assistant: 2,
        snitch: 3,
        head: 4,
        field: 5,
      };
      return levelValue[a] - levelValue[b];
    });
  }, [tests]);

  const languageOptions = useMemo(() => {
    if (!tests) return [];

    return Array.from(new Set(tests.map(t => t.language))).sort((a, b) => a.localeCompare(b));
  }, [tests]);

  const rulebookOptions = useMemo(() => {
    if (!tests) return [];

    const versions = Array.from(new Set(tests.map(getRulebookVersion).filter((version): version is string => !!version)));
    return versions.sort((a, b) => (RULEBOOK_ORDER[b] ?? -1) - (RULEBOOK_ORDER[a] ?? -1));
  }, [tests]);

  useEffect(() => {
    if (!rulebookOptions.length) {
      setRulebookFilter("");
      return;
    }

    if (!rulebookFilter || !rulebookOptions.includes(rulebookFilter)) {
      setRulebookFilter(rulebookOptions[0]);
    }
  }, [rulebookFilter, rulebookOptions]);

  const filteredTests = useMemo(() => {
    if (!sortedTests) return undefined;

    return sortedTests.filter(test => {
      if (testTypeFilter !== ALL_FILTER_VALUE && test.level !== testTypeFilter) return false;
      if (languageFilter !== ALL_FILTER_VALUE && test.language !== languageFilter) return false;
      if (rulebookFilter && getRulebookVersion(test) !== rulebookFilter) return false;
      return true;
    });
  }, [languageFilter, rulebookFilter, sortedTests, testTypeFilter]);

  const clearFilters = () => {
    setTestTypeFilter(ALL_FILTER_VALUE);
    setLanguageFilter(ALL_FILTER_VALUE);
    setRulebookFilter(rulebookOptions[0] ?? "");
  };

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
        return capitalize(item.level == 'snitch' ? 'flag': item.level);
      },
      dataKey: "level",
    },
    {
      cellRenderer: (item) => {
        return getVersion(getRulebookVersion(item));
      },
      dataKey: "certificationVersion",
    },
    {
      cellRenderer: (item) => {
        return formatLanguageCode(item.language);
      },
      dataKey: "language",
    },
  ];

  if (getTestsError) return <p style={{color: "red"}}>{getErrorString(getTestsError)}</p>;
  if (!tests) return <Loader />;

  return (
    <>
      <h2 className="text-navy-blue text-2xl font-semibold my-4">{`Available Tests(${(filteredTests ?? []).filter(t => t.isRefereeEligible).length})`}</h2>
      <div className="bg-gray-100 rounded-lg p-4 mb-5">
        <h3 className="text-base font-semibold text-gray-900 mb-3">Filter Certifications</h3>
        <div className="flex flex-wrap gap-4 items-end">
          <label className="flex flex-col min-w-44">
            <span className="text-sm font-medium text-gray-700 mb-1">Test Type</span>
            <select
              className="tournament-search-select w-full"
              onChange={(event) => setTestTypeFilter(event.target.value)}
              value={testTypeFilter}
            >
              <option value={ALL_FILTER_VALUE}>Any test type</option>
              {testTypeOptions.map(level => (
                <option key={level} value={level}>{capitalize(level === "snitch" ? "flag" : level)}</option>
              ))}
            </select>
          </label>
          <label className="flex flex-col min-w-44">
            <span className="text-sm font-medium text-gray-700 mb-1">Language</span>
            <select
              className="tournament-search-select w-full"
              onChange={(event) => setLanguageFilter(event.target.value)}
              value={languageFilter}
            >
              <option value={ALL_FILTER_VALUE}>Any language</option>
              {languageOptions.map(language => (
                <option key={language} value={language}>{formatLanguageCode(language)}</option>
              ))}
            </select>
          </label>
          <label className="flex flex-col min-w-44">
            <span className="text-sm font-medium text-gray-700 mb-1">Rulebook</span>
            <select
              className="tournament-search-select w-full"
              onChange={(event) => setRulebookFilter(event.target.value)}
              value={rulebookFilter}
            >
              {rulebookOptions.map(version => (
                <option key={version} value={version}>{getVersion(version)}</option>
              ))}
            </select>
          </label>
          <button
            type="button"
            className="btn btn-outline whitespace-nowrap"
            onClick={clearFilters}
          >
            Reset filters
          </button>
        </div>
      </div>
      <Table
        items={filteredTests}
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
