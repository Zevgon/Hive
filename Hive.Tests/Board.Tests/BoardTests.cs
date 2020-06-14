using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

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
    public void testPlacePiece_validTile_succeeds()
    {
      Piece piece = newPiece();
      Board board = new Board();

      board.placePiece(0, piece);

      Assert.True(board.isOccupied(0));
    }

    [Fact]
    public void testPlacePiece_tileOccupied_fails()
    {
      Piece piece2 = newPiece();
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {newPiece()})},
        }
      );

      board.placePiece(0, piece2);

      Assert.Equal(
         $"{ErrorMessages.TILE_OCCUPIED}\n", stringWriter.ToString());
    }

    [Fact]
    public void testPlacePiece_nextToOppositeColor_onePiecePlaced_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
        }
      );

      board.placePiece(1, new Piece(PieceType.Queen, Color.Black));

      Assert.True(board.isOccupied(0));
      Assert.True(board.isOccupied(1));
    }

    [Fact]
    public void testPlacePiece_nextToOppositeColor_multiplePiecesPlaced_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] { new Piece(PieceType.Queen, Color.Black)})},
        }
      );

      board.placePiece(2, new Piece(PieceType.Queen, Color.White));

      Assert.Equal(
         $"{ErrorMessages.PLACEMENT_ADJACENCY}\n", stringWriter.ToString());
      Assert.False(board.isOccupied(2));
    }

    [Fact]
    public void testMovePiece_validMove_movesPiece()
    {
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Piece ant = new Piece(PieceType.Ant, Color.White);
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {queen})},
          {1, new List<Piece>(new Piece[] {ant})},
        }
      );

      board.movePiece(1, 4);

      Assert.False(board.isOccupied(1));
      Assert.True(board.getTopPiece(0) == queen);
      Assert.True(board.getTopPiece(4) == ant);
    }

    [Fact]
    public void testMovePiece_beetle_multiplePiecesOnTileEnd_succeeds()
    {
      Piece beetle = new Piece(PieceType.Beetle, Color.White);
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {beetle})},
          {1, new List<Piece>(new Piece[] {queen})},
        }
      );

      board.movePiece(0, 1);

      Assert.False(board.isOccupied(0));
      Assert.True(board.getTopPiece(1) == beetle);
    }

    [Fact]
    public void testMovePiece_multiplePiecesOnTileStart_onlyMovesTopPiece()
    {
      Piece beetle = new Piece(PieceType.Beetle, Color.White);
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {queen, beetle})},
        }
      );

      board.movePiece(1, 2);

      Assert.True(board.isOccupied(1));
      Assert.True(board.isOccupied(2));
      Assert.True(board.getTopPiece(2) == beetle);
    }

    [Fact]
    public void testBoardClone()
    {
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {queen})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
        }
      );
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
    public void testMovePiece_wouldBreakOneHiveRule_fails_1()
    {
      Piece queen = new Piece(PieceType.Queen, Color.White);
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] { queen })},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {4, new List<Piece>(new Piece[] { new Piece(PieceType.Beetle, Color.White)})},
        }
      );

      board.movePiece(0, 2);

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", stringWriter.ToString());
      Assert.True(board.getTopPiece(0) == queen);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails_2()
    {
      Piece queenW = new Piece(PieceType.Queen, Color.White);
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] { queenW})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.Black)})},
          {3, new List<Piece>(new Piece[] { new Piece(PieceType.Ant, Color.White)})},
          {4, new List<Piece>(new Piece[] { new Piece(PieceType.Beetle, Color.White)})},
        }
      );

      board.movePiece(0, 2);

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", stringWriter.ToString());
      Assert.True(board.getTopPiece(0) == queenW);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails_3()
    {
      Piece queen = new Piece(PieceType.Queen, Color.White);
      // Put the beetles onto occupied tiles to try to mess with piece counts.
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
        {0, new List<Piece>(new Piece[] { queen })},
        {1, new List<Piece>(new Piece[] {
          new Piece(PieceType.Ant, Color.White),
          new Piece(PieceType.Beetle, Color.White)})
        },
        {4, new List<Piece>(new Piece[] {
          new Piece(PieceType.Gh, Color.White),
          new Piece(PieceType.Beetle, Color.White)})
        },
        }
      );

      board.movePiece(0, 2);

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", stringWriter.ToString());
      Assert.True(board.getTopPiece(0) == queen);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails_4()
    {
      Piece queenW = new Piece(PieceType.Queen, Color.White);
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {queenW})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.Black)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {7, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.Black)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.Black)})},
          {5, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {20, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.Black)})},
        }
      );

      board.movePiece(0, 2);

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", stringWriter.ToString());
      Assert.True(board.getTopPiece(0) == queenW);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails_5()
    {
      Piece ant = new Piece(PieceType.Ant, Color.White);
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] { ant })},
        }
      );

      board.movePiece(1, 7);

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", stringWriter.ToString());
      Assert.True(board.getTopPiece(1) == ant);
    }

    [Fact]
    public void testMovePiece_validBecauseLoopExists_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
          {9, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
        }
      );

      board.movePiece(1, 7);

      Assert.True(board.getTopPiece(7).Type == PieceType.Ant);
      Assert.False(board.isOccupied(1));
    }

    [Fact]
    public void testMovePiece_nonBeetleOntoOccupiedTile_fails()
    {
      Piece antW = new Piece(PieceType.Ant, Color.White);
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {antW})},
        }
      );

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
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] { new Piece(PieceType.Queen, Color.Black)})},
          {3, new List<Piece>(new Piece[] { new Piece(PieceType.Gh, Color.White)})},
          {8, new List<Piece>(new Piece[] { new Piece(PieceType.Ant, Color.Black)})},
          {10, new List<Piece>(new Piece[] { new Piece(PieceType.Beetle, Color.White)})},
          {6, new List<Piece>(new Piece[] {antW})},
        }
      );

      board.movePiece(6, 2);

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", stringWriter.ToString());
      Assert.Equal(board.getTopPiece(6), antW);
      Assert.False(board.isOccupied(2));
    }

    [Fact]
    public void testMovePiece_antThroughSpaceWithOneEdgeNextToOrigin_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
          {9, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
        }
      );

      board.movePiece(8, 2);

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", stringWriter.ToString());
      Assert.Equal(PieceType.Ant, board.getTopPiece(8).Type);
      Assert.False(board.isOccupied(2));
    }

    [Fact]
    public void testMovePiece_ant_gapSmallEnough_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          // White pieces
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {18, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {5, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
          {14, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
          {16, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          // Black pieces
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.Black)})},
          {2, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.Black)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.Black)})},
          {11, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.Black)})},
          {12, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.Black)})},
        }
      );

      board.movePiece(16, 9);

      Assert.False(board.isOccupied(16));
      Assert.True(board.isOccupied(9));
    }

    [Fact]
    public void testMovePiece_ant_wouldBlockItselfIfNotRemovedDuringTransit_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
        }
      );

      board.movePiece(8, 2);

      Assert.Equal("", stringWriter.ToString());
      Assert.False(board.isOccupied(8));
      Assert.Equal(PieceType.Ant, board.getTopPiece(2).Type);
    }

    [Fact]
    public void testMovePiece_antThroughSpaceWithTwoEdges_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
        }
      );

      board.movePiece(6, 2);

      Assert.Equal("", stringWriter.ToString());
      Assert.False(board.isOccupied(6));
      Assert.Equal(PieceType.Ant, board.getTopPiece(2).Type);
    }

    [Fact]
    public void testMovePiece_spider_oneTileTraveled_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
        }
      );

      board.movePiece(1, 2);

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", stringWriter.ToString());
      Assert.False(board.isOccupied(2));
      Assert.True(board.getTopPiece(1).Type == PieceType.Spider);
    }

    [Fact]
    public void testMovePiece_spider_twoTilesTraveled_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
        }
      );

      board.movePiece(1, 3);

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", stringWriter.ToString());
      Assert.False(board.isOccupied(3));
      Assert.True(board.getTopPiece(1).Type == PieceType.Spider);
    }

    [Fact]
    public void testMovePiece_spider_threeTilesTraveled_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
        }
      );

      board.movePiece(1, 4);

      Assert.False(board.isOccupied(1));
      Assert.True(board.getTopPiece(4).Type == PieceType.Spider);
    }

    [Fact]
    public void testMovePiece_spider_fourTilesTraveled_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
        }
      );

      board.movePiece(1, 13);

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", stringWriter.ToString());
      Assert.False(board.isOccupied(13));
      Assert.True(board.getTopPiece(1).Type == PieceType.Spider);
    }

    [Fact]
    public void testMovePiece_spider_cannotBacktrackOntoOrigin()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {7, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {2, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {12, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {13, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {14, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {5, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
          {18, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
        }
      );

      board.movePiece(0, 1);

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", stringWriter.ToString());
      Assert.False(board.isOccupied(1));
      Assert.True(board.getTopPiece(0).Type == PieceType.Spider);
    }

    [Fact]
    public void testMovePiece_spider_cannotBacktrackOntoFirstVisitedTile()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {7, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {2, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {12, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {13, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {14, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {5, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
          {18, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
        }
      );

      board.movePiece(1, 0);

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", stringWriter.ToString());
      Assert.False(board.isOccupied(0));
      Assert.True(board.getTopPiece(1).Type == PieceType.Spider);
    }

    [Fact]
    public void testMovePiece_spider_tileCanBeReachedWithTwoOrThreeMoves_succeeds_1()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {9, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {11, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {12, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
        }
      );

      board.movePiece(6, 2);

      Assert.False(board.isOccupied(6));
      Assert.True(board.getTopPiece(2).Type == PieceType.Spider);
    }

    [Fact]
    public void testMovePiece_spider_tileCanBeReachedWithTwoOrThreeMoves_succeeds_2()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {9, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {11, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {12, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
        }
      );

      board.movePiece(6, 3);

      Assert.False(board.isOccupied(6));
      Assert.True(board.getTopPiece(3).Type == PieceType.Spider);
    }

    private Piece newPiece()
    {
      return new Piece(PieceType.Spider, Color.White);
    }
  }
}
