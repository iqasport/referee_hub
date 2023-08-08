import { capitalize } from "lodash";
import React, { useState } from "react";
import { getVersion } from "../../../utils/certUtils";
import FilterToolbar from "../../FilterToolbar";
import Table, { CellConfig } from "../Table/Table";
import { useNavigate } from "../../../utils/navigationUtils";
import { RefereeViewModel, useGetNgbRefereesQuery } from "../../../store/serviceApi";

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

  const highestTwenty = certHashMap.twenty?.sort(sortByLength)[0];
  const highestTwentyTwo = certHashMap.twentytwo?.sort(sortByLength)[0];
  const highestEighteen = certHashMap.eighteen?.sort(sortByLength)[0];

  if (highestTwentyTwo?.length > 0 && highestTwentyTwo?.length < highestTwenty?.length) {
    return `${capitalize(highestTwentyTwo)} ${getVersion("twentytwo")}`;
  }

  if (highestTwenty?.length > 0 && highestTwenty?.length < highestEighteen?.length) {
    return `${capitalize(highestTwenty)} ${getVersion("twenty")}`;
  }

  return `${capitalize(highestEighteen)} ${getVersion("eighteen")}`;
};

type NewRefereeTableProps = {
  ngbId?: string;
  isHeightRestricted?: boolean;
  refereeCount: number;
};

const NewRefereeTable = (props: NewRefereeTableProps) => {
  const { ngbId, isHeightRestricted } = props;
  const navigate = useNavigate();
  const [filter, setFilter] = useState<string | undefined>(undefined);
  const [page, setPage] = useState(1);
  
  const { data: referees, isLoading } = useGetNgbRefereesQuery({ngb: ngbId, filter, page, pageSize: 25})

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
        return "TODO"; // we need to get teams from referees locations then map name to id OR fetch it on the backend
        //return [item.playingTeam, item.coachingTeam].filter(t => !!t).map((team) => team.name).join(", ");
      },
      dataKey: "teams",
    },
  ];

  if (ngbId) {
    rowConfig.push({
      cellRenderer: (item: RefereeViewModel) => {
        const secondary = item?.secondaryNgb;
        const secondaryName = "TODO";
          // secondary.length &&
          // item?.ngbs.find((ngb) => {
          //   return ngb.id === secondary[0].nationalGoverningBodyId.toString();
          // })?.name;
        return secondaryName || "N/A";
      },
      dataKey: "locations",
    });
  } else {
    rowConfig.push({
      cellRenderer: (item: RefereeViewModel) => {
        return "TODO"; //item?.ngbs.map((location) => location.name).join(", ");
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
