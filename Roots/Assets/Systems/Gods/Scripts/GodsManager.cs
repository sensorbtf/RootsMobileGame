using System.Collections.Generic;
using UnityEngine;

namespace Gods
{
    public class GodsManager : MonoBehaviour
    {
        [SerializeField] private GodsDatabase _database;
        [SerializeField] private float _smallBlessing = 0.10f;
        [SerializeField] private float _mediumBlessing = 0.25f;
        [SerializeField] private float _bigBlessing = 0.50f;
        [SerializeField] private int _smallBlessingPrice = 5;
        [SerializeField] private int _mediumBlessingPrice = 10;
        [SerializeField] private int _bigBlessingPrice = 20;
        private Dictionary<BlessingLevel, float> _blessingValues;

        private List<Blessing> _playerStoredBlessings; // blessings in player inventory

        public Dictionary<GodDataSO, BlessingLevel> PlayerCurrentBlessings { get; private set; }

        public Dictionary<BlessingLevel, int> BlessingPrices { get; private set; }

        private void Start()
        {
            _playerStoredBlessings = new List<Blessing>(); // TODO: LOAD/SAVE
            PlayerCurrentBlessings = new Dictionary<GodDataSO, BlessingLevel>();

            foreach (var god in _database.AllGods)
            {
                PlayerCurrentBlessings.Add(god, BlessingLevel.Noone);
                _playerStoredBlessings.Add(new Blessing
                {
                    Type = god.GodName,
                    AmountByType = new BlessingLevelAmount[]
                    {
                        new() { TypeLevel = BlessingLevel.Small, Amount = 0 },
                        new() { TypeLevel = BlessingLevel.Medium, Amount = 0 },
                        new() { TypeLevel = BlessingLevel.Big, Amount = 0 }
                    }
                });
            }

            _blessingValues = new Dictionary<BlessingLevel, float> // TODO: LOAD/SAVE
            {
                { BlessingLevel.Small, _smallBlessing },
                { BlessingLevel.Medium, _mediumBlessing },
                { BlessingLevel.Big, _bigBlessing }
            };

            BlessingPrices = new Dictionary<BlessingLevel, int>
            {
                { BlessingLevel.Small, _smallBlessingPrice },
                { BlessingLevel.Medium, _mediumBlessingPrice },
                { BlessingLevel.Big, _bigBlessingPrice }
            };
        }

        public float GetBlessingValue(GodType p_godType)
        {
            foreach (var blessing in PlayerCurrentBlessings)
                if (blessing.Key.GodName == p_godType)
                    return _blessingValues[blessing.Value];

            return 0f;
        }

        public float GetBlessingValue(BlessingLevel p_level)
        {
            return _blessingValues[p_level];
        }

        public BlessingLevel GetCurrentBlessingLevel(GodType p_god)
        {
            foreach (var blessings in PlayerCurrentBlessings)
                if (blessings.Key.GodName == p_god)
                    return blessings.Value;

            return BlessingLevel.Noone;
        }

        private void ResetBlessingOnNewDayStart()
        {
            foreach (var blessing in PlayerCurrentBlessings) PlayerCurrentBlessings[blessing.Key] = BlessingLevel.Noone;
        }

        public int GetAmountOfAvaiableBlessings(GodType p_god, BlessingLevel p_blessingLevel)
        {
            var specificBlessing = GetBlessingLevelAmount(p_god, p_blessingLevel);

            if (specificBlessing == null)
                return 0;

            return specificBlessing.Amount;
        }

        public void ActivateSpecificBlessing(GodType p_godType, BlessingLevel p_blessingLevel)
        {
            var blessing = GetSpecificBlessingKey(p_godType);
            if (blessing == null)
                return;

            if (PlayerCurrentBlessings[blessing] != 0)
                DeactivateSpecificBlessing(p_godType, PlayerCurrentBlessings[blessing]);

            HandleAmountOfPlayerBlessing(p_godType, p_blessingLevel, false);
            PlayerCurrentBlessings[blessing] = p_blessingLevel;
        }

        public void BuySpecificBlessing(GodType p_godType, BlessingLevel p_blessingLevel)
        {
            var blessing = GetBlessingLevelAmount(p_godType, p_blessingLevel);
            if (blessing == null)
                return;

            HandleAmountOfPlayerBlessing(p_godType, p_blessingLevel, true);
        }

        public bool CanActivateBlessing(GodType p_godType, BlessingLevel p_blessingLevel)
        {
            return true;
        }

        public bool IsGodBlessingOnLevelActivated(GodType p_godType, BlessingLevel p_blessingLevel)
        {
            var blessing = GetSpecificBlessingKey(p_godType);
            if (blessing == null)
                return false;

            return PlayerCurrentBlessings[blessing] == p_blessingLevel;
        }

        public void DeactivateSpecificBlessing(GodType p_godType, BlessingLevel p_currentBlessingLevel)
        {
            var blessing = GetSpecificBlessingKey(p_godType);
            if (blessing == null)
                return;

            HandleAmountOfPlayerBlessing(p_godType, p_currentBlessingLevel, true);
            PlayerCurrentBlessings[blessing] = BlessingLevel.Noone;
        }

        public void HandleAmountOfPlayerBlessing(GodType p_godType, BlessingLevel p_currentBlessingLevel, bool p_add)
        {
            var specificBlessing = GetBlessingLevelAmount(p_godType, p_currentBlessingLevel);

            if (specificBlessing == null)
                return;

            if (p_add)
                specificBlessing.Amount++;
            else
                specificBlessing.Amount--;
        }

        private GodDataSO GetSpecificBlessingKey(GodType p_godType)
        {
            foreach (var blessing in PlayerCurrentBlessings)
            {
                if (blessing.Key.GodName != p_godType)
                    continue;

                return blessing.Key;
            }

            return null;
        }

        private BlessingLevelAmount GetBlessingLevelAmount(GodType p_godType, BlessingLevel p_blessing)
        {
            foreach (var blessings in _playerStoredBlessings)
            {
                if (blessings.Type != p_godType)
                    continue;

                foreach (var blessing in blessings.AmountByType)
                    if (blessing.TypeLevel == p_blessing)
                        return blessing;
            }

            return null;
        }
    }

    public class Blessing
    {
        public BlessingLevelAmount[] AmountByType;
        public GodType Type;
    }

    public class BlessingLevelAmount
    {
        public int Amount;
        public BlessingLevel TypeLevel;
    }
}