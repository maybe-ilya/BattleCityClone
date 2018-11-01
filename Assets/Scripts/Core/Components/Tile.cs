using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
#pragma warning disable 649
	[SerializeField]
	private HealthComponent health;
	[SerializeField]
	private SpriteRenderer sprite;
	[SerializeField]
	private Animator animator;
	[SerializeField]
	private Collider2D collision;
	[SerializeField]
	private SpeedModifier speedModifier;
#pragma warning restore 649

	public void Init (TileConfig config) {
		gameObject.name = config.Name;

		var hp = config.health;
		health.Init (hp, hp);
		health.SetCanBeDamaged (config.breakable);
		collision.isTrigger = config.walkable;

		sprite.sprite = config.sprite;
		sprite.sortingOrder = config.layer;

		var anim = config.anim;
		var animated = anim != null;
		animator.enabled = animated;
		if (animated) {
			animator.runtimeAnimatorController = anim;
		}

		speedModifier.SetModificator (config.speedModifier);
	}
}