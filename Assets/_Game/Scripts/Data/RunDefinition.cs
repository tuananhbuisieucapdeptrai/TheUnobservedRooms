using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnobservedRooms.Data
{
    [Serializable]
    public sealed class RunDefinition
    {
        [JsonProperty("schemaVersion")] public int SchemaVersion;
        [JsonProperty("runId")] public string RunId;
        [JsonProperty("source")] public string Source;
        [JsonProperty("fallbackReason")] public string FallbackReason;
        [JsonProperty("createdAt")] public string CreatedAt;
        [JsonProperty("contentSeed")] public int ContentSeed;
        [JsonProperty("level")] public LevelDefinition Level;
        [JsonProperty("entanglement")] public EntanglementDefinition Entanglement;
        [JsonProperty("events")] public RunEvents Events;
        [JsonProperty("quantum")] public QuantumProvenance Quantum;
    }

    [Serializable]
    public sealed class LevelDefinition
    {
        [JsonProperty("startRoomId")] public string StartRoomId;
        [JsonProperty("exitRoomId")] public string ExitRoomId;
        [JsonProperty("rooms")] public List<RoomDefinition> Rooms = new();
        [JsonProperty("edges")] public List<EdgeDefinition> Edges = new();
    }

    [Serializable]
    public sealed class RoomDefinition
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("archetype")] public string Archetype;
        [JsonProperty("critical")] public bool Critical;
        [JsonProperty("hazard")] public string Hazard;
        [JsonProperty("reward")] public string Reward;
        [JsonProperty("variantSeed")] public int VariantSeed;
    }

    [Serializable]
    public sealed class EdgeDefinition
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("fromRoomId")] public string FromRoomId;
        [JsonProperty("toRoomId")] public string ToRoomId;
        [JsonProperty("fromSocket")] public int FromSocket;
        [JsonProperty("toSocket")] public int ToSocket;
        [JsonProperty("initialStability")] public string InitialStability;
        [JsonProperty("observationCost")] public int ObservationCost = 12;
    }

    [Serializable]
    public sealed class EntanglementDefinition
    {
        [JsonProperty("pairs")] public List<EntangledPairDefinition> Pairs = new();
        [JsonProperty("targetPattern")] public List<PairTargetDefinition> TargetPattern = new();
    }

    [Serializable]
    public sealed class EntangledPairDefinition
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("switchRoomA")] public string SwitchRoomA;
        [JsonProperty("switchRoomB")] public string SwitchRoomB;
        [JsonProperty("initialA")] public int InitialA;
        [JsonProperty("initialB")] public int InitialB;
        [JsonProperty("correlation")] public string Correlation;
        [JsonProperty("lockId")] public string LockId;
    }

    [Serializable]
    public sealed class PairTargetDefinition
    {
        [JsonProperty("pairId")] public string PairId;
        [JsonProperty("a")] public int A;
        [JsonProperty("b")] public int B;
    }

    [Serializable]
    public sealed class RunEvents
    {
        [JsonProperty("shardRooms")] public List<string> ShardRooms = new();
        [JsonProperty("anomalyBias")] public List<int> AnomalyBias = new();
        [JsonProperty("lightingVariants")] public List<int> LightingVariants = new();
    }

    [Serializable]
    public sealed class QuantumProvenance
    {
        [JsonProperty("engines")] public List<EngineProvenance> Engines = new();
        [JsonProperty("payloadHash")] public string PayloadHash;
    }

    [Serializable]
    public sealed class EngineProvenance
    {
        [JsonProperty("engineId")] public string EngineId;
        [JsonProperty("jobIdShort")] public string JobIdShort;
        [JsonProperty("executionMode")] public string ExecutionMode;
        [JsonProperty("generatedAt")] public string GeneratedAt;
        [JsonProperty("status")] public string Status;
        [JsonProperty("statement")] public string Statement;
    }
}
