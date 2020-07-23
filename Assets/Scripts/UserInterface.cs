using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{

    public CreateBalls mainScript;

    public Slider AmountSlider;
    public Text AmountText;



    void Start()
    {

        AmountText.text = mainScript.N.ToString();
        AmountSlider.value = mainScript.N;
        AmountSlider.minValue = 5;
        AmountSlider.maxValue = 450;
        AmountSlider.onValueChanged.AddListener(delegate { ChangeAmount();});
        

    }



    private void ChangeAmount()
    {
        mainScript.N = (int)AmountSlider.value;
        AmountText.text = mainScript.N.ToString();
        mainScript.Clear();
    }
   
}
