using UnityEngine;

namespace Behaviour
{
	public class HoverBehaviour : MovingBehaviour
	{
		private float hoverUntil;

		public override void Start(DrunkenWasp wasp)
		{
			hoverUntil = Time.time + wasp.hoverTime + Random.Range(-1f, 1f);
			reduction = 0.01f;
			targetPosition = wasp.transform.position;
			wasp.OnFlyingStart();
		}

		protected override void UpdateTarget(DrunkenWasp wasp)
		{
			var lookDirection = targetPosition - wasp.transform.position;
			if (lookDirection != Vector3.zero)
			{
				wasp.transform.forward = lookDirection.normalized;
			}
		}

		public override WaspBehaviour NextState(DrunkenWasp wasp)
		{
			if (Time.time < hoverUntil) return null;
			return wasp.flyBehaviour;
		}
	}
}