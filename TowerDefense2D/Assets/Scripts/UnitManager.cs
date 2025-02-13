using UnityEngine;
using UnityEngine.AI;
public class UnitManager : MonoBehaviour
{
    //find furthest unit from base with a melee script
    public GameObject FindFurthestUnit(GameObject[] units, Transform baseTransform)
    {
        GameObject furthestUnit = null;
        float maxDistance = 0;
        foreach (GameObject unit in units)
        {
            if (unit.GetComponent<Melee>())
            {
                float distance = Vector3.Distance(unit.transform.position, baseTransform.position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    furthestUnit = unit;
                }
            }
        }
        return furthestUnit;
    }
}
