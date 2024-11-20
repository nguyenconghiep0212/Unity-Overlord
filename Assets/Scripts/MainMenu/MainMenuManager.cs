using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MainMenuManager;

public class MainMenuManager : MonoBehaviour
{

    public static MainMenuManager Instance { get; private set; }

    public GameObject operationSelection;
     public GameObject mainMenu;
    public List<GameObject> radioButtons;

    public GameObject panelWithButtons;

    private void Awake()
    {
        if (Instance != null & Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Setting.Instance.loadingScreen.SetActive(false);
        mainMenu.SetActive(true);
        operationSelection.SetActive(false);
        Setting.Instance.tutorialUI.SetActive(false);

        foreach (GameObject btn in radioButtons)
        {
            btn.transform.GetChild(1).gameObject.SetActive(false);
        }
        radioButtons[(int)Setting.Instance.difficulty].transform.GetChild(1).gameObject.SetActive(true);


        Button[] buttons = panelWithButtons.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(Setting.Instance.PlayButtonClickSound);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayGame()
    {

        mainMenu.SetActive(false);
        operationSelection.SetActive(true);
    }

    public void SelectDifficulty(int selectedDifficulty)
    {
        foreach (GameObject btn in radioButtons)
        {
            btn.transform.GetChild(1).gameObject.SetActive(false);
        }
        radioButtons[selectedDifficulty].transform.GetChild(1).gameObject.SetActive(true);

        switch (selectedDifficulty)
        {
            case 0:
                Setting.Instance.difficulty = Setting.DifficultyEnum.easy;
                break;
            case 1:
                Setting.Instance.difficulty = Setting.DifficultyEnum.normal;
                break;
            case 2:
                Setting.Instance.difficulty = Setting.DifficultyEnum.hard;
                break;
        }
    }

    public void SelectOperation(string operationName)
    {
        Setting.Instance.LoadingScreen(operationName);
        //SceneManager.LoadScene(operationName);
    }

    public void BackToMenu()
    {
        mainMenu.SetActive(true);
        operationSelection.SetActive(false);
        Setting.Instance.tutorialUI.SetActive(false);
    }

    public void ViewTutorial()
    {
        Setting.Instance.tutorialUI.SetActive(true);
        operationSelection.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // If in the editor, stop playing the scene
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // If in a built application, quit the application
            Application.Quit();
#endif
    }

}
