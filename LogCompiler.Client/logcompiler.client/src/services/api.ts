import type { MessageResult } from '../types/Message';

class ApiService {
    private baseUrl = '/api/CompileFile';

    async fetchMessages(page: number, search: string, pageSize = 20): Promise<MessageResult> {
        const params = new URLSearchParams({
            page: page.toString(),
            pageSize: pageSize.toString(),
            search: search
        });

        const response = await fetch(`${this.baseUrl}?${params}`);
        if (!response.ok) {
            throw new Error(`Error: ${response.statusText}`);
        }
        return response.json();
    }

    async saveMessages(): Promise<boolean> {
        const response = await fetch(this.baseUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            }
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.error || `HTTP error: ${response.status}`);
        }

        return response.json();
    }
}

export const apiService = new ApiService();