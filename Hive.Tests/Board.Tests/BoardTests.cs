using System;
using Xunit;
using System.IO;

namespace Tests
{
  public class BoardTests : IDisposable
  {
    private StringWriter stringWriter;
    private TextWriter originalOutput;
    private String ONE_HIVE_ERROR =
      "Illegal move. Cannot break the \"One Hive Rule\".\n";
    private String PIECE_STACK_ERROR =
      "Illegal move. Cannot move a piece on top of another piece unless it's a beetle\n";

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

      Assert.True(board.isOccupiedAt(0));
    }

    [Fact]
    public void testPlacePiece_tileOccupied_doesNotAddToBoard()
    {
      Piece piece1 = newPiece();
      Piece piece2 = newPiece();
      Board board = new Board();

      board.placePiece(0, piece1);
      board.placePiece(0, piece2);

      Assert.Single(board.getPiecesAt(0));
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

      Assert.True(board.getTopPieceAt(0) == queen);
      Assert.True(board.getTopPieceAt(4) == ant);
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

      Assert.False(board.isOccupiedAt(1));
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

      Assert.False(board.isOccupiedAt(0));
      Assert.True(board.isOccupiedAt(1));
      Assert.True(board.getTopPieceAt(1) == beetle);
      Assert.True(board.getPiecesAt(1)[0] == queen);
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

      Assert.True(board.isOccupiedAt(1));
      Assert.True(board.isOccupiedAt(2));
      Assert.True(board.getTopPieceAt(2) == beetle);
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

      Assert.True(board.isOccupiedAt(0));
      Assert.True(board.isOccupiedAt(1));
      Assert.True(board.getTopPieceAt(0) == queen);
      Assert.True(boardClone.isOccupiedAt(0));
      Assert.True(boardClone.isOccupiedAt(4));
      // Should be a different queen
      Assert.False(boardClone.getTopPieceAt(0) == queen);
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

      Assert.Equal(ONE_HIVE_ERROR, stringWriter.ToString());
      Assert.True(board.getTopPieceAt(0) == queen);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails2()
    {
      Piece antW = new Piece(PieceType.Ant, Color.White);
      Piece queenW = new Piece(PieceType.Queen, Color.White);
      Piece beetleW = new Piece(PieceType.Beetle, Color.White);
      Piece antB = new Piece(PieceType.Ant, Color.Black);
      Piece queenB = new Piece(PieceType.Queen, Color.Black);
      Board board = new Board();
      board.placePiece(0, queenW);
      board.placePiece(1, queenB);
      board.placePiece(3, antW);
      board.placePiece(4, beetleW);

      board.movePiece(0, 2);

      Assert.Equal(ONE_HIVE_ERROR, stringWriter.ToString());
      Assert.True(board.getTopPieceAt(0) == queenW);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails3()
    {
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Piece ant = new Piece(PieceType.Ant, Color.White);
      Piece beetle1 = new Piece(PieceType.Beetle, Color.White);
      Piece beetle2 = new Piece(PieceType.Beetle, Color.White);
      Piece gh = new Piece(PieceType.Queen, Color.Black);
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

      Assert.Equal(ONE_HIVE_ERROR, stringWriter.ToString());
      Assert.True(board.getTopPieceAt(0) == queen);
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

      Assert.Equal(ONE_HIVE_ERROR, stringWriter.ToString());
      Assert.True(board.getTopPieceAt(0) == queenW);
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

      Assert.Equal(ONE_HIVE_ERROR, stringWriter.ToString());
      Assert.True(board.getTopPieceAt(1) == ant);
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

      Assert.Equal(PIECE_STACK_ERROR, stringWriter.ToString());
      Assert.True(board.getTopPieceAt(1) == antW);
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
      // TODO
    }

    [Fact]
    public void testMovePiece_antFromOutsideEdgeToEnclosedEdge_fails()
    {
      // TODO
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
