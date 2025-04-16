import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { useEffect, useMemo, useState } from "react";
import { useParams } from "react-router";

export function Room() {
    const params = useParams();
    const roomId = useMemo(
        () => {
            console.log("params", params);
            return params.roomId || crypto.randomUUID()
        },
        [params]
    );
    const [playerId, setPlayerId] = useState<string>(
        localStorage.getItem("playerId") || crypto.randomUUID())
    const [dealerCards, setDealerCards] = useState<string[]>([]);
    const [myCards, setMyCards] = useState<string[]>([]);
    const [playerCards, setPlayerCards] = useState<Record<string, string[]>>({});

    const [connection, setConnection] = useState<HubConnection>();

    useEffect(() => {
        console.log("roomId", roomId);
        if (!roomId) return;

        localStorage.setItem("playerId", playerId);

        const connection = new HubConnectionBuilder()
            .withUrl(
                `http://localhost:5106/blackjack?playerId=${playerId}&roomId=${roomId}`
            )
            .build();

        connection.on("Welcome", (msg) => console.log(msg));
        connection.on("CardDealt", (cPlayerId, card) =>{
            console.log("CardDealt", cPlayerId, card);
            if (cPlayerId === playerId) {
                setMyCards((prev) => [...prev, card]);
            } else if (cPlayerId === "dealer") {
                setDealerCards((prev) => [...prev, card]);
            }
            else {
                setPlayerCards((prev) => ({
                    ...prev,
                    [cPlayerId]: [...(prev[cPlayerId] || []), card],
                }));
            }

        });
        // connection.on("PlayerConnected", (pid) => console.log(`Jugador conectado: ${pid}`));
        // connection.on("ReceiveMove", (pid, move) => console.log(`${pid} hizo ${move}`));

        connection.start().then(() => {
            console.log("Conectado a la sala");
            setConnection(connection);
            if (!params.roomId) {
                // Move to /room/{roomId}
                window.history.replaceState({}, "", `/blackjack/room/${roomId}`);
            }
        });
    }, [roomId]);

    return(
        <div className="flex flex-col">
            <button onClick={() => connection?.invoke("StartGame", roomId)}>Start</button>
            <span>Cartas del dealer: {dealerCards.join(", ")}</span>
            <span>Mis cartas: {myCards.join(", ")}</span>
            <span>Jugadores:</span>
            {
                Object.entries(playerCards).map(([pid, cards]) => (
                    <span key={pid}>
                        {pid}: {cards.join(", ")}
                    </span>
                ))
            }
        </div>
    )
}
