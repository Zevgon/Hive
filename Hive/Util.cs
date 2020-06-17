using System;
using System.Collections.Generic;
using System.Linq;

public class Util
{
  // TODO: return HashSet instead of List?
  public static List<int> findAdjacents(int tileNumber)
  {
    if (tileNumber == 0)
    {
      return new List<int>(new int[] { 1, 2, 3, 4, 5, 6 });
    }

    int edgeIdx = getEdgeIdx(tileNumber);
    int ringNumber = getRing(tileNumber);
    List<int> neighbors = new List<int>(new int[6]);

    populateOuterRingNeighbors(
      tileNumber, neighbors, edgeIdx, ringNumber
    );
    populateSameRingNeighbors(
      tileNumber, neighbors, edgeIdx, ringNumber
    );
    populateInnerRingNeighbors(
      tileNumber, neighbors, edgeIdx, ringNumber
    );
    return neighbors;
  }

  // Returns the next tile in the indicated direction if you start at tile1
  // and draw a line to tile2. E.g. if tile1 is 0 and tile2 is 2, this should
  // return 9.
  public static int findNextInLine(int tile1, int tile2)
  {
    List<int> tile1Adjes = findAdjacents(tile1);
    return findAdjacents(tile2)[tile1Adjes.IndexOf(tile2)];
  }

  // TODO: make generic??
  public static Dictionary<int, List<Piece>> cloneDictionary(
    Dictionary<int, List<Piece>> original)
  {
    Dictionary<int, List<Piece>> ret = new Dictionary<int, List<Piece>>(
      original.Count, original.Comparer);
    foreach (KeyValuePair<int, List<Piece>> entry in original)
    {
      ret.Add(entry.Key, cloneList(entry.Value));
    }
    return ret;
  }

  public static Dictionary<Color, int?> cloneDictionary(
    Dictionary<Color, int?> original)
  {
    Dictionary<Color, int?> ret = new Dictionary<Color, int?>(
      original.Count, original.Comparer);
    foreach (KeyValuePair<Color, int?> entry in original)
    {
      ret.Add(entry.Key, entry.Value);
    }
    return ret;
  }

  public static List<Piece> cloneList(List<Piece> list)
  {
    return list.ConvertAll(piece => (Piece)piece.Clone());
  }

  // Private methods

  private static void populateOuterRingNeighbors(
    int tileNumber,
    List<int> neighbors,
    int edgeIdx,
    int ringNumber)
  {
    neighbors[edgeIdx % 6] = tileNumber + 6 * ringNumber + edgeIdx;
    neighbors[(edgeIdx + 1) % 6] =
      tileNumber + 6 * ringNumber + 1 + edgeIdx;
    if (isCorner(tileNumber))
    {
      neighbors[positiveMod(edgeIdx - 1, 6)] =
        tileNumber + 6 * ringNumber + edgeIdx - 1;
      if (edgeIdx == 0) neighbors[5] += 6 * (ringNumber + 1);
    }
  }

  private static void populateSameRingNeighbors(
    int tileNumber,
    List<int> neighbors,
    int edgeIdx,
    int ringNumber)
  {
    neighbors[(edgeIdx + 2) % 6] = tileNumber + 1;

    // TODO: can probably clean up ifs/elses some more?
    if (isCorner(tileNumber))
    {
      neighbors[(edgeIdx + 4) % 6] = tileNumber - 1;
      // TODO: solve this with more modulo logic?
      if (edgeIdx == 0) neighbors[4] += ringNumber * 6;
      if (isLastOfRing(tileNumber, ringNumber)) neighbors[1] -= ringNumber * 6;
    }
    else
    {
      neighbors[(edgeIdx + 5) % 6] = tileNumber - 1;
      if (isLastOfRing(tileNumber, ringNumber))
      {
        neighbors[1] -= 6 * ringNumber;
      }
    }
  }

  private static void populateInnerRingNeighbors(
    int tileNumber,
    List<int> neighbors,
    int edgeIdx,
    int ringNumber)
  {
    if (isCorner(tileNumber))
    {
      neighbors[(edgeIdx + 3) % 6] =
        getFirstOfRing(ringNumber - 1) + edgeIdx * (ringNumber - 1);
    }
    else
    {
      neighbors[(edgeIdx + 3) % 6] =
        tileNumber - 6 * (ringNumber - 1) - edgeIdx;
      neighbors[(edgeIdx + 4) % 6] =
        tileNumber - 6 * (ringNumber - 1) - edgeIdx - 1;
      // TODO: solve this with more modulo logic?
      if (isLastOfRing(tileNumber, ringNumber))
      {
        neighbors[2] -= 6 * (ringNumber - 1);
      }
    }
  }

  // A corner tile is one that has 1 inner-ring neighbor, 2 same-ring neighbors and 3 outer-ring neighbors. An edge tile is one that has 2 of each.
  private static bool isCorner(int tileNumber)
  {
    int ringNumber = getRing(tileNumber);
    int offset = getOffsetFromFirstOfRing(tileNumber);
    return offset % ringNumber == 0;
  }

  private static bool isLastOfRing(
    int tileNumber,
    int ringNumber)
  {
    int ringFirst = getFirstOfRing(ringNumber);
    return tileNumber == ringFirst + 6 * ringNumber - 1;
  }

  // Returns which ring edge a tile is on. E.g. 19, 20, and 21 are on edge 0. 22, 23, and 24 are on edge 1. 25, 26, and 27 are on edge 2, etc.
  private static int getEdgeIdx(int edgeTile)
  {
    int ringNumber = getRing(edgeTile);
    int offset = getOffsetFromFirstOfRing(edgeTile);
    return offset / ringNumber;
  }

  private static int getFirstOfRing(int ringNumber)
  {
    // See above.
    if (ringNumber == 0) return 0;
    return 3 * ringNumber * (ringNumber - 1) + 1;
  }

  private static int getOffsetFromFirstOfRing(int tileNumber)
  {
    return tileNumber - getFirstOfRing(getRing(tileNumber));
  }

  // The equation for getting the first of a ring is 3r(r - 1) + 1 = firstOfRing, so this basically does the inverse of that and then floors the answer, so:
  // 3r^2 - 3r + 1 - firstOfRing = 0
  // r = (3 +- sqrt(9 - 4*3*(1 - tileNumber)) / 2*3
  // r = (3 + sqrt(-3 + 12tileNumber)) / 6
  private static int getRing(int tileNumber)
  {
    return (int)(3 + Math.Sqrt(-3 + 12 * tileNumber)) / 6;
  }

  // By default in C#, -1 % 3 = -1. I've only ever seen that cause problems, never seen it help anyone. -1 % 3 should be 2.
  private static int positiveMod(int x, int m)
  {
    return (x % m + m) % m;
  }

  public static List<int> findAdjacents2(int tileNumber)
  {
    int adjacent0;
    int adjacent1;
    int adjacent2;
    int adjacent3;
    int adjacent4;
    int adjacent5;

    int ringValence;
    int adjacentStart;
    int ringNumber = 1;
    int ringFirst = 1;
    while (ringFirst < tileNumber)
    {
      ringFirst += (6 * ringNumber);
      ringNumber++;
    }
    ringNumber--;
    ringFirst -= (6 * ringNumber);
    ringValence = tileNumber - ringFirst;
    adjacentStart = (ringValence / ringNumber);
    if (adjacentStart == 0)
    {
      adjacent0 = (tileNumber + (ringNumber * 6) + (ringValence / ringNumber));
      adjacent1 = (tileNumber + (ringNumber * 6) + (ringValence / ringNumber) + 1);
      adjacent2 = (tileNumber + 1);
      adjacent3 = (tileNumber - ((ringNumber - 1) * 6) - (ringValence / ringNumber));
      adjacent4 = (tileNumber - ((ringNumber - 1) * 6) - (ringValence / ringNumber) - 1);
      adjacent5 = (tileNumber - 1);
    }
    else if (adjacentStart == 1)
    {
      adjacent1 = (tileNumber + (ringNumber * 6) + (ringValence / ringNumber));
      adjacent2 = (tileNumber + (ringNumber * 6) + (ringValence / ringNumber) + 1);
      adjacent3 = (tileNumber + 1);
      adjacent4 = (tileNumber - ((ringNumber - 1) * 6) - (ringValence / ringNumber));
      adjacent5 = (tileNumber - ((ringNumber - 1) * 6) - (ringValence / ringNumber) - 1);
      adjacent0 = (tileNumber - 1);
    }
    else if (adjacentStart == 2)
    {
      adjacent2 = (tileNumber + (ringNumber * 6) + (ringValence / ringNumber));
      adjacent3 = (tileNumber + (ringNumber * 6) + (ringValence / ringNumber) + 1);
      adjacent4 = (tileNumber + 1);
      adjacent5 = (tileNumber - ((ringNumber - 1) * 6) - (ringValence / ringNumber));
      adjacent0 = (tileNumber - ((ringNumber - 1) * 6) - (ringValence / ringNumber) - 1);
      adjacent1 = (tileNumber - 1);
    }
    else if (adjacentStart == 3)
    {
      adjacent3 = (tileNumber + (ringNumber * 6) + (ringValence / ringNumber));
      adjacent4 = (tileNumber + (ringNumber * 6) + (ringValence / ringNumber) + 1);
      adjacent5 = (tileNumber + 1);
      adjacent0 = (tileNumber - ((ringNumber - 1) * 6) - (ringValence / ringNumber));
      adjacent1 = (tileNumber - ((ringNumber - 1) * 6) - (ringValence / ringNumber) - 1);
      adjacent2 = (tileNumber - 1);
    }
    else if (adjacentStart == 4)
    {
      adjacent4 = (tileNumber + (ringNumber * 6) + (ringValence / ringNumber));
      adjacent5 = (tileNumber + (ringNumber * 6) + (ringValence / ringNumber) + 1);
      adjacent0 = (tileNumber + 1);
      adjacent1 = (tileNumber - ((ringNumber - 1) * 6) - (ringValence / ringNumber));
      adjacent2 = (tileNumber - ((ringNumber - 1) * 6) - (ringValence / ringNumber) - 1);
      adjacent3 = (tileNumber - 1);
    }
    else
    {
      adjacent5 = (tileNumber + (ringNumber * 6) + (ringValence / ringNumber));
      adjacent0 = (tileNumber + (ringNumber * 6) + (ringValence / ringNumber) + 1);
      adjacent1 = (tileNumber + 1);
      adjacent2 = (tileNumber - ((ringNumber - 1) * 6) - (ringValence / ringNumber));
      adjacent3 = (tileNumber + ((ringNumber - 1) * 6) + (ringValence / ringNumber) - 1);
      adjacent4 = (tileNumber - 1);
    }

    return new List<int>(new int[] {
      adjacent0,
      adjacent1,
      adjacent2,
      adjacent3,
      adjacent4,
      adjacent5});
  }

  // Function passing demo

  private delegate bool ConditionFunc(int adjacentTileNumber);

  private static bool checkAdjacents(
    int tileNumber, ConditionFunc condition)
  {
    return findAdjacents(tileNumber).Any(adjacent => condition(adjacent));
  }

  public static bool hasAnyNeighborsGreaterThanX(int origin, int x)
  {
    return checkAdjacents(origin, neighbor => neighbor > x);
  }
}
