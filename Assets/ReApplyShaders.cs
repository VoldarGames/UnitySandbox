using UnityEngine;

public class ReApplyShaders : MonoBehaviour
{
    public Renderer[] renderers;
    public Material[] materials;
    public string[] shaders;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        foreach (var rend in renderers)
        {
            materials = rend.sharedMaterials;
            shaders = new string[materials.Length];

            for (int i = 0; i < materials.Length; i++)
            {
                shaders[i] = materials[i].shader.name;
            }

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].shader = Shader.Find(shaders[i]);
            }
        }
    }
}
