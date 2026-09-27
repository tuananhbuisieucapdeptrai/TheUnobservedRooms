using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using UnobservedRooms.Data;
using UnityEngine.Networking;

namespace UnobservedRooms.Quantum
{
    public sealed class BackendRunProvider : IRunProvider
    {
        private readonly string endpoint;
        private readonly int timeoutSeconds;

        public BackendRunProvider(string endpoint, int timeoutSeconds = 20)
        {
            this.endpoint = endpoint?.TrimEnd('/');
            this.timeoutSeconds = timeoutSeconds;
        }

        public IEnumerator Load(string requestId, Action<RunLoadResult> completed)
        {
            if (string.IsNullOrWhiteSpace(endpoint)) { completed(RunLoadResult.Fail("Backend endpoint is empty.")); yield break; }
            var payload = JsonConvert.SerializeObject(new
            {
                schemaVersion = 1,
                clientBuild = "jam-1.0.0",
                requestId,
                preset = "jam-standard",
                executionPreference = "emulator"
            });
            using var request = new UnityWebRequest(endpoint + "/v1/runs", "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = timeoutSeconds;
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            { completed(RunLoadResult.Fail($"Backend request failed ({request.responseCode}): {request.error}")); yield break; }
            try { completed(RunLoadResult.Ok(JsonConvert.DeserializeObject<RunDefinition>(request.downloadHandler.text))); }
            catch (Exception exception) { completed(RunLoadResult.Fail($"Backend JSON failed to parse: {exception.Message}")); }
        }
    }
}
