namespace Protocol.Packet;

public class PacketInfo(Int16 type, byte[] payload)
{
  public PacketType type { get; } = (PacketType)type;
  public byte[] payload { get; } = payload;

  public PacketInfo(Int16 type) : this(type, []) { }
  public PacketInfo(PacketType type) : this((short)type, []) { }
}