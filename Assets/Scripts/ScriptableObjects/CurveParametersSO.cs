using UnityEngine;

[CreateAssetMenu(fileName = "CurveParameters", menuName = "Hand Curve Parameters")]
public class CurveParametersSO : ScriptableObject
{
    public AnimationCurve positioning;
    public float positioningInfluence = .1f;
    public AnimationCurve rotation;
    public float rotationInfluence = 10f;
}