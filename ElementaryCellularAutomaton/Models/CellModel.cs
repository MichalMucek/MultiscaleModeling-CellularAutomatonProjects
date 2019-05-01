namespace ElementaryCellularAutomaton.Models
{
    public class CellModel
    {
        public int Id { get; private set; }
        public bool IsAlive { get; set; }

        public CellModel(int id, bool isAlive)
        {
            Id = id;
            IsAlive = isAlive;
        }

        public CellModel(CellModel obj)
        {
            this.Id = obj.Id;
            this.IsAlive = obj.IsAlive;
        }
    }
}
