interface AppHeaderProps {
    totalCount: number;
    displayedCount: number;
}

export const AppHeader = ({ totalCount, displayedCount }: AppHeaderProps) => (
    <header className="app-header">
        <div className="header-content">
            <div className="header-left">
                <h1 className="app-title">Log Viewer System</h1>
                <div className="log-info">
                    <div className="info-item">
                        <span className="info-label">Total Messages:</span>
                        <span className="info-value">{totalCount}</span>
                    </div>
                    <div className="info-item">
                        <span className="info-label">Showing:</span>
                        <span className="info-value">{displayedCount}</span>
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
);