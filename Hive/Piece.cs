using System;
using System.ComponentModel;
using System.Runtime.Serialization;
public enum PieceType
{
  [Description("Ant")]
  A,
  [Description("Beetle")]
  B,
  [Description("Grasshopper")]
  G,
  [Description("Spider")]
  S,
  [Description("Queen")]
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
