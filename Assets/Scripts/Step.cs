using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Step
{
    [FormerlySerializedAs("count")] [SerializeField] private int _count;
    [FormerlySerializedAs("height")] [SerializeField] private  float _height;
    [FormerlySerializedAs("normalPlatformSample")] [SerializeField] private GameObject _normalPlatformSample;
    [FormerlySerializedAs("obstaclePlatformSample")] [SerializeField] private  GameObject _obstaclePlatformSample;

    public GameObject ObstaclePlatformSample => _obstaclePlatformSample;
    public GameObject NormalPlatformSample => _normalPlatformSample;
    public float Height => _height;
    public int Count => _count;
}