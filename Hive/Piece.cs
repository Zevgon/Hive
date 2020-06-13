using System;
public enum PieceType
{
  Queen,
  Gh,
  Ant,
  Spider,
  Beetle
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
