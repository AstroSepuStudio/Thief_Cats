using UnityEngine;

public class ValuableItem : MonoBehaviour
{
    [SerializeField] ValuableItemSO _valuableItemSO;

    float _objectValue;

    private void Start()
    {
        _objectValue = _valuableItemSO.GenerateObjectValue();
    }

    public float GetValue()
    {
        return _objectValue;
    }
}
