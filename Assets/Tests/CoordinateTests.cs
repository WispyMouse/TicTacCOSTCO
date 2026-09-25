namespace TicTacCOSTCO.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using TicTacCOSTCO.DataStructures;
    using TicTacCOSTCO.DataStructures.Tools;

    public class CoordinateTests
    {
        public class CoordinatePlusResult_DataSource_Object
        {
            public Coordinate A;
            public Coordinate B;
            public Coordinate Expected;

            public CoordinatePlusResult_DataSource_Object(Coordinate a, Coordinate b, Coordinate expected)
            {
                this.A = a;
                this.B = b;
                this.Expected = expected;
            }

            public override string ToString()
            {
                return $"({A} + {B} = {Expected}";
            }
        }

        public static CoordinatePlusResult_DataSource_Object[] CoordinatePlusResult_DataSource =
        {
            new CoordinatePlusResult_DataSource_Object(new Coordinate(1, 0), new Coordinate(1, 0), new Coordinate(2, 0)),
            new CoordinatePlusResult_DataSource_Object(new Coordinate(1, 0), new Coordinate(1, -5), new Coordinate(2, -5)),
            new CoordinatePlusResult_DataSource_Object(new Coordinate(0, 0), new Coordinate(0, 0), new Coordinate(0, 0)),
            new CoordinatePlusResult_DataSource_Object(new Coordinate(3, 3), new Coordinate(-2, -3), new Coordinate(1, 0)),
            new CoordinatePlusResult_DataSource_Object(new Coordinate(-3, -3), new Coordinate(-2, -3), new Coordinate(-5, -6)),
            new CoordinatePlusResult_DataSource_Object(new Coordinate(1999, -50), new Coordinate(2000, 25), new Coordinate(3999, -25))
        };

        /// <summary>
        /// I expect A + B = C, C - A = B, and C - B = A.
        /// </summary>
        [Test]
        [TestCaseSource(nameof(CoordinatePlusResult_DataSource))]
        public void PlusPlusChecks(CoordinatePlusResult_DataSource_Object plan)
        {
            Coordinate result = plan.A + plan.B;
            Coordinate aComponent = result - plan.B;
            Coordinate bComponent = result - plan.A;
            Assert.AreEqual(plan.Expected, result, $"Expected specific coordinate result.");
            Assert.AreEqual(plan.A, aComponent, $"Should minus back to original coordinate");
            Assert.AreEqual(plan.B, bComponent, $"Should minus back to original coordinate");
        }

        public class CoordinateMultiplyResult_DataSource_Object
        {
            public Coordinate A;
            public int Multiplier;
            public Coordinate Expected;

            public CoordinateMultiplyResult_DataSource_Object(Coordinate a, int multiplier, Coordinate expected)
            {
                this.A = a;
                this.Multiplier = multiplier;
                this.Expected = expected;
            }

            public override string ToString()
            {
                return $"({A} * {Multiplier} = {Expected}";
            }
        }

        public static CoordinateMultiplyResult_DataSource_Object[] CoordinateMultiplyResult_DataSource =
        {
            new CoordinateMultiplyResult_DataSource_Object(new Coordinate(1, 0), 3, new Coordinate(3, 0)),
            new CoordinateMultiplyResult_DataSource_Object(new Coordinate(1, 0), 4, new Coordinate(4, 0)),
            new CoordinateMultiplyResult_DataSource_Object(new Coordinate(0, 0), 5, new Coordinate(0, 0)),
            new CoordinateMultiplyResult_DataSource_Object(new Coordinate(3, 3), 1, new Coordinate(3, 3)),
            new CoordinateMultiplyResult_DataSource_Object(new Coordinate(-3, -3), 10, new Coordinate(-30, -30)),
            new CoordinateMultiplyResult_DataSource_Object(new Coordinate(-3, -3), -10, new Coordinate(30, 30)),
            new CoordinateMultiplyResult_DataSource_Object(new Coordinate(1999, -50), 0, new Coordinate(0, 0))
        };

        /// <summary>
        /// I expect that A * B = C.
        /// If B is not 0, I expect C / B = A.
        /// </summary>
        /// <param name="plan"></param>
        [Test]
        [TestCaseSource(nameof(CoordinateMultiplyResult_DataSource))]
        public void MultiplyCheck(CoordinateMultiplyResult_DataSource_Object plan)
        {
            Coordinate result = plan.A * plan.Multiplier;
            Assert.AreEqual(plan.Expected, result, $"Expected specific coordinate result.");

            if (plan.Multiplier != 0)
            {
                Coordinate back = result / plan.Multiplier;
                Assert.AreEqual(plan.A, back, $"Expected to be able to divide back to original result");
            }
        }
    }

}
