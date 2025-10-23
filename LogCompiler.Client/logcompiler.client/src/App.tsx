import { useEffect, useState, useCallback, useRef } from "react";
import type { MessageResult } from "./types/OriginalMessage";
import "./App.css";

function App() {
    const [messages, setMessages] = useState<MessageResult>({
        messages: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: 20,
        totalPages: 0
    });
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [searchTerm, setSearchTerm] = useState("");
    const [expandedMessages, setExpandedMessages] = useState<Set<number>>(new Set());
    const [currentPage, setCurrentPage] = useState(1);
    const [saveLoading, setSaveLoading] = useState(false);
    const [initialLoad, setInitialLoad] = useState(false);

    const searchTimeoutRef = useRef<number | null>(null);

    const fetchMessages = useCallback(async (page: number, search: string) => {
        try {
            setError(null);
            setLoading(true);

            const params = new URLSearchParams({
                page: page.toString(),
                pageSize: "20",
                search: search
            });

            const response = await fetch(`/api/CompileFile?${params}`);
            if (!response.ok) {
                throw new Error(`Error: ${response.statusText}`);
            }
            const data: MessageResult = await response.json();
            setMessages(data);
            setCurrentPage(page);
        } catch (error) {
            console.error("Failed to fetch:", error);
            setError("Failed to load messages. Please try again.");
        } finally {
            setLoading(false);
            setInitialLoad(false);
        }
    }, []);

    const handleSaveMessages = async () => {
        try {
            setSaveLoading(true);
            setError(null);

            const response = await fetch("/api/CompileFile", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                }
            });

            if (!response.ok) {
                throw new Error("Failed to save messages");
            }

            const result = await response.json();

            if (result) {
                await fetchMessages(1, searchTerm);
            } else {
                throw new Error("Save operation failed");
            }
        } catch (error) {
            console.error("Failed to save messages:", error);
            setError("Failed to save messages. Please try again.");
        } finally {
            setSaveLoading(false);
        }
    };

    // On initial load, automatically save sample messages then fetch
    useEffect(() => {
        const initializeData = async () => {
            await handleSaveMessages();
        };

        if (initialLoad) {
            initializeData();
        }
    }, [initialLoad]);

    const handleSearchInput = (term: string) => {
        setSearchTerm(term);

        // Clear existing timeout
        if (searchTimeoutRef.current) {
            clearTimeout(searchTimeoutRef.current);
        }

        // Set new timeout for 2 seconds
        searchTimeoutRef.current = setTimeout(() => {
            setCurrentPage(1);
            fetchMessages(1, term);
        }, 2000);
    };

    const handleClearSearch = () => {
        setSearchTerm("");
        // Clear timeout when clearing search
        if (searchTimeoutRef.current) {
            clearTimeout(searchTimeoutRef.current);
        }
        setCurrentPage(1);
        fetchMessages(1, "");
    };

    const handlePageChange = (page: number) => {
        // Clear any pending search timeout when changing pages
        if (searchTimeoutRef.current) {
            clearTimeout(searchTimeoutRef.current);
        }
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

    // Cleanup timeout on component unmount
    useEffect(() => {
        return () => {
            if (searchTimeoutRef.current) {
                clearTimeout(searchTimeoutRef.current);
            }
        };
    }, []);

    if (loading && initialLoad) {
        return (
            <div className="loading-container">
                <div className="loading-spinner"></div>
                <p className="loading-text">Loading messages...</p>
            </div>
        );
    }

    return (
        <div className="app-container">
            {/* Header with Logo */}
            <header className="app-header">
                <div className="header-content">
                    <div className="header-left">
                        <h1 className="app-title">System Log Viewer</h1>
                        <div className="log-info">
                            <div className="info-item">
                                <span className="info-label">Total Messages:</span>
                                <span className="info-value">{messages.totalCount}</span>
                            </div>
                            <div className="info-item">
                                <span className="info-label">Showing:</span>
                                <span className="info-value">{messages.messages.length}</span>
                            </div>
                            <div className="info-item">
                                <span className="info-label">Last Updated:</span>
                                <span className="info-value">{new Date().toLocaleString()}</span>
                            </div>
                        </div>
                    </div>
                    <div className="logo-container">
                        <img
                            src="/logo.svg"
                            alt="Company Logo"
                            className="logo"
                        />
                    </div>
                </div>
            </header>

            {/* Search Bar */}
            <div className="search-container">
                <div className="search-input-wrapper">
                    <input
                        type="text"
                        placeholder="Search messages... (search starts after 2 seconds)"
                        value={searchTerm}
                        onChange={(e) => handleSearchInput(e.target.value)}
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
                <button
                    onClick={handleSaveMessages}
                    disabled={saveLoading}
                    className="save-button"
                >
                    {saveLoading ? "Saving..." : "Save Messages"}
                </button>
            </div>

            {/* Error Display */}
            {error && (
                <div className="error-container">
                    <span className="error-text">{error}</span>
                </div>
            )}

            {/* Messages List */}
            <div className="messages-container">
                {messages.messages.length === 0 ? (
                    <div className="empty-state">
                        {searchTerm ? "No messages match your search." : "No messages found."}
                    </div>
                ) : (
                    <>
                        <div className="messages-list">
                            {messages.messages.map((msg, index) => (
                                <div key={msg.id} className="message-card">
                                    <div className="message-header">
                                        <span className="message-index">
                                            #{(currentPage - 1) * messages.pageSize + index + 1}
                                        </span>
                                        <span className="timestamp">
                                            {new Date(msg.createdAt).toLocaleString()}
                                        </span>
                                    </div>
                                    <div className="message-content">
                                        {msg.message.length > 500 && !expandedMessages.has(msg.id)
                                            ? `${msg.message.substring(0, 500)}...`
                                            : msg.message
                                        }
                                        {msg.message.length > 500 && (
                                            <button
                                                onClick={() => toggleExpand(msg.id)}
                                                className="expand-button"
                                            >
                                                {expandedMessages.has(msg.id) ? 'Show Less' : 'Show More'}
                                            </button>
                                        )}
                                    </div>
                                </div>
                            ))}
                        </div>

                        {/* Pagination */}
                        {messages.totalPages > 1 && (
                            <div className="pagination">
                                <button
                                    onClick={() => handlePageChange(currentPage - 1)}
                                    disabled={currentPage === 1}
                                    className="pagination-button prev"
                                >
                                    Previous
                                </button>

                                <div className="pagination-pages">
                                        {Array.from({ length: Math.min(5, messages.totalPages) }, (_, i) => {
                                            let pageNum: number; 
                                            if (messages.totalPages <= 5) {
                                                pageNum = i + 1;
                                            } else if (currentPage <= 3) {
                                                pageNum = i + 1;
                                            } else if (currentPage >= messages.totalPages - 2) {
                                                pageNum = messages.totalPages - 4 + i;
                                            } else {
                                                pageNum = currentPage - 2 + i;
                                            }

                                            return (
                                                <button
                                                    key={pageNum}
                                                    onClick={() => handlePageChange(pageNum)}
                                                    className={`pagination-page ${currentPage === pageNum ? 'active' : ''}`}
                                                >
                                                    {pageNum}
                                                </button>
                                            );
                                        })}
                                </div>

                                <button
                                    onClick={() => handlePageChange(currentPage + 1)}
                                    disabled={currentPage === messages.totalPages}
                                    className="pagination-button next"
                                >
                                    Next
                                </button>
                            </div>
                        )}
                    </>
                )}
            </div>

            {/* Footer */}
            <footer className="app-footer">
                <div className="footer-content">
                    <span>
                        Log Viewer • {messages.messages.length} messages displayed •
                        Page {currentPage} of {messages.totalPages}
                    </span>
                </div>
            </footer>
        </div>
    );
}

export default App;