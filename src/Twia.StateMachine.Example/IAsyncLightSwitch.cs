using System.Threading.Tasks;

namespace Twia.StateMachine.Example;

public interface IAsyncLightSwitch
{
    Task ToOnAsync();

    Task ToOffAsync();

    Task DimAsync(int percentage);
}