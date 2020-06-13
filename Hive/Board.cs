using System;
using System.Collections.Generic;

public class Board
{
  // Maps from piece position to the pieces on that position
  private Dictionary<int, List<Piece>> PieceMap { get; }

  public Board()
  {
    PieceMap = new Dictionary<int, List<Piece>>();
  }

  public void placePiece(int tileNumber, Piece piece)
  {
    if (canPlacePieceOn(tileNumber))
    {
      PieceMap[tileNumber] = new List<Piece>(
        new Piece[] { piece }
      );
    }
    else
    {
      Console.WriteLine($"Invalid piece placement. Piece already exists on tile {tileNumber}");
    }
  }

  public void movePiece(int tileStart, int tileEnd)
  {
    int numPiecesOnTile = PieceMap[tileStart].Count;
    Piece pieceToMove = PieceMap[tileStart][numPiecesOnTile - 1];
    PieceMap[tileStart].RemoveAt(numPiecesOnTile - 1);
    addPieceAt(tileEnd, pieceToMove);
    if (PieceMap[tileStart].Count == 0)
    {
      PieceMap.Remove(tileStart);
    }
  }

  public bool isOccupiedAt(int tileNumber)
  {
    return PieceMap.ContainsKey(tileNumber);
  }

  public List<Piece> getPiecesAt(int tileNumber)
  {
    if (isOccupiedAt(tileNumber))
    {
      return PieceMap[tileNumber];
    }
    return new List<Piece>();
  }

  // Private methods

  private bool canPlacePieceOn(int tileNumber)
  {
    return !isOccupiedAt(tileNumber);
  }

  private void addPieceAt(int tileNumber, Piece piece)
  {
    if (PieceMap.ContainsKey(tileNumber))
    {
      PieceMap[tileNumber].Add(piece);
    }
    else
    {
      PieceMap[tileNumber] = new List<Piece>(new Piece[] { piece });
    }
  }
}
