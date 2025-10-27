import type { MessageResult, Message } from '../../types/Message';
import { MessageCard } from './MessageCard';
import { Pagination } from '../Pagination';

interface MessageListProps {
    messages: MessageResult;
    currentPage: number;
    expandedMessages: Set<number>;
    onToggleExpand: (id: number) => void;
    onPageChange: (page: number) => void;
    searchTerm: string;
}

export const MessageList = ({
    messages,
    currentPage,
    expandedMessages,
    onToggleExpand,
    onPageChange,
    searchTerm
}: MessageListProps) => {
    if (messages.messages.length === 0) {
        return (
            <div className="empty-state">
                {searchTerm ? "No messages match your search." : "No messages found."}
            </div>
        );
    }

    return (
        <div className="messages-container">
            <div className="messages-list">
                {messages.messages.map((msg: Message, index: number) => (
                    <MessageCard
                        key={msg.id}
                        message={msg}
                        index={index}
                        currentPage={currentPage}
                        pageSize={messages.pageSize}
                        isExpanded={expandedMessages.has(msg.id)}
                        onToggleExpand={onToggleExpand}
                    />
                ))}
            </div>

            {messages.totalPages > 1 && (
                <Pagination
                    currentPage={currentPage}
                    totalPages={messages.totalPages}
                    onPageChange={onPageChange}
                />
            )}
        </div>
    );
};