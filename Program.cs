using System;
using System.Collections.Generic;
using Tests;

class MainClass
{
  public static void Main(string[] args)
  {
    new UtilTest().runTests();
    new BoardTest().runTests();

    int tileNumber = 18;
    int x = 40;
    Console.WriteLine(
      $"It is {Util.hasAnyNeighborsGreaterThanX(tileNumber, x)} that {tileNumber} has neighbors greater than {x}.");
  }
}
