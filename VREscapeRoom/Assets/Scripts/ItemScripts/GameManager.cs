using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Slider BiasBar, AccuracyBar, DataBar;
    internal float BiasValue, AccuracyValue, DatabreachValue;

    private void Update()
    {
        BiasBar.value = BiasValue;
        AccuracyBar.value = AccuracyValue;
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


