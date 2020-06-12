using static Tests.TestRunner;

namespace Tests
{
  public class BoardTest : TestRunner
  {
    private Board board;

    public void setUp()
    {
      board = new Board();
    }

    public void testPlacePiece_validTile_addsToBoard()
    {
      Piece piece = newPiece();

      board.placePiece(0, piece);

      assertTrue(board.isOccupiedAt(0));
    }

    public void testPlacePiece_tileOccupied_doesNotAddToBoard()
    {
      Piece piece1 = newPiece();
      Piece piece2 = newPiece();

      board.placePiece(0, piece1);
      board.placePiece(0, piece2);

      assertTrue(board.getPiecesAt(0).Count == 1);
    }

    // public void testMovePiece_validMove_movesPiece() {
    //   Piece queen = new Piece(PieceType.Queen, Color.White);
    //   Piece ant = new Piece(PieceType.Ant, Color.White);
    //   board.placePiece(0, beetle);
    //   board.placePiece(1, ant);

    //   board.movePiece(1, 4);

    //   assertTrue(board.getPiecesAt(0)[0] == beetle);
    //   assertTrue(board.getPiecesAt(1)[0] == ant);
    // }

    // Creates a default piece. This is so that there's nothing in the above tests that's irrelevant to the test.
    private Piece newPiece()
    {
      return new Piece(PieceType.Spider, Color.White);
    }
  }
}
