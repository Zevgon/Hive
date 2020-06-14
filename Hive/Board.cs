using System;
using System.Collections.Generic;
using System.Linq;
using static Util;

public class Board : ICloneable
{
  // Maps from piece position to the pieces on that position
  private Dictionary<int, List<Piece>> PieceMap { get; }
  private int SPIDER_DISTANCE = 3;

  public Board()
  {
    PieceMap = new Dictionary<int, List<Piece>>();
  }

  public Board(Dictionary<int, List<Piece>> pieceMap)
  {
    PieceMap = pieceMap;
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

  // Direct board interactions

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

  private List<Piece> getPieces(int tileNumber)
  {
    if (isOccupied(tileNumber))
    {
      return PieceMap[tileNumber];
    }
    return new List<Piece>();
  }

  private Piece removePiece(int tileNumber)
  {
    if (isOccupied(tileNumber))
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

  // Validations

  private void validateMove(int tileStart, int tileEnd)
  {
    validateOneHive(tileStart, tileEnd);
    validatePieceStacking(tileStart, tileEnd);
    validatePieceCanReach(tileStart, tileEnd);
  }

  private void validatePlacement(int tileNumber, Piece piece)
  {
    validateUnoccupied(tileNumber);
    validateNoAdjacentOppositeColors(tileNumber, piece);
  }

  // Validation helper methods

  private void checkMultipleIslands()
  {
    if (hasMultipleIslands())
    {
      throw new ArgumentException(ErrorMessages.ONE_HIVE);
    }
  }

  private HashSet<Piece> findAdjacentPieces(int tileNumber)
  {
    return findOccupiedAdjacents(tileNumber)
      .ConvertAll(getTopPiece)
      .ToHashSet();
  }

  private HashSet<int> findImmediateReachablesByPivot(int tileNumber)
  {
    HashSet<Piece> adjacentPieces = findAdjacentPieces(tileNumber);
    List<int> unoccupiedAdjacents = findUnoccupiedAdjacents(tileNumber);
    return unoccupiedAdjacents.FindAll(adj =>
    {
      if (findAdjacentPieces(adj).Intersect(adjacentPieces).Count() == 0)
      {
        return false;
      }
      return !isTooNarrow(tileNumber, adj);
    }).ToHashSet();
  }

  // TODO: Return HashSet instead of List?
  private List<int> findOccupiedAdjacents(int tileNumber)
  {
    return Util.findAdjacents(tileNumber).FindAll(isOccupied);
  }

  private List<int> findUnoccupiedAdjacents(int tileNumber)
  {
    return Util.findAdjacents(tileNumber).FindAll(adj => !isOccupied(adj));
  }

  private HashSet<int> findReachableTilesForAnt(int tileStart)
  {
    Queue<int> queue = new Queue<int>(new int[] { tileStart });
    HashSet<int> seen = new HashSet<int>(new int[] { tileStart });
    HashSet<int> ret = new HashSet<int>();
    while (queue.Count > 0)
    {
      int tile = queue.Dequeue();
      seen.Add(tile);
      foreach (int adj in findImmediateReachablesByPivot(tile))
      {
        if (!seen.Contains(adj))
        {
          queue.Enqueue(adj);
          ret.Add(adj);
        }
      }
    }
    return ret;
  }

  private HashSet<int> findReachableTilesForGh(int tileStart)
  {
    HashSet<int> occupiedAdjacents = findOccupiedAdjacents(tileStart).ToHashSet();
    return occupiedAdjacents.ToList()
      .ConvertAll(adj => Util.findNextInLine(tileStart, adj))
      .FindAll(possibleDest => !isOccupied(possibleDest))
      .ToHashSet();
  }

  private HashSet<int> findReachableTilesForSpider(
    int tileStart,
    HashSet<int> finalReachables = null,
    List<int> path = null)
  {
    if (path == null) path = new List<int>(new int[] { tileStart });
    if (finalReachables == null) finalReachables = new HashSet<int>();
    if (path.Count == SPIDER_DISTANCE + 1)
    {
      finalReachables.Add(tileStart);
      return finalReachables;
    }
    HashSet<int> immediateReachables =
      findImmediateReachablesByPivot(tileStart)
      .ToList().FindAll(adj => !path.Contains(adj)).ToHashSet();
    foreach (int adj in immediateReachables)
    {
      List<int> nextPath = new List<int>(path);
      nextPath.Add(adj);
      findReachableTilesForSpider(adj, finalReachables, nextPath);
    }
    return finalReachables;
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

  private bool isTooNarrow(int tile1, int tile2)
  {
    HashSet<int> adjesInCommon = findAdjacents(tile1).ToHashSet()
      .Intersect(findAdjacents(tile2)).ToHashSet();
    return adjesInCommon.All(isOccupied);
  }

  private void validateAntCanReach(int tileStart, int tileEnd)
  {
    Board boardClone = (Board)this.Clone();
    boardClone.removePiece(tileStart);
    HashSet<int> reachableTiles = boardClone.findReachableTilesForAnt(tileStart);
    if (!reachableTiles.Contains(tileEnd))
    {
      throw new ArgumentException(ErrorMessages.ILLEGAL_MOVE);
    }
  }

  private void validateGhCanReach(int tileStart, int tileEnd)
  {
    HashSet<int> reachableTiles = findReachableTilesForGh(tileStart);
    if (!reachableTiles.Contains(tileEnd))
    {
      throw new ArgumentException(ErrorMessages.ILLEGAL_MOVE);
    }
  }

  private void validateNoAdjacentOppositeColors(int tileNumber, Piece piece)
  {
    HashSet<Piece> adjacentPieces = findOccupiedAdjacents(tileNumber)
      .ConvertAll(adj => getTopPiece(adj)).ToHashSet();
    if (PieceMap.Count > 1 &&
        adjacentPieces.Any(adjPiece => adjPiece.Color != piece.Color))
    {
      throw new ArgumentException(ErrorMessages.PLACEMENT_ADJACENCY);
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

  private void validatePieceCanReach(int tileStart, int tileEnd)
  {
    Piece piece = getTopPiece(tileStart);
    switch (piece.Type)
    {
      case PieceType.Ant:
        validateAntCanReach(tileStart, tileEnd);
        return;
      case PieceType.Spider:
        validateSpiderCanReach(tileStart, tileEnd);
        return;
      case PieceType.Gh:
        validateGhCanReach(tileStart, tileEnd);
        return;
      default:
        return;
    }
  }

  private void validatePieceStacking(int tileStart, int tileEnd)
  {
    Piece piece = getTopPiece(tileStart);
    if (isOccupied(tileEnd) && piece.Type != PieceType.Beetle)
    {
      throw new ArgumentException(ErrorMessages.PIECE_STACKING);
    }
  }

  private void validateSpiderCanReach(int tileStart, int tileEnd)
  {
    Board boardClone = (Board)this.Clone();
    boardClone.removePiece(tileStart);
    HashSet<int> reachableTiles = boardClone.findReachableTilesForSpider(tileStart);
    if (!reachableTiles.Contains(tileEnd))
    {
      throw new ArgumentException(ErrorMessages.ILLEGAL_MOVE);
    }
  }

  private void validateUnoccupied(int tileNumber)
  {
    if (isOccupied(tileNumber))
    {
      throw new ArgumentException(ErrorMessages.TILE_OCCUPIED);
    }
  }
}
