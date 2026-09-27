using System;
using System.Collections;
using Newtonsoft.Json;
using UnobservedRooms.Data;
using UnityEngine;

namespace UnobservedRooms.Quantum
{
    public sealed class EmbeddedRunProvider : IRunProvider
    {
        private readonly string resourcePath;
        public EmbeddedRunProvider(string resourcePath = "RunFallbacks/valid_demo") => this.resourcePath = resourcePath;

        public IEnumerator Load(string requestId, Action<RunLoadResult> completed)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null) completed(RunLoadResult.Fail($"Missing Resources/{resourcePath}.json"));
            else
            {
                try { completed(RunLoadResult.Ok(JsonConvert.DeserializeObject<RunDefinition>(asset.text))); }
                catch (Exception exception) { completed(RunLoadResult.Fail($"Fallback JSON failed to parse: {exception.Message}")); }
            }
            yield break;
        }
    }
}
