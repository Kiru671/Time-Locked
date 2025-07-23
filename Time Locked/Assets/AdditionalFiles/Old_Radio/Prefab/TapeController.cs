using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapeController : MonoBehaviour
{
    [Header("Tape settings")] [SerializeField]
    private GameObject tapePrefab;

    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField, Range(0.1f, 10f)] private float moveDuration = 3f;

    [Header("Sound effects")] [SerializeField]
    private AudioSource audioSource;

    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip placementClip;

    [Header("Morse-LED settings")] [SerializeField]
    private Renderer dotRenderer;

    [SerializeField] private Renderer dashRenderer;

    [Header("Emission Colors")] [Tooltip("Emission color when the DOT LED is on")] [SerializeField]
    private Color dotOnColor = Color.red * 5f;

    [Tooltip("Emission color when the DASH LED is on")] [SerializeField]
    private Color dashOnColor = Color.yellow * 5f;

    private Color offColor = Color.black;

    [Header("Morse Audio Clips")] [SerializeField]
    private AudioClip dotClip;

    [SerializeField] private AudioClip dashClip;

    [Header("Morse Timings (seconds)")] [SerializeField, Range(0.05f, 1f)]
    private float dotDuration = 0.2f;

    [SerializeField, Range(0.05f, 3f)] private float dashDuration = 0.6f;
    [SerializeField, Range(0.05f, 1f)] private float elementGap = 0.2f;
    [SerializeField, Range(0.1f, 3f)] private float letterGap = 0.6f;
    [SerializeField, Range(0.2f, 5f)] private float wordGap = 1.4f;

    [Header("Morse Message")] [SerializeField]
    private string morseMessage = "RJ481";

    // Internals
    private GameObject currentTape;
    private bool tapePlaced;
    private Coroutine audioRoutine;

    // Material instances
    private Material dotMat, dashMat;
    static readonly int EM = Shader.PropertyToID("_EmissionColor");

    // Morse lookup
    private static readonly Dictionary<char, string> _morseTable = new Dictionary<char, string>
    {
        { 'A', ".-" }, { 'B', "-..." }, { 'C', "-.-." }, { 'D', "-.." }, { 'E', "." },
        { 'F', "..-." }, { 'G', "--." }, { 'H', "...." }, { 'I', ".." }, { 'J', ".---" },
        { 'K', "-.-" }, { 'L', ".-.." }, { 'M', "--" }, { 'N', "-." }, { 'O', "---" },
        { 'P', ".--." }, { 'Q', "--.-" }, { 'R', ".-." }, { 'S', "..." }, { 'T', "-" },
        { 'U', "..-" }, { 'V', "...-" }, { 'W', ".--" }, { 'X', "-..-" }, { 'Y', "-.--" },
        { 'Z', "--.." }, { '0', "-----" }, { '1', ".----" }, { '2', "..---" }, { '3', "...--" },
        { '4', "....-" }, { '5', "....." }, { '6', "-...." }, { '7', "--..." }, { '8', "---.." },
        { '9', "----." }, { ' ', " " }
    };

    private void Awake()
    {
        // Create unique material instances and enable emission
        if (dotRenderer != null)
        {
            dotMat = dotRenderer.material;
            dotMat.EnableKeyword("_EMISSION");
        }

        if (dashRenderer != null)
        {
            dashMat = dashRenderer.material;
            dashMat.EnableKeyword("_EMISSION");
        }

        // Make sure we start fully off
        SetLED(false, dotMat, offColor);
        SetLED(false, dashMat, offColor);
    }

    public void StartTape()
    {
        // Stop any in-flight sounds  
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Stop the old morse coroutine  
        if (audioRoutine != null)
        {
            StopCoroutine(audioRoutine);
            audioRoutine = null;
        }

        // Reset LEDs so nothing is left lit  
        SetLED(false, dotMat, offColor);
        SetLED(false, dashMat, offColor);

        // (Re)place the tape if needed  
        bool playPlacement = false;
        if (!tapePlaced)
        {
            PlaceTape();
            playPlacement = true;
        }

        // Finally, kick off the new audio sequence  
        audioRoutine = StartCoroutine(PlayAudioSequence(playPlacement));
    }

    private void PlaceTape()
    {
        tapePlaced = true;
        if (currentTape != null)
            Destroy(currentTape);

        currentTape = Instantiate(tapePrefab, startPoint.position, startPoint.rotation);
        currentTape.transform.localScale = tapePrefab.transform.localScale;
        StartCoroutine(MoveTape());
    }

    private IEnumerator MoveTape()
    {
        float elapsed = 0f;
        Vector3 from = startPoint.position, to = endPoint.position;
        while (elapsed < moveDuration && currentTape != null)
        {
            currentTape.transform.position = Vector3.Lerp(from, to, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (currentTape != null)
            currentTape.transform.position = to;
    }

    private IEnumerator PlayAudioSequence(bool playPlacementFirst)
    {
        audioSource.Stop();
        audioSource.loop = false;

        if (playPlacementFirst && placementClip)
        {
            audioSource.clip = placementClip;
            audioSource.Play();
            yield return new WaitForSeconds(placementClip.length);
        }

        if (clickClip)
        {
            audioSource.clip = clickClip;
            audioSource.Play();
            yield return new WaitForSeconds(clickClip.length);
        }

        string msg = morseMessage.ToUpperInvariant();
        yield return PlayMorseMessage(msg);
    }

    private IEnumerator PlayMorseMessage(string msg)
    {
        foreach (char c in msg)
        {
            if (!_morseTable.TryGetValue(c, out string pattern))
                continue;

            if (pattern == " ")
            {
                yield return new WaitForSeconds(wordGap);
                continue;
            }

            foreach (char sym in pattern)
            {
                bool isDot = sym == '.';
                Material mat = isDot ? dotMat : dashMat;
                Color onCol = isDot ? dotOnColor : dashOnColor;
                AudioClip clip = isDot ? dotClip : dashClip;
                float dur = isDot ? dotDuration : dashDuration;

                SetLED(true, mat, onCol);
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                }

                yield return new WaitForSeconds(dur);

                SetLED(false, mat, offColor);
                yield return new WaitForSeconds(elementGap);
            }

            yield return new WaitForSeconds(letterGap - elementGap);
        }
    }

    // Helper to toggle emission
    private void SetLED(bool on, Material mat, Color col)
    {
        if (mat != null) mat.SetColor(EM, on ? col : offColor);
    }
}