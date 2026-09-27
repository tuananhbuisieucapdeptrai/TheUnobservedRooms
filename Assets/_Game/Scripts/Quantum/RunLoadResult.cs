using UnobservedRooms.Data;

namespace UnobservedRooms.Quantum
{
    public sealed class RunLoadResult
    {
        public bool Success;
        public string Error;
        public RunDefinition Run;

        public static RunLoadResult Ok(RunDefinition run) => new() { Success = true, Run = run };
        public static RunLoadResult Fail(string error) => new() { Success = false, Error = error };
    }
}
