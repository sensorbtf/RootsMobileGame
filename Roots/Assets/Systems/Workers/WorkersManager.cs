using System;
using UnityEngine;

public class WorkersManager : MonoBehaviour
{
    private int _baseBaseWorkersAmount;
    private int _workersInBuilding;
    private int _workersInDefences;
    private int _workersInResources;
    private int _workersDefending;
    
    public int BaseWorkersAmounts
    {
        get => _baseBaseWorkersAmount;
        set => _baseBaseWorkersAmount = value;
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
    
    public int WorkersDefending
    {
        get => _workersDefending;
        set => _workersDefending = value;
    }
    
    public int OverallAssignedWorkers
    {
        get => _workersInResources + _workersInBuilding + _workersInDefences + _workersDefending;
    }

    public void ResetAssignedWorkers()
    {
        _workersInBuilding = 0;
        _workersInDefences = 0;
        _workersInResources = 0;
        _workersInDefences = 0;
    }

    public bool IsAnyWorkerFree()
    {
        if (_baseBaseWorkersAmount == OverallAssignedWorkers)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
