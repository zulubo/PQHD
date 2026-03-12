using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour
{

	public Vector3 axis;
	public float speed;

	public bool local = true;

	public bool useUnscaledTime = false;

	public bool randomDirection = false;
	[Range(0, 1)]
	public float randomSpeed = 0.0f;

	private void Start()
	{
		speed *= Mathf.Lerp(1, Random.value, randomSpeed) * (randomDirection ? (Random.value > 0.5 ? 1f : -1f) : 1f);
	}

	void Update()
	{

		float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

		transform.Rotate(axis, speed * dt, local ? Space.Self : Space.World);

	}
}
