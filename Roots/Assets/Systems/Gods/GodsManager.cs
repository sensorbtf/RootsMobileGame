using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;
using System.Linq;

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

        private Dictionary<GodDataSO, BlessingLevel> _playerCurrentBlessings;
        private List<Blessing> _playerStoredBlessings;
        private Dictionary<BlessingLevel, float> _blessingValues;
        private Dictionary<BlessingLevel, int> _blessingPrices;

        public Dictionary<GodDataSO, BlessingLevel> PlayerCurrentBlessings => _playerCurrentBlessings;
        public Dictionary<BlessingLevel, int> BlessingPrices => _blessingPrices;

        void Start()
        {
            _playerStoredBlessings = new List<Blessing>(); // TODO: LOAD/SAVE
            _playerCurrentBlessings = new Dictionary<GodDataSO, BlessingLevel>();

            foreach (var god in _database.AllGods)
            {
                _playerCurrentBlessings.Add(god, BlessingLevel.Noone);
                _playerStoredBlessings.Add(new Blessing
                {
                    Type = god.GodName,
                    AmountByType = new BlessingLevelAmount[]
                    {
                        new BlessingLevelAmount { TypeLevel = BlessingLevel.Small, Amount = 0},
                        new BlessingLevelAmount { TypeLevel = BlessingLevel.Medium, Amount = 0},
                        new BlessingLevelAmount { TypeLevel = BlessingLevel.Big, Amount = 0},
                    }
                }) ;
            }

            _blessingValues = new Dictionary<BlessingLevel, float>  // TODO: LOAD/SAVE
            {
                { BlessingLevel.Small, _smallBlessing },
                { BlessingLevel.Medium, _mediumBlessing },
                { BlessingLevel.Big, _bigBlessing }
            };

            _blessingPrices = new Dictionary<BlessingLevel, int>
            {
                { BlessingLevel.Small, _smallBlessingPrice },
                { BlessingLevel.Medium, _mediumBlessingPrice },
                { BlessingLevel.Big, _bigBlessingPrice }
            };
        }

        public float GetBlessingValue(BuildingType p_building)
        {
            foreach (var blessing in _playerCurrentBlessings)
            {
                if (blessing.Key.AffectedBuilding == p_building)
                {
                    return _blessingValues[blessing.Value];
                }
            }

            return 0f;
        }

        public BlessingLevel GetCurrentBlessingLevel(GodType p_god)
        {
            foreach (var blessings in _playerCurrentBlessings)
            {
                if (blessings.Key.GodName == p_god)
                {
                    return blessings.Value;
                }
            }

            return BlessingLevel.Noone;
        }

        public void ResetBlessingOnNewDayStart()
        { 
        
        }

        public void ActivateBlessing()
        {

        }

        public int GetAmountOfAviableBlessings(GodType p_god, BlessingLevel p_blessing)
        {
            foreach (var blessings in _playerStoredBlessings)
            {
                if (blessings.Type == p_god)
                {
                    foreach (var blessing in blessings.AmountByType)
                    {
                        if (blessing.TypeLevel == p_blessing)
                        {
                            return blessing.Amount;
                        }
                    }
                }
            }

            return 0;
        }
    }

    public struct Blessing
    {
        public GodType Type;
        public BlessingLevelAmount[] AmountByType;
    }

    public struct BlessingLevelAmount
    {
        public BlessingLevel TypeLevel;
        public int Amount;
    }
}
