using System;
using System.Collections.Generic;

public class Player
{
  public Color Color;
  public string Name;

  private int NUM_ANTS = 3;
  private int NUM_GHS = 3;
  private int NUM_BEETLES = 2;
  private int NUM_SPIDERS = 2;
  private int NUM_QUEENS = 1;

  private Dictionary<PieceType, int> PieceCounts;

  public Player(string name, Color color)
  {
    Color = color;
    Name = name;
    PieceCounts =
      new Dictionary<PieceType, int>
      {
        {PieceType.A, NUM_ANTS},
        {PieceType.G, NUM_GHS},
        {PieceType.B, NUM_BEETLES},
        {PieceType.S, NUM_SPIDERS},
        {PieceType.Q, NUM_QUEENS},
      };
  }

  // Should call validateHasPiece beforehand
  public Piece givePiece(PieceType pieceType)
  {
    PieceCounts[pieceType] -= 1;
    return new Piece(pieceType, Color);

    // if (PieceCounts[pieceType] > 0)
    // {
    //   PieceCounts[pieceType] -= 1;
    //   return new Piece(pieceType, Color);
    // }
    // throw new ArgumentException(
    //   $"Player has no more pieces of type {pieceType}.");
  }

  public void validateHasPiece(PieceType pieceType)
  {
    if (PieceCounts[pieceType] <= 0)
    {
      throw new ArgumentException(
        $"{this} has no more pieces of type {pieceType}.");
    }
  }

  override
  public string ToString()
  {
    return Name;
  }
}
