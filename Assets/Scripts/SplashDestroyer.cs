using UnityEngine;

public class SplashDestroyer : MonoBehaviour
{
    private int _halfTurns = 0;
    private float _lastAngle = float.NaN;

    private void Update()
    {
        float angle = Vector3.SignedAngle(-Vector3.forward, transform.position, Vector3.up);

        if (float.IsNaN(_lastAngle))
        {
            _lastAngle = angle;
            return;
        }

        float angleDifference = Mathf.DeltaAngle(_lastAngle, angle);

        if (Mathf.Abs(angleDifference) >= 180f)
        {
            _halfTurns++;

            if (_halfTurns >= 3)
            {
                Destroy(gameObject);
            }
        }

        _lastAngle = angle;
    }
}