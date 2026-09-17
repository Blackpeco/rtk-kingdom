using System;

namespace TsOnline
{
    [Serializable]
    public class GameSave
    {
        public int version = 1;
        public float worldX;
        public float worldY;
        public bool hasWorldPos;
        public int questPhase;
        public int questWins;
        public SavedMember[] roster;
        public string[] partyOrder;
        public SavedStack[] inventory;
        public bool autoAttack;
        public bool autoHeal;
        public float healThreshold = 40f;
    }

    [Serializable]
    public class SavedMember
    {
        public string id;
        public int level = 1;
        public int exp;
        public int unspentPoints;
        public int currentHp;
        public int currentSp;
        public int bonusHp;
        public int bonusSp;
        public int bonusAtk;
        public int bonusIntel;
        public int bonusDef;
        public int bonusAgi;
        public bool unlocked = true;
    }

    [Serializable]
    public class SavedStack
    {
        public string itemId;
        public int count;
    }
}
