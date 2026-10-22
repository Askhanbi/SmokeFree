using System;
using UnityEngine;

namespace SmokeFree.Data
{
    /// <summary>
    /// Рантайм-состояние персонажа. Это обычный сериализуемый класс, НЕ ScriptableObject:
    /// SO хранит стартовые/балансные значения, а мутабельное состояние живёт здесь и пишется в JSON.
    /// Причина: SO в билде не сохраняет изменения между сессиями и "залипает" в редакторе между запусками.
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        [SerializeField] private int age;
        [SerializeField] private float health;
        [SerializeField] private float money;
        [SerializeField] private float stress;
        [SerializeField] private float addiction;

        public const float MaxHealth = 100f;
        public const float MaxStress = 100f;
        public const float MaxAddiction = 100f;

        public int Age => age;
        public float Health => health;
        public float Money => money;
        public float Stress => stress;
        public float Addiction => addiction;

        public PlayerStats(int startAge, float startHealth, float startMoney, float startStress, float startAddiction)
        {
            age = startAge;
            health = Mathf.Clamp(startHealth, 0f, MaxHealth);
            money = Mathf.Max(0f, startMoney);
            stress = Mathf.Clamp(startStress, 0f, MaxStress);
            addiction = Mathf.Clamp(startAddiction, 0f, MaxAddiction);
        }

        public float Get(Core.StatType type) => type switch
        {
            Core.StatType.Health => health,
            Core.StatType.Money => money,
            Core.StatType.Stress => stress,
            Core.StatType.Addiction => addiction,
            _ => 0f
        };

        /// <summary>Единая точка мутации. Никто снаружи не пишет в поля напрямую.</summary>
        public void Apply(Core.StatType type, float delta)
        {
            switch (type)
            {
                case Core.StatType.Health:
                    health = Mathf.Clamp(health + delta, 0f, MaxHealth);
                    break;
                case Core.StatType.Money:
                    money = Mathf.Max(0f, money + delta);
                    break;
                case Core.StatType.Stress:
                    stress = Mathf.Clamp(stress + delta, 0f, MaxStress);
                    break;
                case Core.StatType.Addiction:
                    addiction = Mathf.Clamp(addiction + delta, 0f, MaxAddiction);
                    break;
            }
        }

        public void AdvanceAge(int years) => age += Mathf.Max(0, years);

        /// <summary>Нормализованное значение 0..1 для прогресс-баров UI.</summary>
        public float GetNormalized(Core.StatType type) => type switch
        {
            Core.StatType.Health => health / MaxHealth,
            Core.StatType.Stress => stress / MaxStress,
            Core.StatType.Addiction => addiction / MaxAddiction,
            _ => 0f
        };

        public PlayerStats Clone() => new PlayerStats(age, health, money, stress, addiction);
    }
}
