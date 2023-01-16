using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YarnNinja.Common
{
    public class YarnApplicationHeader
    {
        public string Id { get; set; }
        
        public YarnApplicationType Type { get; set; }
        public YarnApplicationStatus Status { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
        public TimeSpan Duration
        {
            get
            {
                return Finish - Start;
            }
        }


        public string Msg { get; internal set; }
        // Sprak
        public int TasksCount { get; set; }
        public int SuccessTasksCount { get; set; }
        public int FailedTasksCount { get; set; }
        public int KilledTasksCount { get; internal set; }

        // Tez
        public int SubmittedDags { get; internal set; }
        public int SuccessfullDags { get; internal set; }
        public int FailedDags { get; internal set; }
        // Mapred
        public int CompletedMappers { get; internal set; }
        public int SuccessfullMappers { get; internal set; }
        public int FailedMappers { get; internal set; }
        public int KilledMappers { get; internal set; }
        public int CompletedReducers { get; internal set; }
        public int SuccessfullReducers { get; internal set; }
        public int FailedReducers { get; internal set; }
        public int KilledReducers { get; internal set; }
        
        // Core
        public string User { get; set; }
        public string QueueName { get; internal set; }
        
    }
}
