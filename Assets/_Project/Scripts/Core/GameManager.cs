using System.Collections.Generic;
using SmokeFree.Data;
using UnityEngine;

namespace SmokeFree.Core
{
    /// <summary>
    /// Центральный менеджер: владеет состоянием персонажа, машиной состояний и правилами концовок.
    /// Не рисует UI, не грузит сцены, не трогает звук — только модель игры.
    /// Лежит на объекте [Managers] в сцене 00_Bootstrap.
    /// </summary>
    public class GameManager : PersistentSingleton<GameManager>
    {
        [Header("Конфиг")]
        [SerializeField] private GameConfigSO config;

        [Header("Отладка (read-only в рантайме)")]
        [SerializeField] private GameState currentState = GameState.Boot;
        [SerializeField] private int currentChapter;

        private PlayerStats stats;
        private readonly HashSet<string> unlockedFlags = new HashSet<string>();

        public GameState CurrentState => currentState;
        public PlayerStats Stats => stats;
        public GameConfigSO Config => config;
        public int CurrentChapter => currentChapter;
        public bool IsRunActive => stats != null && currentState != GameState.MainMenu && currentState != GameState.Boot;

        protected override void OnSingletonAwake()
        {
            if (config == null)
                Debug.LogError("[GameManager] GameConfigSO не назначен в инспекторе.", this);
        }

        // ---------- Жизненный цикл забега ----------

        public void StartNewRun()
        {
            stats = config.CreateStartingStats();
            unlockedFlags.Clear();
            currentChapter = 0;

            GameEvents.RaiseStatsChanged(stats);
            GameEvents.RaiseChapterChanged(currentChapter);
            SetState(GameState.Narrative);
        }

        /// <summary>Восстановление из SaveManager.</summary>
        public void LoadRun(PlayerStats loadedStats, int chapter, IEnumerable<string> flags)
        {
            stats = loadedStats ?? config.CreateStartingStats();
            currentChapter = chapter;

            unlockedFlags.Clear();
            if (flags != null)
                foreach (var f in flags) unlockedFlags.Add(f);

            GameEvents.RaiseStatsChanged(stats);
            GameEvents.RaiseChapterChanged(currentChapter);
            SetState(GameState.Narrative);
        }

        // ---------- Машина состояний ----------

        public void SetState(GameState next)
        {
            if (currentState == next) return;
            currentState = next;
            Time.timeScale = next == GameState.Paused ? 0f : 1f;
            GameEvents.RaiseStateChanged(next);
        }

        // ---------- Мутация параметров ----------

        /// <summary>Единственный публичный способ изменить параметр. Вызывается из эффектов выборов и мини-игр.</summary>
        public void ApplyStat(StatType type, float delta)
        {
            if (stats == null || Mathf.Approximately(delta, 0f)) return;

            stats.Apply(type, delta);
            GameEvents.RaiseStatDelta(type, delta);
            GameEvents.RaiseStatsChanged(stats);

            CheckImmediateEnding();
        }

        public void ApplyStats(IEnumerable<(StatType type, float delta)> effects)
        {
            if (stats == null) return;

            foreach (var (type, delta) in effects)
            {
                if (Mathf.Approximately(delta, 0f)) continue;
                stats.Apply(type, delta);
                GameEvents.RaiseStatDelta(type, delta);
            }

            GameEvents.RaiseStatsChanged(stats);
            CheckImmediateEnding();
        }

        // ---------- Ход времени ----------

        /// <summary>Завершение главы: стареем, начисляем пассивные последствия, проверяем концовку.</summary>
        public void AdvanceChapter()
        {
            if (stats == null) return;

            stats.AdvanceAge(config.YearsPerChapter);
            currentChapter++;

            // Пассивный урон здоровью пропорционален зависимости — "цена" накопленных решений.
            float decay = stats.Addiction * config.HealthDecayPerAddictionPoint * config.YearsPerChapter;
            stats.Apply(StatType.Health, -decay);

            if (stats.Addiction > 40f)
                stats.Apply(StatType.Stress, config.StressGrowthWhenCraving);

            GameEvents.RaiseStatsChanged(stats);
            GameEvents.RaiseChapterChanged(currentChapter);

            if (!CheckImmediateEnding())
                SaveManagerBridge();
        }

        // ---------- Сложность мини-игр ----------

        /// <summary>
        /// Множитель сложности 1.0..~2.0. Используется платформером: чем выше зависимость и стресс,
        /// тем сильнее "шум" в управлении и тем дольше надо продержаться.
        /// Это и есть требуемое курсом "усложнение по ходу игры" (критерий Progress).
        /// </summary>
        public float GetMiniGameDifficulty()
        {
            if (stats == null) return 1f;
            float addictionFactor = stats.Addiction / PlayerStats.MaxAddiction;
            float stressFactor = stats.Stress / PlayerStats.MaxStress;
            float chapterFactor = Mathf.Clamp01(currentChapter / 10f);
            return 1f + addictionFactor * 0.6f + stressFactor * 0.25f + chapterFactor * 0.25f;
        }

        public void RequestMiniGame(MiniGameType type)
        {
            if (type == MiniGameType.None) return;
            SetState(GameState.MiniGame);
            GameEvents.RaiseMiniGameRequested(type, GetMiniGameDifficulty());
        }

        /// <summary>Вызывается контроллером мини-игры по её завершении.</summary>
        public void ReportMiniGameResult(bool survived)
        {
            if (survived)
            {
                ApplyStat(StatType.Addiction, -8f);
                ApplyStat(StatType.Stress, -5f);
            }
            else
            {
                ApplyStat(StatType.Addiction, +10f);
                ApplyStat(StatType.Health, -4f);
            }

            GameEvents.RaiseMiniGameFinished(survived);

            if (currentState != GameState.Ending)
                SetState(GameState.Narrative);
        }

        // ---------- Флаги прогресса (разблокировки, апгрейды) ----------

        public void SetFlag(string flagId)
        {
            if (!string.IsNullOrEmpty(flagId)) unlockedFlags.Add(flagId);
        }

        public bool HasFlag(string flagId) => !string.IsNullOrEmpty(flagId) && unlockedFlags.Contains(flagId);
        public IReadOnlyCollection<string> Flags => unlockedFlags;

        // ---------- Концовки ----------

        private bool CheckImmediateEnding()
        {
            if (stats == null || currentState == GameState.Ending) return false;

            if (stats.Health <= config.CriticalHealth)
            {
                TriggerEnding(stats.Age < config.LateQuitAge ? EndingType.DiedYoung : EndingType.QuitLateWithConsequences);
                return true;
            }

            if (stats.Age >= config.MaxAge)
            {
                TriggerEnding(stats.Addiction <= config.AddictionFreeThreshold
                    ? EndingType.QuitSuccessfully
                    : EndingType.QuitLateWithConsequences);
                return true;
            }

            if (stats.Money <= 0f && stats.Addiction >= 70f)
            {
                TriggerEnding(EndingType.BankruptAndAddicted);
                return true;
            }

            return false;
        }

        private void TriggerEnding(EndingType ending)
        {
            SetState(GameState.Ending);
            GameEvents.RaiseEndingReached(ending);
        }

        // ---------- Точка стыковки с SaveManager ----------

        /// <summary>Заглушка автосейва: на Milestone 3 заменяется на SaveManager.Instance.Save().</summary>
        private void SaveManagerBridge()
        {
            // SaveManager.Instance?.SaveGame();
        }
    }
}
