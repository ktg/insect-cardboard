using UnityEngine;

namespace Behaviour
{
	public class LandedBehaviour : WaspBehaviour
	{
		private float waitUntil;

		public override void Start(DrunkenWasp wasp)
		{
			wasp.OnFlyingEnd();
			waitUntil = Time.time + wasp.landedTime + Random.Range(-1f, 1f);
			var transform = wasp.transform;
			transform.position = wasp.target.transform.position;
			transform.up = Vector3.RotateTowards(transform.up, wasp.target.transform.up,
				wasp.rotateSpeed * Time.deltaTime, 0f);
		}

		public override WaspBehaviour NextState(DrunkenWasp wasp)
		{
			if (!(Time.time > waitUntil)) return null;
			wasp.seekingBehaviour.VisitedTarget(wasp);
			return wasp.seekingBehaviour;
		}
	}

}