namespace GameOfLife.Models
{
    public class RuleModel
    {
        public NumberOfCellsForRulesModel[] Birth { get; private set; }
        public NumberOfCellsForRulesModel[] Survival { get; private set; }

        public RuleModel(CellsNeighborhoodTypeModel neighborhoodType)
        {
            Birth = new NumberOfCellsForRulesModel[(int)neighborhoodType];
            Survival = new NumberOfCellsForRulesModel[(int)neighborhoodType];

            for (int i = 0; i < (int)neighborhoodType; i++)
            {
                Birth[i] = new NumberOfCellsForRulesModel(i);
                Survival[i] = new NumberOfCellsForRulesModel(i);
            }
        }
    }
}
