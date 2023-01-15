using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum effectType
{
    none = 0,
    floating = 1,
}

// Only one component allowed
[RequireComponent(typeof(TextMeshProUGUI)), DisallowMultipleComponent]
public class TextEffector : MonoBehaviour
{
    public TMP_Text m_text;

    [Header("typewriter")]
    [Range(0, 1), Tooltip("smaller, faster")]
    public float speed = 1;
    bool complete = false;
    bool Complete
    {
        get { return complete; }
    }

    [Header("floating")]
    [SerializeField, Range(1, 5)]
    private float frequence = 1.0f;

    [SerializeField, Range(1, 5), Tooltip("the size floating area")]
    private float floatRange = 1.0f;
    [SerializeField]
    private effectType effectType = effectType.none;
    private int characterCount = 0; // total character count
    private Vector3[] originVertices; // origin vertices vectors

    private Coroutine typeWriter;

    // Start is called before the first frame update
    void Awake()
    {
        gameObject.TryGetComponent<TMP_Text>(out m_text);
    }

    void Start()
    {
        if (m_text == null)
        {
            gameObject.AddComponent<TMP_Text>();
            gameObject.TryGetComponent<TMP_Text>(out m_text);
        }
        TMP_TextInfo textInfo = m_text.textInfo;
        characterCount = textInfo.characterCount;
        originVertices = textInfo.meshInfo[0].vertices;
        StartCoroutine(MainBody());
    }

    IEnumerator MainBody()
    {
        while (true)
        {
            if (effectType != effectType.none)
            {
                m_text.ForceMeshUpdate();
                TMP_TextInfo textInfo = m_text.textInfo;
                TMP_MeshInfo[] textInfoCopy = textInfo.CopyMeshInfoVertexData();
                characterCount = textInfo.characterCount;
                for (int i = 0; i < characterCount; i++)
                {
                    TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
                    int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                    int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                    Color32[] vertexColor = textInfo.meshInfo[
                        characterInfo.materialReferenceIndex
                    ].colors32;
                    Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
                    Vector3[] copyVertices = textInfoCopy[materialIndex].vertices;
                    if (!characterInfo.isVisible)
                    {
                        continue;
                    }
                    switch (effectType)
                    {
                        case effectType.floating:
                            Floating(vertexIndex, ref vertices);
                            break;
                        default:
                            break;
                    }
                }
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                    meshInfo.mesh.vertices = meshInfo.vertices;
                    m_text.UpdateGeometry(meshInfo.mesh, i);
                }
                m_text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            }
            yield return null;
        }
    }

    private void Floating(int vertextIndex, ref Vector3[] vertices)
    {
        // 4 vertice of one character
        for (int i = 0; i < 4; i++)
        {
            Vector3 originValue = vertices[vertextIndex + i];
            float verticesYPosition =
                Mathf.Sin(Time.time * frequence * Mathf.PI + originValue.x) * floatRange;
            vertices[vertextIndex + i] = originValue + new Vector3(0, verticesYPosition, 0);
        }
    }

    private IEnumerator Typewriter()
    {
        m_text.ForceMeshUpdate();
        TMP_TextInfo textInfo = m_text.textInfo;
        int total = textInfo.characterCount;
        complete = false;
        int current = 0;
        while (!complete)
        {
            if (current > total)
            {
                current = total;
                complete = true;
            }
            m_text.maxVisibleCharacters = current;
            current += 1;
            if (!isMuted && !complete)
            {
            }
            yield return new WaitForSeconds(speed);
        }
        yield return null;
    }

    bool isMuted;

    private IEnumerator R_Typewriter()
    {
        m_text.ForceMeshUpdate();
        TMP_TextInfo textInfo = m_text.textInfo;
        int total = textInfo.characterCount;
        complete = false;
        int current = total;
        while (!complete)
        {
            if (current < 0)
            {
                current = total;
                complete = true;
                m_text.text = "";
            }
            m_text.maxVisibleCharacters = current;
            current -= 1;
            yield return new WaitForSeconds(speed);
        }
        yield return null;
    }

    public void deleteASentence()
    {
        if (typeWriter != null)
        {
            StopCoroutine(typeWriter);
        }
        typeWriter = StartCoroutine(R_Typewriter());
    }

    public void startASentence(string text)
    {
        m_text.text = text;
        isMuted = false;
        if (typeWriter != null)
        {
            StopCoroutine(typeWriter);
        }
        typeWriter = StartCoroutine(Typewriter());
    }

    public void startASilentSentence(string text)
    {
        isMuted = true;
        m_text.text = text;
        if (typeWriter != null)
        {
            StopCoroutine(typeWriter);
        }
        typeWriter = StartCoroutine(Typewriter());
    }

    public void fastForward()
    {
        StopCoroutine(typeWriter);
        m_text.maxVisibleCharacters = m_text.textInfo.characterCount;
        complete = true;
    }

    public bool getComplete()
    {
        return complete;
    }

    // Update is called once per frame
    void Update() { }
}
