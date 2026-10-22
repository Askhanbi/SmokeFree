using System;
using System.Collections.Generic;
using SmokeFree.Core;
using UnityEngine;

namespace SmokeFree.Data
{
    /// <summary>Один эффект выбора на один параметр.</summary>
    [Serializable]
    public struct StatEffect
    {
        public StatType type;
        public float delta;
    }

    /// <summary>Условие показа варианта — так открываются "новые возможности" по ходу игры.</summary>
    [Serializable]
    public struct ChoiceCondition
    {
        public StatType type;
        public float minValue;
        public float maxValue;
        public string requiredFlag;   // пусто = не проверять
    }

    [Serializable]
    public class StoryChoice
    {
        [TextArea(2, 4)] public string choiceText;
        public List<StatEffect> effects = new List<StatEffect>();

        [Header("Переход")]
        public StoryNodeSO nextNode;
        public MiniGameType triggersMiniGame = MiniGameType.None;

        [Tooltip("Нода, в которую уйдём, если игрок ПРОИГРАЛ мини-игру. Пусто = идти в nextNode в любом случае.")]
        public StoryNodeSO nodeOnMiniGameFail;

        [Header("Доступность")]
        public bool hasCondition;
        public ChoiceCondition condition;
        public string grantsFlag;

        public bool IsAvailable(PlayerStats stats, Func<string, bool> hasFlag)
        {
            if (!hasCondition) return true;
            if (!string.IsNullOrEmpty(condition.requiredFlag) && !hasFlag(condition.requiredFlag)) return false;

            float v = stats.Get(condition.type);
            return v >= condition.minValue && v <= condition.maxValue;
        }
    }

    /// <summary>
    /// Нода сюжета. Граф собирается перетаскиванием ассетов в инспекторе — без кода и без visual scripting.
    /// Assets/_Project/Data/Story/Chapter_01/Node_FirstCigarette.asset
    /// </summary>
    [CreateAssetMenu(fileName = "Node_", menuName = "SmokeFree/Story Node", order = 1)]
    public class StoryNodeSO : ScriptableObject
    {
        [SerializeField] private string nodeId;
        [SerializeField, TextArea(4, 10)] private string bodyText;
        [SerializeField] private Sprite illustration;
        [SerializeField] private AudioClip ambience;

        [Tooltip("Завершает ли эта нода главу (возраст += YearsPerChapter)")]
        [SerializeField] private bool endsChapter;

        [Tooltip("Если задано — нода терминальная, запускается концовка")]
        [SerializeField] private EndingType terminalEnding = EndingType.None;

        [SerializeField] private List<StoryChoice> choices = new List<StoryChoice>();

        public string NodeId => string.IsNullOrEmpty(nodeId) ? name : nodeId;
        public string BodyText => bodyText;
        public Sprite Illustration => illustration;
        public AudioClip Ambience => ambience;
        public bool EndsChapter => endsChapter;
        public EndingType TerminalEnding => terminalEnding;
        public IReadOnlyList<StoryChoice> Choices => choices;

        public List<StoryChoice> GetAvailableChoices(PlayerStats stats, Func<string, bool> hasFlag)
        {
            var result = new List<StoryChoice>();
            foreach (var c in choices)
                if (c.IsAvailable(stats, hasFlag)) result.Add(c);
            return result;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(nodeId)) nodeId = name;
        }
#endif
    }
}
