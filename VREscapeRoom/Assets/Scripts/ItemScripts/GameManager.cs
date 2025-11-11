using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
#endif
    public TextMeshProUGUI Accuracy, Bias, DataBreach;
    internal float BiasValue, AccuracyValue, DatabreachValue;

    private void Update()
    {
        Accuracy.text = "Accuracy-" + AccuracyValue + "/100";
        Bias.text = "Bias-" + BiasValue + "/100";
        DataBreach.text = "DataBreach-" + DatabreachValue + "/100";

        if(DatabreachValue >= 100 || BiasValue >= 100)
        {
            Debug.Log("failed");
            SceneManager.LoadScene(sceneAsset.name);
        }
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


