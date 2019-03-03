namespace TrinityGames
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	using System.Net;
	using System.Xml;

	using UnityEngine.Networking;

	/// <summary>
	/// priority
	/// [snowy -> rainy -> dusty -> sunny -> toohot]
	/// </summary>
	public enum eWeatherState
	{
		None,

		Sunny, /// 맑은 날씨
		Rainy, /// 비오는 날씨
		Snowy, /// 눈오는 날씨
		Dusty, /// 미세먼지 심한 날씨
		TooHot, /// 너무 더운 날씨
		TooCold, /// 너무 추운 날씨
	}

	public class WeatherApi : MonoBase
	{
		private const string UpdateDateKey = "update.weather.date";

		[Header("http://api.openweathermap.org (APP ID)")]
		[SerializeField]
		private string APP_ID = "Enter you APP_ID";

		[Header("https://api.waqi.info (API TOKEN)")]
		[SerializeField]
		private string API_TOKEN = "Enter your API_TOKEN";

		[Space()]
		[SerializeField]
		private int m_UpdateHourLimit = 3;

		[Space()]
		[SerializeField]
		private int m_TempHotLimit = 35;
		[SerializeField]
		private int m_TempColdLimit = 0;

		[Space()]
		[SerializeField]
		private float m_CheckTimeout = 3f;

		private eWeatherState m_WeatherState = eWeatherState.None;

		public eWeatherState Current
		{
			get
			{
				return m_WeatherState;
			}
		}

		public bool Done
		{
			get
			{
				string dateText = PlayerPrefs.GetString(UpdateDateKey, "");

				if (string.IsNullOrEmpty(dateText) == false)
				{
					System.DateTime lastUpdatedDate = System.DateTime.Parse(dateText);
					var elapse = System.DateTime.Now - lastUpdatedDate;

					return (elapse.Hours < this.m_UpdateHourLimit);
				}

				return false;
			}
		}

		protected override void OnAwake()
		{
		}

		protected override void OnStart()
		{
		}

		protected override void OnCombinedDestroy()
		{
		}

		// protected override void OnUpdate()
		internal void FixedUpdate()
		{
			if (m_CheckAirQuality && m_CheckWeatherInfo)
			{
				UpdateApiState(m_AirQuality, m_WeatherCode, m_Temperature);

				UpdateApiDate();

				if (m_OnComplete != null)
				{
					m_OnComplete(true, this.Current);
					m_OnComplete = null;
				}

				StopAllCoroutines();

				m_CheckAirQuality = m_CheckWeatherInfo = false;
			}
		}

		private bool m_CheckAirQuality = false;
		private bool m_CheckWeatherInfo = false;
		private System.Action<bool, eWeatherState> m_OnComplete = null;

		public void UpdateWeather(System.Action<bool, eWeatherState> onComplete)
		{
			if (this.Done == false)
			{
				m_WeatherState = eWeatherState.None;

				StartCoroutine(UpdateAirQuality());
				StartCoroutine(UpdateWeatherInfo());

				DelayAction(m_CheckTimeout, () =>
				{
					if (onComplete != null)
					{
						onComplete(false, eWeatherState.None);
					}

					m_CheckAirQuality = m_CheckWeatherInfo = false;
				});

				m_OnComplete = onComplete;
			}
			else
			{
				if (onComplete != null)
				{
					onComplete(true, this.Current);
				}
			}
		}

		public void UpdateWeather(double latitude, double longitude, System.Action<bool, eWeatherState> onComplete)
		{
			if (this.Done == false)
			{
				m_WeatherState = eWeatherState.None;

				StartCoroutine(UpdateAirQuality(latitude, longitude));
				StartCoroutine(UpdateWeatherInfo(latitude, longitude));

				DelayAction(m_CheckTimeout, () =>
				{
					if (onComplete != null)
					{
						onComplete(false, eWeatherState.None);
					}

					m_CheckAirQuality = m_CheckWeatherInfo = false;
				});

				m_OnComplete = onComplete;
			}
			else
			{
				if (onComplete != null)
				{
					onComplete(true, this.Current);
				}
			}			
		}

		public void UpdateForceWeather(double latitude, double longitude, System.Action<bool, eWeatherState> onComplete)
		{
			StartCoroutine(UpdateAirQuality(latitude, longitude));
			StartCoroutine(UpdateWeatherInfo(latitude, longitude));

			m_OnComplete = onComplete;
		}

		private void UpdateApiDate()
		{
			PlayerPrefs.SetString(UpdateDateKey, System.DateTime.Now.ToString());
		}
		
		private void UpdateApiState(int airQualityPM25, int weatherCode, int temperature)
		{
			///
			/// 0 ~ 25 : Good
			/// 51 ~ 100: Moderate
			/// 101 ~ 150: Unhealthy sensitive
			/// 151 ~ 200: Unhealthy
			/// 201 ~ 300: Very Unhealthy
			/// 300+: Hazardous
			///
			if (airQualityPM25 > 100)
			{
				m_WeatherState = eWeatherState.Dusty;
				return;
			}

			///
			/// https://openweathermap.org/weather-conditions
			/// 
			bool isRainy = (200 <= weatherCode && weatherCode <= 399) || 
				(500 <= weatherCode && weatherCode <= 599) || 
				(700 <= weatherCode && weatherCode <= 799);

			bool isSnowy = (600 <= weatherCode && weatherCode <= 699);

			bool isSunny = (800 == weatherCode);
			bool isCloudy = (801 <= weatherCode && weatherCode <= 809);

			if (isRainy)
			{
				m_WeatherState = eWeatherState.Rainy;
				return;
			}

			if (isSnowy)
			{
				m_WeatherState = eWeatherState.Snowy;
				return;
			}

			if (isSunny || isCloudy)
			{
				m_WeatherState = eWeatherState.Sunny;
			}

			if (temperature > m_TempHotLimit)
			{
				m_WeatherState = eWeatherState.TooHot;
			}

			if (temperature < m_TempColdLimit)
			{
				/// m_WeatherState = eWeatherState.TooCold;
			}
		}

		/// <summary>
		/// http://aqicn.org/json-api/doc/#api-Geolocalized_Feed-GetHereFeed
		/// </summary>
		private int m_AirQuality = 0;
		private IEnumerator UpdateAirQuality(double latitude, double longitude)
		{
			string queryFormat = "https://api.waqi.info/feed/geo:{0};{1}/?token={2}";
			string query = string.Format(queryFormat, latitude, longitude, API_TOKEN);

			UnityWebRequest www = UnityWebRequest.Get(query);
			yield return www.Send();

			if (www.isError)
			{
				Debug.Log(www.error);
			}
			else
			{
				VerifyAirQuality(www.downloadHandler.text);
			}
		}

		private IEnumerator UpdateAirQuality()
		{
			string query = string.Format("https://api.waqi.info/feed/here/?token={0}", API_TOKEN);

			UnityWebRequest www = UnityWebRequest.Get(query);
			yield return www.Send();

			if (www.isError)
			{
				Debug.Log(www.error);
			}
			else
			{
				VerifyAirQuality(www.downloadHandler.text);
			}
		}

		private int m_WeatherCode = 0;
		private int m_Temperature = 0;
		private IEnumerator UpdateWeatherInfo()
		{
			UnityWebRequest reqIpApi = UnityWebRequest.Get("https://ipapi.co/json/");
			yield return reqIpApi.Send();

			if (reqIpApi.isError)
			{
				Debug.LogError(reqIpApi.error);
			}
			else
			{
				string resText = reqIpApi.downloadHandler.text;

				var N = SimpleJSON.JSON.Parse(resText);

				double latitude = N["latitude"].AsDouble;
				double longitude = N["longitude"].AsDouble;

				Debug.Log(resText);

				string queryFormat = "http://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&APPID={2}";

				string query = string.Format(queryFormat, latitude, longitude, APP_ID);

				UnityWebRequest www = UnityWebRequest.Get(query);
				yield return www.Send();

				if (www.isError)
				{
					Debug.LogError(www.error);
				}
				else
				{
					VerifyWeatherInfo(www.downloadHandler.text);
				}
			}			
		}

		private IEnumerator UpdateWeatherInfo(double latitude, double longitude)
		{
			string queryFormat = "http://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&APPID={2}";

			string query = string.Format(queryFormat, latitude, longitude, APP_ID);

			UnityWebRequest www = UnityWebRequest.Get(query);
			yield return www.Send();

			if (www.isError)
			{
				Debug.LogError(www.error);
			}
			else
			{
				VerifyWeatherInfo(www.downloadHandler.text);
			}
		}

		private void VerifyAirQuality(string jsonText)
		{
			m_CheckAirQuality = true;

			var N = SimpleJSON.JSON.Parse(jsonText);

			var status = N["status"].ToString().Trim('\"');
			if (status.ToLower() == "ok")
			{
				var node = N["data"]["aqi"];
				if (node != null)
				{
					m_AirQuality = N["data"]["aqi"].AsInt;
				}
				else
				{
					m_AirQuality = 0;
				}
			}
			else
			{
				Debug.LogWarning("air quality not found.");
			}
			
			Debug.Log(jsonText);
		}

		private void VerifyWeatherInfo(string jsonText)
		{
			m_CheckWeatherInfo = true;

			var N = SimpleJSON.JSON.Parse(jsonText);

			m_WeatherCode = N["weather"][0]["id"].AsInt;
			m_Temperature = N["main"]["temp"].AsInt / 10;

			Debug.Log(jsonText);
		}
    }
}