using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using UnobservedRooms.Data;
using UnityEngine;

namespace UnobservedRooms.Tests
{
    public sealed class RunValidatorTests
    {
        private RunDefinition LoadValid()
        {
            var asset = Resources.Load<TextAsset>("RunFallbacks/valid_demo");
            Assert.That(asset, Is.Not.Null);
            return JsonConvert.DeserializeObject<RunDefinition>(asset.text);
        }

        [Test]
        public void ValidDemoPasses()
        {
            var result = new RunValidator().Validate(LoadValid());
            Assert.That(result.Errors, Is.Empty, result.Summary);
        }

        [Test]
        public void DisconnectedExitFails()
        {
            var run = LoadValid();
            run.Level.Edges.RemoveAll(edge => edge.ToRoomId == run.Level.ExitRoomId || edge.FromRoomId == run.Level.ExitRoomId);
            var result = new RunValidator().Validate(run);
            Assert.That(result.Errors.Any(error => error.Contains("unreachable")), Is.True);
        }

        [Test]
        public void MissingQuantumEngineFails()
        {
            var run = LoadValid();
            run.Quantum.Engines.RemoveAll(engine => engine.EngineId == "graph-v1");
            var result = new RunValidator().Validate(run);
            Assert.That(result.Errors.Any(error => error.Contains("graph-v1")), Is.True);
        }

        [Test]
        public void DuplicateRoomFails()
        {
            var run = LoadValid();
            run.Level.Rooms.Add(run.Level.Rooms[0]);
            var result = new RunValidator().Validate(run);
            Assert.That(result.Errors.Any(error => error.Contains("Duplicate room")), Is.True);
        }
    }
}
