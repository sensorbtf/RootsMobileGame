using System;
using UnityEngine;

public class WorkersManager : MonoBehaviour
{
    private int _workersAmounts;

    public event Action<int> OnWorkersUpdated;
    
    public int WorkersAmount
    {
        get => _workersAmounts;
        set
        {
            _workersAmounts = value;
            OnWorkersUpdated?.Invoke(_workersAmounts);
        }
    }
}
