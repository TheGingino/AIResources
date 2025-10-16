using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillBar : MonoBehaviour
{
    public float BiasProgress, AccuracyProgress, DataBreachBar;
    public AudioClip clip;
    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
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
        GameManager.Instance.BiasValue += BiasProgress;
        GameManager.Instance.AccuracyValue += AccuracyProgress;
        GameManager.Instance.DatabreachValue += DataBreachBar;
        gameObject.SetActive(false);
        source.PlayOneShot(clip);
    }
}
