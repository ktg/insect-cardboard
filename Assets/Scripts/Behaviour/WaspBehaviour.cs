namespace Behaviour
{
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
}