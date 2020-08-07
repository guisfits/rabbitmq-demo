using System;
using MediatR;

namespace MicroRabbit.SharedKernel.Core.CQRS
{
    public abstract class Command : Message, IRequest
    {
        protected Command() : base()
        {
            CommandId = Guid.NewGuid();
        }

        public Guid CommandId { get; }
    }
}
