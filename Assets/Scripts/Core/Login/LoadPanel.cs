using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadPanel : MonoBehaviour
{
    public Slider slider;
    public Text lblProcess;
    private static LoadPanel instance;

    public static LoadPanel Instance { get => instance; }


    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDown(long total,long down)
    {
        lblProcess.text = down + "KB" + " / " + total + "KB";
        slider.value = down / (float)total;
    }
}
