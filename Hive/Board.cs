using System;
using System.Collections.Generic;
using System.Linq;
using static Util;

public class Board : ICloneable
{
  // Maps from piece position to the pieces on that position
  // TODO: remove the get?
  private Dictionary<int, List<Piece>> PieceMap { get; }
  private Dictionary<Color, int?> QueenPositions = new Dictionary<Color, int?> {
    { Color.Black, null },
    { Color.White, null }
  };
  private int SPIDER_DISTANCE = 3;

  public Board()
  {
    PieceMap = new Dictionary<int, List<Piece>>();
  }

  public Board(Dictionary<int, List<Piece>> pieceMap)
  {
    PieceMap = pieceMap;
    setQueenPositions();
  }

  protected Board(Board other)
  {
    PieceMap = Util.cloneDictionary(other.PieceMap);
    QueenPositions = Util.cloneDictionary(other.QueenPositions);
  }

  // Call validatePlacement before this to ensure no messed up board
  public void placePiece(int tileNumber, Piece piece)
  {
    PieceMap[tileNumber] = new List<Piece>(new Piece[] { piece });
    if (piece.Type == PieceType.Q)
    {
      QueenPositions[piece.Color] = tileNumber;
    }
  }

  // Call validateMove before this to ensure no messed up board
  public void movePiece(int tileStart, int tileEnd)
  {
    Piece piece = removePiece(tileStart);
    addPiece(tileEnd, piece);
  }

  public void validateTurn(Turn turn)
  {
    if (turn.Type == TurnType.Move)
    {
      validateMove(turn);
    }
    else
    {
      validatePlacement(turn);
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

  public List<Color> getWinningColors()
  {
    List<Color> winningColors = new List<Color>();
    foreach (KeyValuePair<Color, int?> entry in QueenPositions)
    {
      int? queenPos = entry.Value;
      if (
        queenPos != null &&
        findUnoccupiedAdjacents((int)queenPos).Count == 0)
      {
        winningColors.Add(entry.Key == Color.White ? Color.Black : Color.White);
      }
    }
    return winningColors;
  }

  public object Clone()
  {
    return new Board(this);
  }

  public void printBoard()
  {
    foreach (int tileNumber in PieceMap.Keys)
    {
      Console.WriteLine($"{tileNumber}: {getTopPiece(tileNumber)}");
    }
  }

  // Private methods

  private void setQueenPositions()
  {
    foreach (KeyValuePair<int, List<Piece>> entry in PieceMap)
    {
      foreach (Piece piece in entry.Value)
      {
        if (piece.Type == PieceType.Q)
        {
          QueenPositions[piece.Color] = entry.Key;
        }
      }
    }
  }

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
    if (!isOccupied(tileNumber))
    {
      throw new ArgumentException(
        $"Cannot remove piece on tile {tileNumber}. Tile is unoccupied");
    }
    List<Piece> pieces = PieceMap[tileNumber];
    Piece pieceToRemove = pieces[pieces.Count - 1];
    pieces.RemoveAt(pieces.Count - 1);
    if (pieces.Count == 0)
    {
      PieceMap.Remove(tileNumber);
    }
    return pieceToRemove;
  }

  // Validations

  private void validateCanMoveThisTurn(Color playerColor)
  {
    if (QueenPositions[playerColor] == null)
    {
      throw new ArgumentException(
        ErrorMessages.MUST_PLACE_QUEEN_BEFORE_MOVING);
    }
  }

  private void validateIsMovingOwnPiece(Turn turn)
  {
    if (getTopPiece(turn.TileStart).Color != turn.Player.Color)
    {
      throw new ArgumentException(
        ErrorMessages.CANNOT_MOVE_OPPONENT_PIECE);
    }
  }

  private void validateTileStartOccupied(int tileStart)
  {
    if (!isOccupied(tileStart))
    {
      throw new ArgumentException(ErrorMessages.TILE_START_MUST_BE_OCCUPIED);
    }
  }

  private void validateMove(Turn turn)
  {
    validateTileStartOccupied(turn.TileStart);
    validateCanMoveThisTurn(turn.Player.Color);
    validateIsMovingOwnPiece(turn);
    validateOneHive(turn.TileStart, turn.TileEnd);
    validatePieceStacking(turn.TileStart, turn.TileEnd);
    validatePieceCanReach(turn.TileStart, turn.TileEnd);
  }

  private void validatePlacement(Turn turn)
  {
    validateOneHive(turn.PlacementTile);
    validateUnoccupied(turn.PlacementTile);
    validateNoAdjacentOppositeColors(turn.PlacementTile, turn.Player.Color);
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

  private HashSet<int> findUnoccupiedAdjacentsByPivot(int tileNumber)
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
      foreach (int adj in findUnoccupiedAdjacentsByPivot(tile))
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

  private HashSet<int> findReachableTilesForBeetle(int tileStart)
  {
    HashSet<int> immediateUnoccupiedReachables =
      findUnoccupiedAdjacentsByPivot(tileStart);
    List<int> immediateOccupiedReachables = findOccupiedAdjacents(tileStart);
    List<int> unoccupiedAdjacentsFromOccupiedTile = new List<int>();
    if (isOccupied(tileStart))
    {
      unoccupiedAdjacentsFromOccupiedTile = findUnoccupiedAdjacents(tileStart);
    }
    return immediateUnoccupiedReachables
      .Union(immediateOccupiedReachables)
      .Union(unoccupiedAdjacentsFromOccupiedTile)
      .ToHashSet();
  }

  private HashSet<int> findReachableTilesForGh(int tileStart)
  {
    HashSet<int> occupiedAdjacents = findOccupiedAdjacents(tileStart).ToHashSet();
    return occupiedAdjacents.ToList()
      .ConvertAll(adj => Util.findNextInLine(tileStart, adj))
      .FindAll(possibleDest => !isOccupied(possibleDest))
      .ToHashSet();
  }

  private HashSet<int> findReachableTilesForQueen(
    int tileStart,
    HashSet<int> finalReachables = null,
    List<int> path = null)
  {
    return findUnoccupiedAdjacentsByPivot(tileStart);
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
      findUnoccupiedAdjacentsByPivot(tileStart)
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

  private void validateBeetleCanReach(int tileStart, int tileEnd)
  {
    Board boardClone = (Board)this.Clone();
    boardClone.removePiece(tileStart);
    HashSet<int> reachableTiles = boardClone.findReachableTilesForBeetle(tileStart);
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

  private void validateNoAdjacentOppositeColors(int tileNumber, Color color)
  {
    HashSet<Piece> adjacentPieces = findOccupiedAdjacents(tileNumber)
      .ConvertAll(adj => getTopPiece(adj)).ToHashSet();
    if (PieceMap.Count > 1 &&
        adjacentPieces.Any(adjPiece => adjPiece.Color != color))
    {
      throw new ArgumentException(ErrorMessages.PLACEMENT_ADJACENCY);
    }
  }

  // Validates one hive rule during placement turns
  private void validateOneHive(int placementTile)
  {
    Board boardClone = (Board)this.Clone();
    // Piece type/color doesn't matter here so use arbitrary ones
    boardClone.addPiece(placementTile, new Piece(PieceType.A, Color.White));
    boardClone.checkMultipleIslands();
  }

  // Validates one hive rule during move turns
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
      case PieceType.A:
        validateAntCanReach(tileStart, tileEnd);
        return;
      case PieceType.B:
        validateBeetleCanReach(tileStart, tileEnd);
        return;
      case PieceType.G:
        validateGhCanReach(tileStart, tileEnd);
        return;
      case PieceType.Q:
        validateQueenCanReach(tileStart, tileEnd);
        return;
      case PieceType.S:
        validateSpiderCanReach(tileStart, tileEnd);
        return;
      default:
        return;
    }
  }

  private void validatePieceStacking(int tileStart, int tileEnd)
  {
    Piece piece = getTopPiece(tileStart);
    if (isOccupied(tileEnd) && piece.Type != PieceType.B)
    {
      throw new ArgumentException(ErrorMessages.PIECE_STACKING);
    }
  }

  private void validateQueenCanReach(int tileStart, int tileEnd)
  {
    Board boardClone = (Board)this.Clone();
    boardClone.removePiece(tileStart);
    HashSet<int> reachableTiles = boardClone.findReachableTilesForQueen(tileStart);
    if (!reachableTiles.Contains(tileEnd))
    {
      throw new ArgumentException(ErrorMessages.ILLEGAL_MOVE);
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
