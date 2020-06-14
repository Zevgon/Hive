using System;
using Xunit;
using System.IO;

namespace Tests
{
  public class BoardTests : IDisposable
  {
    // TODO: rename to consoleOutCatcher
    private StringWriter stringWriter;
    private TextWriter originalOutput;

    public BoardTests()
    {
      stringWriter = new StringWriter();
      originalOutput = Console.Out;
      Console.SetOut(stringWriter);
    }

    public void Dispose()
    {
      Console.SetOut(originalOutput);
      stringWriter.Dispose();
    }


    [Fact]
    public void testPlacePiece_validTile_addsToBoard()
    {
      Piece piece = newPiece();
      Board board = new Board();

      board.placePiece(0, piece);

      Assert.True(board.isOccupied(0));
    }

    [Fact]
    public void testPlacePiece_tileOccupied_doesNotAddToBoard()
    {
      Piece piece1 = newPiece();
      Piece piece2 = newPiece();
      Board board = new Board();

      board.placePiece(0, piece1);
      board.placePiece(0, piece2);

      Assert.Single(board.getPieces(0));
    }

    [Fact]
    public void testPlacePiece_nextToOtherColor_fails()
    {
      Piece queenW = new Piece(PieceType.Queen, Color.White);
      Piece queenB = new Piece(PieceType.Queen, Color.Black);
      Piece antW = new Piece(PieceType.Queen, Color.White);
      Board board = new Board();
      board.placePiece(0, queenW);
      board.placePiece(1, queenB);

      board.placePiece(2, antW);

      Assert.Equal(
         $"{ErrorMessages.PLACEMENT_ADJACENCY}\n", stringWriter.ToString());
      Assert.False(board.isOccupied(2));
    }

    [Fact]
    public void testMovePiece_validMove_movesPiece()
    {
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Piece ant = new Piece(PieceType.Ant, Color.White);
      Board board = new Board();
      board.placePiece(0, queen);
      board.placePiece(1, ant);

      board.movePiece(1, 4);

      Assert.True(board.getTopPiece(0) == queen);
      Assert.True(board.getTopPiece(4) == ant);
    }

    [Fact]
    public void testMovePiece_afterMove_tileStartIsUnoccupied()
    {
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Piece ant = new Piece(PieceType.Ant, Color.White);
      Board board = new Board();
      board.placePiece(0, queen);
      board.placePiece(1, ant);

      board.movePiece(1, 4);

      Assert.False(board.isOccupied(1));
    }

    [Fact]
    public void testMovePiece_multiplePiecesOnTileEnd_succeeds()
    {
      Piece beetle = new Piece(PieceType.Beetle, Color.White);
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Board board = new Board();
      board.placePiece(0, beetle);
      board.placePiece(1, queen);

      board.movePiece(0, 1);

      Assert.False(board.isOccupied(0));
      Assert.True(board.isOccupied(1));
      Assert.True(board.getTopPiece(1) == beetle);
      Assert.True(board.getPieces(1)[0] == queen);
    }

    [Fact]
    public void testMovePiece_multiplePiecesOnTileStart_onlyMovesTopPiece()
    {
      Piece beetle = new Piece(PieceType.Beetle, Color.White);
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Board board = new Board();
      board.placePiece(0, beetle);
      board.placePiece(1, queen);

      board.movePiece(0, 1);
      board.movePiece(1, 2);

      Assert.True(board.isOccupied(1));
      Assert.True(board.isOccupied(2));
      Assert.True(board.getTopPiece(2) == beetle);
    }

    [Fact]
    public void testBoardClone()
    {
      Piece ant = new Piece(PieceType.Ant, Color.White);
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Board board = new Board();
      board.placePiece(0, queen);
      board.placePiece(1, ant);
      Board boardClone = (Board)board.Clone();

      boardClone.movePiece(1, 4);

      Assert.True(board.isOccupied(0));
      Assert.True(board.isOccupied(1));
      Assert.True(board.getTopPiece(0) == queen);
      Assert.True(boardClone.isOccupied(0));
      Assert.True(boardClone.isOccupied(4));
      // Should be a different queen
      Assert.False(boardClone.getTopPiece(0) == queen);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails1()
    {
      Piece ant = new Piece(PieceType.Ant, Color.White);
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Piece beetle = new Piece(PieceType.Beetle, Color.White);
      Board board = new Board();
      board.placePiece(0, queen);
      board.placePiece(1, ant);
      board.placePiece(4, beetle);

      board.movePiece(0, 2);

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", stringWriter.ToString());
      Assert.True(board.getTopPiece(0) == queen);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails2()
    {
      // TODO: Be able to initialize board with a specific piece map
      Piece queenW = new Piece(PieceType.Queen, Color.White);
      Piece queenB = new Piece(PieceType.Queen, Color.Black);
      Piece antW = new Piece(PieceType.Ant, Color.White);
      Piece beetleW = new Piece(PieceType.Beetle, Color.White);
      Board board = new Board();
      board.placePiece(0, queenW);
      board.placePiece(1, queenB);
      board.placePiece(3, antW);
      board.placePiece(4, beetleW);

      board.movePiece(0, 2);

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", stringWriter.ToString());
      Assert.True(board.getTopPiece(0) == queenW);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails3()
    {
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Piece ant = new Piece(PieceType.Ant, Color.White);
      Piece beetle1 = new Piece(PieceType.Beetle, Color.White);
      Piece beetle2 = new Piece(PieceType.Beetle, Color.White);
      Piece gh = new Piece(PieceType.Gh, Color.White);
      Board board = new Board();
      board.placePiece(0, queen);
      board.placePiece(1, ant);
      board.placePiece(2, beetle2);
      board.placePiece(3, beetle2);
      board.placePiece(4, gh);
      // Put the beetles onto occupied tiles to maybe mess with piece counts.
      board.movePiece(2, 1);
      board.movePiece(3, 4);

      board.movePiece(0, 2);

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", stringWriter.ToString());
      Assert.True(board.getTopPiece(0) == queen);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails4()
    {
      Piece antW = new Piece(PieceType.Ant, Color.White);
      Piece queenW = new Piece(PieceType.Queen, Color.White);
      Piece beetleW = new Piece(PieceType.Beetle, Color.White);
      Piece ghW = new Piece(PieceType.Gh, Color.White);
      Piece antB = new Piece(PieceType.Ant, Color.Black);
      Piece queenB = new Piece(PieceType.Queen, Color.Black);
      Piece beetleB = new Piece(PieceType.Beetle, Color.Black);
      Piece ghB = new Piece(PieceType.Gh, Color.Black);
      Board board = new Board();
      board.placePiece(0, queenW);
      board.placePiece(1, queenB);
      board.placePiece(3, antW);
      board.placePiece(7, antB);
      board.placePiece(4, beetleW);
      board.placePiece(8, beetleB);
      board.placePiece(5, ghW);
      board.placePiece(20, ghB);

      board.movePiece(0, 2);

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", stringWriter.ToString());
      Assert.True(board.getTopPiece(0) == queenW);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails5()
    {
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Piece ant = new Piece(PieceType.Ant, Color.White);
      Board board = new Board();
      board.placePiece(0, queen);
      board.placePiece(1, ant);

      board.movePiece(1, 7);

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", stringWriter.ToString());
      Assert.True(board.getTopPiece(1) == ant);
    }

    [Fact]
    public void testMovePiece_validBecauseLoopExists_succeeds()
    {
      // TODO
    }

    [Fact]
    public void testMovePiece_nonBeetleOntoOccupiedTile_fails()
    {
      Piece queenW = new Piece(PieceType.Queen, Color.White);
      Piece antW = new Piece(PieceType.Ant, Color.White);
      Board board = new Board();
      board.placePiece(0, queenW);
      board.placePiece(1, antW);

      board.movePiece(1, 0);

      Assert.Equal(
        $"{ErrorMessages.PIECE_STACKING}\n", stringWriter.ToString());
      Assert.True(board.getTopPiece(1) == antW);
    }

    [Fact]
    public void testMovePiece_beetleOntoOccupiedTile_succeeds()
    {
      // TODO
    }

    [Fact]
    public void testMovePiece_grasshopperOverEmptyTile_fails()
    {
      // TODO
    }

    [Fact]
    public void testMovePiece_grasshopperToAdjacentSpace_fails()
    {
      // TODO
    }

    [Fact]
    public void testMovePiece_antThroughSpaceWithOneEdge_fails()
    {
      Piece antW = new Piece(PieceType.Ant, Color.White);
      Piece queenW = new Piece(PieceType.Queen, Color.White);
      Piece beetleW = new Piece(PieceType.Beetle, Color.White);
      Piece ghW = new Piece(PieceType.Gh, Color.White);
      Piece antB = new Piece(PieceType.Ant, Color.Black);
      Piece queenB = new Piece(PieceType.Queen, Color.Black);
      Piece beetleB = new Piece(PieceType.Beetle, Color.Black);
      Piece ghB = new Piece(PieceType.Gh, Color.Black);
      Board board = new Board();
      board.placePiece(0, queenW);
      board.placePiece(1, queenB);
      board.placePiece(3, ghW);
      board.placePiece(8, antB);
      board.placePiece(10, beetleW);
      board.placePiece(6, antW);

      board.movePiece(6, 2);

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", stringWriter.ToString());
      Assert.Equal(board.getTopPiece(6), antW);
      Assert.False(board.isOccupied(2));
    }

    [Fact]
    public void testMovePiece_antThroughSpaceWithOneEdgeNextToOrigin_fails()
    {
      Board board = new Board();
      // 8, 1, 0, 3, 9, 10
      board.placePiece(8, new Piece(PieceType.Ant, Color.White));
      board.placePiece(1, new Piece(PieceType.Queen, Color.White));
      board.placePiece(0, new Piece(PieceType.Beetle, Color.White));
      board.placePiece(3, new Piece(PieceType.Spider, Color.White));
      board.placePiece(9, new Piece(PieceType.Gh, Color.White));
      board.placePiece(10, new Piece(PieceType.Beetle, Color.White));

      board.movePiece(8, 2);

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", stringWriter.ToString());
      Assert.Equal(PieceType.Ant, board.getTopPiece(8).Type);
      Assert.False(board.isOccupied(2));
    }

    [Fact]
    public void testMovePiece_antCrossingManageableGap_succeeds()
    {
      Board board = new Board();
      // 0, 6, 18, 5, 14, 16
      // 2, 8, 10, 11, 12
      board.placePiece(0, new Piece(PieceType.Queen, Color.White));
      board.placePiece(2, new Piece(PieceType.Queen, Color.Black));
      board.placePiece(6, new Piece(PieceType.Ant, Color.White));
      board.placePiece(8, new Piece(PieceType.Ant, Color.Black));
      board.placePiece(18, new Piece(PieceType.Gh, Color.White));
      board.placePiece(10, new Piece(PieceType.Gh, Color.Black));
      board.placePiece(5, new Piece(PieceType.Beetle, Color.White));
      board.placePiece(11, new Piece(PieceType.Beetle, Color.Black));
      board.placePiece(14, new Piece(PieceType.Spider, Color.White));
      board.placePiece(12, new Piece(PieceType.Spider, Color.Black));
      board.placePiece(16, new Piece(PieceType.Ant, Color.White));

      board.movePiece(16, 9);

      Assert.False(board.isOccupied(16));
      Assert.True(board.isOccupied(9));
    }

    [Fact]
    public void testMovePiece_antWouldBlockItselfIfNotRemovedDuringTransit_succeeds()
    {
      Board board = new Board();
      // 8, 1, 0, 3, 10
      board.placePiece(8, new Piece(PieceType.Ant, Color.White));
      board.placePiece(1, new Piece(PieceType.Queen, Color.White));
      board.placePiece(0, new Piece(PieceType.Beetle, Color.White));
      board.placePiece(3, new Piece(PieceType.Spider, Color.White));
      board.placePiece(10, new Piece(PieceType.Gh, Color.White));

      board.movePiece(8, 2);

      Assert.Equal("", stringWriter.ToString());
      Assert.False(board.isOccupied(8));
      Assert.Equal(PieceType.Ant, board.getTopPiece(2).Type);
    }

    [Fact]
    public void testMovePiece_antThroughSpaceWithTwoEdges_succeeds()
    {
      // TODO
    }

    [Fact]
    public void testMovePiece_spiderMoreThanThreeTiles_fails()
    {
      // TODO
    }

    [Fact]
    public void testMovePiece_spiderLessThanThreeTiles_fails()
    {
      // TODO
    }

    [Fact]
    public void testMovePiece_validSpiderMove_succeeds()
    {
      // TODO
    }

    private Piece newPiece()
    {
      return new Piece(PieceType.Spider, Color.White);
    }
  }
}
