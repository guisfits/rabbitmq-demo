using System;

namespace MicroRabbit.SharedKernel.Core.CQRS
{
    public abstract class Message
    {
        protected Message()
        {
            Timestamp = DateTime.Now;
            Type = GetType().Name;
        }

        public DateTime Timestamp { get; }
        public string Type { get; }
    }
}
