using System;
using System.Windows.Media;

namespace GrainGrowthCellularAutomaton.Models
{
    internal class GrainModel
    {
        public int Id { get; private set; }
        public Color Color { get; private set; }

        [ThreadStatic]
        private static Random randomRgb;

        public GrainModel(int id = 0)
        {
            if (randomRgb == null)
                randomRgb = new Random();

            byte red = byte.MaxValue, green = byte.MaxValue, blue = byte.MaxValue;

            SetRgb(ref red, ref green, ref blue, id);

            Id = id;
            Color = new Color();
            Color = Color.FromRgb(red, green, blue);
        }

        private void SetRgb(ref byte red, ref byte green, ref byte blue, int id)
        {
            if (id != 0)
            {
                byte[] rgb = new byte[3];

                while (red == byte.MaxValue && green == byte.MaxValue && blue == byte.MaxValue)
                {
                    randomRgb.NextBytes(rgb);

                    red = rgb[0];
                    green = rgb[1];
                    blue = rgb[2];
                }
            }
        }

        public GrainModel(GrainModel obj)
        {
            Id = obj.Id;
            Color = obj.Color;
        }

        public void NewColor()
        {
            byte red = byte.MaxValue, green = byte.MaxValue, blue = byte.MaxValue;

            SetRgb(ref red, ref green, ref blue, Id);
        }
    }
}