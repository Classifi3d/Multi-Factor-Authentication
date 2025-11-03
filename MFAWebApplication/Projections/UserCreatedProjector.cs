using AutoMapper;
using MessagePack;
using MFAWebApplication.Abstraction.Repository;
using MFAWebApplication.Enteties;

namespace MFAWebApplication.Projections;
public class UserCreatedProjector : IEventProjector
{
    public string EventType => nameof(UserCreatedEvent);

    private readonly IReadModelRepository<UserReadModel> _repo;
    private readonly IMapper _mapper;

    public UserCreatedProjector(IReadModelRepository<UserReadModel> repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task ProjectAsync(byte[] payload, CancellationToken cancellationToken)
    {
        var evt = MessagePackSerializer.Deserialize<UserCreatedEvent>(payload);
        if (evt == null) return;

        var readModel = new UserReadModel
        {
            Id = evt.Id,
            Email = evt.Email,
            Username = evt.Username,
            Password = evt.Password,
            IsMfaEnabled = evt.IsMfaEnabled,
            ConcurrencyIndex = evt.ConcurrencyIndex
        };

        await _repo.UpsertIfNewerConcurrencyAsync(readModel, cancellationToken);
    }
}
