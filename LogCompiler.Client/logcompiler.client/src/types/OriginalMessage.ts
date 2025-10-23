
export interface OriginalMessage {
    id: number;
    message: string;
    createdAt: string;
}

export interface MessageResult {
    messages: OriginalMessage[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
}