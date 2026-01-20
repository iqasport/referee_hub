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
    <div style={{ display: "flex", gap: "0.5rem", width: "100%", alignItems: "center" }}>
      <div style={{ position: "relative", flex: 1 }}>
        <input
          type="text"
          placeholder="Search tournaments..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          onKeyPress={handleKeyPress}
          style={{
            width: "100%",
            padding: "0.5rem 1rem",
            border: "1px solid #e5e7eb",
            borderRadius: "0.5rem",
            fontSize: "0.875rem",
            outline: "none",
          }}
          onFocus={(e) => (e.currentTarget.style.border = "1px solid #16a34a")}
          onBlur={(e) => (e.currentTarget.style.border = "1px solid #e5e7eb")}
        />
      </div>
      <select
        value={selectedType}
        onChange={(e) => onTypeFilter(e.target.value)}
        style={{
          padding: "0.5rem 1rem",
          border: "1px solid #e5e7eb",
          borderRadius: "0.5rem",
          backgroundColor: "#fff",
          fontSize: "0.875rem",
          cursor: "pointer",
          outline: "none",
        }}
        onFocus={(e) => (e.currentTarget.style.border = "1px solid #16a34a")}
        onBlur={(e) => (e.currentTarget.style.border = "1px solid #e5e7eb")}
      >
        {TOURNAMENT_TYPES.map((type) => (
          <option key={type.value} value={type.value}>
            {type.label}
          </option>
        ))}
      </select>
      <button
        onClick={handleSearch}
        style={{
          backgroundColor: "#fff",
          border: "1px solid #16a34a",
          color: "#16a34a",
          padding: "0.5rem 1rem",
          borderRadius: "0.5rem",
          fontWeight: "600",
          cursor: "pointer",
          transition: "background-color 0.2s",
          fontSize: "0.875rem",
        }}
        onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = "#f0fdf4")}
        onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = "#fff")}
      >
        Search
      </button>
    </div>
  );
};

export default Search;
