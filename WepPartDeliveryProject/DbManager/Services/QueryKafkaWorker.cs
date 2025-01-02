using Confluent.Kafka;
using System.Collections.Concurrent;

namespace DbManager.Services
{
    public abstract class QueryKafkaWorker : IDisposable
    {
        protected ConcurrentQueue<ConsumeResult<string, string>> _queue = new ConcurrentQueue<ConsumeResult<string, string>>();
        protected Thread _workThread;
        protected readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        protected QueryKafkaWorker()
        {
            this._workThread = new Thread(this.WorkFunction) { Name = this.GetType().Name };
            this._workThread.Start();
        }

        protected abstract void WorkFunction();

        public virtual void AddToQueue(ConsumeResult<string, string> consumeResult)
        {
            this._queue.Enqueue(consumeResult);
        }

        public virtual void Dispose()
        {
            if (this._workThread != null)
            {
                this._cancellationTokenSource.Cancel();

                this._workThread.Interrupt();
                this._workThread.Join();
                this._workThread = null;

                this._cancellationTokenSource.Dispose();
            }
        }
    }
}
