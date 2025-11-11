using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI Accuracy, Bias, DataBreach;
    internal float BiasValue, AccuracyValue, DatabreachValue;

    private void Update()
    {
        Accuracy.text = "Accuracy-" + AccuracyValue + "/100";
        Bias.text = "Bias-" + BiasValue + "/100";
        DataBreach.text = "DataBreach-" + DatabreachValue + "/100";
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // persists between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
}


