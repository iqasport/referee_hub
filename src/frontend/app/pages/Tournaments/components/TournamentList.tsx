import React from "react";
import { faArrowLeft, faArrowRight, faEllipsisH } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Pagination from "rc-pagination";
import TournamentSection from "./TournamentsSection";
import { TournamentData } from "./TournamentsSection";

const DEFAULT_PAGE_SIZE = 20;

interface TournamentListProps {
  isLoading: boolean;
  isError: boolean;
  privateTournaments: TournamentData[];
  publicTournaments: TournamentData[];
  currentPage: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}

export const TournamentList: React.FC<TournamentListProps> = ({
  isLoading,
  isError,
  privateTournaments,
  publicTournaments,
  currentPage,
  totalCount,
  onPageChange,
}) => {
  if (isLoading) {
    return <div className="tournament-loading">Loading tournaments...</div>;
  }

  if (isError) {
    return <div className="tournament-error">Error loading tournaments. Please try again.</div>;
  }

  const hasNoTournaments = privateTournaments.length === 0 && publicTournaments.length === 0;

  return (
    <div className="tournament-page-container">
      {privateTournaments.length > 0 && (
        <TournamentSection tournaments={privateTournaments} visibility="private" layout="carousel" />
      )}

      {publicTournaments.length > 0 && (
        <>
          <TournamentSection tournaments={publicTournaments} visibility="public" layout="grid" />
          {totalCount > DEFAULT_PAGE_SIZE && (
            <div className="flex justify-center py-4">
              <Pagination
                current={currentPage}
                total={totalCount}
                onChange={onPageChange}
                pageSize={DEFAULT_PAGE_SIZE}
                prevIcon={<FontAwesomeIcon icon={faArrowLeft} />}
                nextIcon={<FontAwesomeIcon icon={faArrowRight} />}
                className="pagination"
                hideOnSinglePage={true}
                jumpPrevIcon={<FontAwesomeIcon icon={faEllipsisH} />}
                jumpNextIcon={<FontAwesomeIcon icon={faEllipsisH} />}
                showTitle={false}
              />
            </div>
          )}
        </>
      )}

      {hasNoTournaments && <div className="tournament-empty">No tournaments available.</div>}
    </div>
  );
};
