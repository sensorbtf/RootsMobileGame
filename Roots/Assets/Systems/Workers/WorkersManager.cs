using System;
using UnityEngine;

public class WorkersManager : MonoBehaviour
{
    private int _baseBaseWorkersAmount;
    private int _workersInBuilding;
    private int _workersInDefences;
    private int _workersInResources;

    public event Action<int> OnWorkersUpdated;
    
    public int BaseWorkersAmounts
    {
        get => _baseBaseWorkersAmount;
        set
        {
            _baseBaseWorkersAmount = value;
            OnWorkersUpdated?.Invoke(_baseBaseWorkersAmount);
        }
    }
    
    public int WorkersInBuilding
    {
        get => _workersInBuilding;
        set => _workersInBuilding = value;
    }
    
    public int WorkersInDefences
    {
        get => _workersInDefences;
        set => _workersInDefences = value;
    }
    
    public int WorkersInResources
    {
        get => _workersInResources;
        set => _workersInResources = value;
    }    
    
    public int OverallAssignedWorkers
    {
        get => _workersInResources + _workersInBuilding + _workersInDefences;
    }

    public void ResetAssignedWorkers()
    {
        _workersInBuilding = 0;
        _workersInDefences = 0;
        _workersInResources = 0;
    }
}
