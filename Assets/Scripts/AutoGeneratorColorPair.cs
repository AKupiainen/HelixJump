using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class AutoGeneratorColorPair
{
    [SerializeField]
    [FormerlySerializedAs("normalPlatformGradientColor")]
    private Gradient _normalPlatformGradientColor;

    [SerializeField]
    [FormerlySerializedAs("obstaclePlatformGradientColor")]
    private Gradient _obstaclePlatformGradientColor;

    public Gradient NormalPlatformGradientColor => _normalPlatformGradientColor;
    public Gradient ObstaclePlatformGradientColor => _obstaclePlatformGradientColor;
}