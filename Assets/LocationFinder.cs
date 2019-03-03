namespace TrinityGames
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public enum eLocationStatus
	{
		None,

		NotAllow,
		Timeout,
		ConnectFail,

		Success,
	}

	/// <summary>
	/// https://docs.unity3d.com/ScriptReference/LocationService.Start.html
	/// </summary>
	public class LocationFinder : MonoBehaviour
	{
		private float m_UpdateTime = 0.0f;
		private float m_UpdateCycle = 1.0f;
		private int m_UpdateCount = 0;

		private eLocationStatus m_Status = eLocationStatus.None;

		public bool UseEveryTime = false;

		public bool Ready
		{
			get
			{
				return (m_Status == eLocationStatus.Success && m_UpdateCount > 0);
			}
		}

		public double Latitude
		{
			get
			{
				return (Ready) ? Input.location.lastData.latitude : 0;
			}
		}

		public double Longitude
		{
			get
			{
				return (Ready) ? Input.location.lastData.longitude : 0;
			}
		}

		internal IEnumerator Start()
		{

#if UNITY_ANROID
         Input.compass.enabled = true;
#endif

			return InitializeGPSServices();
		}

		private IEnumerator InitializeGPSServices()
		{
			// First, check if user has location service enabled
			if (!Input.location.isEnabledByUser)
			{
				m_Status = eLocationStatus.NotAllow;

				yield break;
			}

			// Start service before querying location
			if (UseEveryTime)
			{
				Input.location.Start(0.1f, 0.1f);
			}
			else
			{
				Input.location.Start();
			}

			// Wait until service initializes
			int maxWait = 20;
			while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
			{
				yield return new WaitForSeconds(1);
				maxWait--;
			}

			// Service didn't initialize in 20 seconds
			if (maxWait < 1)
			{
				m_Status = eLocationStatus.Timeout;
				yield break;
			}

			if (UseEveryTime == false)
			{
				// Connection has failed
				if (Input.location.status == LocationServiceStatus.Failed)
				{
					m_Status = eLocationStatus.ConnectFail;
					yield break;
				}
				else
				{
					m_Status = eLocationStatus.Success;
				}

				// Stop service if there is no need to query location updates continuously
				Input.location.Stop();
			}
		}

		internal void Update()
		{
			if (UseEveryTime)
			{
				m_UpdateTime += Time.deltaTime;
				if (m_UpdateTime > m_UpdateCycle)
				{
					UpdateGPS();
					m_UpdateTime = 0.0f;
				}
			}
		}

		private void UpdateGPS()
		{
			// Connection has failed
			if (Input.location.status == LocationServiceStatus.Failed)
			{
				m_Status = eLocationStatus.ConnectFail;

				Input.location.Stop();
				Start();
			}
			else
			{
				m_Status = eLocationStatus.Success;
				m_UpdateCount++;
			}
		}
	}
}