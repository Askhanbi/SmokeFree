using UnityEngine;

namespace SmokeFree.Data
{
    /// <summary>
    /// Балансный конфиг. Один ассет на игру: Assets/_Project/Data/Config/GameConfig.asset
    /// Правка баланса = правка ассета, без перекомпиляции и без лазания в код.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "SmokeFree/Game Config", order = 0)]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Стартовые значения")]
        [SerializeField] private int startAge = 17;
        [SerializeField] private float startHealth = 95f;
        [SerializeField] private float startMoney = 300f;
        [SerializeField] private float startStress = 20f;
        [SerializeField] private float startAddiction = 0f;

        [Header("Ход времени")]
        [SerializeField] private int yearsPerChapter = 2;
        [SerializeField] private int maxAge = 80;

        [Header("Пороги концовок")]
        [SerializeField] private float criticalHealth = 10f;
        [SerializeField] private float addictionFreeThreshold = 15f;
        [SerializeField] private int lateQuitAge = 50;

        [Header("Пассивный урон за ход (масштабируется зависимостью)")]
        [SerializeField] private float healthDecayPerAddictionPoint = 0.12f;
        [SerializeField] private float stressGrowthWhenCraving = 4f;

        [Header("Сложность мини-игр")]
        [Tooltip("Как сильно зависимость портит управление: 0 = нет влияния, 1 = максимум")]
        [SerializeField, Range(0f, 1f)] private float controlNoiseAtMaxAddiction = 0.45f;
        [SerializeField] private float miniGameBaseDuration = 25f;

        public int StartAge => startAge;
        public int YearsPerChapter => yearsPerChapter;
        public int MaxAge => maxAge;
        public float CriticalHealth => criticalHealth;
        public float AddictionFreeThreshold => addictionFreeThreshold;
        public int LateQuitAge => lateQuitAge;
        public float HealthDecayPerAddictionPoint => healthDecayPerAddictionPoint;
        public float StressGrowthWhenCraving => stressGrowthWhenCraving;
        public float ControlNoiseAtMaxAddiction => controlNoiseAtMaxAddiction;
        public float MiniGameBaseDuration => miniGameBaseDuration;

        public PlayerStats CreateStartingStats() =>
            new PlayerStats(startAge, startHealth, startMoney, startStress, startAddiction);
    }
}
