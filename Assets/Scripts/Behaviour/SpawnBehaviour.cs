using UnityEngine;

namespace Behaviour
{
	public class SpawnBehaviour : WaspBehaviour
	{
		public override void Update(DrunkenWasp wasp)
		{
			var spawns = GameObject.FindGameObjectsWithTag("Spawn");
			if (spawns.Length <= 0) return;
			var index = Random.Range(0, spawns.Length);
			wasp.transform.position = spawns[index].transform.position;
		}

		public override WaspBehaviour NextState(DrunkenWasp wasp)
		{
			wasp.seekingBehaviour.Reset(wasp);
			return wasp.seekingBehaviour;
		}
	}
}