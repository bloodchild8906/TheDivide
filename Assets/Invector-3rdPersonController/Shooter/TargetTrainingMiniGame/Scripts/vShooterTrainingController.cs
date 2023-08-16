using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vShooterTrainingController : MonoBehaviour
{
    public vShooterScore shooterScore;
    public float timeToStartTraining= 3f;
    public float timeToFinishTraining=60f;
    public UnityEngine.UI.Text timeDisplay;
   
    public UnityEngine.Events.UnityEvent onInit,onStartCounter, onFinishCounter,onCancelTraining;
  
    Coroutine currentRoutine;
    float currentTime;
    public bool initOnStart;

    private void Start()
    {
      
        if(shooterScore)
        {
            if (initOnStart) StartTraining();
        }
       
    }
    public void StartTraining()
    {
       if (!shooterScore) return;
       shooterScore.StartScore();       
       FinishTraining();
       currentRoutine= StartCoroutine(RunTraining());

    }
    public void CancelTraining()
    {
        if (!shooterScore) return;
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
            onCancelTraining.Invoke();
        }
    }
    public void FinishTraining()
    {
        if (!shooterScore) return;
        if (currentRoutine!=null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
            shooterScore.FinishScore();
        }
       
        timeDisplay.text = "";

    }

    IEnumerator RunTraining()
    {
        onInit.Invoke();
        yield return new WaitForSeconds(1f);
        timeDisplay.text = "";
        var timeToEnd = Time.time + timeToStartTraining;
        while (timeToEnd >Time.time)
        {         
           
            timeDisplay.text = (timeToEnd - Time.time).ToString("00");
            yield return null;
        }

        yield return new WaitForSeconds(.2f);
        timeDisplay.text = "";
        yield return new WaitForSeconds(.2f);
        timeDisplay.text = "GO!";
        yield return new WaitForSeconds(1f);
        timeDisplay.text = "";
        onStartCounter.Invoke();
        yield return new WaitForSeconds(.2f);
        timeToEnd = Time.time + timeToFinishTraining;
     
        while (timeToEnd > Time.time)
        {
           
          
            timeDisplay.text = (timeToEnd - Time.time).ToString("00");
            yield return null;
        }
        onFinishCounter.Invoke();

        shooterScore.FinishScore();
        timeDisplay.text = "";
    }
}
