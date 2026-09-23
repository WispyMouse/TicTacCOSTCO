namespace TicTacCOSTCO.DataStructures
{
    using NUnit.Framework;
    using System;

    /// <summary>
    /// Represents a "direction", useful for determining connections.
    /// </summary>
    public struct DirectionalityVector : IEquatable<DirectionalityVector>
    {
        const string NOTZERO = "Directionality cannot be zero";

        public int X;
        public int Y;

        public DirectionalityVector(int x, int y)
        {
            x = Math.Clamp(x, -1, 1);
            y = Math.Clamp(y, -1, 1);

            this.X = x;
            this.Y = y;
        }

        public override readonly bool Equals(object other)
        {
            if (other is DirectionalityVector dv)
            {
                return this == dv;
            }
            return false;
        }

        public readonly bool Equals(DirectionalityVector other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public readonly bool Equals(DirectionalityVector a, DirectionalityVector b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public override readonly int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(DirectionalityVector lhs, DirectionalityVector rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y;
        public static bool operator !=(DirectionalityVector lhs, DirectionalityVector rhs) => !(lhs.X == rhs.X && lhs.Y == rhs.Y);

        public static Coordinate operator *(int a, DirectionalityVector b) => new Coordinate(a * b.X, a * b.Y);
        public static Coordinate operator *(DirectionalityVector a, int b) => new Coordinate(a.X * b, a.Y * b);

        public static DirectionalityVector operator -(DirectionalityVector a) => new DirectionalityVector(a.X * -1, a.Y * -1);
    }
}
