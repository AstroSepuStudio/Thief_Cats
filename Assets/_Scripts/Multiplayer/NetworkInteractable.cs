using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkInteractable : MonoBehaviourPun
{
    [SerializeField] Rigidbody _rb;
    [SerializeField] AnimationCurve _pullForceCurve;
    [SerializeField] float _maxPullDistance = 10f;
    bool _isBeingPulled = false;
    Transform _targetOrigin;
    float _pullRange;
    float _strength;

    void Awake()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody>();
    }

    public void StartPull(Transform origin, float range, float strength)
    {
        if (!photonView.IsMine)
            return;

        _rb.useGravity = false;
        _targetOrigin = origin;
        _pullRange = range;
        _strength = strength;
        _isBeingPulled = true;
    }

    public void StopPull()
    {
        if (!photonView.IsMine)
            return;

        _rb.useGravity = true;
        _isBeingPulled = false;
    }

    void FixedUpdate()
    {
        if (!_isBeingPulled || !photonView.IsMine)
            return;

        Vector3 targetPoint = _targetOrigin.position + _targetOrigin.forward * _pullRange;
        Vector3 direction = targetPoint - transform.position;
        float distance = direction.magnitude;

        if (distance > _maxPullDistance)
        {
            StopPull();
            return;
        }

        float normalizedDistance = Mathf.Clamp01(distance / _maxPullDistance);

        float curveMultiplier = _pullForceCurve.Evaluate(normalizedDistance);

        float finalForce = _strength * curveMultiplier;
        _rb.AddForce(direction.normalized * finalForce, ForceMode.Force);
    }
}
