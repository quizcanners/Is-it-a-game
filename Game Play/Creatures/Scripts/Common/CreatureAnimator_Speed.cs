using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace QuizCanners.IsItGame.Develop
{
    public class CreatureAnimator_Speed 
    {
        public Vector3 PreviousPosition;
        public Vector3 DirectionVector;

        public void OnMove(Vector3 newPosition, out float speed) 
        {
            var newDirection = (newPosition - PreviousPosition);
            newDirection.y = 0;

            DirectionVector = Vector3.Lerp(DirectionVector, newDirection.normalized, Time.deltaTime * 10);
            DirectionVector.y = 0;

            speed = newDirection.magnitude / Time.deltaTime; // To get Speed per second
            PreviousPosition = newPosition;
        }

        public void Reset(Vector3 newPosition) 
        {
            PreviousPosition = newPosition;
        }

    }
}