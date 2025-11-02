namespace MFAWebApplication.Projections;
public interface IEventProjector
{
    string EventType { get; }
    Task ProjectAsync(byte[] payload, CancellationToken cancellationToken);
}
