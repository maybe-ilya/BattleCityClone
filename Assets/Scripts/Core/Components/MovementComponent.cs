using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MovementComponent : MonoBehaviour {
#pragma warning disable 649
	[SerializeField]
	private float speed; 
	[SerializeField, FormerlySerializedAs("speedModifier")]
	private float speedModificator;
	[SerializeField]
	private Vector3 direction;

	private Coroutine moveRoutine;
#pragma warning restore 649
	void Start () {
		StartMoving ();
	}

	private void StartMoving () {
		moveRoutine = StartCoroutine (MoveRoutine ());
	}

	private void StopMoving () {
		StopCoroutine (moveRoutine);
		moveRoutine = null;
	}

	IEnumerator MoveRoutine () {
		while (true) {
			var shift = direction * speed * speedModificator * Time.deltaTime;
			transform.position += shift;
			yield return null;
		}
	}

	void OnDisable () {
		StopMoving ();
	}

	public void SetSpeed (float newSpeed) {
		speed = Mathf.Clamp (newSpeed, 0.0f, float.MaxValue);
	}

	public void SetDirection (Vector3 newDir) {
		direction = newDir;
	}

	public void ApplySpeedModificator (float delta) {
		speedModificator = Mathf.Clamp (speedModificator + delta, 1, float.MaxValue);
	}
}