interface ActionButtonProps {
    onClick: () => void;
    loading: boolean;
    children: React.ReactNode;
    className?: string;
}

export const ActionButton = ({ onClick, loading, children, className = '' }: ActionButtonProps) => (
    <button
        onClick={onClick}
        disabled={loading}
        className={`action-button ${className}`}
    >
        {loading ? 'Saving...' : children}
    </button>
);