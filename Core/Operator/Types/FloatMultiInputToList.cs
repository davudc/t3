﻿using System;
using System.Collections.Generic;

namespace T3.Core.Operator.Types
{
    public class FloatMultiInputToList : Instance<FloatMultiInputToList>
    {
        [Output(Guid = "{9140BD66-3257-498A-80CD-C516C128F7E5}")]
        public readonly Slot<List<float>> Result = new Slot<List<float>>(new List<float>(20));

        public FloatMultiInputToList()
        {
            Result.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            Result.Value.Clear();
            foreach (var input in Input.GetCollectedInputs())
            {
                Result.Value.Add(input.GetValue(context));
            }
        }
        
        [Input(Guid = "{15874509-FABB-44CA-93A1-858FF95FB5F5}")]
        public readonly MultiInputSlot<float> Input = new MultiInputSlot<float>();
    }
}