using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace QuizCanners.IsItGame
{

    public static class AddressablesExtensions 
    {
		public static bool AddressableResourceExists<T>(string key)
		{
			foreach (var l in Addressables.ResourceLocators)
			{
				if (l.Locate(key, typeof(T), out _))
					return true;
			}
			return false;
		}
	}
}