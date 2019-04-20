using System;
using System.Collections.Generic;

namespace ElementaryCellularAutomaton.Models
{
    class RuleModel
    {
        private readonly string _binaryRepresentation;
        private readonly string _decimalRepresentation;
        public readonly Dictionary<CellsNeighborhood, bool> Table;

        private const int DECIMAL_NUMERAL_SYSTEM = 10;
        private const int BINARY_NUMERAL_SYSTEM = 2;

        public RuleModel(int base10)
        {
            _binaryRepresentation = Convert.ToString(base10, BINARY_NUMERAL_SYSTEM);
            _decimalRepresentation = base10.ToString();

            SetTable();
        }

        public RuleModel(string base2)
        {
            _binaryRepresentation = base2;
            _decimalRepresentation = Convert.ToString(Convert.ToInt32(base2, BINARY_NUMERAL_SYSTEM), DECIMAL_NUMERAL_SYSTEM);

            SetTable();
        }

        private void SetTable()
        {
            for (int neighborhoodIndex = 7, binRepIndex = 0;
                neighborhoodIndex >= 0;
                neighborhoodIndex--, binRepIndex++)
            {
                Table.Add(new CellsNeighborhood(neighborhoodIndex), _binaryRepresentation[binRepIndex] == '1');
            }
        }
    }
}
