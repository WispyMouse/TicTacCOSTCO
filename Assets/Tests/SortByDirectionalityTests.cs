namespace TicTacCOSTCO.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using TicTacCOSTCO.DataStructures;

    public class SortByDirectionalityTests
    {
        public class SortByDirectionality_Ordered_DataSource_Object
        {
            public List<Coordinate> Entries;
            public DirectionalityVector Directionality;
            public List<Coordinate> ExpectedResults;

            public SortByDirectionality_Ordered_DataSource_Object(List<Coordinate> entries, DirectionalityVector directionality, List<Coordinate> expectedResults)
            {
                this.Entries = entries;
                this.Directionality = directionality;
                this.ExpectedResults = expectedResults;
            }

            public override string ToString()
            {
                return $"({Directionality}) => ({string.Join(", ", ExpectedResults.Select(x => x))}";
            }
        }

        public static SortByDirectionality_Ordered_DataSource_Object[] SortByDirectionality_Ordered_DataSource =
        {
            // Already ordered list stays ordered
            new SortByDirectionality_Ordered_DataSource_Object(
                new List<Coordinate>() { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) },
                new DirectionalityVector(1, 0),
                new List<Coordinate>() { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) }
                ),
            new SortByDirectionality_Ordered_DataSource_Object(
                new List<Coordinate>() { new Coordinate(0, 0), new Coordinate(0, 1), new Coordinate(0, 2) },
                new DirectionalityVector(1, 0),
                new List<Coordinate>() { new Coordinate(0, 0), new Coordinate(0, 1), new Coordinate(0, 2) }
                ),
            new SortByDirectionality_Ordered_DataSource_Object(
                new List<Coordinate>() { new Coordinate(2, 0), new Coordinate(1, 0), new Coordinate(0, 0) },
                new DirectionalityVector(-1, 0),
                new List<Coordinate>() { new Coordinate(2, 0), new Coordinate(1, 0), new Coordinate(0, 0) }
                ),
            new SortByDirectionality_Ordered_DataSource_Object(
                new List<Coordinate>() { new Coordinate(0, 0), new Coordinate(1, 1), new Coordinate(2, 2) },
                new DirectionalityVector(1, 1),
                new List<Coordinate>() { new Coordinate(0, 0), new Coordinate(1, 1), new Coordinate(2, 2) }
                ),
            new SortByDirectionality_Ordered_DataSource_Object(
                new List<Coordinate>() { new Coordinate(2, 0), new Coordinate(1, 1), new Coordinate(0, 2) },
                new DirectionalityVector(-1, 1),
                new List<Coordinate>() { new Coordinate(2, 0), new Coordinate(1, 1), new Coordinate(0, 2) }
                ),

            // Reverse list
            new SortByDirectionality_Ordered_DataSource_Object(
                new List<Coordinate>() { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) },
                new DirectionalityVector(-1, 0),
                new List<Coordinate>() { new Coordinate(2, 0), new Coordinate(1, 0), new Coordinate(0, 0) }
                ),
            new SortByDirectionality_Ordered_DataSource_Object(
                new List<Coordinate>() { new Coordinate(0, 0), new Coordinate(1, 1), new Coordinate(2, 2) },
                new DirectionalityVector(-1, -1),
                new List<Coordinate>() { new Coordinate(2, 2), new Coordinate(1, 1), new Coordinate(0, 0) }
                ),

            // Removes redundant pieces
            new SortByDirectionality_Ordered_DataSource_Object(
                new List<Coordinate>() { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 0), new Coordinate(2, 0), new Coordinate(0, 0) },
                new DirectionalityVector(1, 0),
                new List<Coordinate>() { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) }
                ),
        };

        [Test]
        [TestCaseSource(nameof(SortByDirectionality_Ordered_DataSource))]
        public void SortByDirectionalityAsExpected(SortByDirectionality_Ordered_DataSource_Object plan)
        {
            List<Coordinate> ordered = BoardState.SortByDirectionality(plan.Entries, plan.Directionality);

            Assert.AreEqual(plan.ExpectedResults.Count, ordered.Count, $"Expected array in particular length");

            for (int ii = 0, count = ordered.Count; ii < count; ii++)
            {
                Assert.AreEqual(plan.ExpectedResults[ii], ordered[ii], $"Expected array in particular order");
            }
        }
    }

}
