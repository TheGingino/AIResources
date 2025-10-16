using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillBar : MonoBehaviour
{
    public float BiasProgress, AccuracyProgress, DataBreachBar;
    public AudioClip clip;
    private AudioSource source;
    private FillBar bar;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        bar = GetComponent<FillBar>();  
    }


    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.H))
        {
            AddData();
        }
    }


    public void AddData()
    {
        source.PlayOneShot(clip);
        GameManager.Instance.BiasValue += BiasProgress;
        GameManager.Instance.AccuracyValue += AccuracyProgress;
        GameManager.Instance.DatabreachValue += DataBreachBar;
        bar.enabled = false;
        
    }
}
