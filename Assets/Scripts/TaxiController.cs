using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaxiController : MonoBehaviour
{
    private SpriteRenderer _sprite;
    
    [Header("Taxi Controls")]
    [SerializeField] private float _acceleration = 30;
    [SerializeField] private float _deceleration = 5;
    [SerializeField] private float _maxSpeed = 13;

    private float _inputHorizontal, _inputVertical;
    private float _speedHorizontal, _speedVertical;

    [Header("Collision")] 
    [SerializeField] private Bounds _characterBounds;
    [SerializeField] private LayerMask _groundLayer;

    [SerializeField] private int _detectorCount = 3;
    [SerializeField] [Range(0f, 1f)] private float _bouncyness = 0.5f;
    [SerializeField] private float _detectionRayLength = 0.1f;
    [SerializeField] [Range(0.1f, 0.3f)] private float _rayBuffer = 0.1f; // Prevents side detectors hitting the ground

    private RayRange _raysUp, _raysRight, _raysDown, _raysLeft;
    private bool _colUp, _colRight, _colDown, _colLeft;
    
    private void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        CheckCollision();
        Move();
    }

    private void CheckCollision() {
        // Generate ray ranges. 
        CalculateRayRanged();
        
        // The rest
        _colUp = RunDetection(_raysUp);
        _colLeft = RunDetection(_raysLeft);
        _colRight = RunDetection(_raysRight);
        _colDown = RunDetection(_raysDown);

        bool RunDetection(RayRange range) {
            return EvaluateRayPositions(range).Any(point => Physics2D.Raycast(point, range.Dir, _detectionRayLength, _groundLayer));
        }
    }

    private void CalculateRayRanged() {
        // This is crying out for some kind of refactor. 
        var b = new Bounds(transform.position + _characterBounds.center, _characterBounds.size);

        _raysDown = new RayRange(b.min.x + _rayBuffer, b.min.y, b.max.x - _rayBuffer, b.min.y, Vector2.down);
        _raysUp = new RayRange(b.min.x + _rayBuffer, b.max.y, b.max.x - _rayBuffer, b.max.y, Vector2.up);
        _raysLeft = new RayRange(b.min.x, b.min.y + _rayBuffer, b.min.x, b.max.y - _rayBuffer, Vector2.left);
        _raysRight = new RayRange(b.max.x, b.min.y + _rayBuffer, b.max.x, b.max.y - _rayBuffer, Vector2.right);
    }

    private IEnumerable<Vector2> EvaluateRayPositions(RayRange range) {
        for (var i = 0; i < _detectorCount; i++) {
            var t = (float)i / (_detectorCount - 1);
            yield return Vector2.Lerp(range.Start, range.End, t);
        }
    }
    
    private void GetInput()
    {
        _inputHorizontal = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(_inputHorizontal) > 0.05f)
        {
            // _sprite.color = Color.green;
            _speedHorizontal += _inputHorizontal * _acceleration * Time.deltaTime;
            _speedHorizontal = Mathf.Clamp(_speedHorizontal, -_maxSpeed, _maxSpeed);  
            transform.localScale = new Vector3(_inputHorizontal / Mathf.Abs(_inputHorizontal), 1f, 1f);

        }
        else           
        {
            // _sprite.color = Color.yellow;
            _speedHorizontal = Mathf.MoveTowards(_speedHorizontal, 0, _deceleration * Time.deltaTime);
        }

        if (Mathf.Abs(_speedHorizontal) <= 0.05f)
        {
            // _sprite.color = Color.white;
        }
        
        if (_speedHorizontal > 0 && _colRight || _speedHorizontal < 0 && _colLeft) {
            // Don't walk through walls
            _speedHorizontal = -_speedHorizontal * _bouncyness;
        }
        
        
        _inputVertical = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(_inputVertical) > 0.05f)
        {
            // _sprite.color = Color.green;
            _speedVertical += _inputVertical * _acceleration * Time.deltaTime;
            _speedVertical = Mathf.Clamp(_speedVertical, -_maxSpeed, _maxSpeed);    
        }
        else           
        {
            // _sprite.color = Color.yellow;
            _speedVertical = Mathf.MoveTowards(_speedVertical, 0, _deceleration * Time.deltaTime);
        }

        if (Mathf.Abs(_speedVertical) <= 0.05f)
        {
            // _sprite.color = Color.white;
        }

        if (_speedVertical > 0 && _colUp || _speedVertical < 0 && _colDown) {
            // Don't walk through walls
            _speedVertical = -_speedVertical * _bouncyness;
        }
        
    }
    
    

    private int _freeColliderIterations = 10;
    private void Move() {
        var pos = transform.position;
        var move = new Vector3(_speedHorizontal, _speedVertical) * Time.deltaTime;
        var furthestPoint = pos + move;

        // check furthest movement. If nothing hit, move and don't do extra checks
        var hit = Physics2D.OverlapBox(furthestPoint, _characterBounds.size, 0, _groundLayer);
        if (!hit) {
            transform.position += move;
            return;
        }
        
        // otherwise increment away from current pos; see what closest position we can move to
        var positionToMoveTo = transform.position;
        for (int i = 1; i < _freeColliderIterations; i++)
        {
            // increment to check all but furthestPoint - we did that already
            var t = (float) i / _freeColliderIterations;
            var posToTry = Vector2.Lerp(pos, furthestPoint, t);

            if (Physics2D.OverlapBox(posToTry, _characterBounds.size, 0, _groundLayer))
            {
                transform.position = positionToMoveTo;
                
                // We've landed on a corner or hit our head on a ledge. Nudge the player gently
                if (i == 1)
                {
                    _speedHorizontal = -_speedHorizontal * _bouncyness;
                    _speedVertical = -_speedVertical * _bouncyness;
                    // if (_speedVertical < 0) _speedVertical = 0;
                    // var dir = transform.position - hit.transform.position;
                    // transform.position += dir.normalized * move.magnitude;
                }
                
                return;
            }

            positionToMoveTo = posToTry;
        }
    }

    // private void OnDrawGizmos() {
    //     // Bounds
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireCube(transform.position + _characterBounds.center, _characterBounds.size);
    // }
    
    private void OnDrawGizmos() {
        // Bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + _characterBounds.center, _characterBounds.size);

        // Rays
        if (!Application.isPlaying) {
            CalculateRayRanged();
            Gizmos.color = Color.blue;
            foreach (var range in new List<RayRange> { _raysUp, _raysRight, _raysDown, _raysLeft }) {
                foreach (var point in EvaluateRayPositions(range)) {
                    Gizmos.DrawRay(point, range.Dir * _detectionRayLength);
                }
            }
        }

        if (!Application.isPlaying) return;

        // Draw the future position. Handy for visualizing gravity
        Gizmos.color = Color.red;
        var move = new Vector3(_speedHorizontal, _speedVertical) * Time.deltaTime;
        Gizmos.DrawWireCube(transform.position + _characterBounds.center + move, _characterBounds.size);
    }

    public struct RayRange {
        public RayRange(float x1, float y1, float x2, float y2, Vector2 dir) {
            Start = new Vector2(x1, y1);
            End = new Vector2(x2, y2);
            Dir = dir;
        }

        public readonly Vector2 Start, End, Dir;
    }
    
}
