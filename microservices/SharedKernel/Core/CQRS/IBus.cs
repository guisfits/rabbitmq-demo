using System.Threading.Tasks;

namespace MicroRabbit.SharedKernel.Core.CQRS
{
    public interface IBus
    {
        Task SendCommand<T>(T command) where T : Command;
        void Publish<T>(T @event) where T : Event;
        void Subscribe<T, TH>() where T : Event where TH : IEventHandler<T>;
    }
}
