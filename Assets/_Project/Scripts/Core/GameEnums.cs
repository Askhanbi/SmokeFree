namespace SmokeFree.Core
{
    /// <summary>Глобальное состояние игры. UI подписывается на смену через GameEvents.</summary>
    public enum GameState
    {
        Boot,
        MainMenu,
        Narrative,   // показывается карточка выбора
        MiniGame,    // активна платформерная секция
        Paused,
        Ending
    }

    /// <summary>Параметры персонажа. enum нужен, чтобы эффекты в SO задавались без магических строк.</summary>
    public enum StatType
    {
        Health,
        Money,
        Stress,
        Addiction
    }

    /// <summary>Тип мини-игры, который запрашивает нода сюжета.</summary>
    public enum MiniGameType
    {
        None,
        Withdrawal,   // платформер: "ломка", скользкое управление
        Temptation,   // раннер: убежать от соблазна
        BreathHold    // ритм-мини-игра (опционально, для Content)
    }

    public enum EndingType
    {
        None,
        DiedYoung,
        QuitLateWithConsequences,
        QuitSuccessfully,
        BankruptAndAddicted
    }
}
