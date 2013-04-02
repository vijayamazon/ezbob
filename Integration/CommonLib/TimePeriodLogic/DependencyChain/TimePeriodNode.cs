namespace EzBob.CommonLib.TimePeriodLogic.DependencyChain
{
	public enum TimeFrameType
	{
		Monthly
	}

	public class TimePeriodNode
	{
		public TimePeriodNode( TimePeriodEnum timePeriodType, TimePeriodNode child = null )
		{
			Child = child;

			if ( Child != null )
			{
				Child.Parent = this;
			}

			TimePeriodType = timePeriodType;
		}

		public TimePeriodNode Child { get; private set; }
		public TimePeriodNode Parent { get; private set; }
		public TimePeriodEnum TimePeriodType { get; private set; }
		
		public TimeFrameType TimeFrame
		{
			get { return TimeFrameType.Monthly; }
		}

		public TimePeriodNode Leaf
		{
			get { return Child != null ? Child.Leaf : this; }
		}

		public TimePeriodNode Root
		{
			get { return Parent != null ? Parent.Root : this; }
		}

		public bool IsRoot
		{
			get { return Parent == null; }
		}

		public bool IsLeaf
		{
			get { return Child == null; }
		}
	}
}
