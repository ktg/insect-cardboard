using System.Collections.Generic;
using UnityEngine;

namespace Behaviour
{
public class SeekingBehaviour : MovingBehaviour
{
	private float hoverUntil;
	private List<GameObject> targets;

	public void Reset(DrunkenWasp wasp)
	{
		targets = new List<GameObject>(GameObject.FindGameObjectsWithTag("Target"));
	}

	public void VisitedTarget(DrunkenWasp wasp)
	{
		targets.Remove(wasp.target);
		wasp.target = null;
	}

	protected override void UpdateTarget(DrunkenWasp wasp)
	{
		var timerCheck = (int) (Time.time * 10);
		reduction = timerCheck % 10 == 0 ? 1f : 0.01f;
		var lookDirection = targetPosition - wasp.transform.position;
		if (lookDirection != Vector3.zero)
		{
			wasp.transform.forward = lookDirection.normalized;
		}
	}

	public override void Start(DrunkenWasp wasp)
	{
		hoverUntil = Time.time + wasp.hoverTime + Random.Range(-1f, 1f);
		targetPosition = wasp.transform.position;
		reduction = 0.01f;
		//moveOffset = (int) Random.Range(0f, 10f);
		wasp.OnFlyingStart();
	}

	public override WaspBehaviour NextState(DrunkenWasp wasp)
	{
		if (Time.time < hoverUntil) return null;

		var nearestDistance = float.MaxValue;
		GameObject nearest = null;
		foreach (var targetItem in targets)
		{
			var distance = (wasp.transform.position - targetItem.transform.position).magnitude;
			if (distance < nearestDistance)
			{
				var rand = Random.value;
				if (rand >= 0.5f)
				{
					nearest = targetItem;
					nearestDistance = distance;
				}
			}
		}

		if (nearest == null)
		{
			foreach (var targetItem in GameObject.FindGameObjectsWithTag("Spawns"))
			{
				var distance = (wasp.transform.position - targetItem.transform.position).magnitude;
				if (!(distance < nearestDistance)) continue;
				nearest = targetItem;
				nearestDistance = distance;
			}
		}

		//Debug.Log(nearest);
		wasp.target = nearest;
		return wasp.flyBehaviour;
	}
}
}