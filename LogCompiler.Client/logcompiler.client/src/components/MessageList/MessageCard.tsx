import type { Message } from '../../types/Message';

interface MessageCardProps {
    message: Message;
    index: number;
    currentPage: number;
    pageSize: number;
    isExpanded: boolean;
    onToggleExpand: (id: number) => void;
}

export const MessageCard = ({
    message,
    index,
    currentPage,
    pageSize,
    isExpanded,
    onToggleExpand
}: MessageCardProps) => {
    const shouldTruncate = message.message.length > 500;
    const displayText = shouldTruncate && !isExpanded
        ? `${message.message.substring(0, 500)}...`
        : message.message;

    return (
        <div className="message-card">
            <div className="message-header">
                <span className="message-index">
                    #{(currentPage - 1) * pageSize + index + 1}
                </span>
                <span className="timestamp">
                    {new Date(message.createdAt).toLocaleString()}
                </span>
            </div>
            <div className="message-content">
                {displayText}
                {shouldTruncate && (
                    <button
                        onClick={() => onToggleExpand(message.id)}
                        className="expand-button"
                    >
                        {isExpanded ? 'Show Less' : 'Show More'}
                    </button>
                )}
            </div>
        </div>
    );
};