import type { Route } from "./+types/home";
import { Welcome } from "../welcome/welcome";

export function meta({}: Route.MetaArgs) {
  return [
    { title: "Example - New React Router App" },
    { name: "description", content: "Example" },
  ];
}

export async function loader({ params }: Route.LoaderArgs) {
  const response = await fetch("https://jsonplaceholder.typicode.com/todos");
    const todos = await response.json();
  return todos;
}

export default function Example({loaderData} : Route.ComponentProps) {
     

  return (
    <div>
        <code>{JSON.stringify(loaderData, null, 2)}</code>
    </div>
  );
}
