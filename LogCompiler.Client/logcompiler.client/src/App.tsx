import { useEffect, useState } from "react";
import type { OriginalMessage } from "./types/OriginalMessage";


function App() {
    const [messages, setMessages] = useState<OriginalMessage[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchMessages = async () => {
            try {
                const response = await fetch("/api/CompileFile");
                if (!response.ok) {
                    throw new Error(`Error: ${response.statusText}`);
                }
                const data: OriginalMessage[] = await response.json();
                setMessages(data);
            } catch (error) {
                console.error("Failed to fetch:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchMessages();
    }, []);

    if (loading) {
        return <p style={{ textAlign: "center" }}>Loading...</p>;
    }

    return (
        <div style={{ padding: "2rem" }}>
            <h1>Original Messages</h1>
            {messages.length === 0 ? (
                <p>No messages found.</p>
            ) : (
                <ul>
                    {messages.map((msg) => (
                        <li key={msg.id}>
                            <strong>{msg.message}</strong>
                            <br />
                            <small>{new Date(msg.createdAt).toLocaleString()}</small>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}

export default App;
