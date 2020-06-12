using System;
using System.Collections.Generic;
using System.Linq;
using static Tests.TestRunner;

namespace Tests
{
  public class UtilTest : TestRunner
  {
    public void testAdjacents_tile0()
    {
      List<int> expected = new List<int>(
        new int[] {1, 2, 3, 4, 5, 6}
      );
      assertTrue(Util.findAdjacents(0).SequenceEqual(expected));
    }

    public void testAdjacents_tile1()
    {
      List<int> expected = new List<int>(
        new int[] {7, 8, 2, 0, 6, 18}
      );
      assertEquals(Util.findAdjacents(1), expected);
    }

    public void testAdjacents_tile2()
    {
      List<int> expected = new List<int>(
        new int[] {8, 9, 10, 3, 0, 1}
      );
      assertEquals(Util.findAdjacents(2), expected);
    }

    public void testAdjacents_tile7()
    {
      List<int> expected = new List<int>(
        new int[] {19, 20, 8, 1, 18, 36}
      );
      assertEquals(Util.findAdjacents(7), expected);
    }

    public void testAdjacents_tile8()
    {
      List<int> expected = new List<int>(
        new int[] {20, 21, 9, 2, 1, 7}
      );
      assertEquals(Util.findAdjacents(8), expected);
    }

    public void testAdjacents_tile9()
    {
      List<int> expected = new List<int>(
        new int[] {21, 22, 23, 10, 2, 8}
      );
      assertEquals(Util.findAdjacents(9), expected);
    }

    public void testAdjacents_tile11()
    {
      List<int> expected = new List<int>(
        new int[] {10, 24, 25, 26, 12, 3}
      );
      assertEquals(Util.findAdjacents(11), expected);
    }

    public void testAdjacents_tile16()
    {
      List<int> expected = new List<int>(
        new int[] {17, 6, 5, 15, 32, 33}
      );
      assertEquals(Util.findAdjacents(16), expected);
    }

    public void testAdjacents_tile17()
    {
      List<int> expected = new List<int>(
        new int[] {35, 18, 6, 16, 33, 34}
      );
      assertEquals(Util.findAdjacents(17), expected);
    }

    public void testAdjacents_tile18()
    {
      List<int> expected = new List<int>(
        new int[] {36, 7, 1, 6, 17, 35}
      );
      assertEquals(Util.findAdjacents(18), expected);
    }
  }
}
