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

public class Piece
{
  public PieceType Type { get; }
  public Color Color { get; }

  public Piece(PieceType pieceType, Color color)
  {
    Type = pieceType;
    Color = color;
  }
}
