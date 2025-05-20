import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { useEffect, useMemo, useState } from "react";
import { useParams } from "react-router";

type Status = "playing" | "stand" | "bust" | "waiting" | "win" | "lose" | "push";

type Hand = string[];

export function Room() {
    const params = useParams();
    const roomId = useMemo(
        () => {
            console.log("params", params);
            return params.roomId || crypto.randomUUID()
        },
        [params]
    );
    const playerId = useMemo(() => localStorage.getItem("playerId") || crypto.randomUUID(), []);
    const [dealerCards, setDealerCards] = useState<string[]>([]);

    const [players, setPlayers] = useState<Record<string, { hand: Hand, status: Status}>>({});

    const [playerTurn, setPlayerTurn] = useState<string | null>(null);

    const [connection, setConnection] = useState<HubConnection>();

    const calculateTotal = (hand: Hand) => {
        let total = 0;
        let aces = 0;
        hand.forEach((card) => {
            const value = card[0]
            if (value === "A") {
                total += 11;
                aces += 1;
            } else if (["K", "Q", "J"].includes(value)) {
                total += 10;
            } else {
                total += parseInt(value);
            }
        });
        while (total > 21 && aces > 0) {
            total -= 10;
            aces -= 1;
        }
        return total;
    }

    const onCardDealt = (cPlayerId: string, card: string) => {
        console.log("CardDealt", cPlayerId, card);
        if (cPlayerId === "dealer") {
            setDealerCards((prev) => {
                if (prev.length == 2 && prev[1] == "??") {
                    return [prev[0], card]
                }
                return [...prev, card]
            })
        }
        else {
            setPlayers((prev) => {
                const newPlayers = { ...prev };
                if (newPlayers[cPlayerId]) {
                    newPlayers[cPlayerId].hand.push(card);
                } else {
                    newPlayers[cPlayerId] = { hand: [card], status: "waiting" };
                }
                return newPlayers;
            })
        }
    }

    useEffect(() => {
        console.log("roomId", roomId);
        if (!roomId) return;

        localStorage.setItem("playerId", playerId);

        const connection = new HubConnectionBuilder()
            .withUrl(
                `http://localhost:5106/blackjack?playerId=${playerId}&roomId=${roomId}`
            )
            .build();

        connection.on("Welcome", (room) => {
            console.log("Bienvenido", room);
            setPlayers(room.players);
            setPlayerTurn(room.playerTurn);
        });
        connection.on("CardDealt", onCardDealt);
        connection.on("PlayerTurn", (cPlayerId) => {
            setPlayerTurn(cPlayerId);
        })

        connection.on("PlayerConnected", (pid) => {
            console.log(`Jugador conectado: ${pid}`)
            setPlayers((prev) => {
                const newPlayers = { ...prev };
                newPlayers[pid] = { hand: [], status: "waiting" };
                return newPlayers;
            })
        });

        connection.on("PlayerDisconnected", (pid) => {
            console.log(`Jugador desconectado: ${pid}`)
            setPlayers((prev) => {
                const newPlayers = { ...prev };
                delete newPlayers[pid];
                return newPlayers;
            })
        });

        connection.on("RoundOver", (playerWinningStatus: Record<string, Status>) => {
            console.log("Fin de la ronda", playerWinningStatus);
            setPlayers((prev) => {
                const newPlayers = { ...prev };
                Object.entries(playerWinningStatus).forEach(([pid, status]) => {
                    if (newPlayers[pid]) {
                        newPlayers[pid].status = status;
                    }
                })
                return newPlayers;
            })

        })

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
            <span>Jugadores:</span>
            {
                Object.entries(players).map(([pid, player]) => (
                    <span key={pid}>
                        {pid === playerId ? "Yo" : pid}: {player.hand.join(", ")} ({calculateTotal(player.hand)}) - {player.status}
                    </span>
                ))
            }
            {
                playerTurn === playerId && (
                    <>
                        <button onClick={() => connection?.invoke("Hit",   roomId)}>Pedir carta</button>
                        <button onClick={() => connection?.invoke("Stand", roomId)}>Plantarse</button>
                    </>
                )
            }
            {
                playerTurn && playerTurn !== "dealer" && (
                    <span>Turno del jugador: {playerTurn}</span>
                )
            }
        </div>
    )
}
