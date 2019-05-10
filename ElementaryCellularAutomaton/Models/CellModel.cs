namespace ElementaryCellularAutomaton.Models
{
    public class CellModel : ICell
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
            Id = obj.Id;
            IsAlive = obj.IsAlive;
        }

        public void Kill() => IsAlive = false;

        public void Revive() => IsAlive = true;
    }
}