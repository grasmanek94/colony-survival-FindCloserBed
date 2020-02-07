using BlockEntities.Implementations;

namespace grasmanek94.FindCloserBed
{
	public class BedState
	{
		public BedTracker.Bed Bed { get; set; }
		public bool State { get; set; }

		public BedState(BedTracker.Bed bed, bool state)
		{
			Bed = bed;
			State = state;
		}
	}
}
