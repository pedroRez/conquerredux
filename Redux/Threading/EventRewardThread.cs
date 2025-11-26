using System;
using Redux;
using Redux.Managers;

namespace Redux.Threading
{
    public class EventRewardThread : ThreadBase
    {
        private const int THREAD_SPEED = 10000;
        private long _nextTrigger;

        protected override void OnInit()
        {
            _nextTrigger = Common.Clock + THREAD_SPEED;
        }

        protected override bool OnProcess()
        {
            var now = Common.Clock;
            if (now >= _nextTrigger)
            {
                _nextTrigger += THREAD_SPEED;
                EventRewardManager.RunPendingDraws();
            }

            return true;
        }

        protected override void OnDestroy()
        {
        }
    }
}
