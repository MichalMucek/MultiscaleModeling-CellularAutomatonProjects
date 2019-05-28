namespace GameOfLife.Models
{
    public class NumberOfCellsForRulesModel
    {
        public bool Chosen { get; set; }
        public string Text { get; set; }

        public NumberOfCellsForRulesModel(int number) => Text = number.ToString();

        public NumberOfCellsForRulesModel(string number) => Text = number;

        public NumberOfCellsForRulesModel(int number, bool chosen)
        {
            Chosen = chosen;
            Text = number.ToString();
        }

        public NumberOfCellsForRulesModel(string number, bool chosen)
        {
            Chosen = chosen;
            Text = number;
        }
    }
}