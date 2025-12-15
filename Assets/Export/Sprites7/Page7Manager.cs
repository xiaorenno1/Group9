using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Page7Manager : MonoBehaviour
{
    [Header("UI References")]
    public Image background;
    public Image moon;
    public GameObject bunnyEyes;
    public TextMeshProUGUI touchText;
    public StarsManager starsManager;

    [Header("Bunny Reference")]
    public Image bunnyBody; // Bunny body image

    [Header("Audio")]
    public AudioClip lullabyMusic; // Lullaby audio for moon click
    public AudioClip bunnyClickAudio; // Audio for bunny click
    public AudioSource audioSource; // Main audio source

    [Header("Audio Settings")]
    public bool enableBunnyClickAudio = true; // Enable/disable bunny click audio
    [Range(0f, 1f)]
    public float bunnyAudioVolume = 1f; // Volume for bunny audio
    [Range(0f, 1f)]
    public float lullabyVolume = 1f; // Volume for lullaby music

    [Header("Animation Settings")]
    public float fadeDuration = 2f;
    public float textFadeDuration = 1f;
    public float lullabyDelay = 0.5f; // Delay before playing lullaby

    private Button moonButton;
    private Button bunnyBodyButton; // Button component for bunny body
    private Animator moonAnimator; // Animator for moon
    private Animator bunnyEyesAnimator;
    private bool animationPlaying = false;
    private bool bunnyAudioEnabled = true;
    private Color originalBackgroundColor;

    void Start()
    {
        Debug.Log("Page7Manager Start");

        // Check critical components
        if (bunnyBody == null)
        {
            Debug.LogError("bunnyBody is not assigned! Please drag BunnyBody image to the inspector.");
        }

        // Get components
        moonButton = moon.GetComponent<Button>();
        moonAnimator = moon.GetComponent<Animator>();

        if (moonAnimator == null)
        {
            Debug.LogError("Moon GameObject does not have an Animator component!");
        }

        // Initialize audio source
        InitializeAudioSource();

        bunnyEyesAnimator = bunnyEyes.GetComponent<Animator>();

        // Save original background color
        originalBackgroundColor = background.color;

        // Setup bunny body click
        SetupBunnyClick();

        // Preload audio
        LoadAudio();

        // Bind events
        moonButton.onClick.AddListener(OnMoonClicked);

        // Initial state
        touchText.alpha = 1f;

        Debug.Log("Page7Manager initialization complete");
    }

    void InitializeAudioSource()
    {
        // If audioSource is not assigned, try to find or create one
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("Created AudioSource component");
            }
        }

        // Ensure AudioSource is properly configured
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 1f;
        audioSource.spatialBlend = 0f; // Ensure 2D audio

        Debug.Log("AudioSource initialized: " + audioSource.name);
    }

    void LoadAudio()
    {
        // Load lullaby music if not assigned
        if (lullabyMusic == null)
        {
            lullabyMusic = Resources.Load<AudioClip>("lullaby");
            if (lullabyMusic == null)
            {
                Debug.LogError("Cannot find lullaby audio file!");
                Debug.LogError("Please ensure:");
                Debug.LogError("1. Audio file is in Assets/Resources folder");
                Debug.LogError("2. File is named 'lullaby' (without extension)");
                Debug.LogError("3. File type is supported (.wav, .mp3, .ogg)");
            }
            else
            {
                Debug.Log("Successfully loaded lullaby: " + lullabyMusic.name);
            }
        }
        else
        {
            Debug.Log("Using inspector-assigned lullaby: " + lullabyMusic.name);
        }

        // Load bunny click audio if not assigned
        if (bunnyClickAudio == null)
        {
            bunnyClickAudio = Resources.Load<AudioClip>("audio1");
            if (bunnyClickAudio == null)
            {
                Debug.LogWarning("Cannot find bunny click audio file!");
            }
            else
            {
                Debug.Log("Successfully loaded bunny audio: " + bunnyClickAudio.name);
            }
        }
        else
        {
            Debug.Log("Using inspector-assigned bunny audio: " + bunnyClickAudio.name);
        }
    }

    void SetupBunnyClick()
    {
        if (bunnyBody != null)
        {
            // Ensure Image component has Raycast Target enabled
            bunnyBody.raycastTarget = true;

            // Get or add Button component
            bunnyBodyButton = bunnyBody.GetComponent<Button>();
            if (bunnyBodyButton == null)
            {
                bunnyBodyButton = bunnyBody.gameObject.AddComponent<Button>();
                Debug.Log("Added Button component to bunnyBody");
            }

            // Set button colors (transparent)
            ColorBlock colors = bunnyBodyButton.colors;
            colors.normalColor = new Color(1, 1, 1, 0f); // Fully transparent
            colors.highlightedColor = new Color(1, 1, 1, 0f);
            colors.pressedColor = new Color(1, 1, 1, 0.1f);
            colors.selectedColor = new Color(1, 1, 1, 0f);
            colors.disabledColor = new Color(1, 1, 1, 0f);
            bunnyBodyButton.colors = colors;

            // Remove old listeners and add new one
            bunnyBodyButton.onClick.RemoveAllListeners();
            bunnyBodyButton.onClick.AddListener(OnBunnyBodyClicked);

            // Add EventTrigger for better click detection
            AddEventTriggerForDebug();

            Debug.Log("Bunny body click setup complete. RaycastTarget: " + bunnyBody.raycastTarget);
        }
        else
        {
            Debug.LogError("BunnyBody is not assigned in the inspector!");
        }
    }

    void AddEventTriggerForDebug()
    {
        // Add EventTrigger for better click detection
        EventTrigger eventTrigger = bunnyBody.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = bunnyBody.gameObject.AddComponent<EventTrigger>();
        }

        // Add click event
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => {
            Debug.Log("BunnyBody clicked via EventTrigger");
            OnBunnyBodyClicked();
        });

        eventTrigger.triggers.Add(entry);
    }

    public void OnMoonClicked()
    {
        if (!animationPlaying)
        {
            Debug.Log("Moon clicked, starting animation");
            StartCoroutine(PlayMoonAnimation());
        }
        else
        {
            Debug.Log("Moon click ignored - animation already playing");
        }
    }

    public void OnBunnyBodyClicked()
    {
        Debug.Log("BunnyBody clicked!");

        if (bunnyAudioEnabled && enableBunnyClickAudio && !animationPlaying)
        {
            PlayBunnyClickAudio();
        }
        else
        {
            Debug.LogWarning("Bunny click ignored due to conditions not met");
        }
    }

    void PlayBunnyClickAudio()
    {
        if (audioSource != null && bunnyClickAudio != null)
        {
            Debug.Log("Playing bunny audio: " + bunnyClickAudio.name);

            // Set volume
            float originalVolume = audioSource.volume;
            audioSource.volume = bunnyAudioVolume;

            // Play audio
            audioSource.PlayOneShot(bunnyClickAudio);

            // Restore original volume
            audioSource.volume = originalVolume;

            Debug.Log("Bunny audio played successfully!");

            // Add visual feedback
            StartCoroutine(BunnyClickFeedback());
        }
        else
        {
            if (audioSource == null)
                Debug.LogError("AudioSource is null!");
            if (bunnyClickAudio == null)
                Debug.LogError("BunnyClickAudio is null!");
        }
    }

    IEnumerator BunnyClickFeedback()
    {
        // Simple visual feedback: slight scale animation
        if (bunnyBody != null)
        {
            Vector3 originalScale = bunnyBody.transform.localScale;
            Vector3 targetScale = originalScale * 1.1f;

            // Scale up
            float duration = 0.08f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                bunnyBody.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < duration)
            {
                bunnyBody.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            bunnyBody.transform.localScale = originalScale;
        }
    }

    IEnumerator PlayMoonAnimation()
    {
        Debug.Log("Starting moon animation sequence");
        animationPlaying = true;
        bunnyAudioEnabled = false; // Disable bunny audio during animation

        // Hide prompt text
        StartCoroutine(FadeText(touchText, 1f, 0f, textFadeDuration));

        // Play moon glow animation
        if (moonAnimator != null)
        {
            moonAnimator.SetTrigger("Glow");
            Debug.Log("Triggered moon glow animation");
        }
        else
        {
            Debug.LogError("Moon Animator is null! Cannot play glow animation");
        }

        // Wait a moment before playing lullaby
        yield return new WaitForSeconds(lullabyDelay);

        // Play lullaby music - FIXED: Ensure proper playback
        if (audioSource != null && lullabyMusic != null)
        {
            Debug.Log("Playing lullaby music: " + lullabyMusic.name);
            Debug.Log("AudioSource state - IsPlaying: " + audioSource.isPlaying +
                     ", Clip: " + (audioSource.clip != null ? audioSource.clip.name : "null"));

            // Stop any currently playing audio
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            // Set up and play lullaby
            audioSource.clip = lullabyMusic;
            audioSource.volume = lullabyVolume;
            audioSource.loop = true; // Lullaby should loop
            audioSource.Play();

            Debug.Log("Lullaby playback started. Length: " + lullabyMusic.length + " seconds");
        }
        else
        {
            if (audioSource == null)
                Debug.LogError("AudioSource is null - cannot play lullaby!");
            if (lullabyMusic == null)
                Debug.LogError("LullabyMusic is null - assign in inspector or add to Resources!");
        }

        // Darken background
        yield return StartCoroutine(FadeBackground(originalBackgroundColor, new Color(0.1f, 0.1f, 0.2f), fadeDuration));

        // Show stars
        if (starsManager != null)
        {
            starsManager.ShowStars();
            Debug.Log("Stars shown");
        }

        // Bunny closes eyes
        yield return new WaitForSeconds(1f);
        if (bunnyEyesAnimator != null)
        {
            bunnyEyesAnimator.SetTrigger("Blink");
            Debug.Log("Triggered bunny eyes blink");
        }

        // Wait for animation to complete, then go to next page
        yield return new WaitForSeconds(3f);
        LoadNextPage();
    }

    IEnumerator FadeBackground(Color from, Color to, float duration)
    {
        Debug.Log("Fading background from " + from + " to " + to);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            background.color = Color.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        background.color = to;

        Debug.Log("Background fade complete");
    }

    IEnumerator FadeText(TextMeshProUGUI text, float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            text.alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        text.alpha = toAlpha;
    }

    void LoadNextPage()
    {
        Debug.Log("Loading next page: Page8_GoodnightKiss");

        // Stop music before changing scene
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Page8_GoodnightKiss");
    }

    // Public method for testing
    public void TestLullaby()
    {
        Debug.Log("Testing lullaby playback...");

        if (audioSource != null && lullabyMusic != null)
        {
            audioSource.clip = lullabyMusic;
            audioSource.volume = lullabyVolume;
            audioSource.loop = false;
            audioSource.Play();
            Debug.Log("Test lullaby playing");
        }
        else
        {
            Debug.LogError("Cannot test lullaby - audio source or clip is null");
        }
    }

    public void TestBunnyAudio()
    {
        Debug.Log("Testing bunny audio...");
        PlayBunnyClickAudio();
    }

    // Add this method to help debug audio issues
    void OnAudioFilterRead(float[] data, int channels)
    {
        // This can help debug if audio is actually being processed
    }

#if UNITY_EDITOR
    // Editor-only debug visualization
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Display current state
        string stateInfo = $"State: {(animationPlaying ? "Animation Playing" : "Idle")}\n" +
                          $"Bunny Audio: {(bunnyAudioEnabled ? "Enabled" : "Disabled")}\n" +
                          $"Lullaby: {(lullabyMusic != null ? lullabyMusic.name : "Not Loaded")}";

        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, stateInfo);

        if (bunnyBody != null)
        {
            UnityEditor.Handles.Label(bunnyBody.transform.position, "Click for audio");
        }
    }
#endif
}