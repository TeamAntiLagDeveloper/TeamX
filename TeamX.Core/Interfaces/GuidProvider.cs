using TeamX.Core.Interfaces;

namespace TeamX.Core.Services;

public sealed class GuidProvider : IGuidProvider
{
    public Guid NewGuid() => Guid.NewGuid();
}