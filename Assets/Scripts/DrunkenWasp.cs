using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Behaviour;

public class DrunkenWasp : MonoBehaviour, IPointerClickHandler
{
	private WaspBehaviour state;

	public string id = "";
	public bool staticInsect;

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
		if (staticInsect)
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
		if (staticInsect)
		{
			state = heldBehaviour;
		}
		else
		{
			// TODO Escape
			state = spawnBehaviour;
		}

		state.Start(this);
	}

	public void OnPointerClick(PointerEventData ped)
	{
	}
}