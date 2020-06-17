using System;
using System.Runtime.Serialization;
public enum PieceType
{
  [EnumMember(Value = "Ant")]
  A,
  [EnumMember(Value = "Beetle")]
  B,
  [EnumMember(Value = "Grasshopper")]
  G,
  [EnumMember(Value = "Spider")]
  S,
  [EnumMember(Value = "Queen")]
  Q,
}

public enum Color
{
  White,
  Black
}

public class Piece : ICloneable
{
  public PieceType Type { get; }
  public Color Color { get; }

  public Piece(PieceType pieceType, Color color)
  {
    Type = pieceType;
    Color = color;
  }

  public object Clone()
  {
    return new Piece(Type, Color);
  }

  override
  public string ToString()
  {
    return $"{Color} {Type}";
  }
}
