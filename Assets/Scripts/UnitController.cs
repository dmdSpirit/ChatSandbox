using UnityEngine;

namespace dmdspirit
{
    public class UnitController : MonoSingleton<UnitController>
    {
        private int unitsInTeam;
        private Unit[] redTeam;
        private Unit[] greenTeam;

        public void Initialize(int unitsInTeam)
        {
            this.unitsInTeam = unitsInTeam;
            redTeam = new Unit[unitsInTeam];
            greenTeam = new Unit[unitsInTeam];
        }
        
    }
}
