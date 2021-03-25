using System.Collections;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [ExecuteAlways]
    public class KidsInAClassTest : Singleton.BehaniourBase
    {
        
        private int kidsCount = 20;
        
        const int DAYS_IN_YEAR = 365;
        
        int iterations;
        int hadDoubles;
        int simulations = 10000;
        
        
        int[] array;
        int[] byDay = new int[DAYS_IN_YEAR];


        public int GetPercentage()
        {
            if (iterations < 1)
                return  100;
            
            return (hadDoubles * 100) / (iterations);
            
        }

        readonly TimedCoroutine taskRunner = new TimedCoroutine();

        private void Calculate()
        {
            array = new int[kidsCount];
            byDay = new int[DAYS_IN_YEAR];
                    
            bool doubles = false;
                    
            for (int i = 0; i < kidsCount; i++)
            {
                var day = Random.Range(1, DAYS_IN_YEAR);
                array[i] = day;
                byDay[day] += 1;

                if (byDay[day] > 1)
                {
                    doubles = true;
                }
            }
                    
            iterations ++;
            hadDoubles += doubles ? 1 : 0;
        }

        private IEnumerator RunSimulations(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Calculate();
                yield return QcAsync.CallAgain();
            }
            
            
        }
        
        public override void Inspect()
        {
            base.Inspect();
            
            var change = pegi.ChangeTrackStart();

            "Iterations: {0} | Had Doubles: {1}   | {2}%".F(iterations, hadDoubles,
                GetPercentage()).PegiLabel().nl();
            
            if ("Kids Count".PegiLabel().edit(ref kidsCount).nl())
            {
                iterations = 0;
                hadDoubles = 0;
                array = null;
                taskRunner.Stop();
            }

            "Simulations".PegiLabel().edit(ref simulations);

            if (icon.Play.Click())
            {
                taskRunner.Reset(RunSimulations(simulations));
                StartCoroutine(taskRunner.GetCoroutine());
            }
            
            if (kidsCount > 0 &&  "Calculate".PegiLabel().Click().nl())
                    Calculate();
            
            if (array.IsNullOrEmpty() == false)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    var val = array[i];
                    "Student{0}: {1}  {2}".F(i, val, byDay[val]).PegiLabel().nl();
                }
            }
            
        }
    }
}