/*
  Source: https://gist.github.com/markcastle/16387596b288309314febe8e73d01a64
  Description: 
  Utilities for generating Unix Epoch times from UTC in c# (and Unity)
*/

using System;

/// <summary>
/// Utilities for generating Unix Epoch times from UTC.
/// </summary>
namespace Utils
{
public class Epoch
{
	/// <summary>
        /// Unix High Resolution Epoch Time (Milliseconds since UTC 1/1/1970 00:00:00)
        /// </summary>
        /// <returns>double</returns>
        public static double EpochTimeHiRes()
        {
		return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// Unix High Resolution Epoch Time (Milliseconds since UTC 1/1/1970 00:00:00)
        /// </summary>
        /// <returns>decimal</returns>
        public static decimal EpochTimeHiResAsDecimal()
        {
		return Convert.ToDecimal((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
        }

        /// <summary>
        /// Unix High Resolution Epoch Time (Milliseconds since UTC 1/1/1970 00:00:00)
        /// </summary>
        /// <returns>string</returns>
        public static string EpochTimeHiResAsString()
        {
		return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString().Replace(".", "");
        }

	/// <summary>
	/// Unix Epoch Time (Seconds since UTC 1/1/1970 00:00:00)
	/// </summary>
	/// <returns>double</returns>
	public static double epochTime()
	{
		return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
	}

	/// <summary>
	/// Unix Epoch Days (Days since UTC 1/1/1970 00:00:00)
	/// </summary>
	/// <returns>int</returns>
	public static int epochDays()
	{
		return (int)(((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds) / 86400);
	}

	/// <summary>
	/// Unix Epoch Hours (Hours since UTC 1/1/1970 00:00:00)
	/// </summary>
	/// <returns>int</returns>
	public static int epochHours()
	{
		return (int)(((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds) / 3600);
	}

	/// <summary>
	/// Nanoseconds since the last stamp_secs
	/// </summary>
	/// <param name="epochSecs"></param>
	/// <returns>long</returns>
	public static long nanosecSinceStmp(double epochSecs)
	{ 
		// Calculate nanoseconds (stamp.nsec)
		// First, get the fractional part of stamp_secs, then convert it to nanoseconds
		long nsec = (long)((epochSecs - epochSecs) * 1e9);

		// Now sec is your stamp.sec equivalent and nsec is your stamp.nsec equivalent
		return nsec;
	}
}
	
}

