using System;
using System.Threading;

namespace Hangfire.Jobs
{
    public class InsideJob
    {
        public void DoJob(int sleep)
        {
            Thread.Sleep(sleep);

            Console.WriteLine("this is the job");
        }
    }
}
