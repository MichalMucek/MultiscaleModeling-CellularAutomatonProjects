using System;
using System.Collections.Generic;
using System.Text;

namespace ElementaryCellularAutomaton.Models
{
    public class RuleModel
    {
        private const int BINARY_NUMERAL_SYSTEM = 2;
        private const int DECIMAL_NUMERAL_SYSTEM = 10;
        private const int MAX_BINARY_REPRESENTATION_LENGTH = 8;

        private string _binaryRepresentation;
        private int _decimalRepresentation;
        public readonly Dictionary<CellsNeighborhood, bool> Table = new Dictionary<CellsNeighborhood, bool>();

        public int Value
        {
            get => _decimalRepresentation;
            set
            {
                _decimalRepresentation = value;
                BinaryRepresentation = Convert.ToString(value, BINARY_NUMERAL_SYSTEM);

                ResetTable();
            }
        }

        private void ResetTable()
        {
            Table.Clear();
            SetTable();
        }

        private string BinaryRepresentation
        {
            get => _binaryRepresentation;
            set
            {
                _binaryRepresentation = value;
                if (_binaryRepresentation.Length != 8)
                    InsertMissingZerosInBinaryRepresentation();
            }
        }

        private void InsertMissingZerosInBinaryRepresentation()
        {
            StringBuilder stringBuilder = new StringBuilder(_binaryRepresentation);

            stringBuilder.Insert(0, "0", MAX_BINARY_REPRESENTATION_LENGTH - _binaryRepresentation.Length);
            _binaryRepresentation += "0";

            _binaryRepresentation = stringBuilder.ToString();
        }

        public RuleModel(int ruleNumberBase10)
        {
            BinaryRepresentation = Convert.ToString(ruleNumberBase10, BINARY_NUMERAL_SYSTEM);
            _decimalRepresentation = ruleNumberBase10;

            SetTable();
        }

        public RuleModel(string ruleNumberBase2)
        {
            BinaryRepresentation = ruleNumberBase2;
            _decimalRepresentation = Convert.ToInt32(Convert.ToString(Convert.ToInt32(ruleNumberBase2, BINARY_NUMERAL_SYSTEM), DECIMAL_NUMERAL_SYSTEM));

            SetTable();
        }

        private void SetTable()
        {
            for (int neighborhoodIndex = 7, binRepIndex = 0;
                neighborhoodIndex >= 0;
                neighborhoodIndex--, binRepIndex++)
            {
                Table.Add(new CellsNeighborhood(neighborhoodIndex), BinaryRepresentation[binRepIndex] == '1');
            }
        }
    }
}