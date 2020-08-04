using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UserInterface : MonoBehaviour
{

    public CreateBalls mainScript;

    public Slider AmountSlider;
    public Text AmountText;

    public Slider ViscositySlider;
    public Text ViscosityText;

    public Slider SoftnessSlider;
    public Text SoftnessText;
           
    void Start()
    {
                
        AmountSlider.minValue = 5;
        AmountSlider.maxValue = 1500;
        AmountSlider.value = mainScript.N;
        AmountText.text = mainScript.N.ToString();
        AmountSlider.onValueChanged.AddListener(delegate { ChangeAmount();});


        ViscositySlider.minValue = 0.5f;
        ViscositySlider.maxValue = 20;
        ViscositySlider.value = mainScript.mu;
        ViscosityText.text = mainScript.mu.ToString();        
        ViscositySlider.onValueChanged.AddListener(delegate { ChangeViscosity(); });
                      
        SoftnessSlider.minValue = 1;
        SoftnessSlider.maxValue = 100;
        SoftnessText.text = (SoftnessSlider.maxValue - mainScript.k).ToString();
        SoftnessSlider.value = SoftnessSlider.maxValue - mainScript.k;
        SoftnessSlider.onValueChanged.AddListener(delegate { ChangeSoftness(); });
    }



    private void ChangeAmount()
    {
        mainScript.N = (int)AmountSlider.value;
        AmountText.text = mainScript.N.ToString();
        mainScript.Clear();
    }

    private void ChangeViscosity()
    {
        mainScript.mu = ViscositySlider.value;
        ViscosityText.text = mainScript.mu.ToString();
        mainScript.Cv = -40f * mainScript.mu;
        //mainScript.dt = Mathf.Min(0.003f, 0.5f / Mathf.Sqrt(-mainScript.Cp * mainScript.Cv));   //originally it was min(0.003, 0.05 / sqrt())
        //Debug.Log(mainScript.dt);
    }

    private void ChangeSoftness()
    {
        mainScript.k = (int)SoftnessSlider.maxValue - (int)SoftnessSlider.value;
        SoftnessText.text = (SoftnessSlider.maxValue - mainScript.k).ToString();
        
        mainScript.Cp = 15 * mainScript.k;
        
        //mainScript.dt = Mathf.Min(0.003f, 0.5f / Mathf.Sqrt(-mainScript.Cp * mainScript.Cv));   //originally it was min(0.003, 0.05 / sqrt())
    }
}
