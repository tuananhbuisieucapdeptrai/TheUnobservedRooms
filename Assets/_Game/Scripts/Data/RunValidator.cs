using System;
using System.Collections.Generic;
using System.Linq;

namespace UnobservedRooms.Data
{
    public sealed class RunValidator
    {
        public const int SupportedSchemaVersion = 1;
        private static readonly HashSet<string> Archetypes = new(StringComparer.OrdinalIgnoreCase)
        { "Anchor", "Corridor", "Office", "Archive", "Utility", "Junction", "CalibrationLab", "Exit" };

        public ValidationResult Validate(RunDefinition run)
        {
            var result = new ValidationResult();
            if (run == null) { result.Error("RunDefinition is null."); return result; }
            if (run.SchemaVersion != SupportedSchemaVersion) result.Error($"Unsupported schemaVersion {run.SchemaVersion}.");
            if (string.IsNullOrWhiteSpace(run.RunId)) result.Error("runId is required.");
            if (run.Level == null) { result.Error("level is required."); return result; }
            run.Level.Rooms ??= new List<RoomDefinition>();
            run.Level.Edges ??= new List<EdgeDefinition>();
            if (run.Level.Rooms.Count is < 2 or > 32) result.Error("rooms count must be between 2 and 32.");

            var roomIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var room in run.Level.Rooms)
            {
                if (room == null || string.IsNullOrWhiteSpace(room.Id)) { result.Error("Every room requires an id."); continue; }
                if (!roomIds.Add(room.Id)) result.Error($"Duplicate room id '{room.Id}'.");
                if (!Archetypes.Contains(room.Archetype ?? string.Empty)) result.Error($"Room '{room.Id}' has unsupported archetype '{room.Archetype}'.");
            }

            if (!roomIds.Contains(run.Level.StartRoomId ?? string.Empty)) result.Error("startRoomId does not reference a room.");
            if (!roomIds.Contains(run.Level.ExitRoomId ?? string.Empty)) result.Error("exitRoomId does not reference a room.");
            if (run.Level.StartRoomId == run.Level.ExitRoomId) result.Error("Start and exit rooms must differ.");

            var edgeIds = new HashSet<string>(StringComparer.Ordinal);
            var degree = roomIds.ToDictionary(id => id, _ => 0, StringComparer.Ordinal);
            var adjacency = roomIds.ToDictionary(id => id, _ => new List<string>(), StringComparer.Ordinal);
            foreach (var edge in run.Level.Edges)
            {
                if (edge == null || string.IsNullOrWhiteSpace(edge.Id)) { result.Error("Every edge requires an id."); continue; }
                if (!edgeIds.Add(edge.Id)) result.Error($"Duplicate edge id '{edge.Id}'.");
                if (!roomIds.Contains(edge.FromRoomId ?? string.Empty) || !roomIds.Contains(edge.ToRoomId ?? string.Empty))
                { result.Error($"Edge '{edge.Id}' references a missing room."); continue; }
                if (edge.FromRoomId == edge.ToRoomId) { result.Error($"Edge '{edge.Id}' is a self-loop."); continue; }
                degree[edge.FromRoomId]++; degree[edge.ToRoomId]++;
                adjacency[edge.FromRoomId].Add(edge.ToRoomId); adjacency[edge.ToRoomId].Add(edge.FromRoomId);
                if (edge.ObservationCost is < 0 or > 100) result.Error($"Edge '{edge.Id}' has invalid observationCost.");
            }
            foreach (var pair in degree.Where(pair => pair.Value > 4)) result.Error($"Room '{pair.Key}' degree {pair.Value} exceeds four sockets.");

            if (roomIds.Contains(run.Level.StartRoomId ?? string.Empty))
            {
                var distances = Distances(run.Level.StartRoomId, adjacency);
                if (!distances.ContainsKey(run.Level.ExitRoomId ?? string.Empty)) result.Error("Exit is unreachable from start.");
                else if (run.Level.Rooms.Count >= 8 && distances[run.Level.ExitRoomId] < 5) result.Warn("Exit graph distance is below the preferred five-room minimum.");
            }

            ValidateEntanglement(run, roomIds, result);
            ValidateEvents(run, roomIds, result);
            ValidateQuantum(run, result);
            return result;
        }

        private static void ValidateEntanglement(RunDefinition run, HashSet<string> roomIds, ValidationResult result)
        {
            run.Entanglement ??= new EntanglementDefinition();
            run.Entanglement.Pairs ??= new List<EntangledPairDefinition>();
            run.Entanglement.TargetPattern ??= new List<PairTargetDefinition>();
            var pairIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var pair in run.Entanglement.Pairs)
            {
                if (pair == null || string.IsNullOrWhiteSpace(pair.Id)) { result.Error("Every entangled pair requires an id."); continue; }
                if (!pairIds.Add(pair.Id)) result.Error($"Duplicate pair id '{pair.Id}'.");
                if (!roomIds.Contains(pair.SwitchRoomA ?? string.Empty) || !roomIds.Contains(pair.SwitchRoomB ?? string.Empty)) result.Error($"Pair '{pair.Id}' references a missing switch room.");
                if (!Bit(pair.InitialA) || !Bit(pair.InitialB)) result.Error($"Pair '{pair.Id}' initial states must be binary.");
                if (!string.Equals(pair.Correlation, "correlated", StringComparison.OrdinalIgnoreCase) && !string.Equals(pair.Correlation, "anti", StringComparison.OrdinalIgnoreCase)) result.Error($"Pair '{pair.Id}' has unsupported correlation.");
            }
            foreach (var target in run.Entanglement.TargetPattern)
            {
                if (!pairIds.Contains(target.PairId ?? string.Empty)) result.Error($"Target references missing pair '{target.PairId}'.");
                if (!Bit(target.A) || !Bit(target.B)) result.Error($"Target for '{target.PairId}' must be binary.");
            }
        }

        private static void ValidateEvents(RunDefinition run, HashSet<string> roomIds, ValidationResult result)
        {
            run.Events ??= new RunEvents();
            run.Events.ShardRooms ??= new List<string>();
            foreach (var room in run.Events.ShardRooms.Where(room => !roomIds.Contains(room))) result.Error($"Shard references missing room '{room}'.");
        }

        private static void ValidateQuantum(RunDefinition run, ValidationResult result)
        {
            run.Quantum ??= new QuantumProvenance();
            run.Quantum.Engines ??= new List<EngineProvenance>();
            var ids = run.Quantum.Engines.Select(e => e?.EngineId).Where(id => !string.IsNullOrWhiteSpace(id)).ToHashSet(StringComparer.Ordinal);
            foreach (var required in new[] { "labyrinth-v1", "graph-v1", "comet-qrng-v1" })
                if (!ids.Contains(required)) result.Error($"Required engine provenance '{required}' is missing.");
        }

        private static bool Bit(int value) => value is 0 or 1;

        private static Dictionary<string, int> Distances(string start, Dictionary<string, List<string>> adjacency)
        {
            var distances = new Dictionary<string, int>(StringComparer.Ordinal) { [start] = 0 };
            var queue = new Queue<string>(); queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var next in adjacency[current]) if (!distances.ContainsKey(next))
                { distances[next] = distances[current] + 1; queue.Enqueue(next); }
            }
            return distances;
        }
    }
}
