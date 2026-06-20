import { useEffect, useState } from "react";

type Todo = {
    id: number;
    todo: string;
    completed: boolean;
    userId: number;
};

type TodosResponse = {
    todos: Todo[];
};

export default function ExamplePage() {
    const [todos, setTodos] = useState<Todo[]>([]);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const controller = new AbortController();

        async function loadTodos() {
            try {
                setError(null);

                const response = await fetch("https://dummyjson.com/todos", {
                    signal: controller.signal,
                });

                if (!response.ok) {
                    throw new Error(`Request failed with status ${response.status}`);
                }

                const data = (await response.json()) as TodosResponse;
                setTodos(data.todos ?? []);
            } catch (requestError) {
                if (requestError instanceof DOMException && requestError.name === "AbortError") {
                    return;
                }

                setError(requestError instanceof Error ? requestError.message : "Failed to load todos");
            }
        }

        void loadTodos();

        return () => {
            controller.abort();

        }
    }, []);

    return (
        <section>
            <h1>Example Page</h1>
            {error ? <p role="alert">{error}</p> : null}
            <pre>{JSON.stringify(todos, null, 2)}</pre>
        </section>
    );
}
