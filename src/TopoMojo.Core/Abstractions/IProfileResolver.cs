using TopoMojo.Core;

namespace TopoMojo.Abstractions
{
    public interface IProfileResolver
    {
        Person Profile { get; }
    }
}