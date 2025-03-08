using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Volpi.Entertainment.SDK.Utilities
{
    [Serializable, VolumeComponentMenu("Custom/Anime Speed Lines")]
    public class SpeedLinesEffect : VolumeComponent, IPostProcessComponent
    {
        [SerializeField] private ClampedFloatParameter _strength = new(0f, 0f, 1f);
        [SerializeField] private ClampedFloatParameter _speed = new(3f, 0f, 10f);
        [SerializeField] private ClampedFloatParameter _lineCount = new(40f, 10f, 100f);
        [SerializeField] private ClampedFloatParameter _lineWidth = new(0.1f, 0.01f, 0.5f);
        [SerializeField] private ClampedFloatParameter _fadeDistance = new(0.2f, 0.1f, 10f);
        [SerializeField] private Vector3Parameter _centerOffset = new(new Vector3(0.2f, 0.1f, 0.5f));

        public ClampedFloatParameter Strength => _strength;
        public ClampedFloatParameter LineCount => _lineCount;
        public ClampedFloatParameter LineWidth => _lineWidth;
        public ClampedFloatParameter FadeDistance => _fadeDistance;
        public Vector3Parameter CenterOffset => _centerOffset;
        public ClampedFloatParameter Speed => _speed;

        public bool IsActive() => _strength.value > 0f;
        public bool IsTileCompatible() => false;
    }
}