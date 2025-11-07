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
        var userEvent = MessagePackSerializer.Deserialize<UserCreatedEvent>(payload);
        if (userEvent == null) return;

        var readModel = new UserReadModel
        {
            Id = userEvent.Id.ToString(),
            Email = userEvent.Email,
            Username = userEvent.Username,
            Password = userEvent.Password,
            IsMfaEnabled = userEvent.IsMfaEnabled,
            ConcurrencyIndex = userEvent.ConcurrencyIndex
        };
        //readModel = _mapper.Map<UserReadModel>(userEvent);

        await _repo.UpsertIfNewerConcurrencyAsync(readModel, cancellationToken);
    }
}
