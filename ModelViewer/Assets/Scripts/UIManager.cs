using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private List<Button> _spawnedModelButtons;
    private GameObject _modelUIBtn;//De prefab voor de model buttons
    private RectTransform _modelButtonHolder;
    private RectTransform _modelSelectionPanel;
    private Animator _modelSelectionPanelAnimator;

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void SpawnButtons()
    {
        _spawnedModelButtons.Add(Instantiate<GameObject>(_modelUIBtn, _modelButtonHolder.transform.position, _modelButtonHolder.transform.rotation).GetComponent<Button>());
        //de preview van het model toe voegen en naam goed zeten
    
    }

    private void OnModelButtonClick()
    {
        //laadscherm aanzetten
    }

    public void OnEnviromentButtonClicked(int index)
    {
        //call naar enviromentcontroller om de enviroment te veranderen
    }

    private void RefreshModelInfo(/*Model modelinfo*/)
    {

    }

    private void QuitApplication()
    {
        Application.Quit();
    }
}
