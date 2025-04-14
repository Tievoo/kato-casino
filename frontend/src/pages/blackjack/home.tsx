import { useEffect, useState } from "react";
import { Link } from "react-router";

export function Home() {
    const [rooms, setRooms] = useState([]);

    async function fetchRooms() {
        const response = await fetch("http://localhost:5106/blackjack/rooms");
        const data = await response.json();
        setRooms(data);
    }

    useEffect(() => {
        fetchRooms();
    }, []);

    return (
        <div className="flex flex-col items-center justify-center h-screen">
            <h1 className="text-4xl font-bold mb-4">Bienvenido a Blackjack</h1>
            <p className="text-lg mb-8"></p>
            {/* {
                rooms.length > 0 ? (
                    <ul className="list-disc list-inside">
                        {rooms.map((room) => (
                            <li key={room.id} className="mb-2">
                                {room.name} - {room.players} jugadores
                            </li>
                        ))}
                    </ul>
                ) : (
                    <p>No hay salas disponibles.</p>
                )}
            } */}
            <Link to="/blackjack/room" className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600 transition duration-200">
                Crear Sala
            </Link>
        </div>
    );
}
