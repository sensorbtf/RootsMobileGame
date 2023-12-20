using UnityEngine;

public class WorkersManager : MonoBehaviour
{
    public int BaseWorkersAmounts { get; set; }

    public int WorkersInBuilding { get; set; }

    public int WorkersInDefences { get; set; }

    public int WorkersInResources { get; set; }

    public int WorkersDefending { get; set; }

    public int OverallAssignedWorkers => WorkersInResources + WorkersInBuilding + WorkersInDefences + WorkersDefending;

    public void ResetAssignedWorkers()
    {
        WorkersInBuilding = 0;
        WorkersInDefences = 0;
        WorkersInResources = 0;
        WorkersDefending = 0;
    }

    public bool IsAnyWorkerFree()
    {
        if (BaseWorkersAmounts == OverallAssignedWorkers)
            return false;
        return true;
    }
}