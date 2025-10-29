import { useEffect, useState, useCallback } from "react";
import {
    MessageList,
    SearchBar,
    AppHeader,
    AppFooter,
    LoadingSpinner,
    ErrorDisplay,
    ActionButton
} from "./components";
import { useMessages, useDebounce } from "./hooks";
import { apiService } from "./services";
import "./css/App.css";

function App() {
    const { messages, loading, error, fetchMessages, setError } = useMessages();
    const [searchTerm, setSearchTerm] = useState("");
    const [currentPage, setCurrentPage] = useState(1);
    const [saveLoading, setSaveLoading] = useState(false);
    const [expandedMessages, setExpandedMessages] = useState<Set<number>>(new Set());
    const [isSearching, setIsSearching] = useState(false);

    const performSearch = useCallback(() => {
        if (searchTerm.trim() !== "") {
            setIsSearching(true);
            setCurrentPage(1);
            fetchMessages(1, searchTerm.trim());
        }
    }, [searchTerm, fetchMessages]);

    useDebounce(() => {
        if (searchTerm.trim() !== "") {
            performSearch();
        }
    }, 500, [searchTerm]);

    useEffect(() => {
        if (searchTerm.trim() === "" && isSearching) {
            setIsSearching(false);
            setCurrentPage(1);
            fetchMessages(1, "");
        }
    }, [searchTerm, isSearching, fetchMessages]);

    useEffect(() => {
        fetchMessages(1, "");
    }, [fetchMessages]);

    useEffect(() => {
        if (currentPage !== 1 || !isSearching) {
            fetchMessages(currentPage, searchTerm.trim());
        }
    }, [currentPage]); 

    const handleSaveMessages = async () => {
        try {
            setSaveLoading(true);
            setError(null);
            await apiService.saveMessages();
            await fetchMessages(currentPage, searchTerm);
        } catch (error) {
            setError(error instanceof Error ? error.message : "Failed to save messages.");
        } finally {
            setSaveLoading(false);
        }
    };

    const handlePageChange = (page: number) => {
        setCurrentPage(page);
    };

    const handleSearchChange = (term: string) => {
        setSearchTerm(term);
    };

    const handleClearSearch = () => {
        setSearchTerm("");
        setCurrentPage(1);
        setIsSearching(false);
        fetchMessages(1, "");
    };

    const toggleExpand = (messageId: number) => {
        const newExpanded = new Set(expandedMessages);
        if (newExpanded.has(messageId)) {
            newExpanded.delete(messageId);
        } else {
            newExpanded.add(messageId);
        }
        setExpandedMessages(newExpanded);
    };

    if (loading) return <LoadingSpinner />;

    return (
        <div className="app-container">
            <AppHeader
                totalCount={messages.totalCount}
                displayedCount={messages.messages.length}
            />

            <div className="actions-container">
                <ActionButton
                    onClick={handleSaveMessages}
                    loading={saveLoading}
                    className="save-button"
                >
                    Read file
                </ActionButton>
            </div>

            <SearchBar
                searchTerm={searchTerm}
                onSearchChange={handleSearchChange}
                onClearSearch={handleClearSearch} // Add clear search functionality
            />

            <ErrorDisplay
                error={error}
                onDismiss={() => setError(null)}
            />

            <MessageList
                messages={messages}
                currentPage={currentPage}
                expandedMessages={expandedMessages}
                onToggleExpand={toggleExpand}
                onPageChange={handlePageChange}
                searchTerm={searchTerm}
            />

            <AppFooter
                displayedCount={messages.messages.length}
                currentPage={currentPage}
                totalPages={messages.totalPages}
            />
        </div>
    );
}

export default App;