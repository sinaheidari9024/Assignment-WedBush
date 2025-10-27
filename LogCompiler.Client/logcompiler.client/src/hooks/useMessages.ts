import { useState, useCallback } from 'react';
import type { MessageResult } from '../types/Message';
import { apiService } from '../services/api';

export const useMessages = () => {
    const [messages, setMessages] = useState<MessageResult>({
        messages: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: 20,
        totalPages: 0
    });
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchMessages = useCallback(async (page: number, search: string) => {
        try {
            setError(null);
            setLoading(true);
            const data = await apiService.fetchMessages(page, search);
            setMessages(data);
        } catch (error) {
            console.error("Failed to fetch:", error);
            setError("Failed to load messages. Please try again.");
        } finally {
            setLoading(false);
        }
    }, []);

    return {
        messages,
        loading,
        error,
        fetchMessages,
        setError
    };
};