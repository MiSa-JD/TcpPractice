namespace Protocol.Packet.Models;

public class PacketInfo(PacketType type, byte[] payload)
{
  public PacketType type { get; } = type;
  public byte[] payload { get; } = payload;

  public PacketInfo(short type, byte[] payload) : this((PacketType)type, payload) { }
  public PacketInfo(PacketType type) : this(type, []) { }
}