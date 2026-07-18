using TeamX.Core.Interfaces;

namespace TeamX.Core.Services;

public class GuidProvider : IGuidProvider
{
    public Guid NewGuid()
    {
        return Guid.NewGuid();
    }
}