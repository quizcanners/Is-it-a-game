using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;

namespace Dungeons_and_Dragons
{
    public abstract class DnD_SmartId<TValue> : SmartStringIdGeneric<TValue> where TValue: IGotName, new()
    {
        public T TryGetValue<T>(Func<TValue, T> fromCaracter, Func<T> fallback) => TryGetEntity(out var character) ? fromCaracter(character) : fallback();
        public void TrySetValue(Action<TValue> toCaracter, Action fallback)
        {
            if (TryGetEntity(out var character))
                toCaracter.Invoke(character);
            else
                fallback.Invoke();
        }

        protected DnDPrototypesScriptableObject Data
             => Singleton.TryGetValue<DnD_Service, DnDPrototypesScriptableObject>(s => s.DnDPrototypes);
    }
}
