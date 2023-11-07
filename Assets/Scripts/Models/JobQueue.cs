using System.Collections.Generic;
using System;

public class JobQueue
{
    // TODO: Most likely this will be replaced
    // with a dedicated class for managing job queues
    // that might also be semi static or self initializing or something.
    // For now this is just a public member of world
    Queue<Job> jobQueue;

    Action<Job> cbJobCreated;

    public JobQueue()
    {
        jobQueue = new Queue<Job>();
    }

    public void Enqueue(Job j)
    {
        if (j.jobTime < 0)
        {
            // job has a negative job time, so its not actually supposed to be queued up. just instacomplete it.

            j.DoWork(0);
            return;
        }

        jobQueue.Enqueue(j);
        // TODO
        if (cbJobCreated != null)
        {
            cbJobCreated(j);
        }
    }

    public Job Dequeue()
    {
        if (jobQueue.Count == 0)
            return null;

        return jobQueue.Dequeue();
    }

    public void RegisterJobCreationCallback(Action<Job> cb)
    {
        cbJobCreated += cb;
    }

    public void UnregisterJobCreationCallback(Action<Job> cb)
    {
        cbJobCreated -= cb;
    }

    public void Remove(Job j)
    {
        // TODO: check docs to see if theres a less meemory/swappy solution
        List<Job> jobs = new List<Job> (jobQueue);

        if (jobs.Contains(j) == false)
        {
            //Debug.LogError("Trying to remove a job that doesnt exist on the queue.");
            // Most likely this job wasnt on the queue because a character was working it.
            return;
        }

        jobs.Remove(j);
        jobQueue = new Queue<Job>(jobs);
    }
}
