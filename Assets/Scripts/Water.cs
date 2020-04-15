using UnityEngine;
using System.Collections;

public class Water : MonoBehaviour {

    public Material waterMaterial;

    private Vector2 offset = Vector2.zero;

    void Update()
    {
        offset.x += 0.003f * Time.deltaTime;
        offset.y -= 0.06f * Time.deltaTime;

        waterMaterial.SetTextureOffset("_MainTex", offset);
    }

    void Destroy()
    {
        waterMaterial.SetTextureOffset("_MainTex", Vector2.zero);
    }
}
