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

const chipValues = [5, 10, 25, 50];

const statusDisplayMap = {
    // Player Status
    [PlayerStatus.Betting]: "Apostando",
    [PlayerStatus.BetsPlaced]: "Apuesta lista", 
    [PlayerStatus.Deciding]: "Decidiendo",
    [PlayerStatus.Bust]: "Se pasó",
    [PlayerStatus.Stand]: "Se plantó",
    [PlayerStatus.Blackjack]: "¡Blackjack!",
    [PlayerStatus.Waiting]: "Esperando",
    
    // Room Status (for dealer)
    [RoomStatus.WaitingForBets]: "Esperando apuestas",
    [RoomStatus.WaitingForPlayers]: "Esperando jugadores",
    [RoomStatus.Dealing]: "Repartiendo cartas",
    [RoomStatus.Playing]: "Jugando",
    [RoomStatus.Dealer]: "Turno del dealer",
    [RoomStatus.Results]: "Mostrando resultados"
};

export function Room() {
    const params = useParams();
    const roomId = useMemo(() => params.roomId, [params.roomId]);
    const playerId = useMemo(() => localStorage.getItem("playerId") || crypto.randomUUID().slice(0, 8), []);

    const [roomState, setRoomState] = useState<RoomState | null>(null);
    const [selectedChip, setSelectedChip] = useState<number>(10);
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

    const handleSeatClick = (seatIndex: number) => {
        const seat = roomState?.seats[seatIndex];
        
        // Si el asiento está vacío, permitir unirse
        if (!seat) {
            handleJoinTable(seatIndex);
            return;
        }

        // Si es mi asiento y puedo apostar, colocar apuesta
        if (seat.username === playerId && canBet && 
            [PlayerStatus.Betting, PlayerStatus.BetsPlaced].includes(seat.status)) {
            placeBet(seatIndex);
        }
    }

    const placeBet = (seatIndex: number) => {
        if (connection && selectedChip > 0) {
            connection.invoke("HandleCommand", "placeBet", {
                roomId,
                seatIndex,
                amount: selectedChip
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

    if (!roomState) return <div className="flex items-center justify-center h-screen text-white">Cargando...</div>;

    const currentPlayerSeats = roomState.seats.filter((seat): seat is Player =>
        seat !== null && seat.username === playerId
    );
    const canBet = roomState.status === RoomStatus.WaitingForBets &&
        currentPlayerSeats.some(seat => [PlayerStatus.Betting, PlayerStatus.BetsPlaced].includes(seat.status));

    const myTurn = currentPlayerSeats.some(seat => seat.seatIndex === roomState.playerTurn);

    // Posiciones de los asientos en semicírculo
    const seatPositions = [
        { left: '6%', bottom: '24%' },   // Asiento 1
        { left: '22%', bottom: '10%' },    // Asiento 2
        { left: '38%', bottom: '3%' },    // Asiento 3
        { right: '38%', bottom: '3%' },   // Asiento 4
        { right: '22%', bottom: '10%' },   // Asiento 5
        { right: '6%', bottom: '24%' },  // Asiento 6
    ];

    return (
        <div className="min-h-screen bg-gradient-to-b from-green-800 to-green-900 relative overflow-hidden">
            <ToastNotification
                message={toast?.message || null}
                type={toast?.type || "info"}
                onClose={() => setToast(null)}
            />
            
            {/* Mesa de blackjack */}
            <div className="absolute inset-0 flex items-center justify-center">
                {/* Superficie de la mesa */}
                <div className="relative w-[900px] h-[500px] bg-green-700 rounded-full border-8 border-yellow-600 shadow-2xl">
                    {/* Línea del dealer */}
                    <div className="absolute top-8 left-1/2 transform -translate-x-1/2 w-64 h-1 bg-yellow-400 rounded"></div>
                    
                    {/* Área del dealer */}
                    <div className="absolute top-4 left-1/2 transform -translate-x-1/2 text-center">
                        <div className="text-white font-bold text-lg mb-2">DEALER</div>
                        
                        {/* Estado del dealer */}
                        <div className="text-yellow-400 text-xs mb-2 font-semibold">
                            {statusDisplayMap[roomState.dealerStatus]}
                        </div>
                        
                        <div className="flex gap-2 justify-center mb-2">
                            {roomState.dealerCards.map((card, i) => (
                                <div key={i} className="bg-white rounded p-1 text-black text-sm min-w-[40px] text-center border">
                                    {card}
                                </div>
                            ))}
                        </div>
                        <div className="text-white text-sm">
                            Total: {getTotal(roomState.dealerCards)}
                        </div>
                    </div>

                    {/* Asientos de jugadores */}
                    {roomState.seats.map((seat, i) => {
                        const position = seatPositions[i];
                        const isMyTurn = seat?.seatIndex === roomState.playerTurn;
                        const isMyPlayer = seat?.username === playerId;
                        const canClickToBet = isMyPlayer && canBet && 
                            seat && [PlayerStatus.Betting, PlayerStatus.BetsPlaced].includes(seat.status);

                        return (
                            <div
                                key={i}
                                className="absolute"
                                style={position}
                            >
                                {/* Asiento */}
                                <div
                                    className={`
                                        w-16 h-16 rounded-full border-4 cursor-pointer transition-all duration-200 flex flex-col items-center justify-center
                                        ${seat 
                                            ? `bg-blue-600 border-blue-400 ${isMyPlayer ? 'ring-4 ring-yellow-400' : ''} ${isMyTurn ? 'ring-4 ring-red-400 animate-pulse' : ''}`
                                            : 'bg-gray-600 border-gray-400 hover:bg-gray-500'
                                        }
                                        ${canClickToBet ? 'hover:ring-4 hover:ring-green-400' : ''}
                                    `}
                                    onClick={() => handleSeatClick(i)}
                                >
                                    {seat ? (
                                        <div className="text-center text-white text-[10px]">
                                            <div className="font-bold mb-0.5">
                                                {seat.username === playerId ? "TÚ" : seat.username.slice(0, 6)}
                                            </div>
                                            {seat.bet > 0 && (
                                                <div className="bg-yellow-500 text-black px-1 rounded text-[8px] font-bold">
                                                    ${seat.bet}
                                                </div>
                                            )}
                                        </div>
                                    ) : (
                                        <div className="text-center text-gray-300 text-[10px]">
                                            <div className="font-bold">{i + 1}</div>
                                        </div>
                                    )}
                                </div>

                                {/* Cartas del jugador */}
                                {seat && seat.hand.length > 0 && (
                                    <div className="absolute -top-12 left-1/2 transform -translate-x-1/2">
                                        <div className="flex gap-1 justify-center mb-1">
                                            {seat.hand.map((card, cardIndex) => (
                                                <div 
                                                    key={cardIndex} 
                                                    className="bg-white rounded p-1 text-black text-[10px] min-w-[28px] text-center border shadow-md"
                                                    // style={{ transform: `rotate(${(cardIndex - seat.hand.length/2) * 5}deg)` }}
                                                >
                                                    {card}
                                                </div>
                                            ))}
                                        </div>
                                        <div className="text-white text-[10px] text-center bg-black bg-opacity-50 rounded px-1">
                                            {getTotal(seat.hand)}
                                        </div>
                                    </div>
                                )}

                                {/* Estado del jugador */}
                                {seat && (
                                    <div className="absolute -bottom-8 left-1/2 transform -translate-x-1/2 text-center">
                                        <div className="text-white text-[9px] bg-black bg-opacity-60 rounded px-1 py-0.5">
                                            {statusDisplayMap[seat.status]}
                                        </div>
                                    </div>
                                )}
                            </div>
                        );
                    })}
                </div>
            </div>

            {/* Panel de información superior */}
            <div className="absolute top-4 left-4 bg-black bg-opacity-75 rounded-lg p-4 text-white">
                <div className="text-sm">
                    <div>Sala: {roomId}</div>
                    <div>Estado: {statusDisplayMap[roomState.status]}</div>
                    <div>Turno: {roomState.playerTurn === -1 ? "Nadie" : roomState.seats[roomState.playerTurn]?.username || "Dealer"}</div>
                </div>
            </div>

            {/* Controles de acción cuando es tu turno */}
            {myTurn && (
                <div className="absolute bottom-32 left-1/2 transform -translate-x-1/2 flex gap-4">
                    <button 
                        className="bg-green-600 hover:bg-green-700 text-white px-6 py-3 rounded-lg font-bold shadow-lg transition-colors"
                        onClick={() => action(roomState.playerTurn, "hit")}
                    >
                        PEDIR CARTA
                    </button>
                    <button 
                        className="bg-red-600 hover:bg-red-700 text-white px-6 py-3 rounded-lg font-bold shadow-lg transition-colors"
                        onClick={() => action(roomState.playerTurn, "stand")}
                    >
                        PLANTARSE
                    </button>
                </div>
            )}

            {/* Panel de fichas para apostar */}
            {canBet && (
                <div className="absolute bottom-4 left-1/2 transform -translate-x-1/2 bg-black bg-opacity-75 rounded-lg p-4">
                    <div className="text-white text-center mb-3 text-sm">
                        Selecciona ficha y haz click en tu asiento para apostar
                    </div>
                    <div className="flex gap-3 justify-center">
                        {chipValues.map(value => (
                            <button
                                key={value}
                                className={`
                                    w-16 h-16 rounded-full border-4 font-bold text-sm transition-all duration-200
                                    ${selectedChip === value 
                                        ? 'ring-4 ring-yellow-400 scale-110' 
                                        : 'hover:scale-105'
                                    }
                                    ${value === 5 ? 'bg-red-500 border-red-600 text-white' : ''}
                                    ${value === 10 ? 'bg-blue-500 border-blue-600 text-white' : ''}
                                    ${value === 25 ? 'bg-green-500 border-green-600 text-white' : ''}
                                    ${value === 50 ? 'bg-purple-500 border-purple-600 text-white' : ''}
                                `}
                                onClick={() => setSelectedChip(value)}
                            >
                                ${value}
                            </button>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}
