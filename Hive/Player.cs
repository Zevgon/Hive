using System;
using System.Collections.Generic;

using System.Reflection;
using System.Linq;

// TODO: Where to put this?
namespace EnumOps
{
  public static class EnumExtensionMethods
  {
    public static string GetDescription(this Enum GenericEnum)
    {
      Type genericEnumType = GenericEnum.GetType();
      MemberInfo[] memberInfo = genericEnumType.GetMember(GenericEnum.ToString());
      if ((memberInfo != null && memberInfo.Length > 0))
      {
        var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
        if ((_Attribs != null && _Attribs.Count() > 0))
        {
          return ((System.ComponentModel.DescriptionAttribute)_Attribs.ElementAt(0)).Description;
        }
      }
      return GenericEnum.ToString();
    }

  }
}

public class Player
{
  public Color Color;
  public string Name;

  private int NUM_ANTS = 3;
  private int NUM_GHS = 3;
  private int NUM_BEETLES = 2;
  private int NUM_SPIDERS = 2;
  private int NUM_QUEENS = 1;

  private Dictionary<PieceType, int> PieceCounts;

  public Player(string name, Color color)
  {
    Color = color;
    Name = name;
    PieceCounts =
      new Dictionary<PieceType, int>
      {
        {PieceType.A, NUM_ANTS},
        {PieceType.G, NUM_GHS},
        {PieceType.B, NUM_BEETLES},
        {PieceType.S, NUM_SPIDERS},
        {PieceType.Q, NUM_QUEENS},
      };
  }

  // Call validateHasPiece beforehand to catch errors
  public Piece givePiece(PieceType pieceType)
  {
    PieceCounts[pieceType] -= 1;
    return new Piece(pieceType, Color);
  }

  public void validateHasPiece(PieceType pieceType)
  {
    if (PieceCounts[pieceType] <= 0)
    {
      throw new ArgumentException(
        $"{this} has no more {EnumOps.EnumExtensionMethods.GetDescription(pieceType)}s.");
    }
  }

  override
  public string ToString()
  {
    return Name;
  }
}
