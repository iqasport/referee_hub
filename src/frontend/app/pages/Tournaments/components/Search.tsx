import React, { useState } from "react";

interface SearchProps {
  onSearch: (term: string) => void;
  onTypeFilter: (type: string) => void;
  selectedType: string;
}

const TOURNAMENT_TYPES = [
  { value: "", label: "All Types" },
  { value: "Club", label: "Club" },
  { value: "National", label: "National" },
  { value: "Youth", label: "Youth" },
  { value: "Fantasy", label: "Fantasy" },
];

const Search: React.FC<SearchProps> = ({ onSearch, onTypeFilter, selectedType }) => {
  const [searchTerm, setSearchTerm] = useState("");

  const handleSearch = () => {
    onSearch(searchTerm);
  };

  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      handleSearch();
    }
  };

  return (
    <div className="tournament-search">
      <div className="tournament-search-input-wrapper">
        <input
          type="text"
          placeholder="Search tournaments..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          onKeyPress={handleKeyPress}
          className="tournament-search-input"
        />
      </div>
      <select
        value={selectedType}
        onChange={(e) => onTypeFilter(e.target.value)}
        className="tournament-search-select"
      >
        {TOURNAMENT_TYPES.map((type) => (
          <option key={type.value} value={type.value}>
            {type.label}
          </option>
        ))}
      </select>
      <button onClick={handleSearch} className="btn btn-outline">
        Search
      </button>
    </div>
  );
};

export default Search;
