using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.StateMachine
{
    public enum BoolAndLogic { Default, True, False }

    public static class StateParameterLogicExtensions 
    {
        public static BoolAndLogic And(this BoolAndLogic a, BoolAndLogic b) 
        {
            if (a == BoolAndLogic.False || b == BoolAndLogic.False)
                return BoolAndLogic.False;

            if (a == BoolAndLogic.True || b == BoolAndLogic.True)
                return BoolAndLogic.True;

            return BoolAndLogic.Default;
        }
    }



}