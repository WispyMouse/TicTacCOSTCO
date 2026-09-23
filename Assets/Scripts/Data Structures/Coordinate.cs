namespace TicTacCOSTCO.DataStructures
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Coordinate defining 2D space.
    /// Very, very similar to Vector2Int. Most code lifted from that struct.
    /// This is split out so that we can refer to a coordinate without using Unity's core module.
    /// Caches the HashCode so that this can be used for lookup frequently.
    /// </summary>
    public struct Coordinate : IEquatable<Coordinate>, IFormattable
    {
        public readonly int X;
        public readonly int Y;
        private readonly int HashCode;

        public Coordinate(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.HashCode = GenerateHashCode(this.X, this.Y);
        }

        public static Coordinate operator -(Coordinate v) => new Coordinate(-v.X, -v.Y);
        public static Coordinate operator +(Coordinate a, Coordinate b) => new Coordinate(a.X + b.X, a.Y + b.Y);
        public static Coordinate operator -(Coordinate a, Coordinate b) => new Coordinate(a.X - b.X, a.Y - b.Y);
        public static Coordinate operator *(Coordinate a, Coordinate b) => new Coordinate(a.X * b.X, a.Y * b.Y);
        public static Coordinate operator *(int a, Coordinate b) => new Coordinate(a * b.X, a * b.Y);
        public static Coordinate operator *(Coordinate a, int b) => new Coordinate(a.X * b, a.Y * b);
        public static Coordinate operator /(Coordinate a, int b) => new Coordinate(a.X / b, a.Y / b);
        public static bool operator ==(Coordinate lhs, Coordinate rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y;
        public static bool operator !=(Coordinate lhs, Coordinate rhs) => !(lhs.X == rhs.X && lhs.Y == rhs.Y);

        public override readonly bool Equals(object other)
        {
            if (other is Coordinate v)
                return Equals(in v);
            return false;
        }

        public readonly bool Equals(Coordinate other) => X == other.X && Y == other.Y;
        public readonly bool Equals(in Coordinate other) => X == other.X && Y == other.Y;


        public override readonly string ToString() => ToString(null, null);
        public readonly string ToString(string format) => ToString(format, null);
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider == null)
                formatProvider = CultureInfo.InvariantCulture.NumberFormat;
            return string.Format("({0}, {1})", X.ToString(format, formatProvider), Y.ToString(format, formatProvider));
        }

        public static Coordinate zero { get => s_Zero; }
        public static Coordinate one { get => s_One; }
        public static Coordinate up { get => s_Up; }
        public static Coordinate down { get => s_Down; }
        public static Coordinate left { get => s_Left; }
        public static Coordinate right { get => s_Right; }

        private static readonly Coordinate s_Zero = new Coordinate(0, 0);
        private static readonly Coordinate s_One = new Coordinate(1, 1);
        private static readonly Coordinate s_Up = new Coordinate(0, 1);
        private static readonly Coordinate s_Down = new Coordinate(0, -1);
        private static readonly Coordinate s_Left = new Coordinate(-1, 0);
        private static readonly Coordinate s_Right = new Coordinate(1, 0);

        public override readonly int GetHashCode()
        {
            return this.HashCode;
        }

        private static int GenerateHashCode(int x, int y)
        {
            const int p1 = 73856093;
            const int p2 = 83492791;
            return (x * p1) ^ (y * p2);
        }
    }
}
