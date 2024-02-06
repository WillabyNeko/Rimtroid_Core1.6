namespace RT_Core;

public class TimeUtils
{
	public struct TimeTickKeep
	{
		public const int TicksToSeconds = 60;

		public const int SecondsToMinutes = 60;

		public const int MinutesToHours = 60;

		public const int HoursToDays = 24;

		public const int DaysToQuadrums = 15;

		public const int QuadrumsToYears = 4;

		public int ticks;

		public int seconds;

		public int minutes;

		public int hours;

		public int days;

		public int quadrums;

		public int years;

		public TimeTickKeep(TimeTickKeep keep)
			: this(keep.ticks, keep.seconds, keep.minutes, keep.hours, keep.days, keep.quadrums, keep.years)
		{
		}

		public TimeTickKeep(int seconds, int minutes, int hours)
			: this(0, seconds, minutes, hours, 0, 0, 0)
		{
		}

		public TimeTickKeep(int seconds, int minutes, int hours, int days, int quadrums, int years)
			: this(0, seconds, minutes, hours, days, quadrums, years)
		{
		}

		public TimeTickKeep(int ticks, int seconds, int minutes, int hours, int days, int quadrums, int years)
		{
			this.ticks = ticks;
			this.seconds = seconds;
			this.minutes = minutes;
			this.hours = hours;
			this.days = days;
			this.quadrums = quadrums;
			this.years = years;
		}

		public long ToTicks()
		{
			return CollectTicks(this);
		}

		public static TimeTickKeep FromTicks(long ticks)
		{
			TimeTickKeep keep = default(TimeTickKeep);
			SliceTicks(ref keep, ticks);
			return keep;
		}

		public void Refresh()
		{
			SliceTicks(ref this, CollectTicks(this));
		}

		private static long CollectTicks(TimeTickKeep keep)
		{
			long num = keep.years * 4;
			num = (num + keep.quadrums) * 15;
			num = (num + keep.days) * 24;
			num = (num + keep.hours) * 60;
			num = (num + keep.minutes) * 60;
			num = (num + keep.seconds) * 60;
			return num + keep.ticks;
		}

		private static void SliceTicks(ref TimeTickKeep keep, long ticks)
		{
			keep.years = 0;
			keep.ticks = (int)ticks % 60;
			ticks /= 60;
			keep.seconds = (int)ticks % 60;
			ticks /= 60;
			keep.minutes = (int)ticks % 60;
			ticks /= 60;
			keep.hours = (int)ticks % 24;
			ticks /= 24;
			keep.days = (int)ticks % 15;
			ticks /= 15;
			keep.quadrums = (int)ticks % 4;
			ticks /= 4;
			keep.years = (int)ticks;
		}
	}

	public class TimeTickSpan
	{
		private TimeTickKeep keep;

		public int Ticks
		{
			get
			{
				return keep.ticks;
			}
			set
			{
				keep.ticks = value;
				keep.Refresh();
			}
		}

		public int Seconds
		{
			get
			{
				return keep.seconds;
			}
			set
			{
				keep.seconds = value;
				keep.Refresh();
			}
		}

		public int Minutes
		{
			get
			{
				return keep.ticks;
			}
			set
			{
				keep.minutes = value;
				keep.Refresh();
			}
		}

		public int Hours
		{
			get
			{
				return keep.ticks;
			}
			set
			{
				keep.hours = value;
				keep.Refresh();
			}
		}

		public int Days
		{
			get
			{
				return keep.ticks;
			}
			set
			{
				keep.days = value;
				keep.Refresh();
			}
		}

		public int Quadrums
		{
			get
			{
				return keep.ticks;
			}
			set
			{
				keep.quadrums = value;
				keep.Refresh();
			}
		}

		public int Years
		{
			get
			{
				return keep.ticks;
			}
			set
			{
				keep.years = value;
				keep.Refresh();
			}
		}

		public TimeTickSpan()
		{
			keep = default(TimeTickKeep);
		}

		public TimeTickSpan(long ticks)
		{
			keep = TimeTickKeep.FromTicks(ticks);
		}

		public TimeTickSpan(TimeTickKeep span)
		{
			keep = new TimeTickKeep(span);
		}
	}
}
