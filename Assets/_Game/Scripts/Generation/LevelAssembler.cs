using System.Collections.Generic;
using UnobservedRooms.Data;
using UnobservedRooms.Gameplay;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnobservedRooms.Generation
{
    public sealed class LevelAssembler : MonoBehaviour
    {
        [SerializeField] private float gridSpacing = 14f;
        public IReadOnlyDictionary<string, GameObject> RoomObjects => roomObjects;
        public IReadOnlyDictionary<string, Vector3> RoomCenters => roomCenters;
        private readonly Dictionary<string, GameObject> roomObjects = new();
        private readonly Dictionary<string, Vector3> roomCenters = new();
        private Transform connectionRoot;

        public Vector3 GetRoomCenter(string roomId) => roomCenters.TryGetValue(roomId, out var value) ? value : Vector3.zero;

        public bool Assemble(RunDefinition run, out string error)
        {
            Clear();
            if (run?.Level == null) { error = "Run has no level."; return false; }
            var positions = Embed(run.Level);
            if (positions.Count != run.Level.Rooms.Count) { error = "Could not embed every room."; Clear(); return false; }
            foreach (var room in run.Level.Rooms)
            {
                var root = new GameObject($"Room_{room.Id}_{room.Archetype}");
                root.transform.SetParent(transform, false);
                root.transform.localPosition = positions[room.Id];
                CreateGrayboxRoom(root.transform, room);
                roomObjects.Add(room.Id, root);
                roomCenters.Add(room.Id, root.transform.position);
            }
            connectionRoot = new GameObject("Connections").transform;
            connectionRoot.SetParent(transform, false);
            foreach (var edge in run.Level.Edges)
                CreateConnection(edge, positions[edge.FromRoomId], positions[edge.ToRoomId]);
            error = null; return true;
        }

        private Dictionary<string, Vector3> Embed(LevelDefinition level)
        {
            var graph = new LevelGraph(level);
            var result = new Dictionary<string, Vector3> { [level.StartRoomId] = Vector3.zero };
            var occupied = new HashSet<Vector2Int> { Vector2Int.zero };
            var cells = new Dictionary<string, Vector2Int> { [level.StartRoomId] = Vector2Int.zero };
            var queue = new Queue<string>(); queue.Enqueue(level.StartRoomId);
            var directions = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            while (queue.Count > 0)
            {
                var current = queue.Dequeue(); var directionIndex = 0;
                foreach (var edge in graph.EdgesByRoom[current])
                {
                    var next = graph.Other(edge, current); if (cells.ContainsKey(next)) continue;
                    Vector2Int selected = default; var found = false;
                    for (var attempt = 0; attempt < directions.Length; attempt++)
                    {
                        var candidate = cells[current] + directions[(directionIndex + attempt) % directions.Length];
                        if (occupied.Add(candidate)) { selected = candidate; found = true; directionIndex += attempt + 1; break; }
                    }
                    if (!found) continue;
                    cells[next] = selected; result[next] = new Vector3(selected.x * gridSpacing, 0f, selected.y * gridSpacing); queue.Enqueue(next);
                }
            }
            return result;
        }

        private static void CreateGrayboxRoom(Transform root, RoomDefinition room)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor"; floor.transform.SetParent(root, false); floor.transform.localScale = new Vector3(10f, .2f, 10f);
            floor.transform.localPosition = new Vector3(0f, -.1f, 0f);
            var floorTexture = Resources.Load<Texture2D>("Art/quantum_terrazzo_floor_v1");
            ApplyColor(floor, room.Archetype == "Exit" ? new Color(.16f,.28f,.3f) : new Color(.42f,.46f,.46f), .28f, Color.black, floorTexture, new Vector2(3.2f,3.2f));
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = room.Critical ? "CriticalMarker" : "RoomMarker"; marker.transform.SetParent(root, false);
            marker.transform.localScale = new Vector3(.35f, .05f, .35f); marker.transform.localPosition = new Vector3(0f, .05f, 0f);
            ApplyColor(marker, room.Critical ? new Color(.2f,.8f,.85f) : new Color(.4f,.45f,.45f));
            foreach (var x in new[] { -3.25f, 3.25f })
            {
                CreateWall(root, new Vector3(x, 1.5f, 4.85f), new Vector3(3.5f, 3, .3f));
                CreateWall(root, new Vector3(x, 1.5f, -4.85f), new Vector3(3.5f, 3, .3f));
            }
            foreach (var z in new[] { -3.25f, 3.25f })
            {
                CreateWall(root, new Vector3(4.85f, 1.5f, z), new Vector3(.3f, 3, 3.5f));
                CreateWall(root, new Vector3(-4.85f, 1.5f, z), new Vector3(.3f, 3, 3.5f));
            }
            CreateCeilingLight(root, room.VariantSeed);
            CreateArchitecturalTrim(root, room);
            CreateSetDressing(root, room);
            if (room.Archetype != "Anchor" && room.Archetype != "Exit") CreateMazePartitions(root, room.VariantSeed);
            root.gameObject.AddComponent<RoomAtmosphere>().Configure(room.Hazard, room.VariantSeed);
            CreateQuantumMotes(root, new Vector3(8f,.25f,8f), new Color(.08f,.28f,.32f,.35f), 3.5f);
        }

        private void CreateConnection(EdgeDefinition edge, Vector3 from, Vector3 to)
        {
            var delta = to - from;
            var center = (from + to) * .5f;
            var horizontal = Mathf.Abs(delta.x) > Mathf.Abs(delta.z);
            var corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corridor.name = "Corridor_" + edge.Id; corridor.transform.SetParent(connectionRoot, false);
            corridor.transform.position = center + Vector3.down * .1f;
            corridor.transform.localScale = horizontal ? new Vector3(Mathf.Abs(delta.x), .2f, 3.2f) : new Vector3(3.2f, .2f, Mathf.Abs(delta.z));
            var floorTexture = Resources.Load<Texture2D>("Art/quantum_terrazzo_floor_v1");
            ApplyColor(corridor, new Color(.38f,.43f,.44f), .24f, Color.black, floorTexture,
                horizontal ? new Vector2(Mathf.Max(2f, Mathf.Abs(delta.x) / 4f),1.2f) : new Vector2(1.2f,Mathf.Max(2f, Mathf.Abs(delta.z) / 4f)));
            CreateGuideStrip(center + Vector3.up * .025f, horizontal, Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.z)));

            var gate = new GameObject("Threshold_" + edge.Id);
            gate.name = "Threshold_" + edge.Id; gate.transform.SetParent(connectionRoot, false);
            gate.transform.position = center + Vector3.up * 1.45f;
            var collider = gate.AddComponent<BoxCollider>(); collider.size = horizontal ? new Vector3(.25f,2.9f,3f) : new Vector3(3f,2.9f,.25f);
            var veil = ProceduralAssetFactory.Veil("QuantumVeil",gate.transform,3f,2.9f); if (horizontal) veil.transform.localRotation = Quaternion.Euler(0,90,0);
            var instrument = Resources.Load<Texture2D>("Art/quantum_instrument_surface_v1");
            ApplyColor(veil,new Color(.06f,.62f,.78f),.68f,new Color(0f,2.8f,3.6f),instrument,new Vector2(1.5f,1.5f));
            veil.AddComponent<VisualPulse>().Configure(2.2f, 1.15f);
            CreateQuantumMotes(gate.transform, horizontal ? new Vector3(.35f,2.7f,2.8f) : new Vector3(2.8f,2.7f,.35f), new Color(.05f,.8f,1f), 24f);
            CreateGateFrame(gate.transform, horizontal);
            gate.AddComponent<ObservationGate>().Configure(edge.Id, edge.ObservationCost);
        }

        private static void CreateWall(Transform parent, Vector3 position, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = "Wall"; wall.transform.SetParent(parent, false);
            wall.transform.localPosition = position; wall.transform.localScale = scale;
            var texture = Resources.Load<Texture2D>("Art/liminal_wall_ivory_v1");
            ApplyColor(wall, new Color(.82f,.80f,.70f), .55f, Color.black, texture, new Vector2(Mathf.Max(1f, scale.x / 3f), Mathf.Max(1f, scale.y / 2f)));
        }

        private static void CreateCeilingLight(Transform parent, int seed)
        {
            var palettes = new[] { new Color(1f,.93f,.76f), new Color(1f,.58f,.22f), new Color(1f,.18f,.12f), new Color(.28f,.8f,1f) };
            var lightColor = palettes[Mathf.Abs(seed) % palettes.Length];
            var panel = GameObject.CreatePrimitive(PrimitiveType.Cube); panel.name = "CeilingLight"; panel.transform.SetParent(parent, false);
            panel.transform.localPosition = new Vector3(0, 2.85f, 0); panel.transform.localScale = new Vector3(2.6f,.08f,.65f);
            ApplyColor(panel, lightColor, .05f, lightColor * 4.2f);
            panel.AddComponent<VisualPulse>().Configure(.7f + (Mathf.Abs(seed) % 3) * .25f, .12f);
            var lightObject = new GameObject("RoomLight"); lightObject.transform.SetParent(parent, false); lightObject.transform.localPosition = new Vector3(0,2.45f,0);
            var light = lightObject.AddComponent<Light>(); light.type = LightType.Point; light.range = 10.5f; light.intensity = 2.7f;
            light.color = lightColor; light.shadows = LightShadows.Soft;
        }

        private static void CreateArchitecturalTrim(Transform root, RoomDefinition room)
        {
            var accent = room.Critical ? new Color(.04f,.85f,1f) : new Color(.72f,.48f,.16f);
            foreach (var x in new[] { -4.55f, 4.55f })
            foreach (var z in new[] { -4.55f, 4.55f })
            {
                var column = GameObject.CreatePrimitive(PrimitiveType.Cube); column.name = "CornerPillar"; column.transform.SetParent(root, false);
                column.transform.localPosition = new Vector3(x,1.45f,z); column.transform.localScale = new Vector3(.28f,2.9f,.28f);
                ApplyColor(column, new Color(.12f,.16f,.17f), .62f, accent * .45f);
            }
            var sign = new GameObject("RoomSign"); sign.transform.SetParent(root, false); sign.transform.localPosition = new Vector3(0,1.75f,4.66f); sign.transform.localRotation = Quaternion.Euler(0,180,0);
            var text = sign.AddComponent<TextMesh>(); text.text = $"{room.Archetype.ToUpper()}  //  {room.Id.ToUpper()}"; text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center; text.fontSize = 54; text.characterSize = .055f; text.color = accent;
        }

        private static void CreateSetDressing(Transform root, RoomDefinition room)
        {
            var side = room.VariantSeed % 2 == 0 ? -1f : 1f;
            var console = GameObject.CreatePrimitive(PrimitiveType.Cube); console.name = "QuantumConsole"; console.transform.SetParent(root, false);
            console.transform.localPosition = new Vector3(3.65f * side,.65f,-2.7f); console.transform.localScale = new Vector3(1.15f,1.3f,.55f);
            ApplyColor(console, new Color(.09f,.12f,.13f), .72f);
            var screen = GameObject.CreatePrimitive(PrimitiveType.Cube); screen.name = "ConsoleDisplay"; screen.transform.SetParent(console.transform, false);
            screen.transform.localPosition = new Vector3(0,.12f,-.52f); screen.transform.localScale = new Vector3(.72f,.38f,.035f);
            var glow = room.Critical ? new Color(.04f,.9f,1f) : new Color(1f,.52f,.12f);
            ApplyColor(screen, glow, .1f, glow * 3f); screen.AddComponent<VisualPulse>().Configure(1.6f,.35f);

            for (var i = -1; i <= 1; i += 2)
            {
                var inlay = GameObject.CreatePrimitive(PrimitiveType.Cube); inlay.name = "FloorInlay"; inlay.transform.SetParent(root, false);
                inlay.transform.localPosition = new Vector3(i * 2.5f,.018f,2.8f); inlay.transform.localScale = new Vector3(1.6f,.025f,.045f);
                ApplyColor(inlay, glow,.2f,glow * 1.8f); Object.Destroy(inlay.GetComponent<Collider>());
            }
        }

        private static void CreateGateFrame(Transform gate, bool horizontal)
        {
            var color = new Color(.03f,.85f,1f);
            for (var i = -1; i <= 1; i += 2)
            {
                var post = GameObject.CreatePrimitive(PrimitiveType.Cube); post.name = "ThresholdFrame"; post.transform.SetParent(gate, false);
                post.transform.localPosition = horizontal ? new Vector3(0,0,i * 1.68f) : new Vector3(i * 1.68f,0,0);
                post.transform.localScale = horizontal ? new Vector3(.52f,3.14f,.19f) : new Vector3(.19f,3.14f,.52f);
                ApplyColor(post,new Color(.04f,.09f,.11f),.75f,color * 2.4f); Object.Destroy(post.GetComponent<Collider>());
            }
            var scanner = GameObject.CreatePrimitive(PrimitiveType.Cylinder); scanner.name = "RotatingScanner"; scanner.transform.SetParent(gate, false);
            scanner.transform.localPosition = Vector3.zero; scanner.transform.localScale = new Vector3(.22f,.035f,.22f);
            scanner.transform.localRotation = horizontal ? Quaternion.Euler(0,0,90) : Quaternion.Euler(90,0,0);
            ApplyColor(scanner,color,.8f,color * 4f); Object.Destroy(scanner.GetComponent<Collider>());
            scanner.AddComponent<SpinOrbit>().Configure(horizontal ? Vector3.right : Vector3.forward, 120f);
        }

        private static void CreateMazePartitions(Transform root, int seed)
        {
            var pattern = Mathf.Abs(seed) % 3;
            if (pattern == 0)
            {
                CreateInteriorWall(root,new Vector3(-1.75f,1.3f,-1.15f),new Vector3(.22f,2.6f,4.8f));
                CreateInteriorWall(root,new Vector3(1.75f,1.3f,1.15f),new Vector3(.22f,2.6f,4.8f));
            }
            else if (pattern == 1)
            {
                CreateInteriorWall(root,new Vector3(-1.15f,1.3f,1.75f),new Vector3(4.8f,2.6f,.22f));
                CreateInteriorWall(root,new Vector3(1.15f,1.3f,-1.75f),new Vector3(4.8f,2.6f,.22f));
            }
            else
            {
                CreateInteriorWall(root,new Vector3(-2.25f,1.3f,0),new Vector3(.22f,2.6f,3.4f));
                CreateInteriorWall(root,new Vector3(2.25f,1.3f,0),new Vector3(.22f,2.6f,3.4f));
                CreateInteriorWall(root,new Vector3(0,1.3f,2.25f),new Vector3(3.4f,2.6f,.22f));
            }
        }

        private static void CreateInteriorWall(Transform root, Vector3 position, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = "MazePartition"; wall.transform.SetParent(root,false);
            wall.transform.localPosition = position; wall.transform.localScale = scale;
            var texture = Resources.Load<Texture2D>("Art/liminal_wall_ivory_v1");
            ApplyColor(wall,new Color(.32f,.36f,.35f),.42f,Color.black,texture,new Vector2(Mathf.Max(1f,scale.x/2f),Mathf.Max(1f,scale.y/2f)));
            var cap = GameObject.CreatePrimitive(PrimitiveType.Cube); cap.name = "PartitionSignal"; cap.transform.SetParent(wall.transform,false);
            cap.transform.localPosition = new Vector3(0,.49f,0); cap.transform.localScale = new Vector3(1.03f,.025f,1.03f);
            var glow = new Color(.04f,.65f,.82f); ApplyColor(cap,glow,.15f,glow * 2.2f); Object.Destroy(cap.GetComponent<Collider>());
        }

        private void CreateGuideStrip(Vector3 center, bool horizontal, float length)
        {
            var strip = GameObject.CreatePrimitive(PrimitiveType.Cube); strip.name = "GuidanceStrip"; strip.transform.SetParent(connectionRoot, false);
            strip.transform.position = center;
            strip.transform.localScale = horizontal ? new Vector3(length,.035f,.09f) : new Vector3(.09f,.035f,length);
            ApplyColor(strip, new Color(.05f,.7f,.85f), .2f, new Color(0f,2.6f,3.2f));
            strip.AddComponent<VisualPulse>().Configure(2.6f,.55f);
            Object.Destroy(strip.GetComponent<Collider>());
        }

        public static void CreateQuantumMotes(Transform parent, Vector3 box, Color color, float rate)
        {
            var motes = new GameObject("QuantumMotes"); motes.transform.SetParent(parent, false);
            var particles = motes.AddComponent<ParticleSystem>();
            var main = particles.main; main.loop = true; main.startLifetime = 1.7f; main.startSpeed = .18f; main.startSize = .055f; main.maxParticles = 100; main.startColor = color;
            var emission = particles.emission; emission.rateOverTime = rate;
            var shape = particles.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = box;
            var noise = particles.noise; noise.enabled = true; noise.strength = .18f; noise.frequency = .7f;
            var renderer = motes.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Unlit/Color");
            renderer.material = new Material(shader); renderer.material.color = color;
        }

        public static void ApplyColor(GameObject target, Color color, float smoothness = .35f, Color? emission = null, Texture texture = null, Vector2? tiling = null)
        {
            // This project can run with either the Built-in renderer or URP. Using a
            // URP material while no URP asset is active renders every object magenta.
            var shader = GraphicsSettings.currentRenderPipeline != null
                ? Shader.Find("Universal Render Pipeline/Lit")
                : Shader.Find("Standard");
            shader ??= Shader.Find("Unlit/Color");
            var material = new Material(shader) { color = color };
            material.SetFloat("_Smoothness", smoothness);
            if (texture != null)
            {
                material.mainTexture = texture;
                material.mainTextureScale = tiling ?? Vector2.one;
            }
            if (emission.HasValue && emission.Value.maxColorComponent > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
            }
            target.GetComponent<Renderer>().material = material;
        }

        public void Clear()
        {
            roomObjects.Clear();
            roomCenters.Clear();
            for (var i = transform.childCount - 1; i >= 0; i--)
                if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject); else DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
