using System;
using System.Runtime.Serialization;
public enum PieceType
{
  // TODO: change these "Value = " to human-readable values, and change the
  // actual enum values to single letters
  [EnumMember(Value = "a")]
  Ant,
  [EnumMember(Value = "b")]
  Beetle,
  [EnumMember(Value = "g")]
  Gh,
  [EnumMember(Value = "s")]
  Spider,
  [EnumMember(Value = "q")]
  Queen,
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
