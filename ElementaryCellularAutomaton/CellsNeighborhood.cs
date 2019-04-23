using System;

namespace ElementaryCellularAutomaton
{
    public struct CellsNeighborhood : IEquatable<CellsNeighborhood>
    {
        public bool Left, Center, Right;

        private const int BINARY_NUMERAL_SYSTEM = 2;

        public CellsNeighborhood(string binaryRepresentation)
        {
            switch (binaryRepresentation.Length)
            {
                case 1:
                    Left = false;
                    Center = false;
                    Right = binaryRepresentation[0] == '1';
                    break;
                case 2:
                    Left = false;
                    Center = binaryRepresentation[0] == '1';
                    Right = binaryRepresentation[1] == '1';
                    break;
                case 3:
                    Left = binaryRepresentation[0] == '1';
                    Center = binaryRepresentation[1] == '1';
                    Right = binaryRepresentation[2] == '1';
                    break;
                default:
                    Left = false;
                    Center = false;
                    Right = false;
                    break;
            }
        }

        public CellsNeighborhood(int decimalRepresentation)
            : this(Convert.ToString(decimalRepresentation, BINARY_NUMERAL_SYSTEM)) { }

        public CellsNeighborhood(bool left, bool center, bool right)
        {
            Left = left;
            Center = center;
            Right = right;
        }

        public bool Equals(CellsNeighborhood cellsNeighborhood)
            => cellsNeighborhood.Left == Left &&
            cellsNeighborhood.Center == Center &&
            cellsNeighborhood.Right == Right;

        public override string ToString() => (Left ? "1" : "0") + (Center ? "1" : "0") + (Right ? "1" : "0");
    }
}
