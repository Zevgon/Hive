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
    try
    {
      validatePlacement(tileNumber, piece);
      PieceMap[tileNumber] = new List<Piece>(new Piece[] { piece });
    }
    catch (ArgumentException e)
    {
      Console.WriteLine(e.Message);
    }
  }

  public void movePiece(int tileStart, int tileEnd)
  {
    try
    {
      validateMove(tileStart, tileEnd);
      Piece piece = removePiece(tileStart);
      addPiece(tileEnd, piece);
    }
    catch (ArgumentException e)
    {
      Console.WriteLine(e.Message);
    }
  }

  public bool isOccupied(int tileNumber)
  {
    return PieceMap.ContainsKey(tileNumber);
  }

  public List<Piece> getPieces(int tileNumber)
  {
    if (isOccupied(tileNumber))
    {
      return PieceMap[tileNumber];
    }
    return new List<Piece>();
  }

  public Piece getTopPiece(int tileNumber)
  {
    if (!isOccupied(tileNumber))
    {
      throw new ArgumentException($"No pieces on tile {tileNumber}");
    }
    List<Piece> pieces = getPieces(tileNumber);
    return pieces[pieces.Count - 1];
  }

  public object Clone()
  {
    return new Board(this);
  }

  // Private methods

  private void addPiece(int tileNumber, Piece piece)
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

  private Piece removePiece(int tileNumber)
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

  private void validateMove(int tileStart, int tileEnd)
  {
    validateOneHive(tileStart, tileEnd);
    validatePieceStacking(tileStart, tileEnd);
  }

  private void validatePlacement(int tileNumber, Piece piece)
  {
    List<Piece> adjacentPieces = findOccupiedAdjacents(tileNumber)
      .ConvertAll(adj => getTopPiece(adj));
    if (PieceMap.Count > 1 &&
        adjacentPieces.Any(adjPiece => adjPiece.Color != piece.Color))
    {
      throw new ArgumentException(
        "Illegal placement. Cannot place a piece next to another piece of the opposite color");
    }
  }

  private void validateOneHive(int tileStart, int tileEnd)
  {
    Board boardClone = (Board)this.Clone();
    Piece piece = boardClone.removePiece(tileStart);
    boardClone.checkMultipleIslands();
    boardClone.addPiece(tileEnd, piece);
    boardClone.checkMultipleIslands();
  }

  private void validatePieceStacking(int tileStart, int tileEnd)
  {
    Piece piece = getTopPiece(tileStart);
    if (isOccupied(tileEnd) && piece.Type != PieceType.Beetle)
    {
      throw new ArgumentException(
        "Illegal move. Cannot move a piece on top of another piece unless it's a beetle"
      );
    }
  }

  private List<int> findOccupiedAdjacents(int tileNumber)
  {
    return Util.findAdjacents(tileNumber).FindAll(isOccupied);
  }

  private void checkMultipleIslands()
  {
    if (hasMultipleIslands())
    {
      throw new ArgumentException(
        "Illegal move. Cannot break the \"One Hive Rule\".");
    }
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
