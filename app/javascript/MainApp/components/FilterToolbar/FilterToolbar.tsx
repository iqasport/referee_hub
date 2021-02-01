import { faArrowLeft, faArrowRight, faEllipsisH } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Pagination from "rc-pagination";
import React, { useState } from "react";

interface FilterToolbarProps {
  onPageSelect: (page: number) => void;
  currentPage: number;
  total: number;
  onSearchInput: (value: string) => void;
  onClearSearch: () => void;
}

const FilterToolbar = (props: FilterToolbarProps) => {
  const { onSearchInput, onClearSearch, onPageSelect, currentPage, total } = props;
  const [searchInput, setSearchInput] = useState("");

  const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = event.target.value;

    setSearchInput(newValue);
    onSearchInput(newValue);
  };

  const handleClearSearch = () => {
    setSearchInput("");
    onClearSearch();
  };

  return (
    <div className="w-full flex my-4 justify-between">
      <div className="w-1/3 flex">
        <input
          className="form-input mr-4"
          name="search"
          onChange={handleSearch}
          placeholder="Search by name"
          value={searchInput}
        />
        {searchInput.length > 0 && (
          <button className="text-blue-darker" onClick={handleClearSearch}>
            clear search
          </button>
        )}
      </div>
      <div className="w-1/3">
        <Pagination
          current={currentPage}
          total={total}
          onChange={onPageSelect}
          pageSize={25}
          prevIcon={<FontAwesomeIcon icon={faArrowLeft} />}
          nextIcon={<FontAwesomeIcon icon={faArrowRight} />}
          className="pagination"
          hideOnSinglePage={true}
          jumpPrevIcon={<FontAwesomeIcon icon={faEllipsisH} />}
          jumpNextIcon={<FontAwesomeIcon icon={faEllipsisH} />}
        />
      </div>
    </div>
  );
};

export default FilterToolbar;
