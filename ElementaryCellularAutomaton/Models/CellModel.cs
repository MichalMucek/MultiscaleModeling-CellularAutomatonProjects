namespace ElementaryCellularAutomaton.Models
{
    class CellModel
    {
        public int Id { get; private set; }
        public bool IsAlive { get; set; }

        public CellModel(int id, bool isAlive)
        {
            Id = id;
            IsAlive = isAlive;
        }
    }
}
