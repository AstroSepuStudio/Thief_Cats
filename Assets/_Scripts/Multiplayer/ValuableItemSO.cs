using UnityEngine;

[CreateAssetMenu]
public class ValuableItemSO : ScriptableObject
{
    public int ObjectMinValue;
    public int ObjectMaxValue;

    public int GenerateObjectValue()
    {
        return Random.Range(ObjectMinValue, ObjectMaxValue + 1);
    }
}
