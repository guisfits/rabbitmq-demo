using System;

namespace MicroRabbit.SharedKernel.Core.CQRS
{
    public abstract class Event : Message
    {
        protected Event()
        {
            EventId = Guid.NewGuid();
        }

        public Guid EventId { get; }
    }
}
