using System;
using System.Collections;
using UnobservedRooms.Data;
using UnityEngine;

namespace UnobservedRooms.Quantum
{
    public sealed class RunService : MonoBehaviour
    {
        [SerializeField] private bool useBackend;
        [SerializeField] private string backendBaseUrl = "";
        [SerializeField] private string fallbackResource = "RunFallbacks/valid_demo";

        public RunDefinition Current { get; private set; }
        public ValidationResult LastValidation { get; private set; }
        public event Action<RunDefinition> Ready;
        public event Action<string> Failed;

        private IRunProvider primary;
        private IRunProvider fallback;
        private readonly RunValidator validator = new();

        private void Awake()
        {
            fallback = new EmbeddedRunProvider(fallbackResource);
            primary = useBackend ? new BackendRunProvider(backendBaseUrl) : fallback;
        }

        public void GenerateRun() => StartCoroutine(LoadWithFallback(Guid.NewGuid().ToString("D")));

        private IEnumerator LoadWithFallback(string requestId)
        {
            RunLoadResult loaded = null;
            yield return primary.Load(requestId, value => loaded = value);
            if (!Accept(loaded, out var reason) && !ReferenceEquals(primary, fallback))
            {
                Debug.LogWarning($"Primary run provider failed: {reason}. Loading embedded fallback.");
                loaded = null;
                yield return fallback.Load(requestId, value => loaded = value);
                if (loaded?.Run != null)
                {
                    loaded.Run.Source = "offline";
                    loaded.Run.FallbackReason = reason;
                }
            }
            if (!Accept(loaded, out reason)) { Failed?.Invoke(reason); yield break; }
            Current = loaded.Run;
            Ready?.Invoke(Current);
        }

        private bool Accept(RunLoadResult loaded, out string reason)
        {
            if (loaded == null || !loaded.Success || loaded.Run == null)
            { reason = loaded?.Error ?? "Run provider returned no data."; return false; }
            LastValidation = validator.Validate(loaded.Run);
            if (!LastValidation.IsValid) { reason = LastValidation.Summary; return false; }
            reason = null; return true;
        }
    }
}
