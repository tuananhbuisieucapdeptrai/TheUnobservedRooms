using System.Collections.Generic;
using UnobservedRooms.Core;
using UnobservedRooms.Data;
using UnobservedRooms.Generation;
using UnobservedRooms.UI;
using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public sealed class SurveyorAgent : MonoBehaviour
    {
        [SerializeField] private float awakeningDelay = 18f;
        [SerializeField] private float moveSpeed = 1.7f;
        [SerializeField] private float captureDistance = 1.05f;
        private LevelDefinition level;
        private LevelAssembler assembler;
        private Transform player;
        private GameStateMachine stateMachine;
        private LevelGraph graph;
        private string currentRoom;
        private Vector3 waypoint;
        private float awakeAt;
        private bool captured;
        private AudioSource drone;

        public void Configure(LevelDefinition definition, LevelAssembler levelAssembler, Transform playerTransform, GameStateMachine state)
        {
            level = definition; assembler = levelAssembler; player = playerTransform; stateMachine = state;
            graph = new LevelGraph(level); currentRoom = level.ExitRoomId;
            transform.position = assembler.GetRoomCenter(currentRoom) + Vector3.up * 1.25f;
            waypoint = transform.position; awakeAt = Time.time + awakeningDelay;
            foreach (var visual in GetComponentsInChildren<Renderer>(true)) visual.enabled = false;
            GetComponent<Light>().enabled = false;
            drone = gameObject.AddComponent<AudioSource>(); drone.loop = true; drone.spatialBlend = .88f; drone.minDistance = 2f; drone.maxDistance = 24f;
            drone.clip = CreateDrone(); drone.volume = 0f;
            Invoke(nameof(Awaken), awakeningDelay);
        }

        private void Awaken()
        {
            foreach (var visual in GetComponentsInChildren<Renderer>(true)) visual.enabled = true;
            GetComponent<Light>().enabled = true;
            drone.Play();
            GameAudio.Play(AudioCue.Threat,transform.position);
            GameHud.ShowNotice("ANOMALY ACTIVE  •  KEEP THE SURVEYOR IN VIEW TO SLOW IT",7f);
            ChooseNextWaypoint();
        }

        private void Update()
        {
            if (captured || player == null || Time.time < awakeAt) return;
            var flatPlayer = new Vector3(player.position.x, transform.position.y, player.position.z);
            var proximity = 1f - Mathf.Clamp01(Vector3.Distance(transform.position,flatPlayer) / 18f);
            if (drone != null) { drone.volume = Mathf.Lerp(.018f,.24f,proximity); drone.pitch = Mathf.Lerp(.78f,1.05f,proximity); }
            if (Vector3.Distance(transform.position, flatPlayer) <= captureDistance)
            {
                captured = true; stateMachine?.TryEnter(GameFlowState.Collapse);
                GameHud.ShowResult("SURVEY COMPLETE\nThe anomaly resolved your position.\n\nPress R to retry.");
                Cursor.lockState = CursorLockMode.None; Cursor.visible = true; return;
            }
            var direction = waypoint - transform.position;
            if (direction.sqrMagnitude < .15f) { transform.position = waypoint; ChooseNextWaypoint(); return; }
            var pressure = Vector3.Distance(transform.position, flatPlayer) < 10f ? 1.08f : 1f;
            if (IsObserved()) pressure *= .22f;
            transform.position += direction.normalized * moveSpeed * pressure * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction.normalized), Time.deltaTime * 4f);
            var pulse = .75f + Mathf.Sin(Time.time * 5f) * .2f;
            transform.localScale = new Vector3(.72f * pulse, 1.25f, .72f * pulse);
        }

        private static AudioClip CreateDrone()
        {
            const int rate = 22050; const int length = rate * 2; var samples = new float[length];
            for (var i = 0; i < length; i++)
            {
                var t = (float)i / rate;
                var pulse = .55f + Mathf.Sin(t * Mathf.PI * 2f * .7f) * .25f;
                samples[i] = (Mathf.Sin(t * Mathf.PI * 2f * 43f) * .12f + Mathf.Sin(t * Mathf.PI * 2f * 67f) * .055f) * pulse;
            }
            var clip = AudioClip.Create("SurveyorDrone",length,1,rate,false); clip.SetData(samples,0); return clip;
        }

        private bool IsObserved()
        {
            var camera = Camera.main; if (camera == null) return false;
            var toSurveyor = transform.position - camera.transform.position;
            if (toSurveyor.sqrMagnitude > 144f || Vector3.Dot(camera.transform.forward,toSurveyor.normalized) < .9f) return false;
            return !Physics.Linecast(camera.transform.position,transform.position,~0,QueryTriggerInteraction.Ignore);
        }

        private void ChooseNextWaypoint()
        {
            var targetRoom = NearestRoom(player.position);
            if (currentRoom == targetRoom) { waypoint = new Vector3(player.position.x, 1.25f, player.position.z); return; }
            var next = NextHop(currentRoom, targetRoom);
            if (!string.IsNullOrEmpty(next)) currentRoom = next;
            waypoint = assembler.GetRoomCenter(currentRoom) + Vector3.up * 1.25f;
        }

        private string NearestRoom(Vector3 position)
        {
            var best = level.StartRoomId; var bestDistance = float.MaxValue;
            foreach (var pair in assembler.RoomCenters)
            {
                var distance = (pair.Value - position).sqrMagnitude;
                if (distance < bestDistance) { bestDistance = distance; best = pair.Key; }
            }
            return best;
        }

        private string NextHop(string start, string goal)
        {
            var queue = new Queue<string>(); var previous = new Dictionary<string, string>();
            queue.Enqueue(start); previous[start] = null;
            while (queue.Count > 0)
            {
                var room = queue.Dequeue(); if (room == goal) break;
                foreach (var edge in graph.EdgesByRoom[room])
                {
                    var next = graph.Other(edge, room); if (previous.ContainsKey(next)) continue;
                    previous[next] = room; queue.Enqueue(next);
                }
            }
            if (!previous.ContainsKey(goal)) return start;
            var step = goal;
            while (previous[step] != null && previous[step] != start) step = previous[step];
            return step;
        }
    }
}
