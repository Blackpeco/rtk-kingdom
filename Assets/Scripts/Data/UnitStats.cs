using System;
using UnityEngine;

namespace TsOnline
{
    [Serializable]
    public struct UnitStats
    {
        public int hp;
        public int sp;
        public int atk;
        [Tooltip("INT — used by magical skills.")]
        public int intel;
        public int def;
        public int agi;

        public UnitStats(int hp, int sp, int atk, int intel, int def, int agi)
        {
            this.hp = hp;
            this.sp = sp;
            this.atk = atk;
            this.intel = intel;
            this.def = def;
            this.agi = agi;
        }
    }
}
