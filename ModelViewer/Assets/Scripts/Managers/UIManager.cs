using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private List<Button> _spawnedModelButtons = new();
    [Header("Spawnable button info")]
    [SerializeField] private Button _modelUIBtn;
    [SerializeField] private TMP_Text _modelInfoText;
    [Header("Model button location info")]
    [SerializeField] private RectTransform _modelButtonHolder;
    [SerializeField] private RectTransform _modelSelectionPanel;

    [SerializeField] private Animator _modelSelectionPanelAnimator;

    [SerializeField] private GameObject _loadingScreen;

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        //TEMP uitgezet wegens geen model manager in test scene
        //ModelManager.Instance.OnModelChanged += RefreshModelInfo;
        //ModelManager.Instance.OnModelLoaded += SpawnButton;
    }

    private void OnDisable()
    {
        ModelManager.Instance.OnModelChanged -= RefreshModelInfo;
        ModelManager.Instance.OnModelLoaded -= SpawnButton;
    }

    /// <summary>
    /// Spawns a button for the latest model that was preloaded
    /// </summary>
    /// <param name="model"></param>

    public void SpawnButton(Model model)
    {
        Button newButton = Instantiate(_modelUIBtn, _modelButtonHolder.transform.position, _modelButtonHolder.transform.rotation, _modelButtonHolder);
        newButton.GetComponentInChildren<TMP_Text>().text = model.modelName;
        newButton.GetComponent<Image>().sprite = model.previewImage;
        RefreshModelInfo(model);//TEMP
        _spawnedModelButtons.Add(newButton);
    }

    private void OnModelButtonClick()
    {
        //laadscherm aanzetten
    }

    public void OnEnviromentButtonClicked(int index)
    {
        //call naar enviromentcontroller om de enviroment te veranderen
    }

    private void LoadScreen()
    {
        //kijken hoe veel dingen en geladen moeten worden als het niet 0 dingen zijn laadscherm aan als het wel 0 dingen zijn laadscherm uit
    }

    /// <summary>
    /// Sets all the inforation of a model in the info UI based on last clicked model.
    /// </summary>
    /// <param name="modelinfo"></param>

    private void RefreshModelInfo(Model modelinfo)
    {
        _modelInfoText.text = ("Naam model: " + modelinfo.modelName + "\n"
            + "Omschrijving: " + modelinfo.description + "\n"
            + "Naam artist: " + modelinfo.creatorName + "\n"
            + "PollyCount = " + modelinfo.polyCount + "\n"
            + "TriCount = " + modelinfo.triCount + "\n"
            + "TextureCount = " + modelinfo.textureCount);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
