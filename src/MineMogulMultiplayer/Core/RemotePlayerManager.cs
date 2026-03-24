using System.Collections.Generic;
using BepInEx.Logging;
using MineMogulMultiplayer.Models;
using TMPro;
using UnityEngine;

namespace MineMogulMultiplayer.Core
{
    /// <summary>
    /// Spawns and updates visual representations (blocky miner + nametag + held tool)
    /// for remote players. Host creates representations for clients; clients create
    /// representations for everyone else.
    /// </summary>
    public static class RemotePlayerManager
    {
        private static ManualLogSource _logSource;
        private static readonly Dictionary<int, RemotePlayer> _remotePlayers = new Dictionary<int, RemotePlayer>();
        private const float InterpolationSpeed = 12f;

        public static void Init(ManualLogSource log) => _logSource = log;

        private struct RemotePlayer
        {
            public GameObject Go;
            public TextMeshPro Nametag;
            public Vector3 TargetPosition;
            public Quaternion TargetRotation;
            public Transform RightHand;      // parent for held tool visual
            public GameObject HeldToolGo;    // current tool visual (destroyed on swap)
            public string CurrentToolId;     // SavableObjectID name, or null
            public GameObject HeldWorldGo;   // world-space visual for physics-held object
            public string CurrentHeldWorldId;
            public MinerWalkAnim Anim;       // walk animation component (for arm pose)
            public int ToolNullTicks;        // client-side debounce counter for empty tool
            public bool IsCrouching;
        }

        /// <summary>How many consecutive empty-tool ticks before clearing the visual on a remote player.</summary>
        private const int RemoteToolNullDebounce = 120;

        /// <summary>
        /// Vertical offset applied to remote visuals.
        /// The networked player transform sits near capsule center, so lower the
        /// visual to keep feet on the ground.
        /// </summary>
        private const float FallbackYOffset = -0.9f;

        /// <summary>
        /// Update all remote player positions/rotations from the latest player state list.
        /// Call this every tick on both host and client.
        /// </summary>
        public static void UpdatePlayers(List<PlayerState> players, int localPlayerId)
        {
            if (players == null) return;

            var seen = new HashSet<int>();

            foreach (var ps in players)
            {
                if (ps.PlayerId == localPlayerId) continue;
                seen.Add(ps.PlayerId);

                if (!_remotePlayers.TryGetValue(ps.PlayerId, out var rp))
                {
                    rp = SpawnRemotePlayer(ps);
                    _remotePlayers[ps.PlayerId] = rp;
                }

                // Ground-align the model: raycast down to find the floor surface
                Vector3 rawPos = ps.Position.ToUnity();
                rp.TargetPosition = GroundAlign(rawPos);
                rp.TargetRotation = ps.Rotation.ToUnity();

                // While a physics object is being held (crate/lantern/etc), we only render
                // the world-space proxy to avoid duplicate "tool-in-hand + object-in-world" visuals.
                string heldWorldId = NormalizeToolId(ps.HeldObjectId);
                bool suppressHandTool = !string.IsNullOrEmpty(heldWorldId);
                string newTool = suppressHandTool ? null : NormalizeToolId(ps.EquippedTool);
                if (newTool != rp.CurrentToolId)
                {
                    // Debounce: if the new tool is empty but we have a valid current tool, delay the change
                    if (string.IsNullOrEmpty(newTool) && !string.IsNullOrEmpty(rp.CurrentToolId))
                    {
                        // Do not debounce when we're explicitly suppressing hand tool due to held world object.
                        if (!suppressHandTool)
                        {
                            rp.ToolNullTicks++;
                            if (rp.ToolNullTicks < RemoteToolNullDebounce)
                            {
                                // Ignore the empty value for now — keep current tool visual
                                _remotePlayers[ps.PlayerId] = rp;
                                continue;
                            }
                        }
                    }
                    rp.ToolNullTicks = 0;

                    if (rp.HeldToolGo != null) Object.Destroy(rp.HeldToolGo);
                    rp.HeldToolGo = BuildToolVisual(newTool, rp.RightHand);
                    rp.CurrentToolId = newTool;
                    // Raise/lower the right arm based on whether a tool is held
                    if (rp.Anim != null)
                        rp.Anim.HoldingTool = !string.IsNullOrEmpty(newTool);
                    _logSource?.LogInfo($"[RemotePlayer] P{ps.PlayerId} tool changed: '{newTool}' visual={rp.HeldToolGo != null}");
                }
                else
                {
                    rp.ToolNullTicks = 0; // Reset debounce when tool matches

                    // If a tool should be shown but the visual is missing, rebuild it.
                    if (!string.IsNullOrEmpty(newTool) && rp.HeldToolGo == null)
                    {
                        rp.HeldToolGo = BuildToolVisual(newTool, rp.RightHand);
                        if (rp.Anim != null)
                            rp.Anim.HoldingTool = rp.HeldToolGo != null;
                        _logSource?.LogWarning($"[RemotePlayer] Rebuilt missing tool visual for P{ps.PlayerId}: '{newTool}', ok={rp.HeldToolGo != null}");
                    }
                }

                // If the remote player is physically holding a world object (lantern/tool),
                // render a world-space proxy so everyone sees it moving around the map.
                if (!string.IsNullOrEmpty(heldWorldId))
                {
                    if (rp.HeldWorldGo == null || rp.CurrentHeldWorldId != heldWorldId)
                    {
                        if (rp.HeldWorldGo != null) Object.Destroy(rp.HeldWorldGo);
                        rp.HeldWorldGo = BuildWorldToolVisual(heldWorldId);
                        rp.CurrentHeldWorldId = heldWorldId;
                    }
                    if (rp.HeldWorldGo != null)
                    {
                        rp.HeldWorldGo.transform.position = ps.HeldObjectPosition.ToUnity();
                        rp.HeldWorldGo.transform.rotation = ps.HeldObjectRotation.ToUnity();
                    }
                }
                else
                {
                    if (rp.HeldWorldGo != null) Object.Destroy(rp.HeldWorldGo);
                    rp.HeldWorldGo = null;
                    rp.CurrentHeldWorldId = null;
                }

                // Update crouch state
                if (rp.IsCrouching != ps.IsCrouching)
                {
                    rp.IsCrouching = ps.IsCrouching;
                    if (rp.Anim != null)
                        rp.Anim.Crouching = ps.IsCrouching;
                }

                _remotePlayers[ps.PlayerId] = rp;

                if (rp.Nametag != null && rp.Nametag.text != ps.DisplayName)
                    rp.Nametag.text = ps.DisplayName ?? $"Player {ps.PlayerId}";
            }

            // Remove players who are no longer in the list
            var toRemove = new List<int>();
            foreach (var kv in _remotePlayers)
            {
                if (!seen.Contains(kv.Key))
                    toRemove.Add(kv.Key);
            }
            foreach (var id in toRemove)
            {
                if (_remotePlayers.TryGetValue(id, out var rp))
                {
                    if (rp.Go != null) Object.Destroy(rp.Go);
                    if (rp.HeldWorldGo != null) Object.Destroy(rp.HeldWorldGo);
                }
                _remotePlayers.Remove(id);
            }
        }

        /// <summary>Interpolate all remote players toward their target transforms. Call from Update.</summary>
        public static void Interpolate()
        {
            float dt = Time.deltaTime;
            foreach (var kv in _remotePlayers)
            {
                var rp = kv.Value;
                if (rp.Go == null) continue;
                rp.Go.transform.position = Vector3.Lerp(rp.Go.transform.position, rp.TargetPosition, InterpolationSpeed * dt);
                rp.Go.transform.rotation = Quaternion.Slerp(rp.Go.transform.rotation, rp.TargetRotation, InterpolationSpeed * dt);
            }
        }

        /// <summary>Destroy all remote player visuals (call on session stop/scene change).</summary>
        public static void Clear()
        {
            foreach (var kv in _remotePlayers)
            {
                if (kv.Value.Go != null)
                    Object.Destroy(kv.Value.Go);
                if (kv.Value.HeldWorldGo != null)
                    Object.Destroy(kv.Value.HeldWorldGo);
            }
            _remotePlayers.Clear();
        }

        /// <summary>Build a world-space visual proxy for a remote physics-held tool/object.</summary>
        private static GameObject BuildWorldToolVisual(string toolId)
        {
            if (string.IsNullOrEmpty(toolId))
                toolId = "HeldObject";

            var root = new GameObject($"RemoteHeldWorld_{toolId}");
            root.layer = 0;

            // Use prefab clone path first for recognizable visuals.
            var visual = TryCloneToolFromPrefab(toolId, root.transform, 0.95f);
            if (visual == null)
            {
                var shader = FindWorkingShader();
                visual = BuildGenericTool(root.transform, shader);
            }

            if (visual != null)
            {
                foreach (var rend in root.GetComponentsInChildren<Renderer>(true))
                {
                    rend.enabled = true;
                    rend.forceRenderingOff = false;
                    rend.gameObject.layer = 0;
                }
            }
            return root;
        }

        /// <summary>
        /// Offset the remote player's reported capsule-center position so the model's
        /// feet sit on the ground.  Uses a fixed offset instead of raycasting, which
        /// avoids hitting building / trigger colliders and is cheaper.
        /// </summary>
        private static Vector3 GroundAlign(Vector3 worldPos)
        {
            return new Vector3(worldPos.x, worldPos.y + FallbackYOffset, worldPos.z);
        }

        private static RemotePlayer SpawnRemotePlayer(PlayerState ps)
        {
            var pos = GroundAlign(ps.Position.ToUnity());
            _logSource?.LogInfo($"[RemotePlayer] Spawning P{ps.PlayerId} '{ps.DisplayName}' at ({pos.x:F1},{pos.y:F1},{pos.z:F1})");

            var go = CreatePlayerVisual(ps.DisplayName ?? $"Player {ps.PlayerId}", out Transform rightHand, out MinerWalkAnim anim);
            go.transform.position = pos;
            go.transform.rotation = ps.Rotation.ToUnity();

            // Nametag: world-space text floating above the miner
            var nametagGo = new GameObject("Nametag");
            nametagGo.transform.SetParent(go.transform, false);
            nametagGo.transform.localPosition = new Vector3(0, 2.2f, 0);

            var tmp = nametagGo.AddComponent<TextMeshPro>();
            tmp.text = ps.DisplayName ?? $"Player {ps.PlayerId}";
            tmp.fontSize = 4;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.92f, 0.78f, 0.20f, 1f); // gold
            tmp.textWrappingMode = TextWrappingModes.NoWrap;

            nametagGo.AddComponent<BillboardNametag>();

            // Spawn initial held tool
            string toolId = ps.EquippedTool;
            var toolGo = BuildToolVisual(toolId, rightHand);
            if (!string.IsNullOrEmpty(toolId))
                anim.HoldingTool = true;

            return new RemotePlayer
            {
                Go = go,
                Nametag = tmp,
                TargetPosition = ps.Position.ToUnity(),
                TargetRotation = ps.Rotation.ToUnity(),
                RightHand = rightHand,
                HeldToolGo = toolGo,
                CurrentToolId = toolId,
                Anim = anim
            };
        }

        /// <summary>
        /// Build a procedural miner character from Unity primitives.
        /// Returns the root GameObject; outputs the right hand transform for tool attachment.
        /// </summary>
        private static GameObject CreatePlayerVisual(string name, out Transform rightHand, out MinerWalkAnim walkAnim)
        {
            var go = new GameObject($"RemotePlayer_{name}");
            var shader = FindWorkingShader();

            // ── Colour palette ───────────────────────
            var skinCol   = new Color(0.87f, 0.72f, 0.58f); // warm skin
            var shirtCol  = new Color(0.16f, 0.30f, 0.48f); // dark navy work shirt
            var pantsCol  = new Color(0.22f, 0.20f, 0.18f); // dark grey trousers
            var bootCol   = new Color(0.25f, 0.15f, 0.08f); // brown leather boots
            var hatCol    = new Color(0.95f, 0.70f, 0.10f); // safety-yellow hard hat
            var lampCol   = new Color(1.0f,  0.95f, 0.7f);  // warm headlamp glow
            var beltCol   = new Color(0.30f, 0.22f, 0.12f); // tool belt brown
            var vestCol   = new Color(0.95f, 0.55f, 0.05f); // hi-vis orange vest stripes

            // ── Torso ────────────────────────────────
            MakeCube(go.transform, new Vector3(0, 1.05f, 0),
                new Vector3(0.50f, 0.55f, 0.28f), shirtCol, shader);

            // Hi-vis vest stripes
            MakeCube(go.transform, new Vector3(0, 1.15f, 0.141f),
                new Vector3(0.46f, 0.06f, 0.01f), vestCol, shader);
            MakeCube(go.transform, new Vector3(0, 1.00f, 0.141f),
                new Vector3(0.46f, 0.06f, 0.01f), vestCol, shader);

            // ── Belt ─────────────────────────────────
            MakeCube(go.transform, new Vector3(0, 0.77f, 0),
                new Vector3(0.52f, 0.06f, 0.30f), beltCol, shader);

            // ── Hips / Pants upper ───────────────────
            MakeCube(go.transform, new Vector3(0, 0.68f, 0),
                new Vector3(0.48f, 0.14f, 0.28f), pantsCol, shader);

            // ── Left Leg (pivots at hip) ─────────────
            var leftLegPivot = new GameObject("LeftLegPivot");
            leftLegPivot.transform.SetParent(go.transform, false);
            leftLegPivot.transform.localPosition = new Vector3(-0.12f, 0.62f, 0);
            MakeCube(leftLegPivot.transform, new Vector3(0, -0.24f, 0),
                new Vector3(0.18f, 0.48f, 0.22f), pantsCol, shader);
            MakeCube(leftLegPivot.transform, new Vector3(0, -0.52f, 0.02f),
                new Vector3(0.20f, 0.18f, 0.28f), bootCol, shader);

            // ── Right Leg (pivots at hip) ────────────
            var rightLegPivot = new GameObject("RightLegPivot");
            rightLegPivot.transform.SetParent(go.transform, false);
            rightLegPivot.transform.localPosition = new Vector3(0.12f, 0.62f, 0);
            MakeCube(rightLegPivot.transform, new Vector3(0, -0.24f, 0),
                new Vector3(0.18f, 0.48f, 0.22f), pantsCol, shader);
            MakeCube(rightLegPivot.transform, new Vector3(0, -0.52f, 0.02f),
                new Vector3(0.20f, 0.18f, 0.28f), bootCol, shader);

            // ── Left Arm (pivots at shoulder) ────────
            var leftArmPivot = new GameObject("LeftArmPivot");
            leftArmPivot.transform.SetParent(go.transform, false);
            leftArmPivot.transform.localPosition = new Vector3(-0.34f, 1.25f, 0);
            MakeCube(leftArmPivot.transform, new Vector3(0, -0.25f, 0),
                new Vector3(0.16f, 0.50f, 0.20f), shirtCol, shader);
            MakeCube(leftArmPivot.transform, new Vector3(0, -0.55f, 0),
                new Vector3(0.14f, 0.12f, 0.16f), skinCol, shader);

            // ── Right Arm (pivots at shoulder) ───────
            var rightArmPivot = new GameObject("RightArmPivot");
            rightArmPivot.transform.SetParent(go.transform, false);
            rightArmPivot.transform.localPosition = new Vector3(0.34f, 1.25f, 0);
            MakeCube(rightArmPivot.transform, new Vector3(0, -0.25f, 0),
                new Vector3(0.16f, 0.50f, 0.20f), shirtCol, shader);
            var rightHandGo = MakeCube(rightArmPivot.transform, new Vector3(0, -0.55f, 0),
                new Vector3(0.14f, 0.12f, 0.16f), skinCol, shader);

            // Tool attach point: same position as the hand but with unit scale
            // so tools parented here are not squished by the hand cube's tiny localScale.
            var toolAttach = new GameObject("ToolAttach");
            toolAttach.transform.SetParent(rightArmPivot.transform, false);
            toolAttach.transform.localPosition = new Vector3(0, -0.55f, 0);
            rightHand = toolAttach.transform;

            // ── Neck ─────────────────────────────────
            MakeCube(go.transform, new Vector3(0, 1.38f, 0),
                new Vector3(0.14f, 0.06f, 0.14f), skinCol, shader);

            // ── Head ─────────────────────────────────
            MakeCube(go.transform, new Vector3(0, 1.56f, 0),
                new Vector3(0.30f, 0.30f, 0.28f), skinCol, shader);

            // ── Hard Hat ─────────────────────────────
            MakeCube(go.transform, new Vector3(0, 1.76f, 0),
                new Vector3(0.34f, 0.10f, 0.32f), hatCol, shader);
            MakeCube(go.transform, new Vector3(0, 1.72f, 0),
                new Vector3(0.40f, 0.04f, 0.38f), hatCol, shader);

            // ── Headlamp ────────────────────────────
            var lamp = MakeCube(go.transform, new Vector3(0, 1.74f, 0.17f),
                new Vector3(0.08f, 0.06f, 0.04f), lampCol, shader);
            var lampRend = lamp.GetComponent<Renderer>();
            if (lampRend != null)
            {
                lampRend.material.EnableKeyword("_EMISSION");
                lampRend.material.SetColor("_EmissionColor", lampCol * 0.5f);
            }

            // ── Walk animation ───────────────────────
            var anim = go.AddComponent<MinerWalkAnim>();
            anim.LeftLeg = leftLegPivot.transform;
            anim.RightLeg = rightLegPivot.transform;
            anim.LeftArm = leftArmPivot.transform;
            anim.RightArm = rightArmPivot.transform;
            anim.RightHand = rightHand;

            walkAnim = anim;
            return go;
        }

        // ─────────────────────────────────────────────
        //  Tool Visual Factory
        // ─────────────────────────────────────────────

        /// <summary>Build a held-item visual for the given SavableObjectID name. Returns null for empty-handed.</summary>
        private static GameObject BuildToolVisual(string toolId, Transform hand)
        {
            toolId = NormalizeToolId(toolId);
            if (string.IsNullOrEmpty(toolId) || hand == null) return null;

            // Physics-holdable objects (crates) are already visible as real world objects.
            // Don't create a duplicate hand-tool visual — that causes "box inside box".
            if (IsPhysicsHoldable(toolId)) return null;

            // Try to clone the actual game prefab first — gives pixel-perfect visuals
            var cloned = TryCloneToolFromPrefab(toolId, hand, 0.85f);
            if (cloned != null)
            {
                _logSource?.LogInfo($"[RemotePlayer] Cloned prefab visual for '{toolId}'");
                return cloned;
            }

            // Fallback: procedural cube visuals
            _logSource?.LogInfo($"[RemotePlayer] Prefab clone failed for '{toolId}', using fallback cubes");
            var shader = FindWorkingShader();

            switch (toolId)
            {
                case "PickaxeBasic":  return BuildPickaxe(hand, shader);
                case "HammerBasic":   return BuildHammer(hand, shader);
                case "JackHammer":    return BuildDrill(hand, shader);
                case "HardHat":       return BuildHeldHat(hand, shader, new Color(0.95f, 0.70f, 0.10f));
                case "MiningHelmet":  return BuildHeldHat(hand, shader, new Color(0.20f, 0.35f, 0.55f));
                case "MagnetTool":    return BuildMagnet(hand, shader);
                case "WrenchTool":    return BuildWrench(hand, shader);
                case "IngotMold":     return BuildCastingMold(hand, shader);
                case "GearMold":      return BuildCastingMold(hand, shader);
                case "DoubleIngotMold":return BuildCastingMold(hand, shader);
                case "ToolBuilder":   return BuildHammer(hand, shader);
                case "Lantern":       return BuildLantern(hand, shader);
                case "ResourceScannerTool": return BuildGenericTool(hand, shader);
                case "RapidAutoMinerStandardDrillBit":  return BuildDrill(hand, shader);
                case "RapidAutoMinerTurboDrillBit":     return BuildDrill(hand, shader);
                case "RapidAutoMinerHardenedDrillBit":  return BuildDrill(hand, shader);
                case "DebugSpawnTool":return BuildGenericTool(hand, shader);
                default:              return BuildGenericTool(hand, shader);
            }
        }

        /// <summary>
        /// Attempt to clone the visual part of a game tool prefab.
        /// For tools with BaseHeldTool, uses the WorldModel (clean third-person model
        /// without first-person hands/arms). For other objects, uses all renderers.
        /// Returns null if the prefab can't be loaded or has no renderers.
        /// </summary>
        private static GameObject TryCloneToolFromPrefab(string toolId, Transform hand, float targetSize)
        {
            try
            {
                toolId = NormalizeToolId(toolId);
                if (string.IsNullOrEmpty(toolId)) return null;

                if (!System.Enum.TryParse<SavableObjectID>(toolId, out var savableId))
                    return null;

                var slm = Singleton<SavingLoadingManager>.Instance;
                if (slm == null) return null;

                var prefab = slm.GetPrefab(savableId);
                if (prefab == null) return null;

                // Instantiate the prefab far away so it's not visible during setup
                var tempInstance = Object.Instantiate(prefab, new Vector3(0, -9999, 0), Quaternion.identity);
                // Keep active so lossyScale is computed correctly by Unity

                // For tools with BaseHeldTool, use the WorldModel child (clean 3rd-person
                // model without first-person hands/arms). This is the same model the game
                // shows when a tool is lying on the ground.
                Transform modelRoot = tempInstance.transform;
                var baseHeldTool = tempInstance.GetComponent<BaseHeldTool>();
                bool usedWorldModel = false;
                if (baseHeldTool != null && baseHeldTool.WorldModel != null)
                {
                    baseHeldTool.WorldModel.SetActive(true);
                    modelRoot = baseHeldTool.WorldModel.transform;
                    usedWorldModel = true;
                    _logSource?.LogInfo($"[RemotePlayer] Using WorldModel for '{toolId}' — WorldModel localScale={baseHeldTool.WorldModel.transform.localScale}, lossyScale={baseHeldTool.WorldModel.transform.lossyScale}");
                }
                else
                {
                    _logSource?.LogInfo($"[RemotePlayer] No WorldModel for '{toolId}', using all renderers — prefab localScale={tempInstance.transform.localScale}");
                }

                // Gather renderers from the chosen model root
                var renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
                if (renderers == null || renderers.Length == 0)
                {
                    Object.Destroy(tempInstance);
                    return null;
                }

                // Create a clean holder parented to the hand
                var holder = new GameObject($"ToolClone_{toolId}");
                holder.transform.SetParent(hand, false);

                // Per-tool orientation / position overrides
                var overrides = GetToolOverrides(toolId);
                holder.transform.localPosition = overrides.LocalPos;
                holder.transform.localEulerAngles = overrides.LocalRot;

                // Calculate combined bounds in prefab-local space using mesh.bounds
                // (renderer.bounds is world-space and unreliable on freshly created objects)
                bool hasBounds = false;
                Bounds localBounds = default;
                int meshIndex = 0;

                // Clone each renderer into a clean child of our holder
                foreach (var srcRend in renderers)
                {
                    Mesh meshToClone = null;
                    bool isSkinned = false;

                    var srcMf = srcRend.GetComponent<MeshFilter>();
                    if (srcRend is MeshRenderer && srcMf != null && srcMf.sharedMesh != null)
                    {
                        meshToClone = srcMf.sharedMesh;
                    }
                    else if (srcRend is SkinnedMeshRenderer smr && smr.sharedMesh != null)
                    {
                        var baked = new Mesh();
                        smr.BakeMesh(baked);
                        meshToClone = baked;
                        isSkinned = true;
                    }

                    if (meshToClone == null) continue;

                    // Diagnostic: log every mesh we encounter
                    string meshName = isSkinned ? srcRend.gameObject.name + " (skinned)" : (srcMf?.sharedMesh?.name ?? srcRend.gameObject.name);
                    _logSource?.LogInfo($"[ToolClone] '{toolId}' mesh[{meshIndex}]: name='{meshName}' goName='{srcRend.gameObject.name}' verts={meshToClone.vertexCount} submeshes={meshToClone.subMeshCount} bounds={meshToClone.bounds.size} center={meshToClone.bounds.center}");

                    // Skip meshes that are likely non-visual (shadow discs, collider outlines, etc.)
                    string goNameLower = srcRend.gameObject.name.ToLowerInvariant();
                    if (goNameLower.Contains("shadow") || goNameLower.Contains("collider") ||
                        goNameLower.Contains("particle") || goNameLower.Contains("decal") ||
                        goNameLower.Contains("icon"))
                    {
                        _logSource?.LogInfo($"[ToolClone] '{toolId}' mesh[{meshIndex}]: SKIPPED (name filter: '{srcRend.gameObject.name}')");
                        meshIndex++;
                        continue;
                    }

                    // Skip flat quads (4 verts) — these are often shadow/selection indicators
                    if (meshToClone.vertexCount <= 4)
                    {
                        _logSource?.LogInfo($"[ToolClone] '{toolId}' mesh[{meshIndex}]: SKIPPED (too few verts: {meshToClone.vertexCount})");
                        meshIndex++;
                        continue;
                    }

                    meshIndex++;

                    var visualGo = new GameObject(srcRend.gameObject.name);
                    visualGo.transform.SetParent(holder.transform, false);

                    // Copy transform relative to the model root (WorldModel or prefab root)
                    var relPos = modelRoot.InverseTransformPoint(srcRend.transform.position);
                    var relRot = Quaternion.Inverse(modelRoot.rotation) * srcRend.transform.rotation;
                    // Scale relative to model root (not absolute world scale)
                    var rootScale = modelRoot.lossyScale;
                    var relScale = new Vector3(
                        srcRend.transform.lossyScale.x / Mathf.Max(rootScale.x, 0.0001f),
                        srcRend.transform.lossyScale.y / Mathf.Max(rootScale.y, 0.0001f),
                        srcRend.transform.lossyScale.z / Mathf.Max(rootScale.z, 0.0001f));

                    visualGo.transform.localPosition = relPos;
                    visualGo.transform.localRotation = relRot;
                    visualGo.transform.localScale = relScale;

                    // Add mesh components
                    var newMf = visualGo.AddComponent<MeshFilter>();
                    newMf.sharedMesh = meshToClone;

                    var newRend = visualGo.AddComponent<MeshRenderer>();
                    newRend.sharedMaterials = srcRend.sharedMaterials;

                    // Track bounds in holder-local space using mesh.bounds * scale
                    var meshSize = Vector3.Scale(meshToClone.bounds.size, relScale);
                    var meshCenter = relPos + relRot * Vector3.Scale(meshToClone.bounds.center, relScale);
                    var meshBounds = new Bounds(meshCenter, meshSize);
                    if (!hasBounds) { localBounds = meshBounds; hasBounds = true; }
                    else localBounds.Encapsulate(meshBounds);
                }

                // Destroy the temp instance — we only needed the meshes/materials
                Object.Destroy(tempInstance);

                // Verify we actually got geometry
                if (holder.transform.childCount == 0)
                {
                    Object.Destroy(holder);
                    return null;
                }

                // Keep original sharedMaterials — they're already valid URP materials.
                // Only force layer and rendering settings so they're visible on all cameras.
                foreach (var rend in holder.GetComponentsInChildren<Renderer>(true))
                {
                    rend.gameObject.layer = 0; // Default layer — visible to all cameras
                    rend.enabled = true;
                    rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    // Ensure materials are opaque (some game materials use transparent queue)
                    foreach (var mat in rend.sharedMaterials)
                    {
                        if (mat == null) continue;
                        if (mat.HasProperty("_Surface"))
                            mat.SetFloat("_Surface", 0f); // 0 = Opaque
                        if (mat.HasProperty("_AlphaClip"))
                            mat.SetFloat("_AlphaClip", 0f);
                        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                    }
                }
                holder.layer = 0;

                // Scale the cloned tool so its largest dimension fits the target size.
                // Then shift it so the grip area (bottom of bounds along the Y axis)
                // is near the hand — otherwise the center of mass sits at the hand
                // and the tool looks improperly centered.
                if (hasBounds)
                {
                    float actualTargetSize = overrides.TargetSize > 0 ? overrides.TargetSize : targetSize;
                    float maxExtent = Mathf.Max(localBounds.size.x,
                        Mathf.Max(localBounds.size.y, localBounds.size.z));
                    if (maxExtent > 0.001f)
                    {
                        float scaleFactor = actualTargetSize / maxExtent;
                        holder.transform.localScale = Vector3.one * scaleFactor;

                        // Shift the tool so the grip zone sits at the hand.
                        // gripFraction=0.2 means the bottom 20% of Y is at the hand.
                        float gripFrac = overrides.GripFraction;
                        float yMin = localBounds.center.y - localBounds.extents.y;
                        float gripY = yMin + localBounds.size.y * gripFrac;
                        foreach (Transform child in holder.transform)
                            child.localPosition -= new Vector3(localBounds.center.x, gripY, localBounds.center.z);
                    }
                }

                holder.SetActive(true);
                _logSource?.LogInfo($"[RemotePlayer] Cloned prefab '{toolId}' worldModel={usedWorldModel} meshes={holder.transform.childCount} localBounds={localBounds.size} holderScale={holder.transform.localScale} parentLossyScale={hand.lossyScale}");
                return holder;
            }
            catch (System.Exception ex)
            {
                _logSource?.LogWarning($"[RemotePlayer] Prefab clone failed for '{toolId}': {ex.GetType().Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Iterate all known tool SavableObjectIDs and log their WorldModel hierarchy.
        /// Call once to audit every tool prefab in the game — helps verify that cloned
        /// visuals will look correct.
        /// </summary>
        public static void AuditAllToolPrefabs()
        {
            var toolIds = new[]
            {
                SavableObjectID.ToolBuilder,
                SavableObjectID.HammerBasic,
                SavableObjectID.Lantern,
                SavableObjectID.MagnetTool,
                SavableObjectID.PickaxeBasic,
                SavableObjectID.ResourceScannerTool,
                SavableObjectID.RapidAutoMinerStandardDrillBit,
                SavableObjectID.RapidAutoMinerTurboDrillBit,
                SavableObjectID.RapidAutoMinerHardenedDrillBit,
                SavableObjectID.WrenchTool,
                SavableObjectID.DebugSpawnTool,
                SavableObjectID.IngotMold,
                SavableObjectID.GearMold,
                SavableObjectID.DoubleIngotMold,
                SavableObjectID.JackHammer,
                SavableObjectID.HardHat,
                SavableObjectID.MiningHelmet,
            };

            var slm = Singleton<SavingLoadingManager>.Instance;
            if (slm == null)
            {
                _logSource?.LogWarning("[Audit] SavingLoadingManager not available");
                return;
            }

            _logSource?.LogInfo("[Audit] ===== TOOL PREFAB AUDIT START =====");
            foreach (var id in toolIds)
            {
                try
                {
                    var prefab = slm.GetPrefab(id);
                    if (prefab == null) { _logSource?.LogWarning($"[Audit] {id}: prefab is NULL"); continue; }

                    var temp = Object.Instantiate(prefab, new Vector3(0, -9999, 0), Quaternion.identity);
                    var bht = temp.GetComponent<BaseHeldTool>();
                    bool hasWorldModel = bht != null && bht.WorldModel != null;
                    Transform root = hasWorldModel ? bht.WorldModel.transform : temp.transform;
                    if (hasWorldModel) bht.WorldModel.SetActive(true);

                    var allRenderers = root.GetComponentsInChildren<Renderer>(true);
                    _logSource?.LogInfo($"[Audit] {id}: hasWorldModel={hasWorldModel} renderers={allRenderers.Length} rootScale={root.localScale} rootLossyScale={root.lossyScale}");

                    for (int i = 0; i < allRenderers.Length; i++)
                    {
                        var r = allRenderers[i];
                        Mesh m = null;
                        string type = "unknown";
                        var mf = r.GetComponent<MeshFilter>();
                        if (r is MeshRenderer && mf != null && mf.sharedMesh != null)
                        {
                            m = mf.sharedMesh;
                            type = "MeshRenderer";
                        }
                        else if (r is SkinnedMeshRenderer smr && smr.sharedMesh != null)
                        {
                            m = smr.sharedMesh;
                            type = "SkinnedMeshRenderer";
                        }
                        _logSource?.LogInfo($"[Audit]   [{i}] '{r.gameObject.name}' type={type} mesh='{m?.name ?? "null"}' verts={m?.vertexCount ?? 0} submeshes={m?.subMeshCount ?? 0} bounds={m?.bounds.size} matCount={r.sharedMaterials?.Length ?? 0} path={GetRelativePath(root, r.transform)}");
                    }

                    // Also dump the full child hierarchy
                    DumpHierarchy(root, 0, id.ToString());

                    Object.Destroy(temp);
                }
                catch (System.Exception ex)
                {
                    _logSource?.LogWarning($"[Audit] {id}: EXCEPTION {ex.GetType().Name}: {ex.Message}");
                }
            }
            _logSource?.LogInfo("[Audit] ===== TOOL PREFAB AUDIT END =====");
        }

        private static string GetRelativePath(Transform root, Transform child)
        {
            var path = child.gameObject.name;
            var current = child.parent;
            while (current != null && current != root)
            {
                path = current.gameObject.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        private static void DumpHierarchy(Transform root, int depth, string toolId)
        {
            string indent = new string(' ', depth * 2);
            int childCount = root.childCount;
            var renderers = root.GetComponents<Renderer>();
            string rendInfo = renderers.Length > 0 ? $" [has {renderers.Length} renderer(s)]" : "";
            _logSource?.LogInfo($"[Audit:{toolId}] {indent}{root.gameObject.name} active={root.gameObject.activeSelf} localScale={root.localScale}{rendInfo}");
            for (int i = 0; i < childCount; i++)
                DumpHierarchy(root.GetChild(i), depth + 1, toolId);
        }

        /// <summary>Find a shader that works in this render pipeline (URP, Built-in, or fallback).</summary>
        private static Shader FindWorkingShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null) return shader;
            shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader != null) return shader;
            shader = Shader.Find("Standard");
            if (shader != null) return shader;
            shader = Shader.Find("Unlit/Color");
            return shader;
        }

        /// <summary>Returns true if the tool ID refers to a physics-holdable world object
        /// (e.g. crates) whose real game object is already visible in-scene and synced
        /// via the crate/ore position system. We skip creating hand-tool visuals for these.</summary>
        private static bool IsPhysicsHoldable(string toolId)
        {
            if (string.IsNullOrEmpty(toolId)) return false;
            return toolId.StartsWith("BreakableCrate", System.StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeToolId(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var value = raw.Trim();
            if (value.EndsWith("(Clone)", System.StringComparison.OrdinalIgnoreCase))
                value = value.Substring(0, value.Length - "(Clone)".Length).Trim();
            return string.IsNullOrEmpty(value) ? null : value;
        }

        // ─────────────────────────────────────────────
        //  Per-tool orientation / sizing overrides
        // ─────────────────────────────────────────────

        private struct ToolVisualOverrides
        {
            public Vector3 LocalPos;   // holder localPosition relative to hand
            public Vector3 LocalRot;   // holder localEulerAngles
            public float TargetSize;   // 0 = use default
            public float GripFraction; // 0..1, where along the Y axis the hand grips (0=bottom, 1=top)
        }

        /// <summary>
        /// Return per-tool overrides for position, rotation, scale, and grip.
        /// WorldModel meshes have different native orientations:
        ///   - Pickaxe/Hammer/Wrench/DrillBits: handle along +Y (Y-dominant)
        ///   - Magnet/Scanner/JackHammer: barrel/bit along +Z (Z-dominant)
        ///   - Lantern: upright Y-dominant, hung from top handle
        ///
        /// ToolAttach's local +Y points back toward the shoulder.
        /// The arm pivots -55° X when holding a tool (ToolHoldAngle).
        ///
        /// Y-dominant tools: (0,0,180) — the 180° Z flips Y so the tool head
        ///   extends AWAY from the shoulder (outward from hand).
        /// Z-dominant tools: (55,0,0) — the 55° X compensates for the arm tilt
        ///   so the barrel/bit points forward in world space and the body stays upright.
        /// </summary>
        private static ToolVisualOverrides GetToolOverrides(string toolId)
        {
            var defaults = new ToolVisualOverrides
            {
                LocalPos = new Vector3(0, -0.04f, 0.08f),
                LocalRot = new Vector3(-30f, 0, 0),
                TargetSize = 0.85f,
                GripFraction = 0.15f
            };

            switch (toolId)
            {
                // ── Y-dominant: handle along +Y ──
                // 180° Z-rotation flips both X and Y so the tool head extends
                // away from the shoulder instead of running up the arm.
                case "PickaxeBasic":
                    return new ToolVisualOverrides
                    {
                        LocalPos = new Vector3(0, -0.02f, 0.04f),
                        LocalRot = new Vector3(0, 0, 180f),
                        TargetSize = 0.80f,
                        GripFraction = 0.20f
                    };

                case "HammerBasic":
                    return new ToolVisualOverrides
                    {
                        LocalPos = new Vector3(0, -0.02f, 0.04f),
                        LocalRot = new Vector3(0, 0, 180f),
                        TargetSize = 0.70f,
                        GripFraction = 0.20f
                    };

                case "WrenchTool":
                    return new ToolVisualOverrides
                    {
                        LocalPos = new Vector3(0, -0.02f, 0.04f),
                        LocalRot = new Vector3(0, 0, 180f),
                        TargetSize = 0.75f,
                        GripFraction = 0.20f
                    };

                // ── Y-dominant drill bits ──
                case "RapidAutoMinerStandardDrillBit":
                case "RapidAutoMinerTurboDrillBit":
                case "RapidAutoMinerHardenedDrillBit":
                    return new ToolVisualOverrides
                    {
                        LocalPos = new Vector3(0, -0.02f, 0.04f),
                        LocalRot = new Vector3(0, 0, 180f),
                        TargetSize = 0.70f,
                        GripFraction = 0.20f
                    };

                // ── Z-dominant: barrel/bit along +Z ──
                // 55° X-rotation compensates for the arm's -55° ToolHoldAngle so
                // the barrel points forward in world space and the body stays upright.
                case "MagnetTool":
                case "ResourceScannerTool":
                case "DebugSpawnTool":
                    return new ToolVisualOverrides
                    {
                        LocalPos = new Vector3(0, -0.04f, 0.06f),
                        LocalRot = new Vector3(55f, 0, 0),
                        TargetSize = 0.55f,
                        GripFraction = 0.45f
                    };

                case "JackHammer":
                    return new ToolVisualOverrides
                    {
                        // Z=2.48 longest — bit along Z, not Y
                        LocalPos = new Vector3(0, -0.04f, 0.06f),
                        LocalRot = new Vector3(55f, 0, 0),
                        TargetSize = 0.80f,
                        GripFraction = 0.40f
                    };

                // ── Lantern: upright, hung below the hand ──
                // Same 55° X to counter arm tilt so lantern hangs straight down.
                case "Lantern":
                    return new ToolVisualOverrides
                    {
                        LocalPos = new Vector3(0, -0.02f, 0.06f),
                        LocalRot = new Vector3(55f, 0, 0),
                        TargetSize = 0.50f,
                        GripFraction = 0.85f
                    };

                // ── Molds: flat tray-like items ──
                case "IngotMold":
                case "GearMold":
                case "DoubleIngotMold":
                    return new ToolVisualOverrides
                    {
                        LocalPos = new Vector3(0, -0.04f, 0.10f),
                        LocalRot = new Vector3(45f, 0, 0),
                        TargetSize = 0.50f,
                        GripFraction = 0.30f
                    };

                // ── Hats: worn items ──
                case "HardHat":
                case "MiningHelmet":
                    return new ToolVisualOverrides
                    {
                        LocalPos = new Vector3(0, -0.04f, 0.08f),
                        LocalRot = new Vector3(45f, 0, 0),
                        TargetSize = 0.45f,
                        GripFraction = 0.30f
                    };

                case "ToolBuilder":
                    return defaults;

                default:
                    return defaults;
            }
        }

        // Shared colours
        private static readonly Color Steel    = new Color(0.45f, 0.45f, 0.50f);
        private static readonly Color Wood     = new Color(0.40f, 0.25f, 0.12f);
        private static readonly Color DarkMetal = new Color(0.25f, 0.25f, 0.28f);
        private static readonly Color Red      = new Color(0.75f, 0.15f, 0.15f);

        private static GameObject MakeToolPivot(Transform hand, float xAngle = -30f)
        {
            var pivot = new GameObject("ToolPivot");
            pivot.transform.SetParent(hand, false);
            pivot.transform.localPosition = new Vector3(0, -0.04f, 0.10f);
            pivot.transform.localEulerAngles = new Vector3(xAngle, 0, 0);
            return pivot;
        }

        private static GameObject BuildPickaxe(Transform hand, Shader s)
        {
            var pivot = MakeToolPivot(hand);
            MakeCube(pivot.transform, new Vector3(0, -0.22f, 0),
                new Vector3(0.06f, 0.44f, 0.06f), Wood, s);           // handle (thicker)
            MakeCube(pivot.transform, new Vector3(0, -0.44f, 0),
                new Vector3(0.28f, 0.06f, 0.06f), Steel, s);          // head bar
            MakeCube(pivot.transform, new Vector3(-0.17f, -0.44f, 0),
                new Vector3(0.08f, 0.05f, 0.05f), Steel, s);          // point
            MakeCube(pivot.transform, new Vector3(0.16f, -0.44f, 0),
                new Vector3(0.06f, 0.08f, 0.06f), Steel, s);          // chisel
            return pivot;
        }

        private static GameObject BuildHammer(Transform hand, Shader s)
        {
            var pivot = MakeToolPivot(hand);
            MakeCube(pivot.transform, new Vector3(0, -0.18f, 0),
                new Vector3(0.06f, 0.36f, 0.06f), Wood, s);           // handle (thicker)
            MakeCube(pivot.transform, new Vector3(0, -0.36f, 0),
                new Vector3(0.18f, 0.12f, 0.10f), Steel, s);          // hammerhead
            return pivot;
        }

        private static GameObject BuildDrill(Transform hand, Shader s)
        {
            var pivot = MakeToolPivot(hand, -45f);
            var bodyCol = new Color(0.85f, 0.65f, 0.10f); // yellow body
            MakeCube(pivot.transform, new Vector3(0, -0.12f, 0),
                new Vector3(0.12f, 0.26f, 0.12f), bodyCol, s);        // body (bigger)
            MakeCube(pivot.transform, new Vector3(0, -0.30f, 0),
                new Vector3(0.06f, 0.16f, 0.06f), Steel, s);          // drill bit
            MakeCube(pivot.transform, new Vector3(0, 0.02f, 0),
                new Vector3(0.08f, 0.06f, 0.12f), DarkMetal, s);      // grip/motor
            return pivot;
        }

        private static GameObject BuildShovel(Transform hand, Shader s)
        {
            var pivot = MakeToolPivot(hand);
            MakeCube(pivot.transform, new Vector3(0, -0.22f, 0),
                new Vector3(0.06f, 0.44f, 0.06f), Wood, s);          // handle
            MakeCube(pivot.transform, new Vector3(0, -0.46f, 0),
                new Vector3(0.12f, 0.14f, 0.05f), Steel, s);          // blade
            return pivot;
        }

        private static GameObject BuildHeldHat(Transform hand, Shader s, Color hatColor)
        {
            var pivot = MakeToolPivot(hand, -10f);
            MakeCube(pivot.transform, new Vector3(0, -0.06f, 0),
                new Vector3(0.24f, 0.08f, 0.22f), hatColor, s);       // dome (bigger)
            MakeCube(pivot.transform, new Vector3(0, -0.10f, 0),
                new Vector3(0.30f, 0.03f, 0.28f), hatColor, s);       // brim
            return pivot;
        }

        private static GameObject BuildMagnet(Transform hand, Shader s)
        {
            var pivot = MakeToolPivot(hand, -20f);
            MakeCube(pivot.transform, new Vector3(0, -0.12f, 0),
                new Vector3(0.07f, 0.24f, 0.07f), DarkMetal, s);      // body (bigger)
            MakeCube(pivot.transform, new Vector3(-0.06f, -0.26f, 0),
                new Vector3(0.06f, 0.10f, 0.06f), Red, s);            // left pole
            MakeCube(pivot.transform, new Vector3( 0.06f, -0.26f, 0),
                new Vector3(0.06f, 0.10f, 0.06f), Steel, s);          // right pole
            return pivot;
        }

        private static GameObject BuildWrench(Transform hand, Shader s)
        {
            var pivot = MakeToolPivot(hand);
            MakeCube(pivot.transform, new Vector3(0, -0.20f, 0),
                new Vector3(0.05f, 0.40f, 0.04f), Steel, s);          // shaft (thicker)
            MakeCube(pivot.transform, new Vector3(0, -0.42f, 0),
                new Vector3(0.10f, 0.07f, 0.04f), Steel, s);          // jaw
            return pivot;
        }

        private static GameObject BuildCastingMold(Transform hand, Shader s)
        {
            var pivot = MakeToolPivot(hand, -10f);
            var moldCol = new Color(0.55f, 0.45f, 0.35f);
            MakeCube(pivot.transform, new Vector3(0, -0.08f, 0),
                new Vector3(0.18f, 0.08f, 0.12f), moldCol, s);        // tray (bigger)
            MakeCube(pivot.transform, new Vector3(0, -0.04f, 0),
                new Vector3(0.18f, 0.03f, 0.12f), DarkMetal, s);      // rim
            return pivot;
        }

        private static GameObject BuildLantern(Transform hand, Shader s)
        {
            var pivot = MakeToolPivot(hand, -10f);
            var glassCol = new Color(1.0f, 0.95f, 0.7f, 0.8f);
            MakeCube(pivot.transform, new Vector3(0, -0.06f, 0),
                new Vector3(0.06f, 0.06f, 0.06f), DarkMetal, s);      // top cap
            var glass = MakeCube(pivot.transform, new Vector3(0, -0.14f, 0),
                new Vector3(0.10f, 0.12f, 0.10f), glassCol, s);       // glass body
            MakeCube(pivot.transform, new Vector3(0, -0.22f, 0),
                new Vector3(0.08f, 0.04f, 0.08f), DarkMetal, s);      // base
            // Make glass glow
            var glassRend = glass.GetComponent<Renderer>();
            if (glassRend != null)
            {
                glassRend.material.EnableKeyword("_EMISSION");
                glassRend.material.SetColor("_EmissionColor", glassCol * 1.5f);
            }
            return pivot;
        }

        private static GameObject BuildGenericTool(Transform hand, Shader s)
        {
            var pivot = MakeToolPivot(hand);
            MakeCube(pivot.transform, new Vector3(0, -0.15f, 0),
                new Vector3(0.08f, 0.30f, 0.08f), DarkMetal, s);      // thicker generic
            return pivot;
        }

        // ─────────────────────────────────────────────

        /// <summary>Create a cube primitive, parent it, position/scale it, colour it, and strip collider.</summary>
        private static GameObject MakeCube(Transform parent, Vector3 localPos, Vector3 scale, Color color, Shader shader)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPos;
            cube.transform.localScale = scale;

            var col = cube.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            var rend = cube.GetComponent<Renderer>();
            if (rend != null)
            {
                if (shader != null)
                {
                    var mat = new Material(shader);
                    // URP uses _BaseColor, Standard uses _Color
                    if (mat.HasProperty("_BaseColor"))
                        mat.SetColor("_BaseColor", color);
                    else
                        mat.color = color;
                    mat.SetFloat("_Glossiness", 0.15f);
                    mat.SetFloat("_Metallic", 0.0f);
                    mat.SetFloat("_Smoothness", 0.15f); // URP equivalent
                    rend.material = mat;
                }
                else
                {
                    // Last-resort: keep primitive default material but tint if possible.
                    if (rend.material != null)
                    {
                        if (rend.material.HasProperty("_BaseColor"))
                            rend.material.SetColor("_BaseColor", color);
                        else if (rend.material.HasProperty("_Color"))
                            rend.material.color = color;
                    }
                }
                rend.forceRenderingOff = false;
            }
            return cube;
        }
    }

    /// <summary>Simple billboard behaviour that makes the nametag always face the camera.</summary>
    internal class BillboardNametag : MonoBehaviour
    {
        private void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null) return;
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }
    }

    /// <summary>
    /// Procedural walk cycle for the blocky miner. Swings arms and legs
    /// opposite to each other when the character is moving. Detects movement
    /// by measuring position delta each frame.
    /// </summary>
    internal class MinerWalkAnim : MonoBehaviour
    {
        public Transform LeftLeg;
        public Transform RightLeg;
        public Transform LeftArm;
        public Transform RightArm;
        public Transform RightHand;

        /// <summary>When true, the right arm is raised as if holding a tool.</summary>
        public bool HoldingTool;

        /// <summary>When true, the model crouches (lower Y, bent legs).</summary>
        public bool Crouching;

        private const float SwingSpeed = 8f;   // oscillation speed
        private const float MaxSwing   = 35f;  // max angle in degrees
        private const float ReturnSpeed = 6f;  // how fast limbs return to rest
        private const float MoveThreshold = 0.01f;

        /// <summary>Resting angle for right arm when holding a tool (raised forward).</summary>
        private const float ToolHoldAngle = -55f;
        /// <summary>Walk swing is reduced for the tool arm so the tool doesn't flail wildly.</summary>
        private const float ToolArmSwingScale = 0.3f;

        /// <summary>Crouch lower amount and leg bend angle.</summary>
        private const float CrouchYOffset = -0.35f;
        private const float CrouchLegAngle = 40f;
        private const float CrouchTransitionSpeed = 8f;

        private Vector3 _lastPos;
        private float _phase;          // oscillator phase
        private float _currentSwing;   // current amplitude (ramps up/down)
        private float _crouchBlend;    // 0 = standing, 1 = fully crouched
        private float _appliedCrouchYOffset; // currently applied world-space crouch offset

        private void Start()
        {
            _lastPos = transform.position;
            _appliedCrouchYOffset = 0f;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            // Smooth crouch transition
            float crouchTarget = Crouching ? 1f : 0f;
            _crouchBlend = Mathf.MoveTowards(_crouchBlend, crouchTarget, CrouchTransitionSpeed * dt);

            // Apply crouch as an incremental world-space Y offset.
            // Do NOT reset localPosition here, otherwise networked movement gets overwritten.
            float targetCrouchOffset = _crouchBlend * CrouchYOffset;
            float deltaCrouchOffset = targetCrouchOffset - _appliedCrouchYOffset;
            if (Mathf.Abs(deltaCrouchOffset) > 0.0001f)
            {
                var wp = transform.position;
                wp.y += deltaCrouchOffset;
                transform.position = wp;
                _appliedCrouchYOffset = targetCrouchOffset;
            }

            // Measure horizontal movement speed
            var pos = transform.position;
            var delta = pos - _lastPos;
            delta.y = 0; // ignore vertical
            float speed = delta.magnitude / dt;
            _lastPos = pos;

            bool moving = speed > MoveThreshold;

            // Ramp swing amplitude up when moving, down when idle
            float targetSwing = moving ? MaxSwing : 0f;
            _currentSwing = Mathf.MoveTowards(_currentSwing, targetSwing,
                (moving ? 120f : 90f) * dt);

            if (_currentSwing < 0.1f)
            {
                // At rest — snap limbs to neutral (or tool-hold pose) + crouch offset
                float crouchLeg = _crouchBlend * CrouchLegAngle;
                SetLimbAngle(LeftLeg, crouchLeg);
                SetLimbAngle(RightLeg, crouchLeg);
                SetLimbAngle(LeftArm, 0);
                SetLimbAngle(RightArm, HoldingTool ? ToolHoldAngle : 0);
                return;
            }

            // Advance phase
            _phase += SwingSpeed * dt;

            float sin = Mathf.Sin(_phase);
            float swing = sin * _currentSwing;
            float crouchLegOffset = _crouchBlend * CrouchLegAngle;

            // Legs swing opposite: left forward = right back, plus crouch bend
            SetLimbAngle(LeftLeg, swing + crouchLegOffset);
            SetLimbAngle(RightLeg, -swing + crouchLegOffset);

            // Arms swing opposite to legs (natural walk)
            SetLimbAngle(LeftArm, -swing * 0.8f);
            if (HoldingTool)
                SetLimbAngle(RightArm, ToolHoldAngle + swing * ToolArmSwingScale);
            else
                SetLimbAngle(RightArm, swing * 0.8f);
        }

        private static void SetLimbAngle(Transform limb, float angle)
        {
            if (limb == null) return;
            var euler = limb.localEulerAngles;
            euler.x = angle;
            euler.y = 0;
            euler.z = 0;
            limb.localEulerAngles = euler;
        }
    }
}
