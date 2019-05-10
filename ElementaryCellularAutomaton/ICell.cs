using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementaryCellularAutomaton.Models
{
    interface ICell
    {
        int Id { get; }
        bool IsAlive { get; }

        void Kill();
        void Revive();
    }
}
