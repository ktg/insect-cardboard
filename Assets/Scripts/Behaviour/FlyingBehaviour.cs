using UnityEngine;

namespace Behaviour
{
	public class FlyBehaviour : MovingBehaviour
	{
		private readonly LandedBehaviour landedBehaviour = new LandedBehaviour();

		private Vector3 currentTarget;
		private WaspBehaviour nextBehaviour;
		private float triggerDist;

		public override void Start(DrunkenWasp wasp)
		{
			var distance = (wasp.target.transform.position - wasp.transform.position).magnitude;
			//Debug.Log("Fly distance = " + distance);
			if (distance <= wasp.hoverDistance * 2f)
			{
				//Debug.Log("Current Target = Final Target at " + currentTarget);
				currentTarget = wasp.target.transform.position;
				if (wasp.target.CompareTag("Spawn"))
				{
					nextBehaviour = wasp.spawnBehaviour;
					triggerDist = wasp.swerveAmount;
				}
				else
				{
					nextBehaviour = landedBehaviour;
					triggerDist = 0.01f;
				}
			}
			else if (distance <= wasp.flyDistance * 1.2f)
			{
				// If within fly distance, fly above the target
				//Debug.Log(wasp.name + ": " + distance + " > " + (wasp.hoverDistance * 2f) + " < " + (wasp.flyDistance * 1.2f));
				//Debug.Log("Current Target = Hover above at " + currentTarget + "(" + wasp.target.transform.position + " + " + wasp.transform.up + " * " + wasp.hoverDistance+ ")");
				currentTarget = wasp.target.transform.position + (wasp.transform.up * wasp.hoverDistance);
				nextBehaviour = wasp.hoverBehaviour;
				triggerDist = wasp.hoverDistance;
			}
			else
			{
				// If target further than fly distance, then pick a point in that direct to fly to
				//Debug.Log("Current Target = Hover at " + currentTarget);
				var waspPosition = wasp.transform.position;
				currentTarget = waspPosition
				                + ((wasp.target.transform.position - waspPosition).normalized * wasp.flyDistance)
				                + new Vector3(Random.Range(-wasp.randomness, wasp.randomness),
					                Random.Range(-wasp.randomness, wasp.randomness),
					                Random.Range(-wasp.randomness, wasp.randomness));
				nextBehaviour = wasp.seekingBehaviour;
				triggerDist = wasp.swerveAmount;
			}

			targetPosition = wasp.transform.position;
			//Debug.Log("Target distance = " + (wasp.transform.position - currentTarget).magnitude);
			wasp.OnFlyingStart();
		}

		protected override void UpdateTarget(DrunkenWasp wasp)
		{
			// Move targetPosition
			var distance = (currentTarget - targetPosition).magnitude;
			if (distance > 0.01)
			{
				var seekVelocity = (currentTarget - targetPosition).normalized * wasp.forwardVelocity;
				if ((seekVelocity * Time.deltaTime).magnitude > distance)
				{
					targetPosition = currentTarget;
				}
				else if (seekVelocity.magnitude > distance)
				{
					seekVelocity = (currentTarget - targetPosition).normalized * distance;
					targetPosition += seekVelocity * Time.deltaTime;
				}
				else
				{
					targetPosition += seekVelocity * Time.deltaTime;
				}
			}

			var lookDirection = currentTarget - wasp.transform.position;
			if (lookDirection != Vector3.zero)
			{
				wasp.transform.forward = lookDirection.normalized;
			}
		}

		public override WaspBehaviour NextState(DrunkenWasp wasp)
		{
			var distance = (wasp.transform.position - currentTarget).magnitude;
			//Debug.Log(distance + " < " + swerveDist + "?");
			if (distance > triggerDist) return null;
			return nextBehaviour;
		}
	}
}