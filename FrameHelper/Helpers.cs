using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrameHelper
{
	public static class Helpers
	{
		public static async Task<bool> AutoRetry(this Func<Task<bool>> action, int? maxTries = 5, TimeSpan? maxTimeout = null)
		{
			DateTimeOffset start = DateTimeOffset.Now;
			for (int triesSoFar = 0; triesSoFar < maxTries; triesSoFar++)
			{
				if (maxTimeout != null && DateTimeOffset.Now - start > maxTimeout)
				{
					Debug.WriteLine("AutoRetry timed out");
					return false;
				}
				if (await action())
				{
					return true;
				}
				Debug.WriteLine($"AutoRetry failed attempt #{triesSoFar+1}, retrying...");
			}
			Debug.WriteLine("AutoRetry failed all attempts");
			return false;
		}

		public static async Task<bool> AutoRetry(this Func<Task<bool>> action)
		{
			return await AutoRetry(action, 5, TimeSpan.FromSeconds(60));
		}
	}
}
