using System.Collections.Generic;

namespace EzBob.CommonLib.TimePeriodLogic.DependencyChain
{
	public class TimePeriodChain
	{
		private readonly TimePeriodNode _InitNode;

		public TimePeriodChain(TimePeriodNode initNode)
		{
			_InitNode = initNode;
		}

		public TimePeriodNode Root
		{
			get { return HasAtLeastOneNode ? _InitNode : null; }
		}

		public TimePeriodNode Leaf
		{
			get { return HasAtLeastOneNode ? _InitNode.Leaf : null; }
		}

		public bool HasAtLeastOneNode
		{
			get { return _InitNode != null; }
		}

		public int CountNodes
		{
			get 
			{
				if ( !HasAtLeastOneNode )
				{
					return 0;
				}

				var node = Root;

				var counter = 0;

				do
				{
					++counter;

					node = node.Child;
				}
				while ( node != null );

				return counter;
			}
		}

		public IEnumerable<TimePeriodNode> AllNodes
		{
			get 
			{
				var list = new List<TimePeriodNode>();

				if ( !HasAtLeastOneNode )
				{
					return list;
				}

				var node = Root;

				do
				{
					list.Add( node );

					node = node.Child;
				}
				while ( node != null );
				return list;

			}
		}

		public TimePeriodNode FindItem( TimePeriodEnum month )
		{
			var node = Root;

			do
			{
				if ( node.TimePeriodType == month)
				{
					return node;
				}

				node = node.Child;
			}
			while ( node != null );

			return null;
		}
		
	}
}