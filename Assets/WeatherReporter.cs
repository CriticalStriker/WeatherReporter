namespace TrinityGames
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class WeatherReporter : MonoBehaviour
	{
		[SerializeField]
		private WeatherApi m_WeatherApi = null;

		// Use this for initialization
		void Start()
		{
			UpdateWeather();
		}
		
		public void UpdateWeather()
		{
			m_WeatherApi.UpdateWeather((success, weatherState) =>
			{
				if (success)
				{
					Debug.LogFormat("Current Weather: {0}", weatherState.ToString());
				}
				else
				{
					Debug.LogFormat("Update Weather Failure.");
				}
			});
		}
	}
}