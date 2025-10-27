interface AppFooterProps {
    displayedCount: number;
    currentPage: number;
    totalPages: number;
}

export const AppFooter = ({ displayedCount, currentPage, totalPages }: AppFooterProps) => (
    <footer className="app-footer">
        <div className="footer-content">
            <span>
                Log Viewer • {displayedCount} messages displayed •
                Page {currentPage} of {totalPages}
            </span>
        </div>
    </footer>
);