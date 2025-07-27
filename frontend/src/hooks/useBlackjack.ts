import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { useEffect, useState } from "react";
import { Player, PlayerStatus, RoomState, RoomStatus } from "../types/blackjack";

export function useBlackjack(playerId: string, roomId: string | undefined, setRoomState: React.Dispatch<React.SetStateAction<RoomState | null>>) {
    const [connection, setConnection] = useState<HubConnection | null>(null);

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
            setRoomState(room);
        });

        connection.on("playerJoined", (p: Player) => {
            setRoomState((prev) => {
                if (!prev) return prev;
                const newSeats = [...prev.seats];
                newSeats[p.seatIndex] = p;
                return { ...prev, seats: newSeats };
            });
        });

        connection.on("roomStatus", (status: RoomStatus) => {
            console.log("roomStatus", status);
            setRoomState(prev => {
                if (!prev) return prev;
                return { ...prev, status };
            });
        });

        connection.on("playerStatus", ({ seatIndex, status }: { seatIndex: number, status: PlayerStatus }) => {
            setRoomState(prev => {
                if (!prev) return prev;
                const newSeats = [...prev.seats];
                if (newSeats[seatIndex]) {
                    newSeats[seatIndex] = { ...newSeats[seatIndex]!, status };
                }
                return { ...prev, seats: newSeats };
            });
        });

        connection.on("betPlaced", ({ seatIndex, amount }: { seatIndex: number, amount: number }) => {
            setRoomState(prev => {
                if (!prev) return prev;
                const newSeats = [...prev.seats];
                if (newSeats[seatIndex]) {
                    newSeats[seatIndex] = { ...newSeats[seatIndex]!, bet: amount, status: PlayerStatus.BetsPlaced };
                }
                return { ...prev, seats: newSeats };
            });
        });

        connection.on("betsRefunded", () => {
            setRoomState(prev => {
                if (!prev) return prev;
                const newSeats = prev.seats.map(seat => seat ? { ...seat, bet: 0, status: PlayerStatus.Betting } : null);
                return { ...prev, seats: newSeats };
            });
        });

        connection.on("cardDealt", ({ seatIndex, card } : { seatIndex: number, card: string }) => {
            setRoomState(prev => {
                if (!prev) return prev;
                const newSeats = [...prev.seats];
                if (newSeats[seatIndex]) {
                    newSeats[seatIndex] = {
                        ...newSeats[seatIndex]!,
                        hand: [...newSeats[seatIndex]!.hand, card],
                        // status: PlayerStatus.Deciding
                    };
                }
                return { ...prev, seats: newSeats };
            });
        });

        connection.on("dealerCardDealt", ({ card } : { card: string }) => {
            setRoomState(prev => {
                if (!prev) return prev;
                return { ...prev, dealerCards: [...prev.dealerCards, card] };
            });
        });

        connection.on("dealerShowCard", ({ card } : { card: string }) => {
            setRoomState(prev => {
                if (!prev) return prev;
                return { ...prev, dealerCards: [prev.dealerCards[0], card] };
            });
        })

        connection.on("playerTurn", ({ playerTurn }: { playerTurn: number }) => {
            setRoomState(prev => {
                if (!prev) return prev;
                return { ...prev, playerTurn };
            });
        })

        connection.start().then(() => {
            console.log("Conectado a la sala");
            setConnection(connection);
        });

        // Disconnect when the component unmounts
        return () => {
            connection.stop().then(() => {
                console.log("Desconectado de la sala");
            });
        };
    }, [roomId]);

    return {
        connection,
    };
}