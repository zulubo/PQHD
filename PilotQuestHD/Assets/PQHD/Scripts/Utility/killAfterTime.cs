using UnityEngine;
using System.Collections;

public class killAfterTime : MonoBehaviour {
    public float timeToDie;
    float timer;
	// Use this for initialization
	void Update () {
        timer += Time.deltaTime;
        if (timer >= timeToDie)
        {
            kill();
        }
	}
	
	// Update is called once per frame
	void kill () {
        Destroy(gameObject);
	}

    public void resetTimer()
    {
        timer = 0;
    }
}
