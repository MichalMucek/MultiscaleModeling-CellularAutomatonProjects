using CellularAutomaton2D.Models;

namespace LifeLikeCellularAutomaton.Models
{
    public class RuleModel
    {
        public NumberOfCellsForRulesModel[] Birth { get; private set; }
        public NumberOfCellsForRulesModel[] Survival { get; private set; }

        public RuleModel(CellNeighborhoodTypeModel neighborhoodType)
        {
            Birth = new NumberOfCellsForRulesModel[(int)neighborhoodType];
            Survival = new NumberOfCellsForRulesModel[(int)neighborhoodType];

            for (int i = 0; i < (int)neighborhoodType; i++)
            {
                Birth[i] = new NumberOfCellsForRulesModel(i);
                Survival[i] = new NumberOfCellsForRulesModel(i);
            }
        }

        public bool WillBeBorn(int neighboursCount)
        {
            try
            {
                return Birth[neighboursCount].Chosen;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        public bool WillSurvive(int neighboursCount)
        {
            try
            {
                return Survival[neighboursCount].Chosen;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                return false;
            }
        }
    }
}