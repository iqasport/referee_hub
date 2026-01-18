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
    <div className="flex gap-2 w-full max-w-2xl items-center">
      <div className="relative flex-1">
        <input
          type="text"
          placeholder="Search tournaments..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          onKeyPress={handleKeyPress}
          className="w-full px-4 py-2 pl-10 border border-gray-800 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>
      <select
        value={selectedType}
        onChange={(e) => onTypeFilter(e.target.value)}
        className="px-4 py-2 border border-gray-800 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
      >
        {TOURNAMENT_TYPES.map((type) => (
          <option key={type.value} value={type.value}>
            {type.label}
          </option>
        ))}
      </select>
      <button
        onClick={handleSearch}
        className="px-4 py-2 bg-blue-600 text-black rounded-lg font-semibold hover:bg-blue-700 transition-colors"
      >
        Search
      </button>
    </div>
  );
};

export default Search;
