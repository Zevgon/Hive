using System;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using static Board;
using static Utilb;

namespace Stuff.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void testAdjacents_tile0()
        {
            List<int> expected = new List<int>(
                new int[] {1, 2, 3, 4, 5, 6}
            );
            Assert.True(Utilb.findAdjacents(0).SequenceEqual(expected));
        }
    }
}
