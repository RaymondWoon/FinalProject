using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _optionsPanel;
    [SerializeField] private GameObject _quitConfirmationPanel;
    [SerializeField] private GameObject _instructionsPanel;
    [SerializeField] private Text _storylineText;

    [Header("Dungeon Parameters")]
    [SerializeField] private Slider _widthSlider;
    [SerializeField] private InputField _widthInputField;
    [SerializeField] private Slider _depthSlider;
    [SerializeField] private InputField _depthInputField;
    [SerializeField] private Slider _floorSlider;
    [SerializeField] private InputField _floorInputField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Add a listener to the dungeon width input field
        _widthInputField.onValueChanged.AddListener(delegate { DungeonWidthInputValueChanged(); });

        // Add a listener to the dungeon depth input field
        _depthInputField.onValueChanged.AddListener(delegate { DungeonDepthInputValueChanged(); });

        // Add a listener to the dungeon floor input field
        _floorInputField.onValueChanged.AddListener(delegate { DungeonFloorInputValueChanged(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region MAIN MENU

    // Handler for the 'Play' button clicked event
    public void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    // Handler for the 'Help' button clicked event
    public void OnHelpButtonClicked()
    {
        // Disable the 'Storyline'
        _storylineText.gameObject.SetActive(false);

        // activate 'Instructions Panel'
        _instructionsPanel.SetActive(true);
    }

    // Handler to close the 'Help' panel
    public void OnCloseInstructionPanelClicked()
    {
        _instructionsPanel.SetActive(false);

        // Enable the 'Storyline'
        _storylineText.gameObject.SetActive(true);
    }

    // Handler for the 'Options' button clicked event
    public void OnOptionsButtonClicked()
    {
        // Disable the 'Storyline'
        _storylineText.gameObject.SetActive(false);

        // activate 'Options Panel'
        _optionsPanel.SetActive(true);

        // set the width slider and text to the current dungeon width value
        _widthSlider.value = MainManager.Instance.DungeonWidth;
        _widthInputField.text = MainManager.Instance.DungeonWidth.ToString();

        // set the depth slider and text to the current dungeon depth value
        _depthSlider.value = MainManager.Instance.DungeonDepth;
        _depthInputField.text = MainManager.Instance.DungeonWidth.ToString();

        // set the floor slider and text to the current dungeon floow value
        _floorSlider.value = MainManager.Instance.DungeonFloor;
        _floorInputField.text = MainManager.Instance.DungeonFloor.ToString();

        // Set the DungeonWidthInput as the default selected
        InputField dungeonWidthInput = _optionsPanel.transform.Find("Dungeon Width").Find("Dungeon Width Input").GetComponent<InputField>();

        // Clear any previous selection
        EventSystem.current.SetSelectedGameObject(null);

        // Set the new selection
        EventSystem.current.SetSelectedGameObject(dungeonWidthInput.gameObject);

        // width slider event listener
        _widthSlider.onValueChanged.AddListener((v) =>
        {
            // update the width input field text
            _widthInputField.text = v.ToString("0");
        });

        // depth slider event listener
        _depthSlider.onValueChanged.AddListener((v) =>
        {
            // update the depth input field text
            _depthInputField.text = v.ToString("0");
        });

        // floor slider event listener
        _floorSlider.onValueChanged.AddListener((v) =>
        {
            // update the floor input field text
            _floorInputField.text = v.ToString("0");
        });
    }

    // Handler for the 'Quit' button clicked event
    public void ShowQuitConfirmationPanel()
    {
        _quitConfirmationPanel.SetActive(true);

        Button noButton = _quitConfirmationPanel.transform.Find("No Button").GetComponent<Button>();

        // Clear any previous selection
        EventSystem.current.SetSelectedGameObject(null);

        // Set the new selection
        EventSystem.current.SetSelectedGameObject(noButton.gameObject);
    }

    #endregion

    #region OPTIONS PANEL

    // Handler for the 'Apply' button on the 'Options Panel'
    public void OnApplyButtonClicked()
    {
        MainManager.Instance.DungeonWidth = Convert.ToInt32(_widthInputField.text);
        MainManager.Instance.DungeonDepth = Convert.ToInt32(_depthInputField.text);
        MainManager.Instance.DungeonFloor = Convert.ToInt32(_floorInputField.text);

        OnCloseOptionPanelClicked();

    }

    // Handler for the 'Cancel' button on the 'Options Panel'
    public void OnCloseOptionPanelClicked()
    {
        _optionsPanel.SetActive(false);

        // Enable the 'Storyline'
        _storylineText.gameObject.SetActive(true);
    }

    // Handler for DungeonWidthInput field
    private void DungeonWidthInputValueChanged()
    {
        int newValue = Convert.ToInt32(_widthInputField.text);

        if (newValue >= MainManager.Instance.MinDungeonWidth && newValue <= MainManager.Instance.MaxDungeonWidth)
        {
            _widthSlider.value = newValue;
        }
        else if (newValue < MainManager.Instance.MinDungeonWidth)
        {
            _widthSlider.value = MainManager.Instance.MinDungeonWidth;

        }
        else
        {
            _widthSlider.value = MainManager.Instance.MaxDungeonWidth;
        }
    }

    // Handler for DungeonDepthInput field
    private void DungeonDepthInputValueChanged()
    {
        int newValue = Convert.ToInt32(_depthInputField.text);

        if (newValue >= MainManager.Instance.MinDungeonDepth && newValue <= MainManager.Instance.MaxDungeonDepth)
        {
            _depthSlider.value = newValue;
        }
        else if (newValue < MainManager.Instance.MinDungeonDepth)
        {
            _depthSlider.value = MainManager.Instance.MinDungeonDepth;

        }
        else
        {
            _depthSlider.value = MainManager.Instance.MaxDungeonDepth;
        }
    }

    // Handler for DungeonFloorInput field
    private void DungeonFloorInputValueChanged()
    {
        int newValue = Convert.ToInt32(_floorInputField.text);

        if (newValue >= MainManager.Instance.MinDungeonFloor && newValue <= MainManager.Instance.MaxDungeonFloor)
        {
            _floorSlider.value = newValue;
        }
        else if (newValue < MainManager.Instance.MinDungeonFloor)
        {
            _floorSlider.value = MainManager.Instance.MinDungeonFloor;

        }
        else
        {
            _floorSlider.value = MainManager.Instance.MaxDungeonFloor;
        }
    }

    #endregion

    #region QUIT CONFIRMATION PANEL

    public void HideQuitConfirmationPanel()
    {
        _quitConfirmationPanel.SetActive(false);
    }

    public void OnQuitConfirmationButtonClicked()
    {
#if UNITY_EDITOR
        // Stops the Unity engine
        EditorApplication.isPlaying = false;
#endif

        // Quit the application
        Application.Quit();
    }

    #endregion
}
