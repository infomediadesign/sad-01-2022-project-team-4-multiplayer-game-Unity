using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Interactions;
using UnityEngine;

public class ShapeTask : GameTask
{
    [SerializeField] private int MAXcorrectSlotsCount = 3;
    [SerializeField] private int currentCorrectSlotsCount = 0;
    
    public void CorrectSlot()
    {
        currentCorrectSlotsCount++;

        if (currentCorrectSlotsCount >= MAXcorrectSlotsCount)
        {
            // Task Complete.
            TaskCompleted();
            //Close Task
            CloseTask();
        }
    }
}
