public record JoinRoomPayload(string RoomId, string Username);
public record JoinTablePayload(string RoomId, string Username, int SeatIndex);