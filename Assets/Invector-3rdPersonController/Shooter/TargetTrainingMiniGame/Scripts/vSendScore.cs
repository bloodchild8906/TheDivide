using Invector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vSendScore : MonoBehaviour
{
    public int displayID;
    public vShooterScore shooterScore;
    public UnityEngine.Events.UnityEvent onAdd;
    public void SendScore(float value)
    {
        if(shooterScore==null)
        {
            shooterScore = FindObjectOfType<vShooterScore>();
        }

        if(shooterScore!=null)
        {
            shooterScore.AddScore(new vShooterScore.ScorePoint(displayID,value));
        }
        onAdd.Invoke();
    }
}
