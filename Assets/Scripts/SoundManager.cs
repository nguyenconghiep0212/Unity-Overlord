using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; set; }

    private void Awake()
    {
        if (Instance != null & Instance != this)
        {
            Destroy(this); 
         }
        else
        {
            Instance = this;
            mainGameSoundChannel = gameObject.AddComponent<AudioSource>();
            mainGameSoundChannel.playOnAwake = false;
            mainGameSoundChannel.volume = 0.4f;
        }
    }

    public GameObject panelWithButtons;

    public AudioSource combatAudio;
    internal AudioSource mainGameSoundChannel;
    public AudioClip victoryMusic;
    public AudioClip defeatedMusic;
    public AudioClip hammerSound;
    public AudioClip infantryReadySound;
    public AudioClip mechanizeReadySound;
    public AudioClip airReadySound;
    public AudioClip researchSound;

    public bool isPlayingCombatSound;
    private float fadeDuration = 1f;
    // Start is called before the first frame update
    void Start()
    {
        Setting.Instance.PlayMenuMusic();
        Button[] buttons = panelWithButtons.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(Setting.Instance.PlayButtonClickSound);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CombatExisted();
    }

    public void CombatExisted()
    {
        isPlayingCombatSound = false;
        foreach (Unit unit in OperationManager.Instance.totalDeployUnit)
        {
            if (unit.inCombat)
            {
                if (!combatAudio.isPlaying)
                {
                    combatAudio.loop = true;
                    combatAudio.Play();
                }
                isPlayingCombatSound = true;
            }
        }
        foreach (Unit_Enemy unit_enemy in EnemyOperationManager.Instance.totalDeployUnit)
        {
            if (unit_enemy.inCombat)
            {
                if (!combatAudio.isPlaying)
                {
                    combatAudio.loop = true;
                    combatAudio.Play();
                }
                isPlayingCombatSound = true;
            }
        }
        if (!isPlayingCombatSound)
        {
            isPlayingCombatSound = false;
            if (combatAudio.isPlaying)
            {
                combatAudio.Stop();
                //StartFadeOut(combatAudio);
            }
        }
    }

    public void StartFadeOut(AudioSource audioSource)
    {
        StartCoroutine(FadeOut(audioSource, fadeDuration));
    }

    private IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0.2)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;

            yield return null; // Wait for the next frame
        }

        audioSource.Stop(); // Stop the audio after fading out
        audioSource.volume = startVolume; // Reset volume for potential future use
    }

    public void PlayVictoryMusic()
    {
        combatAudio.Stop();
        Setting.Instance.menuAudioSource.Stop();
        mainGameSoundChannel.PlayOneShot(victoryMusic);
    }
    public void PlayDefeatedMusic()
    {
        combatAudio.Stop();
        Setting.Instance.menuAudioSource.Stop();
        mainGameSoundChannel.PlayOneShot(defeatedMusic);
    }
}
