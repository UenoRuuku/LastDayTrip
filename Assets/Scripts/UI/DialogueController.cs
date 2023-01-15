using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[DisallowMultipleComponent]
public class DialogueController : Singleton<DialogueController>
{
    public List<Sprite> avatars;
    public TextEffector tx;
    public TMP_Text a_name;
    public Image sr;
    public GameObject panel;
    public Transform endPos;
    public Transform startPos;
    private bool activated = false;
    private Coroutine t;

    public void startNewDialogue(TextAsset content)
    {
        panel.transform.DOMove(endPos.position, 0.5f);
        activated = true;
        if (t != null)
        {
            StopCoroutine(t);
        }
        t = StartCoroutine(talk(content.ToString().Split("\n")));
    }

    public void StartANewAlert(string str)
    {
        panel.transform.DOMove(endPos.position, 0.5f);
        activated = true;
        if (t != null)
        {
            StopCoroutine(t);
        }
        sr.sprite = avatars[0];

        a_name.text = "Instruction";
        t = StartCoroutine(talk(str.Split("\n")));
    }

    public void endCurrentDialogue()
    {
        if (activated)
        {
            activated = false;
            if (t != null)
            {
                StopCoroutine(t);
            }
        }
    }

    public bool isActivated()
    {
        return activated;
    }

    IEnumerator talk(string[] lines)
    {
        foreach (string line in lines)
        {
            if (line == "")
            {
                continue;
            }
            if (line[0] == '#')
            {
                sr.sprite = avatars[int.Parse(line[1].ToString())];
                continue;
            }
            else if (line[0] == '&')
            {
                a_name.text = line.Replace("&", "");
                continue;
            }
            else if (line[0] == '*')
            {
                continue;
            }
            else
            {
                tx.startASentence(line);
                yield return new WaitUntil(
                    (() => ((Input.GetKeyDown(KeyCode.J) && !tx.getComplete()) || tx.getComplete()))
                );
                if (!tx.getComplete())
                {
                    tx.fastForward();
                }
                yield return new WaitForFixedUpdate();
                yield return new WaitUntil(() => (Input.GetKeyDown(KeyCode.J) && tx.getComplete()));
                yield return new WaitForFixedUpdate();
            }
        }
        panel.transform.DOMove(startPos.position, 0.5f).OnComplete(() => activated = false);
    }

    [SerializeField]
    Image item;

    public void ShowItemImage(Sprite sprite){
        item.gameObject.SetActive(true);
        item.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
    }

    public void CloseItemImage(){
        item.gameObject.SetActive(false);
    }
}
