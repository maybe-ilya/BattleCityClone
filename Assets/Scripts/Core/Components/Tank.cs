using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour {
#pragma warning disable 649
	[SerializeField] private HealthComponent health;
	[SerializeField] private MovementComponent movement;
	[SerializeField] private ShootComponent shoot;
#pragma warning restore 649
}