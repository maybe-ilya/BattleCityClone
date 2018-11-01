using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedModifier : MonoBehaviour {
#pragma warning disable 649
	[SerializeField]
	private float speedModificator;
	[SerializeField]
	private Collider2D collision;
	[SerializeField]
	private LayerMask modifiableLayers;

	private bool canModify;
#pragma warning restore 649

	public void SetModificator (float modificator) {
		canModify = modificator > 0;
		speedModificator = modificator;
	}

	public void OnTriggerEnter2D (Collider2D other) {
		TryModify (other);
	}

	public void OnTriggerExit2D (Collider2D other) {
		TryModify (other, -1);
	}

	private bool CanModify (Collider2D other) {
		return canModify && other.IsTouchingLayers (modifiableLayers);
	}

	private void TryModify (Collider2D other, float deltaSign = 1.0f) {
		if (CanModify (other)) {
			var move = other.GetComponent<MovementComponent> ();
			if (move != null) {
				move.ApplySpeedModificator (speedModificator * deltaSign);
			}
		}
	}
}