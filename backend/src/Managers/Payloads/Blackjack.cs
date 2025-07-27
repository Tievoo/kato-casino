#pragma warning disable IDE1006 // Naming Styles
public record JoinRoomPayload(string roomId, string username);
public record JoinTablePayload(string roomId, int seatIndex);
public record PlaceBetPayload(string roomId, int seatIndex, int amount);
public record PlayerActionPayload(string roomId, int seatIndex, string action);