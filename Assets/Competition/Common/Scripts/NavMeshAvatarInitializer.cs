using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

namespace SIGVerse.FCSC.Common
{
	public class NavMeshAvatarInitializer : MonoBehaviour
	{
		private void Awake()
		{
			this.EnableScript<NavMeshAgent>();
			this.EnableScript<NavMeshObstacle>();
			this.EnableScript<NavMeshRouting>();
		}

		private void EnableScript<T>() where T:Behaviour
		{
			T script = this.GetComponent<T>();

			if (script != null) { script.enabled = true; }
		}
	}
}
