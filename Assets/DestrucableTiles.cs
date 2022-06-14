using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DestrucableTiles : MonoBehaviour
{
    private Tilemap _tilemap;
    private ShadowCaster2DTileMap _shadowCaster;

    private void Start()
    {
        _tilemap = GetComponent<Tilemap>();
        _shadowCaster = GetComponent<ShadowCaster2DTileMap>();
    }
    
    private void DestroyTile(Vector3 tile)
    {
        _tilemap.SetTile(_tilemap.WorldToCell(tile), null);
        StartCoroutine(nameof(UpdateShadows));
    }

    private IEnumerator UpdateShadows()
    {
        yield return null;
        _shadowCaster.DestroyAllChildren();
        _shadowCaster.Generate();
    }
    
    private void OnEnable()
    {
        LazerGun.OnHitTile += DestroyTile;
    }

    private void OnDisable()
    {
        LazerGun.OnHitTile -= DestroyTile;
    }
}
