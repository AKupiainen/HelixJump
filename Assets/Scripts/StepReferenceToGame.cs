using UnityEngine;

public class StepReferenceToGame
{
    private readonly float _rotationSpeed;
    private readonly int _index;
    private readonly Transform _step;

    public float RotationSpeed => _rotationSpeed;
    
    public int Index => _index;
    public Transform Step => _step;

    public StepReferenceToGame(float rotationSpeed, int index, Transform step)
    {
        _rotationSpeed = rotationSpeed;
        _index = index;
        _step = step;
    }
}