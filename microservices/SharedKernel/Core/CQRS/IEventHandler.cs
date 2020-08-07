using System.Threading.Tasks;

namespace MicroRabbit.SharedKernel.Core.CQRS
{
    public interface IEventHandler<in TEvent> : IEventHandler
        where TEvent : Event
    {
        Task Handle(TEvent @event);
    }

    public interface IEventHandler
    {
        
    }
}
