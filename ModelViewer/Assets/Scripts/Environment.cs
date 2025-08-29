using UnityEngine;

[CreateAssetMenu(fileName = "New Environment", menuName = "Environment")]
public class Environment : ScriptableObject
{
    public string Name;
    public Sprite previewImage;
    public LightingType lightingType;

    public Cubemap skyboxTexture;
    public Vector3 lightAngle;
}
