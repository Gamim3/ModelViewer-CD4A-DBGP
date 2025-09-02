using System.Collections.Generic;
using UnityEngine;

public class Model : MonoBehaviour
{
    [Header("Art References")]
    [SerializeField] private GameObject _model;
    [SerializeField] private List<Material> _materials = new();
    [SerializeField] private Renderer[] _renderer;
    [SerializeField] private MeshFilter[] _meshFilter;
    [SerializeField] private Vector3 _rotationOffset;

    public Vector3 RotationOffset => _rotationOffset;

    [Header("Model Info")]
    public string modelName = "Cool model";
    public string description = "This is a model";
    public string creatorName = "Tyler";
    [Tooltip("Polycount is automatically calculated on start")]
    public int polyCount = -1;
    [Tooltip("TriCount is automatically calculated on start")]
    public int triCount = -1;
    [Tooltip("TextureCount is automatically calculated on start")]
    public int textureCount = -1;
    [Space]
    public Sprite previewImage;
    public Renderer Renderer => _renderer != null && _renderer.Length > 0 ? _renderer[0] : null;

    private void Start()
    {
        if (_model == null)
        {
            Debug.LogError($"Model not setup on {name}!");
            Destroy(gameObject);
            return;
        }

        if (_meshFilter == null)
        {
            if (_meshFilter.Length == 0)
            {
                Debug.LogError($"MeshFilter not setup on {name}!");
                SetActive(false);
                return;
            }
        }

        if (_renderer == null)
        {
            if (_renderer == null)
            {
                Debug.LogError($"Renderer not setup on {name}!");
                SetActive(false);
                return;
            }
        }


        for (int i = 0; i < _renderer.Length; i++)
        {
            if (_renderer[i] == null) continue;
            for (int j = 0; j < _renderer[i].materials.Length; j++)
            {
                _materials.Add(new Material(_renderer[i].materials[j]));
            }
        }


        for (int i = 0; i < _meshFilter.Length; i++)
        {
            polyCount += _meshFilter[i].mesh.vertexCount;
            triCount += _meshFilter[i].mesh.triangles.Length;
        }
        textureCount = _materials.Count;
    }

    /// <summary>
    /// Set the model active or inactive
    /// </summary>
    /// <param name="active"> True = On </param>
    public void SetActive(bool active)
    {
        if (_model == null)
        {
            Debug.LogError($"Model of {name} is not set up in the inspector!");
            return;
        }

        _model.SetActive(active);
    }

    /// <summary>
    /// Change the render mode of the model based on <paramref name="renderType"/>
    /// </summary>
    /// <param name="renderType"> RenderType to switch to </param>
    public void ChangeRenderMode(RenderType renderType)
    {
        if (_materials.Count == 0) return;

        switch (renderType)
        {
            case RenderType.Textured:
                int materialIndex = 0;

                foreach (var rend in _renderer)
                {
                    Material[] materials = new Material[rend.materials.Length];
                    for (int i = 0; i < materials.Length; i++)
                    {

                        materials[i] = _materials[materialIndex];
                        materialIndex++;
                    }

                    rend.materials = materials;
                }

                break;
            case RenderType.Clay:
                Material clayMat = new(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = Color.gray
                };

                foreach (var rend in _renderer)
                {
                    Material[] materials = rend.materials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = clayMat;
                    }

                    rend.materials = materials;
                }
                break;
            case RenderType.Unlit:
                materialIndex = 0;
                foreach (var rend in _renderer)
                {
                    Material[] materials = rend.materials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        Material unlitMat = new(Shader.Find("Universal Render Pipeline/Unlit"))
                        {
                            mainTexture = _materials[materialIndex].HasProperty("_MainTex") ? _materials[materialIndex].mainTexture : Texture2D.whiteTexture
                        };

                        materials[i] = unlitMat;
                        materialIndex++;
                    }

                    rend.materials = materials;
                }
                break;

        }
    }
}