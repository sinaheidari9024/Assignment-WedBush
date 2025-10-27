export interface Message {
    id: number;
    message: string;
    createdAt: string;
}

export interface MessageResult {
    messages: Message[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
}