using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Tests
{
  public class BoardTests : IDisposable
  {
    private StringWriter consoleOutCatcher;
    private TextWriter originalOutput;

    public BoardTests()
    {
      consoleOutCatcher = new StringWriter();
      originalOutput = Console.Out;
      Console.SetOut(consoleOutCatcher);
    }

    public void Dispose()
    {
      Console.SetOut(originalOutput);
      consoleOutCatcher.Dispose();
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
      Assert.Equal(queen, board.getTopPiece(0));
      Assert.True(boardClone.isOccupied(0));
      Assert.True(boardClone.isOccupied(4));
      // Should be a different queen
      Assert.NotEqual(queen, boardClone.getTopPiece(0));
    }

    [Fact]
    public void testPlacePiece_firstPiece_succeeds()
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
         $"{ErrorMessages.TILE_OCCUPIED}\n", consoleOutCatcher.ToString());
    }

    [Fact]
    public void testPlacePiece_beetle_tileOccupied_fails()
    {
      Piece beetle = new Piece(PieceType.Beetle, Color.White);
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
        }
      );

      board.placePiece(0, beetle);

      Assert.Equal(
         $"{ErrorMessages.TILE_OCCUPIED}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Queen, board.getTopPiece(0).Type);
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
         $"{ErrorMessages.PLACEMENT_ADJACENCY}\n", consoleOutCatcher.ToString());
      Assert.False(board.isOccupied(2));
    }

    [Fact]
    public void testMovePiece_validMove_movesPieceOffOfOriginAndOntoDest()
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
      Assert.Equal(queen, board.getTopPiece(0));
      Assert.Equal(ant, board.getTopPiece(4));
    }

    [Fact]
    public void testMovePiece_multiplePiecesOnOrigin_onlyMovesTopPiece()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {
            new Piece(PieceType.Queen, Color.White),
            new Piece(PieceType.Beetle, Color.White)
          })},
        }
      );

      board.movePiece(1, 2);

      Assert.Equal(PieceType.Queen, board.getTopPiece(1).Type);
      Assert.Equal(PieceType.Beetle, board.getTopPiece(2).Type);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails_1()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
        }
      );

      board.movePiece(0, 2);

      Assert.Equal(
        $"{ErrorMessages.ONE_HIVE}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Queen, board.getTopPiece(0).Type);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails_2()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.Black)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
        }
      );

      board.movePiece(0, 2);

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Queen, board.getTopPiece(0).Type);
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

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", consoleOutCatcher.ToString());
      Assert.Equal(queen, board.getTopPiece(0));
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails_4()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
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

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Queen, board.getTopPiece(0).Type);
    }

    [Fact]
    public void testMovePiece_wouldBreakOneHiveRule_fails_5()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
        }
      );

      board.movePiece(1, 7);

      Assert.Equal($"{ErrorMessages.ONE_HIVE}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Ant, board.getTopPiece(1).Type);
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

      Assert.Equal(PieceType.Ant, board.getTopPiece(7).Type);
      Assert.False(board.isOccupied(1));
    }

    [Fact]
    public void testMovePiece_ant_accrossManageableGap_succeeds()
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
    public void testMovePiece_ant_throughSpaceWithOneEdge_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.Black)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.Black)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
        }
      );

      board.movePiece(6, 2);

      Assert.Equal(
        $"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Ant, board.getTopPiece(6).Type);
      Assert.False(board.isOccupied(2));
    }

    [Fact]
    public void testMovePiece_ant_throughSpaceWithOneEdge_destIsAdjacentToOrigin_fails()
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

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Ant, board.getTopPiece(8).Type);
      Assert.False(board.isOccupied(2));
    }

    [Fact]
    public void testMovePiece_ant_throughSpaceWithTwoEdges_succeeds()
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

      Assert.Equal("", consoleOutCatcher.ToString());
      Assert.False(board.isOccupied(6));
      Assert.Equal(PieceType.Ant, board.getTopPiece(2).Type);
    }

    [Fact]
    public void testMovePiece_beetle_ontoOccupiedTile_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {newPiece()})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
        }
      );

      board.movePiece(1, 0);

      Assert.Equal(PieceType.Beetle, board.getTopPiece(0).Type);
      Assert.False(board.isOccupied(1));
    }

    [Fact]
    public void testMovePiece_beetle_ontoUnoccupiedTile_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {newPiece()})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
        }
      );

      board.movePiece(1, 2);

      Assert.Equal(PieceType.Beetle, board.getTopPiece(2).Type);
      Assert.False(board.isOccupied(1));
    }

    [Fact]
    public void testMovePiece_beetle_multipleTilesAway_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {newPiece()})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
        }
      );

      board.movePiece(1, 3);

      Assert.Equal(
        $"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Beetle, board.getTopPiece(1).Type);
      Assert.False(board.isOccupied(3));
    }

    [Fact]
    public void testMovePiece_beetle_jumpInsteadOfPivot_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
        }
      );

      board.movePiece(8, 9);

      Assert.Equal(
        $"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Beetle, board.getTopPiece(8).Type);
      Assert.False(board.isOccupied(9));
    }

    [Fact]
    public void testMovePiece_beetle_ontoSameColorPiece_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
        }
      );

      board.movePiece(1, 0);

      Assert.Equal(PieceType.Beetle, board.getTopPiece(0).Type);
      Assert.False(board.isOccupied(1));
    }

    [Fact]
    public void testMovePiece_beetle_ontoOppositeColorPiece_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.Black)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
        }
      );

      board.movePiece(1, 0);

      Assert.Equal(PieceType.Beetle, board.getTopPiece(0).Type);
      Assert.False(board.isOccupied(1));
    }

    [Fact]
    public void testMovePiece_beetle_fromOccupiedTileToEmptyTile_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {
            new Piece(PieceType.Queen, Color.White),
            new Piece(PieceType.Beetle, Color.White)
          })},
        }
      );

      board.movePiece(0, 1);

      Assert.Equal(PieceType.Queen, board.getTopPiece(0).Type);
      Assert.Equal(PieceType.Beetle, board.getTopPiece(1).Type);
    }

    [Fact]
    public void testMovePiece_beetle_fromOccupiedTileToOccupiedTile_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {
            new Piece(PieceType.Queen, Color.White),
            new Piece(PieceType.Beetle, Color.White)
          })},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
        }
      );

      board.movePiece(0, 1);

      Assert.Equal(PieceType.Queen, board.getTopPiece(0).Type);
      Assert.Equal(PieceType.Beetle, board.getTopPiece(1).Type);
    }

    [Fact]
    public void testMovePiece_grasshopper_overEmptyTile_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
        }
      );

      board.movePiece(1, 10);

      Assert.Equal(
        $"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Gh, board.getTopPiece(1).Type);
      Assert.False(board.isOccupied(10));
    }

    [Fact]
    public void testMovePiece_grasshopper_ontoAdjacentSpace_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
        }
      );

      board.movePiece(1, 2);

      Assert.Equal(
        $"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Gh, board.getTopPiece(1).Type);
      Assert.False(board.isOccupied(2));
    }

    [Fact]
    public void testMovePiece_grasshopper_ontoOccupiedTile_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
        }
      );

      board.movePiece(1, 4);

      Assert.Equal(
        $"{ErrorMessages.PIECE_STACKING}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Gh, board.getTopPiece(1).Type);
      Assert.Equal(PieceType.Spider, board.getTopPiece(4).Type);
    }

    [Fact]
    public void testMovePiece_grasshopper_overSameColorPiece_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
        }
      );

      board.movePiece(1, 4);

      Assert.Equal(PieceType.Gh, board.getTopPiece(4).Type);
      Assert.False(board.isOccupied(1));
    }

    [Fact]
    public void testMovePiece_grasshopper_overOppositeColorPiece_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.Black)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
        }
      );

      board.movePiece(1, 4);

      Assert.Equal(PieceType.Gh, board.getTopPiece(4).Type);
      Assert.False(board.isOccupied(1));
    }

    [Fact]
    public void testMovePiece_grasshopper_crookedJump_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
        }
      );

      board.movePiece(1, 3);

      Assert.Equal(
        $"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Gh, board.getTopPiece(1).Type);
      Assert.False(board.isOccupied(3));
    }

    [Fact]
    public void testMovePiece_queen_validMove_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.Black)})},
        }
      );

      board.movePiece(0, 2);

      Assert.False(board.isOccupied(0));
      Assert.Equal(PieceType.Queen, board.getTopPiece(2).Type);
    }

    [Fact]
    public void testMovePiece_queen_destOccupied_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.Black)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
        }
      );

      board.movePiece(1, 0);

      Assert.Equal(
        $"{ErrorMessages.PIECE_STACKING}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Spider, board.getTopPiece(0).Type);
      Assert.Equal(PieceType.Queen, board.getTopPiece(1).Type);
    }

    [Fact]
    public void testMovePiece_queen_multipleTilesAway_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.Black)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
        }
      );

      board.movePiece(1, 3);

      Assert.Equal(
        $"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.False(board.isOccupied(3));
      Assert.Equal(PieceType.Queen, board.getTopPiece(1).Type);
    }

    [Fact]
    public void testMovePiece_queen_jumpInsteadOfPivot_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Beetle, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
        }
      );

      board.movePiece(8, 9);

      Assert.Equal(
        $"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.Equal(PieceType.Queen, board.getTopPiece(8).Type);
      Assert.False(board.isOccupied(9));
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

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.False(board.isOccupied(2));
      Assert.Equal(PieceType.Spider, board.getTopPiece(1).Type);
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

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.False(board.isOccupied(3));
      Assert.Equal(PieceType.Spider, board.getTopPiece(1).Type);
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
      Assert.Equal(PieceType.Spider, board.getTopPiece(4).Type);
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

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.False(board.isOccupied(13));
      Assert.Equal(PieceType.Spider, board.getTopPiece(1).Type);
    }

    [Fact]
    public void testMovePiece_spider_backtracking_fails_1()
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
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
        }
      );

      board.movePiece(0, 2);

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.False(board.isOccupied(2));
      Assert.Equal(PieceType.Spider, board.getTopPiece(0).Type);
    }

    [Fact]
    public void testMovePiece_spider_backtracking_fails_2()
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
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
        }
      );

      board.movePiece(0, 3);

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}\n", consoleOutCatcher.ToString());
      Assert.False(board.isOccupied(3));
      Assert.Equal(PieceType.Spider, board.getTopPiece(0).Type);
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
      Assert.Equal(PieceType.Spider, board.getTopPiece(2).Type);
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
      Assert.Equal(PieceType.Spider, board.getTopPiece(3).Type);
    }

    [Fact]
    public void testMovePiece_spider_wouldBlockItselfIfNotRemovedBeforeTransit_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Queen, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {2, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.Ant, Color.White)})},
          {12, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.Gh, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.Spider, Color.White)})},
        }
      );

      board.movePiece(6, 14);

      Assert.False(board.isOccupied(6));
      Assert.Equal(PieceType.Spider, board.getTopPiece(14).Type);
    }

    private Piece newPiece()
    {
      return new Piece(PieceType.Spider, Color.White);
    }
  }
}
