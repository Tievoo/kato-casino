export type Room = {
    id: string;
    players: number;
    status: string;
}

export enum PlayerStatus {
    Betting = "Betting",
    BetsPlaced = "BetsPlaced", 
    Deciding = "Deciding",
    Bust = "Bust",
    Stand = "Stand",
    Blackjack = "Blackjack",
    Waiting = "Waiting"
}

export enum RoomStatus
{
    WaitingForBets = "WaitingForBets",
    Dealer = "Dealer",
    Results = "Results",
    WaitingForPlayers = "WaitingForPlayers",
    Playing = "Playing",
    Dealing = "Dealing"
}

export type Player = {
    username: string;
    connectionId: string;
    status: PlayerStatus;
    seatIndex: number;
    hand: string[];
    bet: number;
}

export type RoomState = {
    id: string;
    seats: (null | Player)[];
    playerTurn: number;
    dealerCards: string[];
    status: RoomStatus;
}

export type Card = {
    suit: string;
    value: string;
    isVisible: boolean;
}
