// Decompiled with JetBrains decompiler
// Type: MoreMountains.InfiniteRunnerEngine.Parallax
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EE9AC18D-DD28-4E21-930D-E4B7F674B547
// Assembly location: /home/yanniboi/Games/Freschyboi/InfiniteRunnerAssembly.dll

using UnityEngine;

namespace InfiniteRunner
{
  public class Parallax : MonoBehaviour
  {
    public float ParallaxSpeed;
    public PossibleDirections ParallaxDirection;
    protected GameObject _clone;
    protected Vector3 _movement;
    protected Vector3 _initialPosition;
    protected Vector3 _newPosition;
    protected Vector3 _direction;
    protected float _width;

    protected virtual void Start()
    {
      var bounds = GetComponent<Collider2D>().bounds; 
      if (ParallaxDirection == PossibleDirections.Left || ParallaxDirection == PossibleDirections.Right)
      {
        _width = bounds.size.x;
        _newPosition = new Vector3(transform.position.x + _width, transform.position.y, transform.position.z);
      }
      if (ParallaxDirection == PossibleDirections.Up || ParallaxDirection == PossibleDirections.Down)
      {
        _width = bounds.size.y;
        _newPosition = new Vector3(transform.position.x, transform.position.y + _width, transform.position.z);
      }
      if (ParallaxDirection == PossibleDirections.Forwards || ParallaxDirection == PossibleDirections.Backwards)
      {
        _width = bounds.size.z;
        _newPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z + _width);
      }
      switch (ParallaxDirection)
      {
        case PossibleDirections.Left:
          _direction = Vector3.left;
          break;
        case PossibleDirections.Right:
          _direction = Vector3.right;
          break;
        case PossibleDirections.Up:
          _direction = Vector3.up;
          break;
        case PossibleDirections.Down:
          _direction = Vector3.down;
          break;
        case PossibleDirections.Forwards:
          _direction = Vector3.forward;
          break;
        case PossibleDirections.Backwards:
          _direction = Vector3.back;
          break;
      }
      _initialPosition = transform.position;
      _clone = Instantiate(gameObject, _newPosition, transform.rotation);
      Destroy(_clone.GetComponent<Parallax>());
    }

    protected virtual void Update()
    {
      _movement = _direction * (ParallaxSpeed / 10f) * Time.deltaTime;
      _clone.transform.Translate(_movement);
      transform.Translate(_movement);
      if (!ShouldResetPosition())
        return;
      transform.Translate(-_direction * _width);
      _clone.transform.Translate(-_direction * _width);
    }

    protected virtual bool ShouldResetPosition()
    {
      switch (ParallaxDirection)
      {
        case PossibleDirections.Left:
          return transform.position.x + (double) _width < _initialPosition.x;
        case PossibleDirections.Right:
          return transform.position.x - (double) _width > _initialPosition.x;
        case PossibleDirections.Up:
          return transform.position.y - (double) _width > _initialPosition.y;
        case PossibleDirections.Down:
          return transform.position.y + (double) _width < _initialPosition.y;
        case PossibleDirections.Forwards:
          return transform.position.z - (double) _width > _initialPosition.z;
        case PossibleDirections.Backwards:
          return transform.position.z + (double) _width < _initialPosition.z;
        default:
          return false;
      }
    }

    public enum PossibleDirections
    {
      Left,
      Right,
      Up,
      Down,
      Forwards,
      Backwards,
    }
  }
}
