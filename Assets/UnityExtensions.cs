namespace TrinityGames
{
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;

	public static class UnityExtensions
	{
		public static IEnumerator WaitForRealSeconds(float time)
		{
			float start = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup < start + time)
			{
				yield return null;
			}
		}
	}
}