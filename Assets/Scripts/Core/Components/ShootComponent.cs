using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootComponent : MonoBehaviour {
#pragma warning disable 649
	[SerializeField]
	private float fireRate;
	[SerializeField]
	private GameObject projectilePrefab;
	[SerializeField]
	private Vector3 shootPointDirection;

	[SerializeField]
	private Color gizmoColor;
	[SerializeField]
	private float gizmoSize;
	private bool canShoot;
#pragma warning restore 649

	private Vector3 ShootPoint {
		get {
			return transform.position + shootPointDirection;
		}
	}

	public void Awake () {
		canShoot = projectilePrefab != null;
	}

	public void Shoot () {
		if (canShoot) {
			SetTimer ();
			SpawnProjectile ();
		}
	}

	private void SpawnProjectile () {
		var point = ShootPoint;
		Instantiate (projectilePrefab, point, transform.rotation);
	}

	private void SetTimer () {
		StartCoroutine (TimerRoutine ());
	}

	private IEnumerator TimerRoutine () {
		canShoot = false;
		yield return new WaitForSeconds (fireRate);
		canShoot = true;
	}

	public void SetFireRate (float newRate) {
		fireRate = Mathf.Clamp (newRate, 0, float.MaxValue);
	}

	private void OnDrawGizmosSelected () {
		Gizmos.color = gizmoColor;
		Gizmos.DrawSphere (ShootPoint, gizmoSize);
	}
}