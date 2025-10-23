import { useEffect, useState } from "react";
import type { OriginalMessage } from "./types/OriginalMessage";
import "./App.css";

function App() {
    const [messages, setMessages] = useState<OriginalMessage[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [searchTerm, setSearchTerm] = useState("");
    const [expandedMessages, setExpandedMessages] = useState<Set<number>>(new Set());

    useEffect(() => {
        const fetchMessages = async () => {
            try {
                setError(null);
                const response = await fetch("/api/CompileFile");
                if (!response.ok) {
                    throw new Error(`Error: ${response.statusText}`);
                }
                const data: OriginalMessage[] = await response.json();
                setMessages(data);
            } catch (error) {
                console.error("Failed to fetch:", error);
                setError("Failed to load messages. Please try again.");
            } finally {
                setLoading(false);
            }
        };

        fetchMessages();
    }, []);

    const toggleExpand = (messageId: number) => {
        const newExpanded = new Set(expandedMessages);
        if (newExpanded.has(messageId)) {
            newExpanded.delete(messageId);
        } else {
            newExpanded.add(messageId);
        }
        setExpandedMessages(newExpanded);
    };

    // Filter messages based on search term
    const filteredMessages = messages.filter(msg =>
        msg.message.toLowerCase().includes(searchTerm.toLowerCase())
    );

    if (loading) {
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
                                <span className="info-value">{messages.length}</span>
                            </div>
                            <div className="info-item">
                                <span className="info-label">Showing:</span>
                                <span className="info-value">{filteredMessages.length}</span>
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
                <input
                    type="text"
                    placeholder="Search messages..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="search-input"
                />
                {searchTerm && (
                    <button
                        onClick={() => setSearchTerm("")}
                        className="clear-button"
                    >
                        Clear
                    </button>
                )}
            </div>

            {/* Error Display */}
            {error && (
                <div className="error-container">
                    <span className="error-text">{error}</span>
                </div>
            )}

            {/* Messages List */}
            <div className="messages-container">
                {filteredMessages.length === 0 ? (
                    <div className="empty-state">
                        {searchTerm ? "No messages match your search." : "No messages found."}
                    </div>
                ) : (
                    <div className="messages-list">
                        {filteredMessages.map((msg, index) => (
                            <div key={msg.id} className="message-card">
                                <div className="message-header">
                                    <span className="message-index">#{index + 1}</span>
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
                )}
            </div>

            {/* Footer */}
            <footer className="app-footer">
                <div className="footer-content">
                    <span>Log Viewer • {filteredMessages.length} messages displayed</span>
                </div>
            </footer>
        </div>
    );
}

export default App;