using UnityEngine;

public class Model : MonoBehaviour
{
    [Header("Art References")]
    [SerializeField] GameObject _model;
    [SerializeField] Material[] _materials;
    [SerializeField] Renderer _renderer;
    [SerializeField] MeshFilter _meshFilter;

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

    void Start()
    {
        if (_model == null)
        {
            Debug.LogError($"Model not setup on {name}!");
            Destroy(gameObject);
            return;
        }

        if (_meshFilter == null)
        {
            _meshFilter = _model.GetComponent<MeshFilter>();
            if (_meshFilter == null)
            {
                Debug.LogError($"MeshFilter not setup on {name}!");
                SetActive(false);
                return;
            }
        }

        if (_renderer == null)
        {
            _renderer = _model.GetComponent<Renderer>();
            if (_renderer == null)
            {
                Debug.LogError($"Renderer not setup on {name}!");
                SetActive(false);
                return;
            }
        }

        _materials = _renderer.materials;

        polyCount = _meshFilter.mesh.vertexCount;
        triCount = _meshFilter.mesh.triangles.Length;
        textureCount = _renderer.materials.Length;
    }

    public void SetActive(bool active)
    {
        if (_model == null)
        {
            Debug.LogError($"Model of {name} is not set up in the inspector!");
            return;
        }

        _model.SetActive(active);
    }

    public void ChangeRenderMode(RenderType renderType)
    {
        switch (renderType)
        {
            case RenderType.Textured:
                _renderer.materials = _materials;
                break;
            case RenderType.Clay:
                Material clayMat = new(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = Color.gray
                };
                _renderer.material = clayMat;

                break;
            case RenderType.Unlit:
                Material unlitMat = new(Shader.Find("Universal Render Pipeline/Unlit"))
                {
                    mainTexture = _materials[0].mainTexture
                };

                _renderer.material = unlitMat;
                break;

        }
    }
}
