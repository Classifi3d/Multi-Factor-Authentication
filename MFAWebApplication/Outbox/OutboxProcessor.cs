using Microsoft.EntityFrameworkCore;

namespace MFAWebApplication.Outbox;
public class OutboxProcessor
{
    private readonly DbContext _dbContext;
    private readonly IOutboxService _outbox;

}
