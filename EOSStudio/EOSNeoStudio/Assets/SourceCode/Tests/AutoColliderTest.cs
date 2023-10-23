using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class AutoColliderTest : MonoBehaviour
{
    // Start is called before the first frame update
    private CapsuleCollider _collider;
    void Start()
    {
        _collider = gameObject.AddComponent<CapsuleCollider>();

        var backupscale = transform.localScale;
        var backupparent = transform.parent;

        transform.parent = null;
        transform.localScale = Vector3.one;
        var renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;
        Vector3 tcenter = Vector3.zero;
        foreach (var r in renderers)
        {
            var lcenter = r.bounds.center;
            tcenter += lcenter;
        }
        tcenter /= renderers.Length;
        var bound = new Bounds(tcenter,Vector3.zero);
        for (int i = 0; i < renderers.Length; i++)
        {
            bound.Encapsulate(renderers[i].bounds);
        }

        var col = _collider as CapsuleCollider;
        col.center = bound.center - transform.position;
        col.radius = bound.extents.x > bound.extents.z ? bound.extents.x : bound.extents.z;  
        col.height = bound.extents.y * 2;

        transform.parent = backupparent;
        transform.localScale = backupscale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
