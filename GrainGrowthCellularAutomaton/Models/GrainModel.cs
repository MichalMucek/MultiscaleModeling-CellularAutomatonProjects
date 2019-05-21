using System;
using System.Windows.Media;
using CellularAutomaton2D;

namespace GrainGrowthCellularAutomaton.Models
{
    internal class GrainModel : ICellState
    {
        public int Id { get; private set; }
        public Color Color { get; private set; }

        [ThreadStatic]
        private static Random randomRgb;

        public GrainModel(int id = 0)
        {
            if (randomRgb == null)
                randomRgb = new Random();

            Id = id;
            Color = new Color();
            SetRandomColor();
        }

        public GrainModel(GrainModel obj)
        {
            if (randomRgb == null)
                randomRgb = new Random();

            Id = obj.Id;
            Color = obj.Color;
        }

        public void SetColor(Color color)
            => Color = color;

        public void SetColor(byte red, byte green, byte blue)
            => Color = Color.FromRgb(red, green, blue);

        public void SetRandomColor()
        {
            byte[] rgb = new byte[]
            {
                byte.MaxValue,
                byte.MaxValue,
                byte.MaxValue
            };

            if (Id != 0)
            {
                while (rgb[0] == byte.MaxValue && rgb[1] == byte.MaxValue && rgb[2] == byte.MaxValue)
                    randomRgb.NextBytes(rgb);
            }

            Color = Color.FromRgb(rgb[0], rgb[1], rgb[2]);
        }
    }
}