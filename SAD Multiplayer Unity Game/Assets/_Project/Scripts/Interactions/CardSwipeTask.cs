namespace _Project.Scripts.Interactions
{
    public class CardSwipeTask : GameTask
    {
        public void TaskFinish()
        {
            // Task Complete.
            TaskCompleted();
            //Close Task
            TaskClosed();
        }
        
        public void CloseTask()
        {
            TaskClosed();
        }
    }
}