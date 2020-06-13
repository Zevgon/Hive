using System;
using System.Collections.Generic;
using System.Linq;
using static Util;

public class Board : ICloneable
{
  // Maps from piece position to the pieces on that position
  private Dictionary<int, List<Piece>> PieceMap { get; }

  public Board()
  {
    PieceMap = new Dictionary<int, List<Piece>>();
  }

  protected Board(Board other)
  {
    PieceMap = Util.cloneDictionary(other.PieceMap);
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
    if (!isValidMove(tileStart, tileEnd)) return;
    Piece piece = removePieceAt(tileStart);
    addPieceAt(tileEnd, piece);
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

  public object Clone()
  {
    return new Board(this);
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

  private Piece removePieceAt(int tileNumber)
  {
    if (PieceMap.ContainsKey(tileNumber))
    {
      List<Piece> pieces = PieceMap[tileNumber];
      Piece pieceToRemove = pieces[pieces.Count - 1];
      pieces.RemoveAt(pieces.Count - 1);
      if (pieces.Count == 0)
      {
        PieceMap.Remove(tileNumber);
      }
      return pieceToRemove;
    }
    else
    {
      throw new ArgumentException(
        $"Cannot remove piece on tile {tileNumber}. Tile is unoccupied");
    }
  }

  private bool isValidMove(int tileStart, int tileEnd)
  {
    try
    {
      validateOneHive(tileStart, tileEnd);
      validatePieceStacking(tileStart, tileEnd);
    }
    catch (ArgumentException e)
    {
      Console.WriteLine(e.Message);
      return false;
    }
    return true;
  }

  private void validateOneHive(int tileStart, int tileEnd)
  {
    Board boardClone = (Board)this.Clone();
    boardClone.removePieceAt(tileStart);
    if (boardClone.hasMultipleIslands())
    {
      throw new ArgumentException(
        "Illegal move. Cannot break the \"One Hive Rule\".");
    }
  }

  private void validatePieceStacking(int tileStart, int tileEnd)
  {
    Piece piece = getTopPieceAt(tileStart);
    if (isOccupiedAt(tileEnd) && piece.Type != PieceType.Beetle)
    {
      throw new ArgumentException(
        "Illegal move. Cannot move a piece on top of another piece unless it's a beetle"
      );
    }
  }

  private Piece getTopPieceAt(int tileNumber)
  {
    if (!isOccupiedAt(tileNumber))
    {
      throw new ArgumentException($"No pieces on tile {tileNumber}");
    }
    List<Piece> pieces = getPiecesAt(tileNumber);
    return pieces[pieces.Count - 1];
  }

  private List<int> findOccupiedAdjacents(int tileNumber)
  {
    return Util.findAdjacents(tileNumber).FindAll(isOccupiedAt);
  }

  private bool hasMultipleIslands()
  {
    int origin = PieceMap.First().Key;
    Queue<int> queue = new Queue<int>(new int[] { origin });
    HashSet<int> seen = new HashSet<int>();
    while (queue.Count > 0)
    {
      int tile = queue.Dequeue();
      seen.Add(tile);
      foreach (int adj in findOccupiedAdjacents(tile))
      {
        if (!seen.Contains(adj))
        {
          queue.Enqueue(adj);
        }
      }
    }
    return PieceMap.Count != seen.Count;
  }
}
