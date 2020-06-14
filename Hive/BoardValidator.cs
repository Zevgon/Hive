using System;

public class BoardValidator
{
  public void validateUnoccupied(Board board, int tileNumber)
  {
    if (board.isOccupied(tileNumber))
    {
      throw new ArgumentException(ErrorMessages.TILE_OCCUPIED);
    }
  }
}
