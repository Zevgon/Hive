using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Tests
{
  public class BoardTests : IDisposable
  {
    // TODO: remove after adding game tests
    private StringWriter consoleOutCatcher;
    private TextWriter originalOutput;
    private static Random random = new Random();

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
      Piece queen = new Piece(PieceType.Q, Color.White);
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {queen})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
        }
      );
      Board boardClone = (Board)board.Clone();

      boardClone.movePiece(1, 4);

      Assert.True(board.isOccupied(0));
      Assert.True(board.isOccupied(1));
      Assert.Same(queen, board.getTopPiece(0));
      Assert.True(boardClone.isOccupied(0));
      Assert.True(boardClone.isOccupied(4));
      // Should be a different piece object
      Assert.NotSame(queen, boardClone.getTopPiece(0));
    }

    [Fact]
    public void testPlacePiece_succeeds()
    {
      Piece piece = newPiece();
      Board board = new Board();

      board.placePiece(0, piece);

      Assert.True(board.isOccupied(0));
    }

    [Fact]
    public void testPlaceMultiplePieces_succeeds()
    {
      Piece piece1 = newPiece();
      Piece piece2 = newPiece();
      Board board = new Board();

      board.placePiece(0, piece1);
      board.placePiece(1, piece2);

      Assert.True(board.isOccupied(0));
      Assert.True(board.isOccupied(1));
    }

    [Fact]
    public void testValidatePlacement_firstPiece_succeeds()
    {
      Board board = new Board();
      Turn turn = newPlacementTurn(0);

      board.validateTurn(turn);
    }

    [Fact]
    public void testValidatePlacement_nextToOppositeColor_onePiecePlaced_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
        }
      );
      Turn turn = newPlacementTurn(1, Color.Black);

      board.validateTurn(turn);
    }

    [Fact]
    public void testValidatePlacement_tileOccupied_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {newPiece()})},
        }
      );
      Turn turn = newPlacementTurn(0);

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(turn);
        });

      Assert.Equal($"{ErrorMessages.TILE_OCCUPIED}", e.Message);
    }

    [Fact]
    public void testValidatePlacement_beetle_tileOccupied_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {newPiece()})},
        }
      );
      Turn turn = newPlacementTurn(0, PieceType.B);

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(turn);
        });

      Assert.Equal($"{ErrorMessages.TILE_OCCUPIED}", e.Message);
    }

    [Fact]
    public void testValidatePlacement_nextToOppositeColor_multiplePiecesPlaced_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] { new Piece(PieceType.Q, Color.Black)})},
        }
      );
      Turn turn = newPlacementTurn(2, PieceType.B);

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(turn);
        });

      Assert.Equal($"{ErrorMessages.PLACEMENT_ADJACENCY}", e.Message);
    }

    [Fact]
    public void testMovePiece_movesPieceOffOfOriginAndOntoDest()
    {
      Piece queen = new Piece(PieceType.Q, Color.White);
      Piece ant = new Piece(PieceType.A, Color.White);
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
            new Piece(PieceType.Q, Color.White),
            new Piece(PieceType.B, Color.White)
          })},
        }
      );

      board.movePiece(1, 2);

      Assert.Equal(PieceType.Q, board.getTopPiece(1).Type);
      Assert.Equal(PieceType.B, board.getTopPiece(2).Type);
    }

    [Fact]
    public void testMovePiece_destOccupied_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {newPiece(PieceType.B)})},
          {1, new List<Piece>(new Piece[] {newPiece()})},
        }
      );

      board.movePiece(0, 1);

      Assert.Equal(PieceType.B, board.getTopPiece(1).Type);
      Assert.False(board.isOccupied(0));
    }

    [Fact]
    public void testMovePiece_originAndDestOccupied_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {
            newPiece(PieceType.Q),
            newPiece(PieceType.B),
          })},
          {1, new List<Piece>(new Piece[] {newPiece()})},
        }
      );

      board.movePiece(0, 1);

      Assert.Equal(PieceType.B, board.getTopPiece(1).Type);
      Assert.Equal(PieceType.Q, board.getTopPiece(0).Type);
    }

    [Fact]
    public void testValidateMove_wouldBreakOneHiveRule_fails_1()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(0, 2, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ONE_HIVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_wouldBreakOneHiveRule_fails_2()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.Black)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(0, 2, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ONE_HIVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_wouldBreakOneHiveRule_fails_3()
    {
      // Put the beetles onto occupied tiles to try to mess with piece counts.
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {
            new Piece(PieceType.G, Color.White)
           })},
          {1, new List<Piece>(new Piece[] {
            new Piece(PieceType.Q, Color.White),
            new Piece(PieceType.B, Color.White)
          })},
          {4, new List<Piece>(new Piece[] {
            newPiece(),
            new Piece(PieceType.B, Color.White)
          })},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(0, 2, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ONE_HIVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_wouldBreakOneHiveRule_fails_4()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.Black)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {7, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.Black)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.Black)})},
          {5, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {20, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.Black)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(0, 2, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ONE_HIVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_wouldBreakOneHiveRule_fails_5()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(1, 7, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ONE_HIVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_validBecauseLoopExists_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
          {9, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
        }
      );

      board.validateTurn(newMoveTurn(1, 7, Color.White));
    }

    [Fact]
    public void testValidateMove_ant_accrossManageableGap_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          // White pieces
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {18, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {5, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
          {14, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
          {16, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          // Black pieces
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.Black)})},
          {2, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.Black)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.Black)})},
          {11, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.Black)})},
          {12, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.Black)})},
        }
      );

      board.validateTurn(newMoveTurn(16, 9, Color.White));
    }

    [Fact]
    public void testValidateMove_ant_throughSpaceWithOneEdge_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.Black)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.Black)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(6, 2, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValieateMove_ant_throughSpaceWithOneEdge_destIsAdjacentToOrigin_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
          {9, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(8, 2, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_ant_throughSpaceWithTwoEdges_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
        }
      );

      board.validateTurn(newMoveTurn(6, 2, Color.White));
    }

    [Fact]
    public void testValidateMove_beetle_ontoOccupiedTile_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
        }
      );

      board.validateTurn(newMoveTurn(1, 0, Color.White));
    }

    [Fact]
    public void testValidateMove_beetle_ontoUnoccupiedTile_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
        }
      );

      board.validateTurn(newMoveTurn(1, 2, Color.White));
    }

    [Fact]
    public void testValidateMove_beetle_multipleTilesAway_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(1, 3, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_beetle_jumpInsteadOfPivot_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(8, 9, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_beetle_ontoSameColorPiece_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
        }
      );

      board.validateTurn(newMoveTurn(1, 0, Color.White));
    }

    [Fact]
    public void testValidateMove_beetle_ontoOppositeColorPiece_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.Black)})},
          {2, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
        }
      );

      board.validateTurn(newMoveTurn(1, 0, Color.White));
    }

    [Fact]
    public void testValidateMove_beetle_fromOccupiedTileToEmptyTile_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {
            new Piece(PieceType.Q, Color.White),
            new Piece(PieceType.B, Color.White)
          })},
        }
      );

      board.validateTurn(newMoveTurn(0, 1, Color.White));
    }

    [Fact]
    public void testValidateMove_beetle_fromOccupiedTileToOccupiedTile_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {
            new Piece(PieceType.Q, Color.White),
            new Piece(PieceType.B, Color.White)
          })},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
        }
      );

      board.validateTurn(newMoveTurn(0, 1, Color.White));
    }

    [Fact]
    public void testValidateMove_grasshopper_overEmptyTile_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(1, 10, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_grasshopper_ontoAdjacentSpace_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(1, 2, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_grasshopper_ontoOccupiedTile_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(1, 4, Color.White));
        });

      Assert.Equal($"{ErrorMessages.PIECE_STACKING}", e.Message);
    }

    [Fact]
    public void testValidateMove_grasshopper_overSameColorPiece_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
        }
      );

      board.validateTurn(newMoveTurn(1, 4, Color.White));
    }

    [Fact]
    public void testValidateMove_grasshopper_overOppositeColorPiece_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.Black)})},
          {2, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
        }
      );

      board.validateTurn(newMoveTurn(1, 4, Color.White));
    }

    [Fact]
    public void testValidateMove_grasshopper_crookedJump_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(1, 3, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_queen_validMove_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.Black)})},
        }
      );

      board.validateTurn(newMoveTurn(0, 2, Color.White));
    }

    [Fact]
    public void testValidateMove_queen_destOccupied_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.Black)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(1, 0, Color.White));
        });

      Assert.Equal($"{ErrorMessages.PIECE_STACKING}", e.Message);
    }

    [Fact]
    public void testValidateMove_queen_multipleTilesAway_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.Black)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(1, 3, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_queen_jumpInsteadOfPivot_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(8, 9, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_spider_oneTileTraveled_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(1, 2, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_spider_twoTileTraveled_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(1, 3, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_spider_threeTilesTraveled_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
        }
      );

      board.validateTurn(newMoveTurn(1, 4, Color.White));
    }

    [Fact]
    public void testValidateMove_spider_fourTilesTraveled_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.B, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(1, 13, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_spider_backtracking_fails_1()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {9, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {11, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {12, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(0, 2, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_spider_backtracking_fails_2()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {9, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {11, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {12, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(0, 3, Color.White));
        });

      Assert.Equal($"{ErrorMessages.ILLEGAL_MOVE}", e.Message);
    }

    [Fact]
    public void testValidateMove_spider_tileCanBeReachedWithTwoOrThreeMoves_succeeds_1()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {9, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {11, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {12, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
        }
      );

      board.validateTurn(newMoveTurn(6, 2, Color.White));
    }

    [Fact]
    public void testValidateMove_spider_tileCanBeReachedWithTwoOrThreeMoves_succeeds_2()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {9, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {10, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {11, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {12, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
        }
      );

      board.validateTurn(newMoveTurn(6, 3, Color.White));
    }

    [Fact]
    public void testValidateMove_spider_wouldBlockItselfIfNotRemovedBeforeTransit_succeeds()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {8, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {2, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {3, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {12, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {4, new List<Piece>(new Piece[] {new Piece(PieceType.G, Color.White)})},
          {6, new List<Piece>(new Piece[] {new Piece(PieceType.S, Color.White)})},
        }
      );

      board.validateTurn(newMoveTurn(6, 14, Color.White));
    }

    [Fact]
    public void testValidateMove_playerTriesToMoveOpponentPiece_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.Black)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(1, 2, Color.White));
        });

      Assert.Equal($"{ErrorMessages.CANNOT_MOVE_OPPONENT_PIECE}", e.Message);
    }

    [Fact]
    public void testValidateMove_tileStartUnoccupied_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.Black)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(2, 3, Color.White));
        });

      Assert.Equal($"{ErrorMessages.TILE_START_MUST_BE_OCCUPIED}", e.Message);
    }

    [Fact]
    public void testValidateMove_queenNotPlaced_fails()
    {
      Board board = new Board(
        new Dictionary<int, List<Piece>>
        {
          {0, new List<Piece>(new Piece[] {new Piece(PieceType.A, Color.White)})},
          {1, new List<Piece>(new Piece[] {new Piece(PieceType.Q, Color.Black)})},
        }
      );

      ArgumentException e = Assert.Throws<ArgumentException>(() =>
        {
          board.validateTurn(newMoveTurn(0, 2, Color.White));
        });

      Assert.Equal(
        $"{ErrorMessages.MUST_PLACE_QUEEN_BEFORE_MOVING}", e.Message);
    }

    private Piece newPiece()
    {
      return new Piece(RandomPieceType(), RandomColor());
    }

    private Piece newPiece(PieceType pieceType)
    {
      return new Piece(pieceType, RandomColor());
    }

    public static string RandomString(int length = 5)
    {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    // TODO: generic to any enum?? how?
    public static PieceType RandomPieceType()
    {
      Array values = Enum.GetValues(typeof(PieceType));
      Random random = new Random();
      return (PieceType)values.GetValue(random.Next(values.Length));
    }

    public static Color RandomColor()
    {
      Array values = Enum.GetValues(typeof(Color));
      Random random = new Random();
      return (Color)values.GetValue(random.Next(values.Length));
    }

    public static Player newPlayer()
    {
      return new Player(RandomString(), RandomColor());
    }

    public static Player newPlayer(Color color)
    {
      return new Player(RandomString(), color);
    }

    public static Turn newTurn()
    {
      Turn turn = new Turn();
      turn.Player = newPlayer();
      return turn;
    }

    public static Turn newMoveTurn(int tileStart, int tileEnd, Color color)
    {
      Turn turn = new Turn();
      turn.Type = TurnType.Move;
      turn.TileStart = tileStart;
      turn.TileEnd = tileEnd;
      turn.Player = newPlayer(color);
      return turn;
    }

    public static Turn newPlacementTurn(int placementTile)
    {
      Turn turn = new Turn();
      turn.PieceType = RandomPieceType();
      turn.PlacementTile = placementTile;
      turn.Type = TurnType.Placement;
      turn.Player = newPlayer();
      return turn;
    }

    public static Turn newPlacementTurn(int placementTile, Color color)
    {
      Turn turn = new Turn();
      turn.PieceType = RandomPieceType();
      turn.PlacementTile = placementTile;
      turn.Type = TurnType.Placement;
      turn.Player = newPlayer(color);
      return turn;
    }

    public static Turn newPlacementTurn(int placementTile, PieceType pieceType)
    {
      Turn turn = new Turn();
      turn.PieceType = pieceType;
      turn.PlacementTile = placementTile;
      turn.Type = TurnType.Placement;
      turn.Player = newPlayer();
      return turn;
    }
  }
}
