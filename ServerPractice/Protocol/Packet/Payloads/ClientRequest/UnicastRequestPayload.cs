namespace Protocol.Packet.Payloads.ClientRequest;

public record UnicastRequestPayload(Guid id, string message);