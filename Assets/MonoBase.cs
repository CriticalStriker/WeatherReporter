namespace TrinityGames
{
	using UnityEngine;
	using System.Collections;

	public abstract class MonoBase : MonoBehaviour
	{
		public bool Awaken = false;
		public bool Started = false;
		public bool Destroyed = false;

		protected abstract void OnAwake();
		protected abstract void OnStart();
		protected abstract void OnCombinedDestroy();

		// Do not use this method.
		// protected abstract void OnUpdate();

		internal void Awake()
		{
			OnAwake();

			Awaken = true;
		}

		internal void Start()
		{
			OnStart();

			Started = true;
		}

		internal void OnDestroy()
		{
			OnCombinedDestroy();

			Destroyed = true;
		}

		/*
		internal void Update()
		{
			///OnUpdate();
		}

		internal void FixedUpdate()
		{
			OnUpdate();
		}
		*/

		protected virtual void DelayAction(float delay, System.Action action)
		{
			StartCoroutine(DelayActionHandler(delay, action));
		}

		private IEnumerator DelayActionHandler(float delay, System.Action action)
		{
			yield return new WaitForSeconds(delay);

			if (action != null)
			{
				action();
			}
		}

		protected virtual void RealTimeDelayAction(float delay, System.Action action)
		{
			StartCoroutine(RealTimeDelayActionHandler(delay, action));
		}

		private IEnumerator RealTimeDelayActionHandler(float delay, System.Action action)
		{
			yield return StartCoroutine(UnityExtensions.WaitForRealSeconds(delay));

			if (action != null)
			{
				action();
			}
		}
	}
}