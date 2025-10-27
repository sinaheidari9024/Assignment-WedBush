import { useEffect, useState } from "react";
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

    useDebounce(() => {
        if (searchTerm !== "" || currentPage !== 1) {
            setCurrentPage(1);
            fetchMessages(1, searchTerm);
        }
    }, 500, [searchTerm]);

    useEffect(() => {
        fetchMessages(1, "");
    }, [fetchMessages]);

    const handleSaveMessages = async () => {
        try {
            setSaveLoading(true);
            setError(null);
            await apiService.saveMessages();
            await fetchMessages(1, searchTerm);
        } catch (error) {
            setError(error instanceof Error ? error.message : "Failed to save messages.");
        } finally {
            setSaveLoading(false);
        }
    };

    const handlePageChange = (page: number) => {
        setCurrentPage(page);
        fetchMessages(page, searchTerm);
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
                    Save Sample Messages
                </ActionButton>
            </div>

            <SearchBar
                searchTerm={searchTerm}
                onSearchChange={setSearchTerm}
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