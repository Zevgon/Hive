using System;
using Xunit;
using static Board;
using static Piece;

namespace Stuff.Tests
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

            Assert.True(board.getPiecesAt(0).Count == 1);
        }

        private Piece newPiece()
        {
            return new Piece(PieceType.Spider, Color.White);
        }
    }
}
