using UnobservedRooms.Generation;
using UnobservedRooms.Gameplay;
using UnobservedRooms.Quantum;
using UnobservedRooms.UI;
using UnityEngine;

namespace UnobservedRooms.Core
{
    public sealed class GameRuntime : MonoBehaviour
    {
        [SerializeField] private RunService runService;
        [SerializeField] private LevelAssembler levelAssembler;
        [SerializeField] private GameStateMachine stateMachine;
        [SerializeField] private CoherenceSystem coherence;
        private EntanglementSystem entanglement;
        private bool collapsed;

        private void Awake()
        {
            runService ??= FindAnyObjectByType<RunService>();
            levelAssembler ??= FindAnyObjectByType<LevelAssembler>();
            stateMachine ??= FindAnyObjectByType<GameStateMachine>();
            coherence ??= FindAnyObjectByType<CoherenceSystem>();
            coherence?.ConfigureChallenge(90);
            if (GetComponent<GameAudio>() == null) gameObject.AddComponent<GameAudio>();
        }

        private void OnEnable()
        {
            runService.Ready += OnRunReady;
            runService.Failed += OnRunFailed;
            coherence.Depleted += OnCoherenceDepleted;
        }

        private void OnDisable()
        {
            runService.Ready -= OnRunReady;
            runService.Failed -= OnRunFailed;
            coherence.Depleted -= OnCoherenceDepleted;
        }

        private void Start()
        {
            stateMachine.TryEnter(GameFlowState.Generate);
            runService.GenerateRun();
        }

        private void OnRunReady(Data.RunDefinition run)
        {
            if (!levelAssembler.Assemble(run, out var error)) { OnRunFailed(error); return; }
            BuildPlayableRun(run);
            stateMachine.TryEnter(GameFlowState.Briefing);
            stateMachine.TryEnter(GameFlowState.Explore);
            Debug.Log($"Run '{run.RunId}' ready from '{run.Source}' with {run.Level.Rooms.Count} rooms.");
        }

        private void BuildPlayableRun(Data.RunDefinition run)
        {
            var oldCamera = Camera.main;
            if (oldCamera != null) Destroy(oldCamera.gameObject);

            entanglement = gameObject.AddComponent<EntanglementSystem>();
            entanglement.Configure(run.Entanglement);

            var player = new GameObject("Player");
            player.transform.position = levelAssembler.GetRoomCenter(run.Level.StartRoomId) + Vector3.up * 1.1f;
            var character = player.AddComponent<CharacterController>();
            character.height = 1.8f; character.radius = .35f; character.center = new Vector3(0, .9f, 0);
            var cameraObject = new GameObject("PlayerCamera"); cameraObject.tag = "MainCamera"; cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0, 1.62f, 0);
            var playerCamera = cameraObject.AddComponent<Camera>(); playerCamera.clearFlags = CameraClearFlags.SolidColor; playerCamera.backgroundColor = new Color(.018f,.035f,.045f);
            playerCamera.fieldOfView = 72f; playerCamera.allowHDR = true;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<PlayerFlashlight>();
            cameraObject.AddComponent<HorrorVignette>();
            player.AddComponent<FirstPersonController>().Configure(cameraObject.transform, stateMachine);
            var interactor = player.AddComponent<PlayerInteractor>(); interactor.Configure(playerCamera, coherence);

            foreach (var shardRoom in run.Events.ShardRooms)
                CreateShard(levelAssembler.GetRoomCenter(shardRoom) + new Vector3(0, .75f, 0));
            if (run.Entanglement.Pairs.Count > 0)
            {
                var pair = run.Entanglement.Pairs[0];
                CreateSwitch(levelAssembler.GetRoomCenter(pair.SwitchRoomA) + new Vector3(-1.2f, .65f, 0), 'A');
                CreateSwitch(levelAssembler.GetRoomCenter(pair.SwitchRoomB) + new Vector3(1.2f, .65f, 0), 'B');
            }
            CreateExit(levelAssembler.GetRoomCenter(run.Level.ExitRoomId) + new Vector3(0, 1.25f, 0));
            CreateSurveyor(run, player.transform);
            gameObject.AddComponent<GameHud>().Configure(interactor, coherence, entanglement, run);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.24f,.26f,.25f);
            RenderSettings.ambientEquatorColor = new Color(.12f,.14f,.14f);
            RenderSettings.ambientGroundColor = new Color(.035f,.045f,.05f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.035f,.055f,.06f); RenderSettings.fogDensity = .012f;
        }

        private void OnCoherenceDepleted()
        {
            if (collapsed) return; collapsed = true;
            stateMachine.TryEnter(GameFlowState.Collapse);
            GameHud.ShowResult("COHERENCE COLLAPSE\nThe route could no longer remain stable.\n\nPress R to retry.");
            GameAudio.Play(AudioCue.Collapse,transform.position);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }

        private static void CreateShard(Vector3 position)
        {
            var shard = new GameObject("StabilizerShard"); shard.transform.position = position;
            var collider = shard.AddComponent<SphereCollider>(); collider.radius = .62f;
            var visual = ProceduralAssetFactory.Crystal("FacetedCoherenceCrystal",shard.transform,.38f,1.25f,7);
            var cyan = new Color(.08f,.9f,1f);
            var surface = Resources.Load<Texture2D>("Art/quantum_instrument_surface_v1");
            LevelAssembler.ApplyColor(visual, cyan, .8f, cyan * 4f, surface, Vector2.one); shard.AddComponent<StabilizerShard>(); visual.AddComponent<VisualPulse>().Configure(3.5f,.65f);
            var light = shard.AddComponent<Light>(); light.type = LightType.Point; light.color = cyan; light.range = 4.5f; light.intensity = 2.2f;
            LevelAssembler.CreateQuantumMotes(shard.transform, Vector3.one * 1.4f, cyan, 16f);
        }

        private void CreateSwitch(Vector3 position, char channel)
        {
            var root = new GameObject("EntangledSwitch_" + channel); root.transform.position = position;
            var collider = root.AddComponent<CapsuleCollider>(); collider.radius = .6f; collider.height = 1.5f;
            var visual = ProceduralAssetFactory.Prism("EntanglementInstrument",root.transform,.52f,1.3f,8);
            var color = channel == 'A' ? new Color(.05f,.85f,1f) : new Color(1f,.18f,.6f);
            var surface = Resources.Load<Texture2D>("Art/quantum_instrument_surface_v1");
            LevelAssembler.ApplyColor(visual, color, .65f, color * 2.2f, surface, Vector2.one);
            var halo = root.AddComponent<Light>(); halo.type = LightType.Point; halo.color = color; halo.range = 3.5f; halo.intensity = 1.4f;
            LevelAssembler.CreateQuantumMotes(root.transform, new Vector3(1.4f,1.5f,1.4f), color, 8f);
            root.AddComponent<QuantumSwitch>().Configure(channel, entanglement);
        }

        private void CreateExit(Vector3 position)
        {
            var exit = new GameObject("ExitAperture"); exit.transform.position = position;
            var collider = exit.AddComponent<BoxCollider>(); collider.size = new Vector3(2.4f,2.5f,.35f);
            var visual = ProceduralAssetFactory.Veil("ExitMembrane",exit.transform,2.4f,2.5f,12,14);
            var surface = Resources.Load<Texture2D>("Art/quantum_instrument_surface_v1");
            LevelAssembler.ApplyColor(visual,new Color(.2f,.035f,.32f),.72f,new Color(.42f,.06f,.72f),surface,new Vector2(1.2f,1.2f));
            visual.AddComponent<VisualPulse>().Configure(1.8f,.28f);
            LevelAssembler.CreateQuantumMotes(exit.transform, new Vector3(2.5f,2.6f,.8f), new Color(.35f,.08f,.65f), 22f);
            for (var i = -1; i <= 1; i += 2)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube); pillar.name = "ExitFrame"; pillar.transform.SetParent(exit.transform,false);
                pillar.transform.localPosition = new Vector3(i * .58f,0,-.35f); pillar.transform.localScale = new Vector3(.08f,1.18f,.65f);
                var frameColor = new Color(.32f,.08f,.62f); LevelAssembler.ApplyColor(pillar,frameColor,.8f,frameColor * 3f);
                var pillarCollider = pillar.GetComponent<Collider>(); if (pillarCollider != null) pillarCollider.enabled = false;
            }
            exit.AddComponent<ExitAperture>().Configure(entanglement, stateMachine);
        }

        private void CreateSurveyor(Data.RunDefinition run, Transform player)
        {
            var surveyor = ProceduralAssetFactory.SurveyorBody("TheSurveyor",null);
            var skin = Resources.Load<Texture2D>("Art/surveyor_porcelain_skin_v2");
            LevelAssembler.ApplyColor(surveyor,new Color(.62f,.58f,.5f),.72f,new Color(0f,.22f,.3f),skin,new Vector2(1.35f,1.35f));
            var light = surveyor.AddComponent<Light>(); light.type = LightType.Point; light.range = 5f; light.intensity = 1.4f;
            light.color = new Color(.1f, .65f, .85f);
            for (var i = 0; i < 5; i++)
            {
                var fragment = ProceduralAssetFactory.Crystal("MisalignedFragment",surveyor.transform,.12f + i * .015f,.65f,5);
                fragment.transform.localPosition = new Vector3(Mathf.Sin(i * 2.1f) * .72f, -.65f + i * .32f, Mathf.Cos(i * 1.7f) * .35f);
                fragment.transform.localRotation = Quaternion.Euler(i * 17f,i * 31f,i * 9f);
                var fragmentColor = new Color(.03f,.55f,.72f); LevelAssembler.ApplyColor(fragment,fragmentColor,.75f,fragmentColor * 2f,skin,Vector2.one);
            }
            BuildSurveyorFigure(surveyor.transform,skin);
            LevelAssembler.CreateQuantumMotes(surveyor.transform, new Vector3(1.8f,3f,1.8f), new Color(.05f,.65f,.82f), 18f);
            surveyor.AddComponent<SurveyorAgent>().Configure(run.Level, levelAssembler, player, stateMachine);
        }

        private static void BuildSurveyorFigure(Transform root, Texture2D skin)
        {
            var leftArm = MonsterLimb("LeftArm",root,new Vector3(-.67f,.35f,0),new Vector3(.15f,1.4f,.15f),new Vector3(0,0,-13f),skin);
            var rightArm = MonsterLimb("RightArm",root,new Vector3(.67f,.18f,0),new Vector3(.15f,1.65f,.15f),new Vector3(0,0,11f),skin);
            var leftLeg = MonsterLimb("LeftLeg",root,new Vector3(-.28f,-1.55f,0),new Vector3(.19f,1.6f,.21f),new Vector3(2,0,-3),skin);
            var rightLeg = MonsterLimb("RightLeg",root,new Vector3(.28f,-1.55f,0),new Vector3(.19f,1.6f,.21f),new Vector3(-2,0,3),skin);
            var headLeft = ProceduralAssetFactory.Crystal("SplitHeadLeft",root,.3f,.92f,5).transform;
            var headRight = ProceduralAssetFactory.Crystal("SplitHeadRight",root,.3f,.92f,5).transform;
            headLeft.localPosition = new Vector3(-.12f,1.56f,0); headRight.localPosition = new Vector3(.12f,1.56f,0);
            headLeft.localRotation = Quaternion.Euler(0,0,-18); headRight.localRotation = Quaternion.Euler(0,0,18);
            var bone = new Color(.68f,.63f,.53f);
            LevelAssembler.ApplyColor(headLeft.gameObject,bone,.65f,new Color(0,.18f,.25f),skin,Vector2.one);
            LevelAssembler.ApplyColor(headRight.gameObject,bone,.65f,new Color(0,.18f,.25f),skin,Vector2.one);
            for (var i = -2; i <= 2; i++)
            {
                var rib = ProceduralAssetFactory.Crystal("RibShard",root,.085f,.95f,4); rib.transform.localPosition = new Vector3(0,.2f + i*.22f,-.42f);
                rib.transform.localRotation = Quaternion.Euler(90,i*12f,0); LevelAssembler.ApplyColor(rib,bone,.55f,Color.black,skin,Vector2.one);
            }
            root.gameObject.AddComponent<SurveyorVisualRig>().Configure(leftArm,rightArm,leftLeg,rightLeg,headLeft,headRight);
        }

        private static Transform MonsterLimb(string name, Transform root, Vector3 position, Vector3 scale, Vector3 rotation, Texture2D skin)
        {
            var limb = ProceduralAssetFactory.Prism(name,root,1f,1f,6); limb.transform.localPosition = position; limb.transform.localScale = scale;
            limb.transform.localEulerAngles = rotation; LevelAssembler.ApplyColor(limb,new Color(.55f,.5f,.43f),.62f,new Color(0,.12f,.16f),skin,Vector2.one);
            return limb.transform;
        }

        private void OnRunFailed(string error)
        {
            Debug.LogError("Run generation failed: " + error);
            stateMachine.TryEnter(GameFlowState.Results);
        }
    }
}
