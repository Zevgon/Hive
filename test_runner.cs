using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tests
{
  public class TestRunner
  {
    public void runTests()
    {
      var methods = GetType()
        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .Where(item => item.Name.StartsWith("test"));

      List<System.Reflection.MethodInfo> setupMethodList =
        GetType()
          .GetMethods(
            BindingFlags.Public | BindingFlags.Instance)
          .Where(item => item.Name == "setUp")
          .ToList();
      System.Reflection.MethodInfo setupMethod = null;
      if (setupMethodList.Count() > 0)
      {
        setupMethod = setupMethodList[0];
      }

      foreach (var method in methods)
      {
        try
        {
          if (setupMethod != null)
          {
            setupMethod.Invoke(this, new Object[0]);
          }
          method.Invoke(this, new Object[0]);
          Console.ForegroundColor = ConsoleColor.White;
          Console.WriteLine($"{method.Name} passed!");
        }
        catch (System.Exception)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"{method.Name} failed!");
        }
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("--------------------------------");
      }
    }

    protected void assertTrue(bool boolean)
    {
      if (!boolean)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Assertion error. Expected true but got false.");
        throw new System.Exception();
      }
    }

    protected void assertEquals<T>(List<T> actual, List<T> expected)
    {
      if (!actual.SequenceEqual(expected))
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Lists are not equal. Expected:\n[{String.Join(", ", expected)}]\nbut got:\n[{String.Join(", ", actual)}]");
        throw new System.Exception();
      }
    }
  }
}
