using System.Threading.Tasks;

namespace Twia.StateMachine.Example;

public interface IAsyncLightSensor
{
    Task<decimal> GetSensorValueAsync();
}