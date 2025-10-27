interface SearchBarProps {
    searchTerm: string;
    onSearchChange: (term: string) => void;
}

export const SearchBar = ({ searchTerm, onSearchChange }: SearchBarProps) => {
    const handleClearSearch = () => {
        onSearchChange("");
    };

    return (
        <div className="search-container">
            <div className="search-input-wrapper">
                <input
                    type="text"
                    placeholder="Search messages... (search starts after 500ms)"
                    value={searchTerm}
                    onChange={(e) => onSearchChange(e.target.value)}
                    className="search-input"
                />
                {searchTerm && (
                    <button
                        onClick={handleClearSearch}
                        className="clear-button"
                    >
                        Clear
                    </button>
                )}
            </div>
        </div>
    );
};