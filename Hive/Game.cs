using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum TurnType
{
  Move,
  Placement,
}

public class Turn
{
  public TurnType Type;
  // Will be null if piece type is Move
  public PieceType PieceType;
  // Will be null if piece type is Move
  public int PlacementTile;
  public Player Player;
  // Will be null if piece type is Placement
  public int TileStart;
  // Will be null if piece type is Placement
  public int TileEnd;
  private static string DeserializationPattern = @"^(\d+|\w+):(\d+)$";

  public static Turn createTurn(Player player, string turnStr)
  {
    System.Text.RegularExpressions.Match regexMatch =
      Regex.Match(turnStr, DeserializationPattern);
    if (!regexMatch.Success)
    {
      throw new ArgumentException(
        $"Invalid turn input. Input must match {DeserializationPattern}");
    }
    Turn turn = new Turn();
    turn.Type = Regex.Match(turnStr, @"[A-Za-z]").Success ?
      TurnType.Placement :
      TurnType.Move;
    if (turn.Type == TurnType.Move)
    {
      turn.TileStart = Int32.Parse(regexMatch.Groups[1].Value);
      turn.TileEnd = Int32.Parse(regexMatch.Groups[2].Value);
    }
    else
    {
      turn.PieceType = PieceType.Parse<PieceType>(regexMatch.Groups[1].Value);
      turn.PlacementTile = turn.TileEnd =
        Int32.Parse(regexMatch.Groups[2].Value);
    }
    turn.Player = player;
    return turn;
  }
}

public class Game
{
  private List<Player> Players;
  private Board board = new Board();

  public void start()
  {
    // TODO: implement
    // Players = choosePlayers();
    Players = new List<Player>(new Player[] {
      new Player("Winona", Color.White),
      new Player("Bob", Color.Black),
    });
    int turnIdx = 0;
    // TODO: implement
    // while (!board.isEitherQueenSurrounded())
    while (true)
    {
      Player currentPlayer = Players[turnIdx % Players.Count];
      bool turnSuccess = false;
      while (!turnSuccess)
      {
        // TODO: test for atomicity
        try
        {
          string turnStr = promptTurn(currentPlayer);
          Turn turn = Turn.createTurn(currentPlayer, turnStr);
          turn.Player = currentPlayer;
          validateTurn(turn);
          if (turn.Type == TurnType.Placement)
          {
            board.placePiece(
              turn.PlacementTile, currentPlayer.givePiece(turn.PieceType));
          }
          else
          {
            board.movePiece(turn.TileStart, turn.TileEnd);
          }
          turnSuccess = true;
        }
        catch (ArgumentException e)
        {
          Console.Write(e.Message);
          Console.WriteLine(" Please try again:");
          continue;
        }
      }
      board.printBoard();
      turnIdx++;
    }
  }

  private string promptTurn(Player player)
  {
    Console.WriteLine(
        $"{player}'s turn. Enter turn (examples: 'Q:0', 'A:6', '7:19', '0:5')");
    Console.Write(">>> ");
    return Console.ReadLine();
  }

  private void validateTurn(Turn turn)
  {
    // TODO:
    // Validate whether move or place is allowed
    // Validate that the player is moving their own piece
    board.validateTurn(turn);
    turn.Player.validateHasPiece(turn.PieceType);
  }
}
