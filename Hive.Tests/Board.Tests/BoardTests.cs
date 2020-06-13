using System;
using System.Collections.Generic;
using Xunit;
using static Board;
using static Piece;

namespace Tests
{
  public class BoardTests
  {
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

      Assert.True(board.getPiecesAt(0)[0] == queen);
      Assert.True(board.getPiecesAt(4)[0] == ant);
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
    public void testMovePiece_multiplePiecesOnTileEnd_isSuccessful()
    {
      Piece beetle = new Piece(PieceType.Beetle, Color.White);
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Board board = new Board();
      board.placePiece(0, beetle);
      board.placePiece(1, queen);

      board.movePiece(0, 1);

      Assert.False(board.isOccupiedAt(0));
      Assert.True(board.isOccupiedAt(1));
      Assert.True(board.getPiecesAt(1)[1] == beetle);
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
      Assert.True(board.getPiecesAt(2)[0] == beetle);
    }

    private Piece newPiece()
    {
      return new Piece(PieceType.Spider, Color.White);
    }
  }
}
