using System;
using SmokeFree.Data;

namespace SmokeFree.Core
{
    /// <summary>
    /// Статическая шина событий. UI и геймплей НЕ держат прямых ссылок друг на друга —
    /// это то, что защищает от "null reference при смене сцены" (главный источник багов в Mechanics).
    /// Правило: подписка в OnEnable, отписка в OnDisable. Всегда.
    /// </summary>
    public static class GameEvents
    {
        public static event Action<GameState> OnStateChanged;
        public static event Action<PlayerStats> OnStatsChanged;
        public static event Action<StatType, float> OnStatDelta;   // для всплывающих "+5 / -10" в UI
        public static event Action<MiniGameType, float> OnMiniGameRequested; // тип + множитель сложности
        public static event Action<bool> OnMiniGameFinished;       // true = игрок выстоял
        public static event Action<EndingType> OnEndingReached;
        public static event Action<int> OnChapterChanged;

        public static void RaiseStateChanged(GameState s) => OnStateChanged?.Invoke(s);
        public static void RaiseStatsChanged(PlayerStats s) => OnStatsChanged?.Invoke(s);
        public static void RaiseStatDelta(StatType t, float d) => OnStatDelta?.Invoke(t, d);
        public static void RaiseMiniGameRequested(MiniGameType t, float difficulty) => OnMiniGameRequested?.Invoke(t, difficulty);
        public static void RaiseMiniGameFinished(bool survived) => OnMiniGameFinished?.Invoke(survived);
        public static void RaiseEndingReached(EndingType e) => OnEndingReached?.Invoke(e);
        public static void RaiseChapterChanged(int chapter) => OnChapterChanged?.Invoke(chapter);

        /// <summary>Вызывать при выходе в главное меню: статические события переживают загрузку сцены.</summary>
        public static void ClearAll()
        {
            OnStateChanged = null;
            OnStatsChanged = null;
            OnStatDelta = null;
            OnMiniGameRequested = null;
            OnMiniGameFinished = null;
            OnEndingReached = null;
            OnChapterChanged = null;
        }
    }
}
