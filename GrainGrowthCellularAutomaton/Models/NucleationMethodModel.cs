using System.ComponentModel;

namespace GrainGrowthCellularAutomaton.Models
{
    public enum NucleationMethodModel
    {
        [Description("Uniform")]
        Uniform,

        [Description("Random")]
        Random,

        [Description("Random with radius")]
        RandomWithRadius,
    }
}