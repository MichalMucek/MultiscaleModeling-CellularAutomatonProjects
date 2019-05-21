using System.Windows.Media;

namespace CellularAutomaton2D
{
    internal interface ICellState
    {
        int Id { get; }
        Color Color { get; }

        void SetColor(Color color);

        void SetColor(byte red, byte green, byte blue);

        void SetNewRandomColor();
    }
}