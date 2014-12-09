using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzAutoResponder
{
	using System.Diagnostics;

	public class WeekendMarker
	{
		private readonly LinkedList<Weekend> _days = new LinkedList<Weekend>();

		public WeekendMarker(DayOfWeek dayBegin, DayOfWeek dayEnd)
		{
			_days.AddLast(new Weekend{ Day = DayOfWeek.Sunday });
			_days.AddLast(new Weekend{ Day = DayOfWeek.Monday});
			_days.AddLast(new Weekend{ Day = DayOfWeek.Thursday});
			_days.AddLast(new Weekend{ Day = DayOfWeek.Tuesday});
			_days.AddLast(new Weekend{ Day = DayOfWeek.Wednesday});
			_days.AddLast(new Weekend{ Day = DayOfWeek.Friday});
			_days.AddLast(new Weekend { Day = DayOfWeek.Saturday });

			SetWeekendFlags(dayBegin, dayEnd);
		}

		private void SetWeekendFlags(DayOfWeek dayBegin, DayOfWeek dayEnd)
		{
			var begin = WeekendBegin(dayBegin);
			var end = WeekendEnd(dayEnd);

			while (begin != end)
			{
				begin = begin.NextOrFirst();
				if(begin == end){return;}
				begin.Value.WeekendMiddle = true;
			}
		}

		public bool IsWeekendBegin(DayOfWeek dayOfWeek)
		{
			return GetWeekend(dayOfWeek).WeekendBegin;
		}
		public bool IsWeekendEnd(DayOfWeek dayOfWeek)
		{
			return GetWeekend(dayOfWeek).WeekendEnd;
		}
		public bool IsWeekendMiddle(DayOfWeek dayOfWeek)
		{
			return GetWeekend(dayOfWeek).WeekendMiddle;
		}

		private Weekend GetWeekend(DayOfWeek dayOfWeek)
		{
			return (from day in _days where day.Day == dayOfWeek select _days.Find(day).Value).FirstOrDefault();
		}

		private LinkedListNode<Weekend> WeekendBegin(DayOfWeek dayOfWeek)
		{
			foreach (var day in _days)
			{
				if (day.Day == dayOfWeek)
				{
					var begin = _days.Find(day);
					begin.Value.WeekendBegin = true;
					return begin;
				}
			}
			return null;
		}

		private LinkedListNode<Weekend> WeekendEnd(DayOfWeek dayOfWeek)
		{
			foreach (var day in _days)
			{
				if (day.Day == dayOfWeek)
				{
					var end = _days.Find(day);
					end.Value.WeekendEnd = true;
					return end;
				}
			}
			return null;
		}

	}

	public class Weekend
	{
		public DayOfWeek Day { get; set; }
		public bool WeekendBegin { get; set; }
		public bool WeekendEnd { get; set; }
		public bool WeekendMiddle { get; set; }

		public override string ToString()
		{
			return string.Format("{0} begin:{1} end:{2} middle:{3}", Day, WeekendBegin, WeekendEnd, WeekendMiddle);
		}
	}

	static class CircularLinkedList
	{
		public static LinkedListNode<Weekend> NextOrFirst(this LinkedListNode<Weekend> current)
		{
			return current.Next ?? current.List.First;
		}

		public static LinkedListNode<Weekend> PreviousOrLast(this LinkedListNode<Weekend> current)
		{
			return current.Previous ?? current.List.Last;
		}
	}
}
