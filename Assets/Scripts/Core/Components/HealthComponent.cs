using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour {
#pragma warning disable 649
	[SerializeField]
	private bool canBeDamaged;
	[SerializeField]
	private int hitPoints, maxHitPoints;
	[SerializeField]
	private Collider2D collision;
#pragma warning restore 649

	public int HP { get { return hitPoints; } }
	public int MaxHP { get { return maxHitPoints; } }
	public float Health { get { return (float) hitPoints / maxHitPoints; } }

	public void Init (int HP, int maxHP) {
		SetHP (HP);
		SetMaxHP (maxHP);
	}

	public void SetHP (int HP) {
		hitPoints = HP;
	}

	public void SetMaxHP (int HP) {
		maxHitPoints = HP;
	}

	public bool TakeDamage (int damage) {
		hitPoints = Mathf.Clamp (hitPoints - damage, 0, maxHitPoints);
		return hitPoints <= 0;
	}

	public void SetCanBeDamaged (bool damaged) {
		canBeDamaged = damaged;
		collision.enabled = canBeDamaged;
	}
}