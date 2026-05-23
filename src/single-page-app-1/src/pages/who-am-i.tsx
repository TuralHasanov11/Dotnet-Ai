import { WhoAmIService } from "@/generated";
import { useEffect, useState } from "react";

export default function WhoAmIPage() {
    const [result, setResult] = useState<Awaited<ReturnType<typeof WhoAmIService.WhoAmI>> | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let active = true;
        WhoAmIService.WhoAmI()
            .then((data) => {
                console.log(data)
                if (active) setResult(data);
            })
            .catch((cause) => {
                if (active) setError(cause instanceof Error ? cause.message : String(cause));
            });

        return () => {
            active = false;
        };
    }, []);

    return (
        <div>
            <h1>Who am I?</h1>
            {error && <p>{error}</p>}
            {!result && !error && <p>Loading WhoAmI...</p>}
            {result && <pre>{JSON.stringify(result, null, 2)}</pre>}
        </div>
    );
}