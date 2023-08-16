using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vScoreDataDisplay : MonoBehaviour
{
    public UnityEngine.UI.Text index;
    public UnityEngine.UI.Text score;
    public UnityEngine.UI.Text[] hits;

    public void Show(int index,float? score,List<float> hits)
    {
        if (this.index) this.index.text = (index).ToString("00");
        if (this.score) this.score.text = score!=null?((float)score).ToString("00"):"--";
        if(hits!=null)
            for (int i = 0; i < this.hits.Length; i++)
            {
                var text = this.hits[i];
                if (i < hits.Count)
                {
                    text.text = hits[i].ToString("00");
                }
                else break;
            }
        else
        {
            for (int i = 0; i < this.hits.Length; i++)
            {
                var text = this.hits[i];
                text.text = "--";
            }
        }


    }
}
