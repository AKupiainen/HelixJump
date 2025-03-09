using UnityEngine;

public class StepGeneration
{
    private readonly float _rotationSpeed;
    private readonly int _stepIndex;
    private readonly float _stepAngle;
    private readonly float _scaleX, _scaleZ;
    private readonly Color _stepColor;
    private readonly Color _obstacleColor;
    private readonly bool[] _isObstacleArray;

    public float RotationSpeed => _rotationSpeed;
    public int StepIndex => _stepIndex;
    public float StepAngle => _stepAngle;
    public float ScaleX => _scaleX;
    public float ScaleZ => _scaleZ;
    public Color StepColor => _stepColor;
    public Color ObstacleColor => _obstacleColor;
    public bool[] IsObstacleArray => _isObstacleArray;

    public StepGeneration(float rotationSpeed, int stepIndex, float stepAngle, Color stepColor, Color obstacleColor, float scaleX, float scaleZ, bool[] isObstacleArray)
    {
        _rotationSpeed = rotationSpeed;
        _stepIndex = stepIndex;
        _stepAngle = stepAngle;
        _stepColor = stepColor;
        _obstacleColor = obstacleColor;
        _scaleX = scaleX;
        _scaleZ = scaleZ;
        _isObstacleArray = isObstacleArray;
    }
}