using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    public static Setting Instance { get; private set; }

    internal AudioSource menuAudioSource;
    public AudioClip menuMusic;

    private AudioSource buttonAudioSource;
    public AudioClip buttonClickSound; // Assign your sound here

    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public Image progressBar;
    public TextMeshProUGUI selectedMap;
    private float progressTarget;

    [Header("Tutorial")]
    public GameObject tutorialUI;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            buttonAudioSource = gameObject.AddComponent<AudioSource>();
            buttonAudioSource.volume = 0.25f;
        }
    }

    public enum DifficultyEnum
    {
        easy,
        normal,
        hard
    }

    [Header("Game Setting")]
    public DifficultyEnum difficulty;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        progressBar.fillAmount = Mathf.MoveTowards(progressBar.fillAmount, progressTarget, 3 * Time.deltaTime);
    }

    public void PlayMenuMusic()
    {
        if (menuAudioSource)
        {
            menuAudioSource.Play();
        }
        else
        {
            menuAudioSource = gameObject.AddComponent<AudioSource>();
            menuAudioSource.playOnAwake = false;
            menuAudioSource.loop = true;
            menuAudioSource.volume = 0.3f;
            menuAudioSource.clip = menuMusic;
            menuAudioSource.Play();
        }
    }

    public void PlayButtonClickSound()
    {
        buttonAudioSource.PlayOneShot(buttonClickSound);
    }

    public async void LoadingScreen(string sceneName)
    {
        selectedMap.text = sceneName;
        progressTarget = 0;
        progressBar.fillAmount = 0;

        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        loadingScreen.SetActive(true);
        do
        {
            await Task.Delay(100);
            progressTarget = scene.progress; 
        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;
        loadingScreen.SetActive(false);

    }


    public void CloseTutorialUI()
    {
        tutorialUI.SetActive(false);

        Time.timeScale = 1;

        AudioSource[] audios = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in audios)
        {
            if (!audio.isPlaying)
                audio.Play();
        }

        if (TimeManagement.Instance)
        {
            TimeManagement.Instance.isPause = false;
        }
    }

}
