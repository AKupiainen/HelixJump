using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct BackgroundColorPair
{
    [SerializeField]
    [FormerlySerializedAs("BackgroundColorTop")]
    private Gradient _backgroundColorTop;

    [SerializeField]
    [FormerlySerializedAs("BackgroundColorBottom")]
    private Gradient _backgroundColorBottom;

    public Gradient BackgroundColorTop => _backgroundColorTop;
    public Gradient BackgroundColorBottom => _backgroundColorBottom;
}