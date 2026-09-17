namespace TsOnline
{
    public enum QuestPhase
    {
        None = 0,
        Active = 1,
        Ready = 2,
        Done = 3
    }

    /// <summary>Single city quest: accept → win N forest fights → turn in.</summary>
    public static class QuestTracker
    {
        public const string QuestId = "forest_watch";
        public const int WinsNeeded = 2;
        public const int RewardExp = 40;
        public const int RewardHerbs = 2;

        public static QuestPhase Phase = QuestPhase.None;
        public static int ForestWins;
        public static string LastReward = "";

        public static string Title
        {
            get { return "ป่าไม่สงบ"; }
        }

        public static string ObjectiveLine()
        {
            switch (Phase)
            {
                case QuestPhase.None:
                    return "เควสต์: คุยกับยายเมือง (ซ้าย) กด E";
                case QuestPhase.Active:
                    return "เควสต์: ชนะการรบในป่า " + ForestWins + "/" + WinsNeeded;
                case QuestPhase.Ready:
                    return "เควสต์: กลับไปหายายเมืองเพื่อรับรางวัล";
                case QuestPhase.Done:
                    return "เควสต์เสร็จ: " + Title + " — ได้สมุนไพรแล้ว";
                default:
                    return "";
            }
        }

        public static void Accept()
        {
            if (Phase != QuestPhase.None)
                return;
            Phase = QuestPhase.Active;
            ForestWins = 0;
            LastReward = "";
        }

        public static void NotifyForestWin()
        {
            if (Phase != QuestPhase.Active)
                return;
            ForestWins++;
            if (ForestWins >= WinsNeeded)
                Phase = QuestPhase.Ready;
        }

        public static bool TryComplete(out string message)
        {
            message = "";
            if (Phase != QuestPhase.Ready)
                return false;

            Phase = QuestPhase.Done;
            InventoryService.Add(InventoryService.HerbId, RewardHerbs);
            PartyManager.Ensure().AwardFlatExp(RewardExp);
            message = "ได้สมุนไพร ×" + RewardHerbs + " และ EXP " + RewardExp + " ทั้งปาร์ตี้";
            LastReward = message;
            return true;
        }

        public static void Apply(int phase, int wins)
        {
            Phase = (QuestPhase)phase;
            ForestWins = wins;
            if (Phase == QuestPhase.Active && ForestWins >= WinsNeeded)
                Phase = QuestPhase.Ready;
        }
    }
}
