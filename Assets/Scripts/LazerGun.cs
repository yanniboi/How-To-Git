using System;
using UnityEngine;

public class LazerGun : MonoBehaviour
{
    public static event Action<Vector3> OnHitTile;
    
    public LineRenderer _line;
    public float lazerdistance;
    public LayerMask LayerMask;

    private Vector3 _lazerTarget;

    void Update()
    {
        GetTarget();
        ShootLazer();
    }

    private void GetTarget()
    {
        // Set to zero by default so we dont shoot unless we press a key.
        _lazerTarget = Vector3.zero;
        if (Input.GetKey(KeyCode.Space))
        {
            // Take ship orientation into account
            Vector2 direction = Vector2.right * gameObject.transform.parent.transform.localScale.x;
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, lazerdistance, LayerMask);
            if (hit.collider != null)
            {
                _lazerTarget = (hit.point) - (Vector2) transform.position;
                
                Vector3 tilePosition = Vector3.zero;
                tilePosition.x = hit.point.x + 0.01f * direction.x;
                tilePosition.y = hit.point.y + 0.01f * direction.y;
                OnHitTile?.Invoke(tilePosition);
            }
            else
            {
                _lazerTarget = direction * lazerdistance;
            }

            // Debug
            Debug.DrawRay(transform.position, direction*lazerdistance, Color.green);
        }
    }
    
    private void ShootLazer()
    {
        // Line position is relative to the line origin and so should always be positive.
        Vector3 linePosition = _lazerTarget * _lazerTarget.normalized.x;
        _line.SetPosition(1, linePosition);
    }
    
}
