using Invector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class vShooterScore : MonoBehaviour
{
    [vButton("ShowData", "ShowData", typeof(vShooterScore), false)]
    [vButton("ClearData", "ClearData", typeof(vShooterScore), false)]
    public TargetPointCounter scoreDisplay;
    public TargetPointCounter[] hitCounters;
    public vScoreDataDisplay[] dataDisplays;

    ScoreDATAList scoreDATAList;
    [System.Serializable]
    public class TargetPointCounter
    {
        [Invector.vReadOnly(false)] public float currentScore;
        public vScorePointDisplay display;
        public void ShowValue()
        {
            display.ShowValue(currentScore);
        }
    }

    public void AddScore(ScorePoint score)
    {
        scoreDisplay.currentScore += score.value;
        scoreDisplay.ShowValue();

        if (hitCounters.Length > 0 && score.id < hitCounters.Length)
        {
            TargetPointCounter targetPoint = hitCounters[score.id];

            if (targetPoint != null)
            {
                targetPoint.currentScore++;
                targetPoint.ShowValue();
            }
        }
    }

    public class ScorePoint
    {
        public int id;
        public float value;

        public ScorePoint(float value)
        {
            this.value = value;
        }
        public ScorePoint(int id, float value)
        {
            this.id = id;
            this.value = value;
        }
    }

    internal void StartScore()
    {
        scoreDisplay.currentScore = 0;
        foreach (var s in hitCounters)
        {
            s.currentScore = 0;
        }

        scoreDATAList = LoadData("ShooterScore");
    }
    internal void FinishScore()
    {
        if (scoreDATAList != null)
        {
            //Debug.Log("FINISH SCORE");
            var data = new ScoreDATA();
            data.score = this.scoreDisplay.currentScore;
            data.hits = new List<float>();
            foreach (var s in hitCounters)
            {
                data.hits.Add(s.currentScore);
            }
            if (scoreDATAList.datas.Count < dataDisplays.Length)
            {
                scoreDATAList.datas.Add(data);
                scoreDATAList.datas = scoreDATAList.datas.OrderBy(d => d.score).Reverse().ToList();
                //Debug.Log("ADD NEW SCORE");
            }
            else
            {

                scoreDATAList.datas.Add(data);
                scoreDATAList.datas = scoreDATAList.datas.OrderBy(d => d.score).Reverse().ToList();
                var dataCount = scoreDATAList.datas.Count - dataDisplays.Length;
                bool add = true;
                for (int i = 0; i < dataCount; i++)
                {
                    if (scoreDATAList.datas[scoreDATAList.datas.Count - 1].Equals(data))
                    {
                        add = false;
                    }
                    scoreDATAList.datas.RemoveAt(scoreDATAList.datas.Count - 1);
                }
                if (add)
                {
                    //Debug.Log("ADD NEW SCORE");
                }
            }

            SaveData("ShooterScore");
            ShowData();
        }
    }

    public void ClearData()
    {
        this.scoreDATAList = new ScoreDATAList();
        SaveData("ShooterScore");
    }

    public void ShowData()
    {
        if (scoreDATAList == null || !Application.isPlaying)
        {
            scoreDATAList = LoadData("ShooterScore");
        }

        //Debug.Log("SHOW DATA");
        for (int i = 0; i < dataDisplays.Length; i++)
        {
            if (scoreDATAList.datas.Count > 0 && i < scoreDATAList.datas.Count)
            {
                var d = scoreDATAList.datas[i];
                dataDisplays[i].Show(i + 1, d.score, d.hits);
            }
            else
            {
                dataDisplays[i].Show(i + 1, null, null);
            }
        }
    }

    public void SaveData(string dataName)
    {
        if (scoreDATAList == null)
        {
            scoreDATAList = new ScoreDATAList();
        }

        string data = JsonUtility.ToJson(scoreDATAList);
        string path = Application.dataPath + $"/{dataName}.json";
        System.IO.File.WriteAllText(path, data);
        //Debug.Log("SAVE SCORE FILE");
    }

    public ScoreDATAList LoadData(string dataName)
    {
        string path = Application.dataPath + $"/{dataName}.json";
        if (!System.IO.File.Exists(path))
        {
            //Debug.Log("CREATE SCORE FILE");
            SaveData("ShooterScore");
        }
        else
        {
            //Debug.Log("LOAD SCORE FILE");
            string data = System.IO.File.ReadAllText(path);
            scoreDATAList = JsonUtility.FromJson<ScoreDATAList>(data);
        }
        return scoreDATAList;

    }

    [System.Serializable]
    public class ScoreDATAList
    {
        public List<ScoreDATA> datas = new List<ScoreDATA>();
    }
    [System.Serializable]
    public class ScoreDATA
    {
        public float score;
        public List<float> hits = new List<float>();
    }
}