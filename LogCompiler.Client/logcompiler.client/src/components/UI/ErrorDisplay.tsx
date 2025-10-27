interface ErrorDisplayProps {
    error: string | null;
    onDismiss?: () => void;
}


export const ErrorDisplay = ({ error, onDismiss }: ErrorDisplayProps) => {
    if (!error) return null;

    return (
        <div className="error-container">
            <span className="error-text">{error}</span>
            {onDismiss && (
                <button onClick={onDismiss} className="error-dismiss">×</button>
            )}
        </div>
    );
};
