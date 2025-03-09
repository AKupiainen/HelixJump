using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private readonly Dictionary<Transform, bool> _touchingPlatforms = new();

    private const string DeletedStepTag = "WillBeDeletedStep";

    public Dictionary<Transform, bool> TouchingPlatforms => _touchingPlatforms;

    private void OnTriggerEnter(Collider other)
    {
        TouchingAPlatform(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TouchingAPlatform(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Transform step = GetStepTransform(other);
        
        if (step == null)
        {
            return;
        }

        if (_touchingPlatforms.ContainsKey(step))
        {
            TouchingPlatforms.Remove(step);
        }
    }

    private void TouchingAPlatform(Collider other)
    {
        Transform step = GetStepTransform(other);
        if (step == null)
        {
            return;
        }

        bool isObstacle = other.transform.parent.name.EndsWith("_1");
        TouchingPlatforms[step] = isObstacle;
    }

    private Transform GetStepTransform(Collider other)
    {
        Transform step = other.transform.parent.parent;
        
        if (step == null)
        {
            return null;
        }

        if (step.CompareTag(DeletedStepTag))
        {
            return null;
        }

        return step;
    }
}