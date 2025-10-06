
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ScanObject : MonoBehaviour
{
    [SerializeField] private Scanner scanner;
    [SerializeField] private ScannableObject targetObj;

    /// <summary>
    /// Casts a ray forward when E is pressed and scans the first object with ScannableObject component.
    /// </summary>
    async void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.forward, out hit, 10f)) return;

        var scannable = hit.transform.GetComponent<ScannableObject>()
                        ?? hit.transform.GetComponentInParent<ScannableObject>();
        if (Input.GetKeyDown(KeyCode.S))
        {
            
        }
        if (scannable == null) return;

        SetCurrentTarget(scannable);
        await scanner.ScanAsync(targetObj);
    }

    private void SetCurrentTarget(ScannableObject target)
    {
        targetObj = target;

    }
}




// Coroutine that spawns a wave of enemies based on the provided wave data
private IEnumerator SpawnWaveData(WaveData data)
{
    // Loop through the number of enemies to spawn in this wave
    for (int i = 0; i < data.amount; i++)
    {
        // Randomly pick one of the two available spawn positions (0 or 1)
        int chosenIndex = Random.Range(0, 2);

        // Instantiate the enemy prefab at the chosen spawn position with no rotation
        Transform spawnedEnemy = Instantiate(
            enemies[(int)data.id].transform,
            spawnpos[chosenIndex].position,
            Quaternion.identity
        );

        // Assign the correct lane to the enemy's Move script, based on the spawn index
        spawnedEnemy.GetComponent<Move>().Lane = GameObject
            .Find("Waypoint Manager")
            .GetComponent<LanesScript>()
            .lanes[chosenIndex];

        // Add the spawned enemy to the sound manager's list for tracking
        soundManager.enemies.Add(spawnedEnemy.gameObject);

        // Wait a set amount of time before spawning the next enemy (controls pacing)
        yield return new WaitForSeconds(data.spacing);
    }
}
