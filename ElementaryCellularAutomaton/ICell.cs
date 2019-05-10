namespace ElementaryCellularAutomaton
{
    interface ICell
    {
        int Id { get; }
        bool IsAlive { get; }

        void Kill();
        void Revive();
    }
}
