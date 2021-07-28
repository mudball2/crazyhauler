using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownController : MonoBehaviour
{
    [Tooltip("Level timer in SECONDS.")]
    [SerializeField] float countdownTime;
    [Tooltip("Add text UI object here.")]
    [SerializeField] Text countdownDisplay;

    private TimeSpan countdownTimer;

    public void Start()
    {
        StartCoroutine(CountdownToStart());
    }

    IEnumerator CountdownToStart()
    {
        while (countdownTime > 0)
        {
            countdownTime -= Time.deltaTime;
            countdownTimer = TimeSpan.FromSeconds(countdownTime);
            string countdownTimerStr = countdownTimer.ToString("mm':'ss'.'ff");
            countdownDisplay.text = countdownTimerStr;
            yield return null;
        }

        countdownDisplay.text = "TIME UP";

        // Need to call a method to end the game here.

        yield return new WaitForSeconds(1f);
        countdownDisplay.gameObject.SetActive(false);
    }
}
