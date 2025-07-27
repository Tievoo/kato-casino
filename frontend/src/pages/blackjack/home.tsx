import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router";
import { Room } from "../../types/blackjack";

export function Home() {
    const [rooms, setRooms] = useState<Room[]>([]);
    const navigate = useNavigate();

    async function fetchRooms() {
        const response = await fetch("http://localhost:5106/blackjack/rooms");
        const data = await response.json();
        setRooms(data);
    }

    async function createRoom() {
        const response = await fetch("http://localhost:5106/blackjack/rooms", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            }
        });
        if (response.ok) {
            const newRoom: { roomId: string } = await response.json();
            navigate(`/blackjack/room/${newRoom.roomId}`);
        } else {
            console.error("Error creating room");
        }
    }

    useEffect(() => {
        fetchRooms();
    }, []);

    return (
        <div className="flex flex-col items-center justify-center h-screen">
            <h1 className="text-4xl font-bold mb-4">Bienvenido a Blackjack</h1>
            <p className="text-lg mb-8"></p>
            {
                rooms.length > 0 ? (
                    <ul className="list-disc list-inside">
                        {rooms.map((room) => (
                            <div className="mb-3">
                                <li key={room.id} className="mb-2">
                                    {room.id} - {room.players} jugadores - Estado: {room.status}
                                </li>
                                <Link to={`/blackjack/room/${room.id}`} className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600 transition duration-200">
                                    Unirse a la sala
                                </Link>
                            </div>
                        ))}
                    </ul>
                ) : (
                    <p>No hay salas disponibles.</p>
                )
            }
            <button onClick={createRoom} className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600 transition duration-200">
                Crear Sala
            </button>
        </div>
    );
}
