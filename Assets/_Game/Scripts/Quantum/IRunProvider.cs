using System.Collections;

namespace UnobservedRooms.Quantum
{
    public interface IRunProvider
    {
        IEnumerator Load(string requestId, System.Action<RunLoadResult> completed);
    }
}
