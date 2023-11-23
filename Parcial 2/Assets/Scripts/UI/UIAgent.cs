using System;
using System.Collections;
using System.Collections.Generic;
using IA.Gameplay;
using IA.Managers;
using UnityEngine;

public class UIAgent : MonoBehaviour
{
    [SerializeField] private Agent agent;
    [SerializeField] private TMPro.TextMeshProUGUI actionText;
    [SerializeField] private float yMaxOffset = 1;
    [SerializeField] private float textSpeed = 2f;
    
    private Vector3 actionTextStartingPosition;
    private Vector3 actionTextEndPosition;
    private bool active = false;
    private IEnumerator textCoroutine = null;

    private void Awake()
    {
        active = GameManager.Instance.TextOn;
        SetGameManagerEvents();
        
        GameManager.Instance.OnTurnEnd += delegate(int i)
        {
            if (active == GameManager.Instance.TextOn) return;
            
            active = GameManager.Instance.TextOn;
            SetGameManagerEvents();
        };
        
        actionText.gameObject.SetActive(false);
    }

    private void Start()
    {
        actionTextStartingPosition = actionText.transform.position;
        actionTextEndPosition = actionTextStartingPosition;
        actionTextEndPosition.y += yMaxOffset;
    }

    private void SetGameManagerEvents()
    {
        if (active)
        {
            agent.OnAgentFlee += ShowAgentFleeText;
            agent.OnAgentDie += ShowAgentDieText;
        }
        else
        {
            StopAllCoroutines();
            agent.OnAgentFlee -= ShowAgentFleeText;
            agent.OnAgentDie -= ShowAgentDieText;
        }
    }
    
    private void ShowAgentFleeText()
    {
        if(textCoroutine != null)
            StopCoroutine(textCoroutine);
        
        textCoroutine = ShowingAgentText("FLEE");
        StartCoroutine(textCoroutine);
    }

    private void ShowAgentDieText()
    {
        if(textCoroutine != null)
            StopCoroutine(textCoroutine);
        
        textCoroutine = ShowingAgentText("DIE");
        StartCoroutine(textCoroutine);
    }

    private IEnumerator ShowingAgentText(string text)
    {
        actionText.gameObject.SetActive(true);
        float t = 0;
        while (t < 1)
        {
            actionText.transform.position = Vector3.Lerp(actionTextStartingPosition, actionTextEndPosition, t);
            t += Time.deltaTime * textSpeed;
            yield return null;
        }
        actionText.transform.position = actionTextEndPosition;
        actionText.gameObject.SetActive(false);
    }

}
