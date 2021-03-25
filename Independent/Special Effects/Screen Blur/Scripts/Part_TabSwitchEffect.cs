using System;
using UnityEngine;

namespace QuizCanners.SpecialEffects
{
	[Serializable]
	public class TabSwitchEffect
	{
		[SerializeField] public BlurTransitionSimple BlurTransitionSimple;

		private bool firstFillDone = false;
		private bool transitionInProgress = false;
		private int _latestRequestedValue;

		public T CurrentTab<T>() => (T)(object)_latestRequestedValue;

		public void SetTab<T>(T targetTabEnum, Action<T> finalizeChange)
		{
			var current = (T)(object)_latestRequestedValue;

			SetTab(ref current, targetTabEnum, finalizeChange);
		}

		public void SetTab<T>(ref T currentTabEnum, T targetTabEnum, Action<T> finalizeChange)
		{
			if (firstFillDone && currentTabEnum.Equals(targetTabEnum))
				return;

			currentTabEnum = targetTabEnum;
			_latestRequestedValue = Convert.ToInt32(targetTabEnum);

			if (transitionInProgress)
				return;

			if (firstFillDone)
			{
				transitionInProgress = true;
				BlurTransitionSimple.Transition(Finalize);
			}
			else
				Finalize();


			void Finalize()
			{
				transitionInProgress = false;
				firstFillDone = true;
				finalizeChange?.Invoke((T)(object)_latestRequestedValue);
			}
		}
	}
}
