import { useMemo, useState } from "react";
import { useParams } from "react-router";
import { Player, PlayerStatus, RoomState, RoomStatus } from "../../types/blackjack";
import { useBlackjack } from "../../hooks/useBlackjack";
import ToastNotification, { ToastNotificationProps } from "../../components/toastNotification";

function getTotal(hand: string[]) {
    let total = 0;
    let aces = 0;
    
    for (const card of hand) {
        const value = card.slice(0, -2); // El emoji cuenta como 2 caracteres.
        if (value === "A") {
            aces++;
        } else if (["K", "Q", "J"].includes(value)) {
            total += 10;
        } else {
            total += parseInt(value, 10);
        }
    }
    
    total += aces;
    
    while (aces > 0 && total + 10 <= 21) {
        total += 10;
        aces--;
    }
    
    return total;
}

export function Room() {
    const params = useParams();
    const roomId = useMemo(() => params.roomId, [params.roomId]);
    const playerId = useMemo(() => localStorage.getItem("playerId") || crypto.randomUUID().slice(0, 8), []);

    const [roomState, setRoomState] = useState<RoomState | null>(null);
    const [betAmount, setBetAmount] = useState<number>(10); // Apuesta por defecto
    const [toast, setToast] = useState<ToastNotificationProps | null>(null);
    const { connection } = useBlackjack(playerId, roomId, setRoomState, setToast);

    const handleJoinTable = (seatIndex: number) => {
        if (connection && !roomState?.seats[seatIndex]) {
            connection.invoke("HandleCommand", "joinTable", {
                roomId,
                username: playerId,
                seatIndex
            });
        }
    }

    const action = (seatIndex: number, action: string) => {
        if (connection) {
            connection.invoke("HandleCommand", "playerAction", {
                roomId,
                seatIndex,
                action
            });
        }
    }

    if (!roomState) return <div>Cargando...</div>;

    // Obtener todos los asientos del jugador actual
    const currentPlayerSeats = roomState.seats.filter((seat): seat is Player =>
        seat !== null && seat.username === playerId
    );
    const canBet = roomState.status === RoomStatus.WaitingForBets &&
        currentPlayerSeats.some(seat => [PlayerStatus.Betting, PlayerStatus.BetsPlaced].includes(seat.status));

    const myTurn = currentPlayerSeats.some(seat => seat.seatIndex === roomState.playerTurn);

    return (
        <div className="flex flex-col">
            <ToastNotification
                message={toast?.message || null}
                type={toast?.type || "info"}
                onClose={() => setToast(null)}
            />
            <span>Cartas del dealer: {(roomState.dealerCards || []).join(", ")}</span>
            <span className="mb-6">Total del dealer: {getTotal(roomState.dealerCards)}</span>
            {/* Sección de apuestas */}
            {canBet && (
                <div className="border p-4 mb-4 bg-yellow-50">
                    <h3 className="font-bold mb-2">¡Es hora de apostar!</h3>
                    <p className="text-sm text-gray-600 mb-2">
                        Tienes {currentPlayerSeats.length} asiento(s). Selecciona uno para apostar:
                    </p>

                    {/* Mostrar cada asiento que puede apostar */}
                    {currentPlayerSeats
                        .filter(seat => [PlayerStatus.Betting, PlayerStatus.BetsPlaced].includes(seat.status))
                        .map(seat => (
                            <div key={seat.seatIndex} className="border p-3 mb-3 bg-white rounded text-black">
                                <h4 className="font-semibold mb-2">Asiento {seat.seatIndex + 1}</h4>
                                <div className="flex items-center gap-2 mb-2">
                                    <label htmlFor={`betAmount-${seat.seatIndex}`}>Monto a apostar:</label>
                                    <input
                                        id={`betAmount-${seat.seatIndex}`}
                                        type="number"
                                        min="1"
                                        value={betAmount}
                                        onChange={(e) => setBetAmount(Number(e.target.value))}
                                        className="border px-2 py-1 w-20"
                                    />
                                    <button
                                        onClick={() => {
                                            if (connection && betAmount > 0) {
                                                connection.invoke("HandleCommand", "placeBet", {
                                                    roomId,
                                                    seatIndex: seat.seatIndex,
                                                    amount: betAmount
                                                });
                                            }
                                        }}
                                        disabled={betAmount <= 0}
                                        className="bg-green-500 text-white px-4 py-1 rounded hover:bg-green-600 disabled:bg-gray-300"
                                    >
                                        Apostar
                                    </button>
                                </div>
                                <div className="flex gap-2">
                                    <button onClick={() => setBetAmount(5)} className="bg-blue-500 text-white px-2 py-1 rounded text-sm">$5</button>
                                    <button onClick={() => setBetAmount(10)} className="bg-blue-500 text-white px-2 py-1 rounded text-sm">$10</button>
                                    <button onClick={() => setBetAmount(25)} className="bg-blue-500 text-white px-2 py-1 rounded text-sm">$25</button>
                                    <button onClick={() => setBetAmount(50)} className="bg-blue-500 text-white px-2 py-1 rounded text-sm">$50</button>
                                </div>
                            </div>
                        ))}
                </div>
            )}
            <span>Jugadores:</span>
            {
                roomState.seats.filter(Boolean).map((seat, i) => (
                    <div key={i} className="border p-2 mb-2 rounded">
                        <div>
                            <strong>{seat?.username === playerId ? "Yo" : seat?.username}</strong>
                            <span className="ml-2 text-sm text-gray-600">({seat?.status})</span>
                        </div>
                        <div>Cartas: {seat?.hand.join(", ") || "Sin cartas"}</div>
                        <div>Total: {seat ? getTotal(seat.hand) : 0}</div>
                        <div>Apuesta: ${seat?.bet || 0}</div>
                    </div>
                ))
            }
            {
                myTurn && (
                    <div className="flex flex-row gap-3 my-1">
                        <button className={`px-4 py-2 font-medium rounded-lg bg-[#1a1a1a] cursor-pointer`} onClick={() => action(roomState.playerTurn, "hit")}>Pedir carta</button>
                        <button className={`px-4 py-2 font-medium rounded-lg bg-[#1a1a1a] cursor-pointer`} onClick={() => action(roomState.playerTurn, "stand")}>Plantarse</button>
                    </div>
                )
            }

            <span>Selecciona tu asiento:</span>
            <div className="flex gap-2 mb-4">
                {roomState?.seats.map((seat, i) => (
                    <button
                        key={i}
                        className={`border border-transparent transition-colors px-4 py-2 font-medium rounded-lg bg-[#1a1a1a] disabled:opacity-50 ${seat ? "cursor-not-allowed" : "cursor-pointer hover:border-blue-500"}`}
                        onClick={() => handleJoinTable(i)}
                        disabled={!!seat}
                    >
                        {seat ? `${seat.username === playerId ? "Tú" : seat.username}` : `Asiento ${i + 1}`}
                    </button>
                ))}
            </div>

            <span>Turno del jugador: {roomState.playerTurn === -1 ? "Nadie" : roomState.seats[roomState.playerTurn]?.username}</span>
            <span>Room Status: {roomState.status}</span>
        </div>
    )
}
