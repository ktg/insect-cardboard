using UnityEngine;

namespace Behaviour
{
	public abstract class MovingBehaviour : WaspBehaviour
	{
		internal Vector3 targetPosition;
		private float swerveDist = 1f;
		protected float reduction = 1f;

		protected virtual void UpdateTarget(DrunkenWasp wasp)
		{
		}

		public override void Update(DrunkenWasp wasp)
		{
			var layerMask = LayerMask.GetMask("World");
			UpdateTarget(wasp);
			wasp.transform.up = Vector3.up;
			swerveDist = wasp.swerveAmount;
			var maxVelocity = wasp.MaxVelocity;
			if (wasp.target != null)
			{
				var dist = (wasp.target.transform.position - wasp.transform.position).magnitude;
				swerveDist = wasp.swerveAmount * Mathf.Clamp01((dist * 2 / wasp.flyDistance));
				maxVelocity = wasp.MaxVelocity * Mathf.Clamp01((dist * 2 / wasp.hoverDistance));
			}

			var timing = Time.time * wasp.swerveSpeed;
			var offsetX = Mathf.Sin(timing + Random.Range(-wasp.randomness, wasp.randomness)) * swerveDist;
			var offsetY = Random.Range(-wasp.randomness, wasp.randomness) * swerveDist;
			var offsetZ = Mathf.Cos(timing + Random.Range(-wasp.randomness, wasp.randomness)) * swerveDist;
			var offset = new Vector3(offsetX, offsetY, offsetZ);
			var swerveTarget = targetPosition + offset;

			var desiredVelocity = (swerveTarget - wasp.transform.position).normalized * (maxVelocity * reduction);

			var steering = desiredVelocity - wasp.Velocity;
			steering = Vector3.ClampMagnitude(steering, wasp.MaxForce);
			steering /= wasp.Mass;

			var newVelocity = Vector3.ClampMagnitude(wasp.Velocity + steering, maxVelocity);

			var direction = newVelocity * Time.deltaTime;
			if (Physics.Raycast(wasp.transform.position, direction, out var hit, direction.magnitude, layerMask))
			{
				//Debug.Log("Collision with " + hit.collider.name);
				var reflectVelocity = Vector3.Reflect(newVelocity, hit.normal);
				//Debug.Log(newVelocity + " => " + reflectVelocity);
				newVelocity = reflectVelocity;
			}

			wasp.Velocity = newVelocity;

			if (wasp.target != null)
			{
				var dist = (wasp.target.transform.position - wasp.transform.position).magnitude;
				var velDist = wasp.Velocity.magnitude * Time.deltaTime;
				if (velDist > dist)
				{
					wasp.transform.position = targetPosition;
				}
				else
				{
					wasp.transform.position += wasp.Velocity * Time.deltaTime;
				}
			}
			else
			{
				wasp.transform.position += wasp.Velocity * Time.deltaTime;
			}

			Debug.DrawRay(wasp.transform.position, wasp.Velocity.normalized * 2, Color.green);
			//Debug.DrawRay(transform.position, desiredVelocity.normalized * 2, Color.magenta);
		}
	}
}