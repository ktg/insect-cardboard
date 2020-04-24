using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public abstract class WaspBehaviour
{
	public virtual void Start(DrunkenWasp wasp)
	{
	}

	public virtual void Update(DrunkenWasp wasp)
	{
	}

	public virtual WaspBehaviour NextState(DrunkenWasp wasp)
	{
		return null;
	}
}

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
			Debug.Log("Collision with " + hit.collider.name);
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

[Serializable]
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

[Serializable]
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

public class HeldBehaviour : WaspBehaviour
{
	public override void Start(DrunkenWasp wasp)
	{
		wasp.OnFlyingEnd();
	}

	public override void Update(DrunkenWasp wasp)
	{
	}
}

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

[Serializable]
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

public class DrunkenWasp : MonoBehaviour, IPointerClickHandler
{
	private WaspBehaviour state;

	public string id = "";
	public bool something = false;

	[Header("Flight")] public float flyDistance = 5f;

	[FormerlySerializedAs("ForwardVelocity")]
	public float forwardVelocity = 1;

	[Header("Drunkenness")] public float swerveAmount = 3f;
	public float swerveSpeed = 2f;
	public float randomness = 2f;

	[Header("Hover")] public float hoverTime = 3f;

	public float hoverDistance = 1f;
	//public float MinHeight = 0f;

	[Space(10)] public float rotateSpeed = 2f;
	public float landedTime = 5f;

	[Header("Physics")] public float Mass = 1f;
	public float MaxVelocity = 4f;
	public float MaxForce = 2f;

	internal readonly SpawnBehaviour spawnBehaviour = new SpawnBehaviour();
	internal readonly FlyBehaviour flyBehaviour = new FlyBehaviour();
	internal readonly SeekingBehaviour seekingBehaviour = new SeekingBehaviour();
	internal readonly HoverBehaviour hoverBehaviour = new HoverBehaviour();
	internal readonly LandedBehaviour landedBehaviour = new LandedBehaviour();

	private readonly HeldBehaviour heldBehaviour = new HeldBehaviour();

	internal GameObject target;
	internal Vector3 Velocity = Vector3.zero;

	private Animator waspSkeleton;
	private AudioSource buzzing;

	private static readonly int FlyAnim = Animator.StringToHash("Fly");
	private static readonly int FinishAnim = Animator.StringToHash("Finish");

	// Start is called before the first frame update
	void Start()
	{
		waspSkeleton = GetComponent<Animator>();
		buzzing = GetComponent<AudioSource>();
		if (something)
		{
			state = heldBehaviour;
		}
		else
		{
			state = spawnBehaviour;
		}
	}

	void Update()
	{
		if (state == null) return;
		state.Update(this);
		var nextState = state.NextState(this);
		if (nextState == null) return;
		state = nextState;
		//Debug.Log(state + ":" + Time.time);
		state.Start(this);
	}

	public void Respawn()
	{
		state = spawnBehaviour;
	}

	internal void OnFlyingStart()
	{
		waspSkeleton.SetBool(FlyAnim, true);
		waspSkeleton.SetBool(FinishAnim, false);
		if (!buzzing.isPlaying)
		{
			buzzing.Play();
		}
	}

	internal void OnFlyingEnd()
	{
		waspSkeleton.SetBool(FlyAnim, false);
		waspSkeleton.SetBool(FinishAnim, false);
		if (buzzing.isPlaying)
		{
			buzzing.Stop();
		}
	}

	public void OnHeldStart()
	{
		state = heldBehaviour;
		state.Start(this);
	}

	public void OnHeldEnd()
	{
		// TODO Escape
		state = seekingBehaviour;
		state.Start(this);
	}

	public void OnPointerClick(PointerEventData ped)
	{
	}
}