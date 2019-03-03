namespace TrinityGames
{
	using UnityEngine;
	using System.Collections;
	using UnityEditor;
	using TrinityGames;

	[CustomEditor(typeof(WeatherApi))]
	public class WeatherApiInspector : Editor
	{
		protected WeatherApi api = null;

		protected void OnEnable()
		{
			api = (WeatherApi)target;
		}

		protected void OnDisable()
		{
			api = null;
		}

		private float m_TestLatitude = 0f;
		private float m_TestLongitude = 0f;

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Weather Information Query", MessageType.Info);

			m_TestLatitude = EditorGUILayout.FloatField("Latitude: ", m_TestLatitude);
			m_TestLongitude = EditorGUILayout.FloatField("Longitude", m_TestLongitude);

			EditorGUILayout.Separator();

			if (GUILayout.Button("Weather Api Test", GUILayout.MinHeight(30)))
			{
				Debug.LogFormat("Weather Test: {0} / {1}", m_TestLatitude, m_TestLongitude);

				api.UpdateForceWeather(m_TestLatitude, m_TestLongitude, (success, state) =>
				{
					if (success)
					{
						Debug.LogFormat("Current Weather: {0}", state.ToString());
					}
					else
					{
						Debug.LogFormat("Update Weather Failure.");
					}
				});
			}
		}
	}
}