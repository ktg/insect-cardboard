namespace Behaviour
{
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
}