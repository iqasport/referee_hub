import { capitalize } from "lodash";
import React, { useState } from "react";
import { getVersion } from "../../../utils/certUtils";
import FilterToolbar from "../../FilterToolbar";
import Table, { CellConfig } from "../Table/Table";
import { useNavigate } from "../../../utils/navigationUtils";
import { RefereeViewModel, useGetNgbRefereesQuery, useGetRefereesQuery } from "../../../store/serviceApi";

const HEADER_CELLS = ["name", "highest certification", "associated teams", "secondary NGB"];
const ADMIN_HEADER_CELLS = ["name", "highest certification", "associated teams", "associated NGBs"];

// sorts the levels by string length resulting in: ['head', 'snitch', 'assistant']
// this also happens to be the hierarchy order of the levels.
const sortByLength = (a: string, b: string): number => {
  return a.length - b.length;
};

const findHighestCert = (referee: RefereeViewModel): string => {
  const certHashMap: { [version: string]: string[] } = {};
  referee?.acquiredCertifications.forEach((cert) => {
    if (certHashMap[cert.version]) {
      certHashMap[cert.version].push(cert.level);
    } else {
      certHashMap[cert.version] = [cert.level];
    }
  });
  if (!Object.keys(certHashMap).length) return "Uncertified";

  // TODO: use correct comparison between cert levels
  const snitchAsFlag = (level?: string) => level === "snitch" ? "flag" : level;
  const highestTwenty = snitchAsFlag(certHashMap.twenty?.sort(sortByLength)[0]);
  const highestTwentyTwo = snitchAsFlag(certHashMap.twentytwo?.sort(sortByLength)[0]);
  const highestEighteen = snitchAsFlag(certHashMap.eighteen?.sort(sortByLength)[0]);

  // We will promote current certification first and foremost
  // We don't care if someone was a HR in 2020 if they only have AR in 2022
  if (highestTwentyTwo) {
    return `${capitalize(highestTwentyTwo)} ${getVersion("twentytwo")}`;
  }

  if (highestTwenty) {
    return `${capitalize(highestTwenty)} ${getVersion("twenty")}`;
  }

  return `${capitalize(highestEighteen)} ${getVersion("eighteen")}`;
};

type NewRefereeTableProps = {
  ngbId?: string;
  isHeightRestricted?: boolean;
};

const NewRefereeTable = (props: NewRefereeTableProps) => {
  const { ngbId, isHeightRestricted } = props;
  const navigate = useNavigate();
  const [filter, setFilter] = useState<string | undefined>(undefined);
  const [page, setPage] = useState(1);
  
  const { data: referees, isLoading } =
    ngbId === undefined
      ? useGetRefereesQuery({filter, page, pageSize: 25})
      : useGetNgbRefereesQuery({ngb: ngbId, filter, page, pageSize: 25})

  const headerCells = ngbId ? HEADER_CELLS : ADMIN_HEADER_CELLS;

  const handleRowClick = (id: string) => {
    navigate(`/referees/${id}`);
  };

  const handleClearSearch = () => handleSearch("");

  const handleSearch = (newValue: string) => {
    setFilter(newValue ?? undefined);
  };

  const handlePageSelect = (newPage: number) => {
    setPage(newPage);
  };

  const renderEmpty = () => {
    return <h2>No referees found.</h2>;
  };

  const rowConfig: CellConfig<RefereeViewModel>[] = [
    {
      cellRenderer: (item: RefereeViewModel) => item.name,
      dataKey: "name",
    },
    {
      cellRenderer: (item: RefereeViewModel) => {
        return findHighestCert(item);
      },
      dataKey: "certifications",
    },
    {
      cellRenderer: (item: RefereeViewModel) => {
        return [item.playingTeam, item.coachingTeam].filter(t => !!t).map((team) => team.name).join(", ");
      },
      dataKey: "teams",
    },
  ];

  if (ngbId) {
    rowConfig.push({
      cellRenderer: (item: RefereeViewModel) => {
        const secondary = item?.secondaryNgb;
        return secondary || "";
      },
      dataKey: "locations",
    });
  } else {
    rowConfig.push({
      cellRenderer: (item: RefereeViewModel) => {
        return [item.primaryNgb, item.secondaryNgb].filter(t => !!t).join(", ");
      },
      dataKey: "locations",
    });
  }

  return (
    <div className="w-full">
      <FilterToolbar
        currentPage={page}
        onClearSearch={handleClearSearch}
        total={referees?.metadata?.totalCount}
        onSearchInput={handleSearch}
        onPageSelect={handlePageSelect}
      />
      <Table
        items={referees?.items}
        isLoading={isLoading}
        headerCells={headerCells}
        rowConfig={rowConfig}
        onRowClick={handleRowClick}
        emptyRenderer={renderEmpty}
        isHeightRestricted={isHeightRestricted}
        getId={ref => ref.userId}
      />
    </div>
  );
};

export default NewRefereeTable;
