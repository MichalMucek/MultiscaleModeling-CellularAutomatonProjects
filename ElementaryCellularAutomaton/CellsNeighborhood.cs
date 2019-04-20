﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementaryCellularAutomaton
{
    public struct CellsNeighborhood : IEquatable<CellsNeighborhood>
    {
        public bool Left, Center, Right;

        private const int BINARY_NUMERAL_SYSTEM = 2;

        public CellsNeighborhood(string binaryRepresentation)
        {
            Left = binaryRepresentation[0] == '1';
            Center = binaryRepresentation[1] == '1';
            Right = binaryRepresentation[2] == '1';
        }

        public CellsNeighborhood(int decimalRepresentation) 
            : this(Convert.ToString(decimalRepresentation, BINARY_NUMERAL_SYSTEM)) { }

        public bool Equals(CellsNeighborhood cellsNeighborhood)
            => cellsNeighborhood.Left == Left &&
            cellsNeighborhood.Center == Center &&
            cellsNeighborhood.Right == Right;
    }
}
