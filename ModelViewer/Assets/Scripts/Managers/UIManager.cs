using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private List<Button> _spawnedModelButtons;
    private Button _modelUIBtn;//De prefab voor de model buttons
    private RectTransform _modelButtonHolder;
    private RectTransform _modelSelectionPanel;
    private Animator _modelSelectionPanelAnimator;

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        ModelManager.Instance.OnModelChanged += RefreshModelInfo;
        ModelManager.Instance.OnModelLoaded += SpawnButtons;
    }

    private void OnDisable()
    {
        ModelManager.Instance.OnModelChanged -= RefreshModelInfo;
        ModelManager.Instance.OnModelLoaded -= SpawnButtons;
    }

    private void SpawnButtons(Model model)
    {
            Button newButton = (Instantiate<Button>(_modelUIBtn, _modelButtonHolder.transform.position, _modelButtonHolder.transform.rotation));
            newButton.GetComponentInChildren<TMP_Text>().text = model.modelName;
            newButton.GetComponent<Image>().sprite = model.previewImage;
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

    private void RefreshModelInfo(Model modelinfo)
    {

    }

    private void QuitApplication()
    {
        Application.Quit();
    }
}
