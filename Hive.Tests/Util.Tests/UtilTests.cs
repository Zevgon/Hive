using System;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using static Board;
using static Util;

namespace Tests
{
  public class UnitTest1
  {
    [Fact]
    public void testAdjacents_tile0()
    {
      List<int> expected = new List<int>(
          new int[] { 1, 2, 3, 4, 5, 6 }
      );
      Assert.Equal(expected, Util.findAdjacents(0));
    }

    [Fact]
    public void testAdjacents_tile1()
    {
      List<int> expected = new List<int>(
          new int[] { 7, 8, 2, 0, 6, 18 }
      );
      Assert.Equal(expected, Util.findAdjacents(1));
    }

    [Fact]
    public void testAdjacents_tile2()
    {
      List<int> expected = new List<int>(
          new int[] { 8, 9, 10, 3, 0, 1 }
      );
      Assert.Equal(expected, Util.findAdjacents(2));
    }

    [Fact]
    public void testAdjacents_tile6()
    {
      List<int> expected = new List<int>(
          new int[] { 18, 1, 0, 5, 16, 17 }
      );
      Assert.Equal(expected, Util.findAdjacents(6));
    }

    [Fact]
    public void testAdjacents_tile7()
    {
      List<int> expected = new List<int>(
          new int[] { 19, 20, 8, 1, 18, 36 }
      );
      Assert.Equal(expected, Util.findAdjacents(7));
    }

    [Fact]
    public void testAdjacents_tile8()
    {
      List<int> expected = new List<int>(
          new int[] { 20, 21, 9, 2, 1, 7 }
      );
      Assert.Equal(expected, Util.findAdjacents(8));
    }

    [Fact]
    public void testAdjacents_tile9()
    {
      List<int> expected = new List<int>(
          new int[] { 21, 22, 23, 10, 2, 8 }
      );
      Assert.Equal(expected, Util.findAdjacents(9));
    }

    [Fact]
    public void testAdjacents_tile11()
    {
      List<int> expected = new List<int>(
          new int[] { 10, 24, 25, 26, 12, 3 }
      );
      Assert.Equal(expected, Util.findAdjacents(11));
    }

    [Fact]
    public void testAdjacents_tile16()
    {
      List<int> expected = new List<int>(
          new int[] { 17, 6, 5, 15, 32, 33 }
      );
      Assert.Equal(expected, Util.findAdjacents(16));
    }

    [Fact]
    public void testAdjacents_tile17()
    {
      List<int> expected = new List<int>(
          new int[] { 35, 18, 6, 16, 33, 34 }
      );
      Assert.Equal(expected, Util.findAdjacents(17));
    }

    [Fact]
    public void testAdjacents_tile18()
    {
      List<int> expected = new List<int>(
          new int[] { 36, 7, 1, 6, 17, 35 }
      );
      Assert.Equal(expected, Util.findAdjacents(18));
    }
  }
}
