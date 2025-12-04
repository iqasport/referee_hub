import React from "react";

const Search = () => {
  return (
    <div className="relative w-full max-w-md">
      <input
        type="text"
        placeholder="Search tournaments..."
        className="w-full px-4 py-2 pl-10 border border-gray-800 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
      />
    </div>
  );
};

export default Search;
