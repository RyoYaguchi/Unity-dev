using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HD2DSceneBuilder : EditorWindow
{
    [MenuItem("Tools/HD-2D/Build Scene")]
    public static void BuildHD2DScene()
    {
        // 1. Prepare Folder/Asset paths
        string texPath = "Assets/Textures";
        string matPath = "Assets/Materials";
        if (!Directory.Exists(matPath))
        {
            Directory.CreateDirectory(matPath);
        }

        // Verify textures exist, if not generate them first
        if (!File.Exists(texPath + "/grass.png") || !File.Exists(texPath + "/roof.png"))
        {
            HD2DTextureGenerator.GenerateAllTextures();
        }

        // 2. Load generated textures
        Texture2D grassTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/grass.png");
        Texture2D cliffTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/cliff.png");
        Texture2D stoneTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/stone.png");
        Texture2D woodTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/wood.png");
        Texture2D waterTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/water.png");
        Texture2D roofTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/roof.png");
        Texture2D treeTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/tree.png");
        Texture2D lanternTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/lantern.png");
        Texture2D chestTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/chest.png");
        Texture2D playerTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/player.png");
        Texture2D plasterTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/plaster.png");
        Texture2D clothRedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/cloth_red.png");
        Texture2D clothWhiteTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/cloth_white.png");
        Texture2D clothGreenTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + "/cloth_green.png");

        // 3. Create/Configure URP materials
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLitShader == null)
        {
            Debug.LogError("HD-2D: Universal Render Pipeline/Lit shader not found! Make sure URP is set up.");
            return;
        }

        Material grassMat = CreateURPMaterial(urpLitShader, grassTex, matPath + "/GrassMat.mat", 0.0f, 0.0f);
        Material cliffMat = CreateURPMaterial(urpLitShader, cliffTex, matPath + "/CliffMat.mat", 0.0f, 0.0f);
        Material stoneMat = CreateURPMaterial(urpLitShader, stoneTex, matPath + "/StoneMat.mat", 0.1f, 0.0f);
        Material woodMat = CreateURPMaterial(urpLitShader, woodTex, matPath + "/WoodMat.mat", 0.05f, 0.0f);
        Material roofMat = CreateURPMaterial(urpLitShader, roofTex, matPath + "/RoofMat.mat", 0.05f, 0.0f);
        Material plasterMat = CreateURPMaterial(urpLitShader, plasterTex, matPath + "/PlasterMat.mat", 0.0f, 0.0f);
        Material clothRedMat = CreateURPMaterial(urpLitShader, clothRedTex, matPath + "/ClothRedMat.mat", 0.0f, 0.0f);
        Material clothWhiteMat = CreateURPMaterial(urpLitShader, clothWhiteTex, matPath + "/ClothWhiteMat.mat", 0.0f, 0.0f);
        Material clothGreenMat = CreateURPMaterial(urpLitShader, clothGreenTex, matPath + "/ClothGreenMat.mat", 0.0f, 0.0f);

        // Water Material (Transparent URP Lit)
        Material waterMat = AssetDatabase.LoadAssetAtPath<Material>(matPath + "/WaterMat.mat");
        if (waterMat == null)
        {
            waterMat = new Material(urpLitShader);
            waterMat.SetTexture("_BaseMap", waterTex);
            waterMat.SetFloat("_Surface", 1.0f); // Transparent
            waterMat.SetFloat("_Blend", 0.0f);   // Alpha blend
            waterMat.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.78f));
            waterMat.SetFloat("_Smoothness", 0.7f);
            waterMat.SetFloat("_Metallic", 0.1f);
            waterMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            waterMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            waterMat.SetInt("_ZWrite", 0);
            waterMat.DisableKeyword("_ALPHATEST_ON");
            waterMat.EnableKeyword("_ALPHABLEND_ON");
            waterMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            AssetDatabase.CreateAsset(waterMat, matPath + "/WaterMat.mat");
        }

        // Sprite lit materials with ALPHA CLIPPING for 3D dynamic shadow casting!
        Material spriteMat = AssetDatabase.LoadAssetAtPath<Material>(matPath + "/SpriteLitMat.mat");
        if (spriteMat == null)
        {
            spriteMat = new Material(urpLitShader);
            spriteMat.SetFloat("_AlphaClip", 1.0f);
            spriteMat.SetFloat("_Cutoff", 0.5f);
            spriteMat.EnableKeyword("_ALPHATEST_ON");
            spriteMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            AssetDatabase.CreateAsset(spriteMat, matPath + "/SpriteLitMat.mat");
        }

        // 4. Clean up Scene
        GameObject existingValley = GameObject.Find("HD2D Valley");
        if (existingValley != null)
        {
            DestroyImmediate(existingValley);
        }

        GameObject valleyRoot = new GameObject("HD2D Valley");

        // 5. Setup Camera & Lights
        ConfigureLightsAndCamera(valleyRoot);

        // 6. Generate Landscape (colliders disabled on individual terrain cubes)
        GenerateLandscapeBlocks(valleyRoot, grassMat, cliffMat, stoneMat);

        // 6A. Spawn Unified Tier Box Colliders for extremely smooth movement (zero micro-bumps)
        CreateTierColliders(valleyRoot);

        // 7. Spawn River Plane & Animation
        SpawnRiverPlane(valleyRoot, waterMat);

        // 8. Spawn Wooden Bridges
        SpawnBridges(valleyRoot, woodMat);

        // 9. Sliced Sprite assets loading
        Sprite[] chestSprites = AssetDatabase.LoadAllAssetsAtPath(texPath + "/chest.png").OfType<Sprite>().OrderBy(GetSpriteIndex).ToArray();
        Sprite[] playerSprites = AssetDatabase.LoadAllAssetsAtPath(texPath + "/player.png").OfType<Sprite>().OrderBy(GetSpriteIndex).ToArray();
        Sprite[] chiefSprites = AssetDatabase.LoadAllAssetsAtPath(texPath + "/npc_chief.png").OfType<Sprite>().OrderBy(GetSpriteIndex).ToArray();
        Sprite[] merchantSprites = AssetDatabase.LoadAllAssetsAtPath(texPath + "/npc_merchant.png").OfType<Sprite>().OrderBy(GetSpriteIndex).ToArray();
        Sprite[] adventurerSprites = AssetDatabase.LoadAllAssetsAtPath(texPath + "/npc_adventurer.png").OfType<Sprite>().OrderBy(GetSpriteIndex).ToArray();
        Sprite grassTuftSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath + "/grass_tuft.png");
        Sprite treeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath + "/tree.png");
        Sprite lanternSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath + "/lantern.png");

        // 10. Build Starting Village Structures (Houses, Well, Market Stalls)
        BuildVillageStructures(valleyRoot, woodMat, stoneMat, roofMat, cliffMat, grassMat, plasterMat, clothRedMat, clothWhiteMat, clothGreenMat);

        // 10A. Build Cozy Interior Rooms (Teleport targets)
        BuildInteriorRooms(valleyRoot, plasterMat, stoneMat, woodMat, roofMat);

        // 11. Spawn Billboards & Trees
        SpawnBillboardTrees(valleyRoot, treeSprite, spriteMat);
        SpawnLanterns(valleyRoot, lanternSprite, spriteMat);

        // 12. Spawn Chest, campfires & NPCs
        SpawnPropsAndNPCs(valleyRoot, chestSprites, playerSprites, chiefSprites, merchantSprites, adventurerSprites, spriteMat);

        // 12A. Spawn Lush Flora (2D grass tufts & wildflowers)
        SpawnFlora(valleyRoot, grassTuftSprite, treeSprite, spriteMat);

        // 13. Spawn Player
        GameObject playerObj = SpawnPlayer(valleyRoot, playerSprites, spriteMat);

        // 14. Configure Volume and Post Processing
        ConfigurePostProcessing(valleyRoot);

        // 15. Day/Night Master Script Attachment (Automatic 24-hour cycle)
        Light sunLight = GameObject.Find("Directional Light")?.GetComponent<Light>();
        if (sunLight != null)
        {
            DayNightCycle cycle = valleyRoot.AddComponent<DayNightCycle>();
            cycle.sunLight = sunLight;
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.Refresh();
        Debug.Log("HD-2D: Starting Village successfully built! Explore the new buildings, shops, and NPCs. Press 'T' to cycle Day/Night!");
    }

    private static Material CreateURPMaterial(Shader shader, Texture2D tex, string path, float smoothness, float metallic)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            mat.SetTexture("_BaseMap", tex);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.SetTexture("_BaseMap", tex);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(mat);
        }
        return mat;
    }

    private static void ConfigureLightsAndCamera(GameObject root)
    {
        // Directional Light
        GameObject lightObj = GameObject.Find("Directional Light");
        if (lightObj == null)
        {
            lightObj = new GameObject("Directional Light");
            lightObj.AddComponent<Light>();
        }
        lightObj.transform.position = new Vector3(0, 20, 0);
        lightObj.transform.rotation = Quaternion.Euler(19f, -135f, 0f);

        Light light = lightObj.GetComponent<Light>();
        light.type = LightType.Directional;
        light.shadows = LightShadows.Soft;
        light.color = new Color(1.0f, 0.78f, 0.62f); // Warm JRPG sunset sunlight
        light.intensity = 1.4f;
        
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.2f, 0.18f, 0.25f); // Cool contrasting ambient

        // Enable JRPG Fog
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.2f, 0.18f, 0.25f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 10f;
        RenderSettings.fogEndDistance = 25f;

        // Camera Setup
        GameObject camObj = GameObject.FindWithTag("MainCamera");
        if (camObj == null)
        {
            camObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camObj.tag = "MainCamera";
        }
        camObj.transform.position = new Vector3(0, 6.5f, -8.5f);
        camObj.transform.rotation = Quaternion.Euler(28f, 0f, 0f);

        Camera camera = camObj.GetComponent<Camera>();
        camera.orthographic = false;
        camera.fieldOfView = 27f; // Pulled back slightly for better scenic view
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 100f;

        // Attach Camera Controller
        CinematicCameraController camCtrl = camObj.GetComponent<CinematicCameraController>();
        if (camCtrl == null) camCtrl = camObj.AddComponent<CinematicCameraController>();
    }

    private static void GenerateLandscapeBlocks(GameObject root, Material grass, Material cliff, Material stone)
    {
        GameObject terrainFolder = new GameObject("Blocks");
        terrainFolder.transform.SetParent(root.transform);

        int minX = -18;
        int maxX = 18;
        int minZ = -18;
        int maxZ = 18;

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                // River gap (between z = -2 and z = 2)
                bool isRiver = (z >= -2 && z <= 2);
                
                int ySurface = 2; // Default starting height
                Material surfaceMat = grass;

                if (isRiver)
                {
                    ySurface = 0; // River bottom
                    surfaceMat = stone;
                }
                else
                {
                    // Left bank hill levels (z <= -3)
                    if (z == -3 || z == -4)
                    {
                        ySurface = 2;
                    }
                    else if (z >= -8 && z <= -5)
                    {
                        ySurface = 2;
                    }
                    else if (z == -9)
                    {
                        ySurface = 4; // Step up
                    }
                    else if (z <= -10)
                    {
                        ySurface = 4; // Higher meadow
                    }

                    // Right bank village plaza (z >= 3)
                    if (z >= 3 && z <= 12)
                    {
                        ySurface = 2;
                        surfaceMat = stone; // Paved cobblestone streets & plaza
                    }
                    else if (z >= 13 && z <= 14)
                    {
                        ySurface = 2;
                        surfaceMat = grass; // Grassy yards
                    }
                    else if (z == 15)
                    {
                        ySurface = 4; // Stepped cliff
                    }
                    else if (z >= 16)
                    {
                        ySurface = 4; // High village cliff
                    }
                }

                int bottomFill = isRiver ? -1 : 0;
                // Place surface block (colliders removed for smooth unified tier collider walking)
                CreateBlock(terrainFolder, x, ySurface, z, surfaceMat, false);

                // Fill downwards with cliff blocks
                for (int y = ySurface - 1; y >= bottomFill; y--)
                {
                    CreateBlock(terrainFolder, x, y, z, cliff, false);
                }
            }
        }
    }

    private static int GetZCellHeight(int z)
    {
        if (z >= -2 && z <= 2) return 0;
        if (z >= -8 && z <= -3) return 2;
        if (z <= -9) return 4;
        if (z >= 3 && z <= 14) return 2;
        return 4;
    }

    private static void CreateBlock(GameObject parent, int x, int y, int z, Material mat, bool includeCollider = true)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = $"Block_{x}_{y}_{z}";
        cube.transform.position = new Vector3(x, y - 0.5f, z); // Adjust pivot offset so surface aligns to integer coordinates
        cube.transform.SetParent(parent.transform);
        
        Renderer r = cube.GetComponent<Renderer>();
        r.material = mat;
        r.shadowCastingMode = ShadowCastingMode.On;
        r.receiveShadows = true;
        cube.isStatic = true;

        if (!includeCollider)
        {
            DestroyImmediate(cube.GetComponent<Collider>());
        }
    }

    private static void SpawnRiverPlane(GameObject root, Material waterMat)
    {
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Quad);
        water.name = "RiverWater";
        water.transform.SetParent(root.transform);
        water.transform.position = new Vector3(0f, 0.7f, 0f);
        water.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        water.transform.localScale = new Vector3(38f, 5.4f, 1f); // Spans entire valley wide

        DestroyImmediate(water.GetComponent<Collider>()); // Avoid solid collision on water surface

        Renderer r = water.GetComponent<Renderer>();
        r.material = waterMat;
        r.shadowCastingMode = ShadowCastingMode.Off;
        r.receiveShadows = true;

        water.AddComponent<WaterFlowAnimator>();
    }

    private static void SpawnBridges(GameObject root, Material woodMat)
    {
        GameObject bridgeFolder = new GameObject("Bridges");
        bridgeFolder.transform.SetParent(root.transform);

        // Bridge 1: Center Bridge (X = 0)
        BuildBridgeAt(bridgeFolder, 0f, woodMat);

        // Bridge 2: Western Bridge (X = -11)
        BuildBridgeAt(bridgeFolder, -11f, woodMat);
    }

    private static void BuildBridgeAt(GameObject parent, float xPos, Material woodMat)
    {
        GameObject bridge = new GameObject($"WoodenBridge_{xPos}");
        bridge.transform.SetParent(parent.transform);

        // Main Plank Deck
        GameObject bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bed.name = "BridgeBed";
        bed.transform.position = new Vector3(xPos, 2.05f, 0f);
        bed.transform.localScale = new Vector3(1.8f, 0.15f, 5.4f);
        bed.transform.SetParent(bridge.transform);
        bed.GetComponent<Renderer>().material = woodMat;
        bed.isStatic = true;
        DestroyImmediate(bed.GetComponent<Collider>());

        // Dedicated flat bridge collider flush with main Tier 2/3 (Y = 2.0f)
        GameObject bridgeCol = new GameObject("BridgeCollider");
        bridgeCol.transform.SetParent(bridge.transform);
        bridgeCol.transform.position = new Vector3(xPos, 2.0f, 0f);
        bridgeCol.isStatic = true;
        BoxCollider bc = bridgeCol.AddComponent<BoxCollider>();
        bc.center = new Vector3(0f, -0.05f, 0f);
        bc.size = new Vector3(1.8f, 0.1f, 5.4f);

        // Supports
        GameObject supportL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        supportL.transform.position = new Vector3(xPos, 1.0f, -2.4f);
        supportL.transform.localScale = new Vector3(1.8f, 2.0f, 0.5f);
        supportL.transform.SetParent(bridge.transform);
        supportL.GetComponent<Renderer>().material = woodMat;
        supportL.isStatic = true;

        GameObject supportR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        supportR.transform.position = new Vector3(xPos, 1.0f, 2.4f);
        supportR.transform.localScale = new Vector3(1.8f, 2.0f, 0.5f);
        supportR.transform.SetParent(bridge.transform);
        supportR.GetComponent<Renderer>().material = woodMat;
        supportR.isStatic = true;

        // Handrails
        float[] xCoords = { xPos - 0.85f, xPos + 0.85f };
        for (int i = 0; i < 2; i++)
        {
            float x = xCoords[i];
            
            // Posts
            for (int zVal = -2; zVal <= 2; zVal += 2)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.transform.position = new Vector3(x, 2.6f, zVal);
                post.transform.localScale = new Vector3(0.12f, 1.0f, 0.12f);
                post.transform.SetParent(bridge.transform);
                post.GetComponent<Renderer>().material = woodMat;
                post.isStatic = true;
            }

            // Top Rail
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.transform.position = new Vector3(x, 3.0f, 0f);
            rail.transform.localScale = new Vector3(0.1f, 0.12f, 5.4f);
            rail.transform.SetParent(bridge.transform);
            rail.GetComponent<Renderer>().material = woodMat;
            rail.isStatic = true;
        }
    }

    private static void BuildVillageStructures(GameObject root, Material woodMat, Material stoneMat, Material roofMat, Material brickMat, Material grassMat, Material plasterMat, Material clothRedMat, Material clothWhiteMat, Material clothGreenMat)
    {
        GameObject villageFolder = new GameObject("Village_Buildings");
        villageFolder.transform.SetParent(root.transform);

        // 1. Village Chief's House (House 1 - can enter!)
        // Located at X = -9, Z = 9 on Y = 2. Size: 6x5x5
        BuildHouse(villageFolder, -9, 2, 9, 6, 5, 5, plasterMat, stoneMat, roofMat, woodMat, "VillageChiefHouse", false, true, true, new Vector3(50f, -23.6f, 48.5f));

        // 2. Item Shop / Tavern (House 2 with Overhanging Jettying & Chimney - can enter!)
        // Located at X = 7, Z = 9 on Y = 2. Size: 7x5x5
        BuildHouse(villageFolder, 7, 2, 9, 7, 5, 5, plasterMat, stoneMat, roofMat, woodMat, "TavernShop", true, true, true, new Vector3(75f, -23.6f, 47.5f));

        // 3. (Removed) HillCottage was blocking the player's view from the hilltop

        // 4. Central Village Water Well (Revamped to Octagonal ring!)
        BuildWell(villageFolder, 0, 2, 7, stoneMat, woodMat);

        // 5. Plaza Market Stalls (Revamped to beautiful sloped awnings!)
        BuildMarketStall(villageFolder, 4, 2, 5, woodMat, stoneMat, clothRedMat, clothWhiteMat, "PotionStall");
        BuildMarketStall(villageFolder, 10, 2, 5, woodMat, stoneMat, clothGreenMat, clothWhiteMat, "FruitStall");
    }

    private static void BuildHouse(GameObject parent, int startX, int startY, int startZ, int sizeX, int sizeY, int sizeZ, Material wallMat, Material foundationMat, Material roofMat, Material woodMat, string name, bool hasJettying = false, bool hasChimney = false, bool hasEnterTransition = false, Vector3 interiorTeleportPos = default(Vector3))
    {
        GameObject houseObj = new GameObject(name);
        houseObj.transform.SetParent(parent.transform);
        houseObj.isStatic = true;

        // Top floor / roof baseline dimensions (avoids integer reassignments and truncation bugs)
        float rStartX = startX;
        float rSizeX = sizeX;
        float rStartZ = startZ;
        float rSizeZ = sizeZ;
        float rStartY = startY;
        float rSizeY = sizeY;

        // 1. Foundation layer - Spans the entire house footprint with one single solid stone box
        GameObject foundation = GameObject.CreatePrimitive(PrimitiveType.Cube);
        foundation.name = "FoundationBoard";
        foundation.transform.SetParent(houseObj.transform);
        foundation.transform.position = new Vector3(startX + sizeX / 2f - 0.5f, startY + 0.1f, startZ + sizeZ / 2f - 0.5f);
        foundation.transform.localScale = new Vector3(sizeX, 0.2f, sizeZ);
        foundation.GetComponent<Renderer>().material = foundationMat;
        foundation.isStatic = true;
        DestroyImmediate(foundation.GetComponent<Collider>());

        // Spawn a beautiful physical exterior door panel with Enter-key interaction trigger
        if (hasEnterTransition)
        {
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "HouseExteriorDoor";
            door.transform.SetParent(houseObj.transform);
            
            float doorX = startX + sizeX / 2f - 0.5f;
            door.transform.position = new Vector3(doorX, startY + 1.1f, startZ - 0.08f);
            door.transform.localScale = new Vector3(1.1f, 1.8f, 0.12f);
            door.GetComponent<Renderer>().material = woodMat; // rich dark wood panel
            door.isStatic = true;
            DestroyImmediate(door.GetComponent<Collider>()); // Destroy the solid primitive collider to allow trigger entry!

            // Small shiny stone/brass door knob handle
            GameObject knob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            knob.name = "DoorKnob";
            knob.transform.SetParent(door.transform);
            knob.transform.localPosition = new Vector3(0.35f, 0f, -0.45f);
            knob.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            knob.GetComponent<Renderer>().material = foundationMat;
            DestroyImmediate(knob.GetComponent<Collider>());

            // Trigger zone for Enter key teleportation
            BoxCollider trigger = door.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2.2f, 1.8f, 22.0f);

            // Door transition script
            HouseDoorTransition transition = door.AddComponent<HouseDoorTransition>();
            transition.targetPosition = interiorTeleportPos;
        }

        // 2. Seamless Wall Facades and Tudor Framing
        float wallHeight = sizeY - 2;
        float yBottom = startY + 0.2f;
        float yTop = startY + 0.2f + wallHeight;
        float wallCenterY = startY + 0.2f + wallHeight / 2f;

        if (hasJettying)
        {
            // TavernShop has jettying overhang!
            // First Floor: Y from startY+0.2 to startY+2.0
            float yBottom1 = startY + 0.2f;
            float yTop1 = startY + 2.0f;
            float h1 = 1.8f;
            float centerY1 = yBottom1 + h1 / 2f;

            // First Floor walls
            GameObject backWall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall1.name = "BackWall_1st";
            backWall1.transform.SetParent(houseObj.transform);
            backWall1.transform.position = new Vector3(startX + sizeX / 2f - 0.5f, centerY1, startZ + sizeZ - 1f);
            backWall1.transform.localScale = new Vector3(sizeX, h1, 0.2f);
            backWall1.GetComponent<Renderer>().material = wallMat;
            backWall1.isStatic = true;

            GameObject leftWall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall1.name = "LeftWall_1st";
            leftWall1.transform.SetParent(houseObj.transform);
            leftWall1.transform.position = new Vector3(startX, centerY1, startZ + sizeZ / 2f - 0.5f);
            leftWall1.transform.localScale = new Vector3(0.2f, h1, sizeZ - 1.8f);
            leftWall1.GetComponent<Renderer>().material = wallMat;
            leftWall1.isStatic = true;

            GameObject rightWall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall1.name = "RightWall_1st";
            rightWall1.transform.SetParent(houseObj.transform);
            rightWall1.transform.position = new Vector3(startX + sizeX - 1f, centerY1, startZ + sizeZ / 2f - 0.5f);
            rightWall1.transform.localScale = new Vector3(0.2f, h1, sizeZ - 1.8f);
            rightWall1.GetComponent<Renderer>().material = wallMat;
            rightWall1.isStatic = true;

            float frontWidth1 = (sizeX - 1.2f) / 2f;
            float frontOffset1 = sizeX / 2f - frontWidth1 / 2f - 0.6f;
            GameObject fL1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fL1.name = "FrontWallLeft_1st";
            fL1.transform.SetParent(houseObj.transform);
            fL1.transform.position = new Vector3(startX + sizeX / 2f - 0.5f - frontOffset1 - 0.6f, centerY1, startZ);
            fL1.transform.localScale = new Vector3(frontWidth1, h1, 0.2f);
            fL1.GetComponent<Renderer>().material = wallMat;
            fL1.isStatic = true;

            GameObject fR1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fR1.name = "FrontWallRight_1st";
            fR1.transform.SetParent(houseObj.transform);
            fR1.transform.position = new Vector3(startX + sizeX / 2f - 0.5f + frontOffset1 + 0.6f, centerY1, startZ);
            fR1.transform.localScale = new Vector3(frontWidth1, h1, 0.2f);
            fR1.GetComponent<Renderer>().material = wallMat;
            fR1.isStatic = true;

            float doorH = 1.6f;
            float lintelH = h1 - doorH;
            if (lintelH > 0f)
            {
                GameObject lintel1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lintel1.name = "Lintel_1st";
                lintel1.transform.SetParent(houseObj.transform);
                lintel1.transform.position = new Vector3(startX + sizeX / 2f - 0.5f, centerY1 + doorH / 2f, startZ);
                lintel1.transform.localScale = new Vector3(1.2f, lintelH, 0.2f);
                lintel1.GetComponent<Renderer>().material = wallMat;
                lintel1.isStatic = true;
            }

            // 1st Floor Timber Frames
            CreateCornerPost(houseObj, startX, yBottom1, h1, startZ, woodMat);
            CreateCornerPost(houseObj, startX + sizeX - 1f, yBottom1, h1, startZ, woodMat);
            CreateCornerPost(houseObj, startX, yBottom1, h1, startZ + sizeZ - 1f, woodMat);
            CreateCornerPost(houseObj, startX + sizeX - 1f, yBottom1, h1, startZ + sizeZ - 1f, woodMat);

            CreateHorizontalBeam(houseObj, startX + sizeX/2f - 0.5f, yBottom1 + 0.08f, startZ - 0.11f, sizeX, 0.14f, 0.08f, woodMat, false);
            CreateHorizontalBeam(houseObj, startX + sizeX/2f - 0.5f, yBottom1 + 0.08f, startZ + sizeZ - 0.89f, sizeX, 0.14f, 0.08f, woodMat, false);
            CreateHorizontalBeam(houseObj, startX - 0.11f, yBottom1 + 0.08f, startZ + sizeZ/2f - 0.5f, sizeZ, 0.14f, 0.08f, woodMat, true);
            CreateHorizontalBeam(houseObj, startX + sizeX - 0.89f, yBottom1 + 0.08f, startZ + sizeZ/2f - 0.5f, sizeZ, 0.14f, 0.08f, woodMat, true);

            CreateHorizontalBeam(houseObj, startX + sizeX/2f - 0.5f, yTop1 - 0.08f, startZ - 0.11f, sizeX, 0.14f, 0.08f, woodMat, false);
            CreateHorizontalBeam(houseObj, startX + sizeX/2f - 0.5f, yTop1 - 0.08f, startZ + sizeZ - 0.89f, sizeX, 0.14f, 0.08f, woodMat, false);
            CreateHorizontalBeam(houseObj, startX - 0.11f, yTop1 - 0.08f, startZ + sizeZ/2f - 0.5f, sizeZ, 0.14f, 0.08f, woodMat, true);
            CreateHorizontalBeam(houseObj, startX + sizeX - 0.89f, yTop1 - 0.08f, startZ + sizeZ/2f - 0.5f, sizeZ, 0.14f, 0.08f, woodMat, true);

            for (float z = startZ + 1.5f; z <= startZ + sizeZ - 2f; z += 1.5f)
            {
                CreateVerticalPost(houseObj, startX - 0.11f, centerY1, z, h1, 0.12f, 0.08f, woodMat, true);
                CreateVerticalPost(houseObj, startX + sizeX - 0.89f, centerY1, z, h1, 0.12f, 0.08f, woodMat, true);
                
                CreateDiagonalBrace(houseObj, new Vector3(startX - 0.11f, yBottom1 + 0.15f, z - 1.4f), new Vector3(startX - 0.11f, yTop1 - 0.15f, z - 0.1f), woodMat, true);
                CreateDiagonalBrace(houseObj, new Vector3(startX + sizeX - 0.89f, yBottom1 + 0.15f, z - 1.4f), new Vector3(startX + sizeX - 0.89f, yTop1 - 0.15f, z - 0.1f), woodMat, true);
            }

            CreateVerticalPost(houseObj, startX + sizeX/2f - 0.7f, centerY1, startZ - 0.11f, h1, 0.12f, 0.08f, woodMat, false);
            CreateVerticalPost(houseObj, startX + sizeX/2f + 0.7f, centerY1, startZ - 0.11f, h1, 0.12f, 0.08f, woodMat, false);

            // Second Floor: Y from startY+2.0 to startY+sizeY-1.8
            float yBottom2 = startY + 2.0f;
            float yTop2 = startY + sizeY - 1.8f;
            float h2 = sizeY - 3.8f;
            float centerY2 = yBottom2 + h2 / 2f;

            float ox = 0.35f;
            float startX2 = startX - ox;
            float sizeX2 = sizeX + ox * 2f;
            float startZ2 = startZ - ox;
            float sizeZ2 = sizeZ + ox; // only overhangs front

            // Jettying Bracket supports underneath the overhang
            for (float x = startX; x <= startX + sizeX - 1f; x += 1.5f)
            {
                GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bracket.name = "JettyingBracket_Front";
                bracket.transform.SetParent(houseObj.transform);
                bracket.transform.position = new Vector3(x, yBottom2 - 0.15f, startZ - 0.15f);
                bracket.transform.localScale = new Vector3(0.14f, 0.35f, 0.35f);
                bracket.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
                bracket.GetComponent<Renderer>().material = woodMat;
                bracket.isStatic = true;
                DestroyImmediate(bracket.GetComponent<Collider>());
            }
            for (float z = startZ + 1f; z <= startZ + sizeZ - 2f; z += 1.5f)
            {
                GameObject bracketL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bracketL.name = "JettyingBracket_Left";
                bracketL.transform.SetParent(houseObj.transform);
                bracketL.transform.position = new Vector3(startX - 0.15f, yBottom2 - 0.15f, z);
                bracketL.transform.localScale = new Vector3(0.35f, 0.35f, 0.14f);
                bracketL.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
                bracketL.GetComponent<Renderer>().material = woodMat;
                bracketL.isStatic = true;
                DestroyImmediate(bracketL.GetComponent<Collider>());

                GameObject bracketR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bracketR.name = "JettyingBracket_Right";
                bracketR.transform.SetParent(houseObj.transform);
                bracketR.transform.position = new Vector3(startX + sizeX - 1f + 0.15f, yBottom2 - 0.15f, z);
                bracketR.transform.localScale = new Vector3(0.35f, 0.35f, 0.14f);
                bracketR.transform.rotation = Quaternion.Euler(0f, 0f, -45f);
                bracketR.GetComponent<Renderer>().material = woodMat;
                bracketR.isStatic = true;
                DestroyImmediate(bracketR.GetComponent<Collider>());
            }

            // Second Floor Facades
            GameObject backWall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall2.name = "BackWall_2nd";
            backWall2.transform.SetParent(houseObj.transform);
            backWall2.transform.position = new Vector3(startX2 + sizeX2 / 2f - 0.5f, centerY2, startZ2 + sizeZ2 - 1f);
            backWall2.transform.localScale = new Vector3(sizeX2, h2, 0.2f);
            backWall2.GetComponent<Renderer>().material = wallMat;
            backWall2.isStatic = true;

            GameObject leftWall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall2.name = "LeftWall_2nd";
            leftWall2.transform.SetParent(houseObj.transform);
            leftWall2.transform.position = new Vector3(startX2, centerY2, startZ2 + sizeZ2 / 2f - 0.5f);
            leftWall2.transform.localScale = new Vector3(0.2f, h2, sizeZ2 - 1.8f);
            leftWall2.GetComponent<Renderer>().material = wallMat;
            leftWall2.isStatic = true;

            GameObject rightWall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall2.name = "RightWall_2nd";
            rightWall2.transform.SetParent(houseObj.transform);
            rightWall2.transform.position = new Vector3(startX2 + sizeX2 - 1f, centerY2, startZ2 + sizeZ2 / 2f - 0.5f);
            rightWall2.transform.localScale = new Vector3(0.2f, h2, sizeZ2 - 1.8f);
            rightWall2.GetComponent<Renderer>().material = wallMat;
            rightWall2.isStatic = true;

            GameObject frontWall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontWall2.name = "FrontWall_2nd";
            frontWall2.transform.SetParent(houseObj.transform);
            frontWall2.transform.position = new Vector3(startX2 + sizeX2 / 2f - 0.5f, centerY2, startZ2);
            frontWall2.transform.localScale = new Vector3(sizeX2, h2, 0.2f);
            frontWall2.GetComponent<Renderer>().material = wallMat;
            frontWall2.isStatic = true;

            // Windows on 2nd floor
            float[] w2Offsets = { 1.5f, sizeX2 - 2.5f };
            for (int i = 0; i < 2; i++)
            {
                float wx = startX2 + w2Offsets[i];
                float wy = centerY2;
                GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
                window.name = "WindowGlow_2nd";
                window.transform.SetParent(houseObj.transform);
                window.transform.position = new Vector3(wx, wy, startZ2 - 0.11f);
                window.transform.localScale = new Vector3(0.7f, 0.7f, 0.05f);
                
                Renderer r = window.GetComponent<Renderer>();
                r.material = new Material(wallMat.shader);
                r.material.SetColor("_BaseColor", new Color(1.0f, 0.85f, 0.35f, 1f));
                r.material.SetFloat("_Smoothness", 0.8f);
                r.material.EnableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", new Color(1.0f, 0.85f, 0.35f, 1f) * 1.5f);
                window.isStatic = true;

                CreateRectFrame(houseObj, wx, wy, startZ2 - 0.12f, 0.8f, 0.8f, 0.08f, woodMat);
            }

            // 2nd Floor Tudor Framing
            CreateCornerPost(houseObj, startX2, yBottom2, h2, startZ2, woodMat);
            CreateCornerPost(houseObj, startX2 + sizeX2 - 1f, yBottom2, h2, startZ2, woodMat);
            CreateCornerPost(houseObj, startX2, yBottom2, h2, startZ2 + sizeZ2 - 1f, woodMat);
            CreateCornerPost(houseObj, startX2 + sizeX2 - 1f, yBottom2, h2, startZ2 + sizeZ2 - 1f, woodMat);

            CreateHorizontalBeam(houseObj, startX2 + sizeX2/2f - 0.5f, yBottom2 + 0.08f, startZ2 - 0.11f, sizeX2, 0.14f, 0.08f, woodMat, false);
            CreateHorizontalBeam(houseObj, startX2 + sizeX2/2f - 0.5f, yBottom2 + 0.08f, startZ2 + sizeZ2 - 0.89f, sizeX2, 0.14f, 0.08f, woodMat, false);
            CreateHorizontalBeam(houseObj, startX2 - 0.11f, yBottom2 + 0.08f, startZ2 + sizeZ2/2f - 0.5f, sizeZ2, 0.14f, 0.08f, woodMat, true);
            CreateHorizontalBeam(houseObj, startX2 + sizeX2 - 0.89f, yBottom2 + 0.08f, startZ2 + sizeZ2/2f - 0.5f, sizeZ2, 0.14f, 0.08f, woodMat, true);

            CreateHorizontalBeam(houseObj, startX2 + sizeX2/2f - 0.5f, yTop2 - 0.08f, startZ2 - 0.11f, sizeX2, 0.14f, 0.08f, woodMat, false);
            CreateHorizontalBeam(houseObj, startX2 + sizeX2/2f - 0.5f, yTop2 - 0.08f, startZ2 + sizeZ2 - 0.89f, sizeX2, 0.14f, 0.08f, woodMat, false);
            CreateHorizontalBeam(houseObj, startX2 - 0.11f, yTop2 - 0.08f, startZ2 + sizeZ2/2f - 0.5f, sizeZ2, 0.14f, 0.08f, woodMat, true);
            CreateHorizontalBeam(houseObj, startX2 + sizeX2 - 0.89f, yTop2 - 0.08f, startZ2 + sizeZ2/2f - 0.5f, sizeZ2, 0.14f, 0.08f, woodMat, true);

            for (float x = startX2 + 2f; x <= startX2 + sizeX2 - 2f; x += 2f)
            {
                CreateVerticalPost(houseObj, x, centerY2, startZ2 + sizeZ2 - 0.89f, h2, 0.12f, 0.08f, woodMat, false);
                CreateDiagonalBrace(houseObj, new Vector3(x - 1.8f, yBottom2 + 0.15f, startZ2 + sizeZ2 - 0.89f), new Vector3(x - 0.2f, yTop2 - 0.15f, startZ2 + sizeZ2 - 0.89f), woodMat, false);
                CreateDiagonalBrace(houseObj, new Vector3(x - 0.2f, yBottom2 + 0.15f, startZ2 + sizeZ2 - 0.89f), new Vector3(x - 1.8f, yTop2 - 0.15f, startZ2 + sizeZ2 - 0.89f), woodMat, false);
            }

            rStartX = startX2;
            rSizeX = sizeX2;
            rStartZ = startZ2;
            rSizeZ = sizeZ2;
            rStartY = startY + sizeY - 2;
            rSizeY = 2f;
        }
        else
        {
            // Standard Single Floor
            GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall.name = "BackWallFacade";
            backWall.transform.SetParent(houseObj.transform);
            backWall.transform.position = new Vector3(startX + sizeX / 2f - 0.5f, wallCenterY, startZ + sizeZ - 1f);
            backWall.transform.localScale = new Vector3(sizeX, wallHeight, 0.2f);
            backWall.GetComponent<Renderer>().material = wallMat;
            backWall.isStatic = true;

            GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "LeftWallFacade";
            leftWall.transform.SetParent(houseObj.transform);
            leftWall.transform.position = new Vector3(startX, wallCenterY, startZ + sizeZ / 2f - 0.5f);
            leftWall.transform.localScale = new Vector3(0.2f, wallHeight, sizeZ - 1.8f);
            leftWall.GetComponent<Renderer>().material = wallMat;
            leftWall.isStatic = true;

            GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "RightWallFacade";
            rightWall.transform.SetParent(houseObj.transform);
            rightWall.transform.position = new Vector3(startX + sizeX - 1f, wallCenterY, startZ + sizeZ / 2f - 0.5f);
            rightWall.transform.localScale = new Vector3(0.2f, wallHeight, sizeZ - 1.8f);
            rightWall.GetComponent<Renderer>().material = wallMat;
            rightWall.isStatic = true;

            float frontSegmentWidth = (sizeX - 1.2f) / 2f;
            float frontSegmentCenterXOffset = sizeX / 2f - frontSegmentWidth / 2f - 0.6f;
            GameObject leftFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftFront.name = "FrontWallLeftFacade";
            leftFront.transform.SetParent(houseObj.transform);
            leftFront.transform.position = new Vector3(startX + sizeX / 2f - 0.5f - frontSegmentCenterXOffset - 0.6f, wallCenterY, startZ);
            leftFront.transform.localScale = new Vector3(frontSegmentWidth, wallHeight, 0.2f);
            leftFront.GetComponent<Renderer>().material = wallMat;
            leftFront.isStatic = true;

            GameObject rightFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightFront.name = "FrontWallRightFacade";
            rightFront.transform.SetParent(houseObj.transform);
            rightFront.transform.position = new Vector3(startX + sizeX / 2f - 0.5f + frontSegmentCenterXOffset + 0.6f, wallCenterY, startZ);
            rightFront.transform.localScale = new Vector3(frontSegmentWidth, wallHeight, 0.2f);
            rightFront.GetComponent<Renderer>().material = wallMat;
            rightFront.isStatic = true;

            float doorHeight = 1.8f;
            float lintelHeight = wallHeight - doorHeight;
            if (lintelHeight > 0f)
            {
                GameObject lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lintel.name = "FrontWallLintel";
                lintel.transform.SetParent(houseObj.transform);
                lintel.transform.position = new Vector3(startX + sizeX / 2f - 0.5f, wallCenterY + doorHeight / 2f, startZ);
                lintel.transform.localScale = new Vector3(1.2f, lintelHeight, 0.2f);
                lintel.GetComponent<Renderer>().material = wallMat;
                lintel.isStatic = true;
            }

            // Windows on single floor
            float[] windowXOffsets = { 1.2f, sizeX - 2.2f };
            for (int i = 0; i < 2; i++)
            {
                float wx = startX + windowXOffsets[i];
                float wy = startY + 1.2f;
                GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
                window.name = "WindowGlowBoard";
                window.transform.SetParent(houseObj.transform);
                window.transform.position = new Vector3(wx, wy, startZ - 0.11f);
                window.transform.localScale = new Vector3(0.7f, 0.7f, 0.05f);
                
                Renderer r = window.GetComponent<Renderer>();
                r.material = new Material(wallMat.shader);
                r.material.SetColor("_BaseColor", new Color(1.0f, 0.85f, 0.35f, 1f));
                r.material.SetFloat("_Smoothness", 0.8f);
                r.material.EnableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", new Color(1.0f, 0.85f, 0.35f, 1f) * 1.5f);
                window.isStatic = true;

                CreateRectFrame(houseObj, wx, wy, startZ - 0.12f, 0.8f, 0.8f, 0.08f, woodMat);
            }

            // 1st Floor Tudor Framing
            CreateCornerPost(houseObj, startX, yBottom, wallHeight, startZ, woodMat);
            CreateCornerPost(houseObj, startX + sizeX - 1f, yBottom, wallHeight, startZ, woodMat);
            CreateCornerPost(houseObj, startX, yBottom, wallHeight, startZ + sizeZ - 1f, woodMat);
            CreateCornerPost(houseObj, startX + sizeX - 1f, yBottom, wallHeight, startZ + sizeZ - 1f, woodMat);

            CreateHorizontalBeam(houseObj, startX + sizeX/2f - 0.5f, yBottom + 0.08f, startZ - 0.11f, sizeX, 0.14f, 0.08f, woodMat, false);
            CreateHorizontalBeam(houseObj, startX + sizeX/2f - 0.5f, yBottom + 0.08f, startZ + sizeZ - 0.89f, sizeX, 0.14f, 0.08f, woodMat, false);
            CreateHorizontalBeam(houseObj, startX - 0.11f, yBottom + 0.08f, startZ + sizeZ/2f - 0.5f, sizeZ, 0.14f, 0.08f, woodMat, true);
            CreateHorizontalBeam(houseObj, startX + sizeX - 0.89f, yBottom + 0.08f, startZ + sizeZ/2f - 0.5f, sizeZ, 0.14f, 0.08f, woodMat, true);

            CreateHorizontalBeam(houseObj, startX + sizeX/2f - 0.5f, yTop - 0.08f, startZ - 0.11f, sizeX, 0.14f, 0.08f, woodMat, false);
            CreateHorizontalBeam(houseObj, startX + sizeX/2f - 0.5f, yTop - 0.08f, startZ + sizeZ - 0.89f, sizeX, 0.14f, 0.08f, woodMat, false);
            CreateHorizontalBeam(houseObj, startX - 0.11f, yTop - 0.08f, startZ + sizeZ/2f - 0.5f, sizeZ, 0.14f, 0.08f, woodMat, true);
            CreateHorizontalBeam(houseObj, startX + sizeX - 0.89f, yTop - 0.08f, startZ + sizeZ/2f - 0.5f, sizeZ, 0.14f, 0.08f, woodMat, true);

            for (float z = startZ + 1.5f; z <= startZ + sizeZ - 2f; z += 1.5f)
            {
                CreateVerticalPost(houseObj, startX - 0.11f, wallCenterY, z, wallHeight, 0.12f, 0.08f, woodMat, true);
                CreateVerticalPost(houseObj, startX + sizeX - 0.89f, wallCenterY, z, wallHeight, 0.12f, 0.08f, woodMat, true);

                CreateDiagonalBrace(houseObj, new Vector3(startX - 0.11f, yBottom + 0.15f, z - 1.4f), new Vector3(startX - 0.11f, yTop - 0.15f, z - 0.1f), woodMat, true);
                CreateDiagonalBrace(houseObj, new Vector3(startX + sizeX - 0.89f, yBottom + 0.15f, z - 1.4f), new Vector3(startX + sizeX - 0.89f, yTop - 0.15f, z - 0.1f), woodMat, true);
            }

            for (float x = startX + 2f; x <= startX + sizeX - 2f; x += 2f)
            {
                CreateVerticalPost(houseObj, x, wallCenterY, startZ + sizeZ - 0.89f, wallHeight, 0.12f, 0.08f, woodMat, false);
                CreateDiagonalBrace(houseObj, new Vector3(x - 1.8f, yBottom + 0.15f, startZ + sizeZ - 0.89f), new Vector3(x - 0.2f, yTop - 0.15f, startZ + sizeZ - 0.89f), woodMat, false);
                CreateDiagonalBrace(houseObj, new Vector3(x - 0.2f, yBottom + 0.15f, startZ + sizeZ - 0.89f), new Vector3(x - 1.8f, yTop - 0.15f, startZ + sizeZ - 0.89f), woodMat, false);
            }

            CreateVerticalPost(houseObj, startX + sizeX/2f - 0.7f, wallCenterY, startZ - 0.11f, wallHeight, 0.12f, 0.08f, woodMat, false);
            CreateVerticalPost(houseObj, startX + sizeX/2f + 0.7f, wallCenterY, startZ - 0.11f, wallHeight, 0.12f, 0.08f, woodMat, false);
        }

        // 3. Overhanging Peaked Sloped Roof Panels (mathematical seamless tilted boards)
        float angle = 35f; // steep medieval roof angle
        float roofHeight = rStartY + rSizeY - 1.8f;
        float roofCenter = rStartX + (rSizeX - 1f) / 2.0f;
        float halfWidth = rSizeX / 2.0f + 0.7f; // overhang on sides
        float cosA = Mathf.Cos(angle * Mathf.Deg2Rad);
        float sinA = Mathf.Sin(angle * Mathf.Deg2Rad);

        float slopeLength = halfWidth / cosA;
        float slopeHeight = halfWidth * sinA;

        // Left Slope Panel
        GameObject leftRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftRoof.name = "LeftRoofPanel";
        leftRoof.transform.SetParent(houseObj.transform);
        leftRoof.transform.localScale = new Vector3(slopeLength, 0.16f, rSizeZ + 1.4f); // overhang on Z both sides
        leftRoof.transform.position = new Vector3(
            roofCenter - halfWidth / 2f,
            roofHeight + slopeHeight / 2f - 0.08f,
            rStartZ + (rSizeZ - 1f) / 2f
        );
        leftRoof.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        leftRoof.GetComponent<Renderer>().material = roofMat;
        leftRoof.isStatic = true;

        // Right Slope Panel
        GameObject rightRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightRoof.name = "RightRoofPanel";
        rightRoof.transform.SetParent(houseObj.transform);
        rightRoof.transform.localScale = new Vector3(slopeLength, 0.16f, rSizeZ + 1.4f);
        rightRoof.transform.position = new Vector3(
            roofCenter + halfWidth / 2f,
            roofHeight + slopeHeight / 2f - 0.08f,
            rStartZ + (rSizeZ - 1f) / 2f
        );
        rightRoof.transform.rotation = Quaternion.Euler(0f, 0f, -angle);
        rightRoof.GetComponent<Renderer>().material = roofMat;
        rightRoof.isStatic = true;

        // Gable ends
        GameObject frontGable = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontGable.name = "FrontGableFacade";
        frontGable.transform.SetParent(houseObj.transform);
        frontGable.transform.position = new Vector3(rStartX + rSizeX / 2f - 0.5f, roofHeight + slopeHeight / 2f - 0.1f, rStartZ);
        frontGable.transform.localScale = new Vector3(rSizeX - 0.2f, slopeHeight, 0.18f);
        frontGable.GetComponent<Renderer>().material = wallMat;
        frontGable.isStatic = true;

        GameObject backGable = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backGable.name = "BackGableFacade";
        backGable.transform.SetParent(houseObj.transform);
        backGable.transform.position = new Vector3(rStartX + rSizeX / 2f - 0.5f, roofHeight + slopeHeight / 2f - 0.1f, rStartZ + rSizeZ - 1f);
        backGable.transform.localScale = new Vector3(rSizeX - 0.2f, slopeHeight, 0.18f);
        backGable.GetComponent<Renderer>().material = wallMat;
        backGable.isStatic = true;

        // Decorative framing on front/back gables
        CreateVerticalPost(houseObj, roofCenter, roofHeight + slopeHeight/2f - 0.1f, rStartZ - 0.11f, slopeHeight, 0.12f, 0.08f, woodMat, false);
        CreateVerticalPost(houseObj, roofCenter, roofHeight + slopeHeight/2f - 0.1f, rStartZ + rSizeZ - 0.89f, slopeHeight, 0.12f, 0.08f, woodMat, false);

        // 4. Thick wooden Bargeboards along roof edges
        float bargeZFront = rStartZ - 0.72f;
        float bargeZBack = rStartZ + rSizeZ + 0.52f;

        // Front Bargeboards
        GameObject bLF = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bLF.name = "Bargeboard_FrontLeft";
        bLF.transform.SetParent(houseObj.transform);
        bLF.transform.localScale = new Vector3(slopeLength + 0.1f, 0.2f, 0.2f);
        bLF.transform.position = new Vector3(roofCenter - halfWidth / 2f, roofHeight + slopeHeight / 2f, bargeZFront);
        bLF.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        bLF.GetComponent<Renderer>().material = woodMat;
        bLF.isStatic = true;
        DestroyImmediate(bLF.GetComponent<Collider>());

        GameObject bRF = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bRF.name = "Bargeboard_FrontRight";
        bRF.transform.SetParent(houseObj.transform);
        bRF.transform.localScale = new Vector3(slopeLength + 0.1f, 0.2f, 0.2f);
        bRF.transform.position = new Vector3(roofCenter + halfWidth / 2f, roofHeight + slopeHeight / 2f, bargeZFront);
        bRF.transform.rotation = Quaternion.Euler(0f, 0f, -angle);
        bRF.GetComponent<Renderer>().material = woodMat;
        bRF.isStatic = true;
        DestroyImmediate(bRF.GetComponent<Collider>());

        // Back Bargeboards
        GameObject bLB = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bLB.name = "Bargeboard_BackLeft";
        bLB.transform.SetParent(houseObj.transform);
        bLB.transform.localScale = new Vector3(slopeLength + 0.1f, 0.2f, 0.2f);
        bLB.transform.position = new Vector3(roofCenter - halfWidth / 2f, roofHeight + slopeHeight / 2f, bargeZBack);
        bLB.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        bLB.GetComponent<Renderer>().material = woodMat;
        bLB.isStatic = true;
        DestroyImmediate(bLB.GetComponent<Collider>());

        GameObject bRB = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bRB.name = "Bargeboard_BackRight";
        bRB.transform.SetParent(houseObj.transform);
        bRB.transform.localScale = new Vector3(slopeLength + 0.1f, 0.2f, 0.2f);
        bRB.transform.position = new Vector3(roofCenter + halfWidth / 2f, roofHeight + slopeHeight / 2f, bargeZBack);
        bRB.transform.rotation = Quaternion.Euler(0f, 0f, -angle);
        bRB.GetComponent<Renderer>().material = woodMat;
        bRB.isStatic = true;
        DestroyImmediate(bRB.GetComponent<Collider>());

        // 5. Vertical spire finial at the peaked gables
        GameObject finialFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
        finialFront.name = "SpireFinial_Front";
        finialFront.transform.SetParent(houseObj.transform);
        finialFront.transform.position = new Vector3(roofCenter, roofHeight + slopeHeight + 0.35f, bargeZFront);
        finialFront.transform.localScale = new Vector3(0.12f, 0.8f, 0.12f);
        finialFront.GetComponent<Renderer>().material = woodMat;
        finialFront.isStatic = true;
        DestroyImmediate(finialFront.GetComponent<Collider>());

        GameObject finialBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        finialBack.name = "SpireFinial_Back";
        finialBack.transform.SetParent(houseObj.transform);
        finialBack.transform.position = new Vector3(roofCenter, roofHeight + slopeHeight + 0.35f, bargeZBack);
        finialBack.transform.localScale = new Vector3(0.12f, 0.8f, 0.12f);
        finialBack.GetComponent<Renderer>().material = woodMat;
        finialBack.isStatic = true;
        DestroyImmediate(finialBack.GetComponent<Collider>());

        // 6. Entryway door porch awning supported by diagonal corbels!
        GameObject porchAwning = GameObject.CreatePrimitive(PrimitiveType.Cube);
        porchAwning.name = "DoorPorchAwning";
        porchAwning.transform.SetParent(houseObj.transform);
        porchAwning.transform.position = new Vector3(startX + sizeX/2f - 0.5f, startY + 2.05f, startZ - 0.38f);
        porchAwning.transform.localScale = new Vector3(1.6f, 0.08f, 1.0f);
        porchAwning.transform.rotation = Quaternion.Euler(30f, 0f, 0f);
        porchAwning.GetComponent<Renderer>().material = roofMat;
        porchAwning.isStatic = true;
        DestroyImmediate(porchAwning.GetComponent<Collider>());

        GameObject porchBraceL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        porchBraceL.name = "DoorPorchCorbel_Left";
        porchBraceL.transform.SetParent(houseObj.transform);
        porchBraceL.transform.position = new Vector3(startX + sizeX/2f - 1.2f, startY + 1.8f, startZ - 0.15f);
        porchBraceL.transform.localScale = new Vector3(0.12f, 0.35f, 0.35f);
        porchBraceL.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        porchBraceL.GetComponent<Renderer>().material = woodMat;
        porchBraceL.isStatic = true;
        DestroyImmediate(postL_collider_removal(porchBraceL));

        GameObject porchBraceR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        porchBraceR.name = "DoorPorchCorbel_Right";
        porchBraceR.transform.SetParent(houseObj.transform);
        porchBraceR.transform.position = new Vector3(startX + sizeX/2f + 0.2f, startY + 1.8f, startZ - 0.15f);
        porchBraceR.transform.localScale = new Vector3(0.12f, 0.35f, 0.35f);
        porchBraceR.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        porchBraceR.GetComponent<Renderer>().material = woodMat;
        porchBraceR.isStatic = true;
        DestroyImmediate(postL_collider_removal(porchBraceR));

        // 7. Chimney Installation
        if (hasChimney)
        {
            float chimneyX = rStartX + rSizeX - 0.35f;
            float chimneyZ = rStartZ + rSizeZ / 2f - 0.5f;
            float chimneyH = rSizeY + 1.8f;

            // Fireplace base (Y = 1.0f base to 2.5f)
            float baseWidth = 0.85f;
            float baseX = chimneyX;
            if (hasJettying)
            {
                // Thicker base bridging the gap between first-floor wall and second-floor overhang
                baseWidth = 1.4f;
                baseX = startX + sizeX - 0.35f; 
            }

            GameObject chBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chBase.name = "ChimneyBase";
            chBase.transform.SetParent(houseObj.transform);
            chBase.transform.position = new Vector3(baseX, rStartY + 0.45f, chimneyZ);
            chBase.transform.localScale = new Vector3(baseWidth, 2.5f, 1.2f);
            chBase.GetComponent<Renderer>().material = foundationMat;
            chBase.isStatic = true;

            // Chimney shaft
            GameObject chShaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chShaft.name = "ChimneyShaft";
            chShaft.transform.SetParent(houseObj.transform);
            chShaft.transform.position = new Vector3(chimneyX, rStartY + 0.45f + chimneyH / 2f, chimneyZ);
            chShaft.transform.localScale = new Vector3(0.65f, chimneyH, 0.75f);
            chShaft.GetComponent<Renderer>().material = foundationMat;
            chShaft.isStatic = true;

            // Chimney cap
            GameObject chCap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chCap.name = "ChimneyCap";
            chCap.transform.SetParent(houseObj.transform);
            chCap.transform.position = new Vector3(chimneyX, rStartY + 0.45f + chimneyH + 0.08f, chimneyZ);
            chCap.transform.localScale = new Vector3(0.75f, 0.16f, 0.85f);
            chCap.GetComponent<Renderer>().material = foundationMat;
            chCap.isStatic = true;

            // Chimney pot / flue pipe
            GameObject chPot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chPot.name = "ChimneyPot";
            chPot.transform.SetParent(houseObj.transform);
            chPot.transform.position = new Vector3(chimneyX, rStartY + 0.45f + chimneyH + 0.35f, chimneyZ);
            chPot.transform.localScale = new Vector3(0.3f, 0.38f, 0.3f);
            chPot.GetComponent<Renderer>().material = foundationMat; // High-quality stone mat instead of woodMat
            chPot.isStatic = true;

            // Puffy semi-transparent chimney smoke spheres
            GameObject smoke1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            smoke1.name = "ChimneySmoke_1";
            smoke1.transform.SetParent(houseObj.transform);
            smoke1.transform.position = chPot.transform.position + new Vector3(0f, 0.38f, 0f);
            smoke1.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            DestroyImmediate(smoke1.GetComponent<Collider>());
            
            Renderer smR1 = smoke1.GetComponent<Renderer>();
            smR1.material = new Material(wallMat.shader);
            smR1.material.SetColor("_BaseColor", new Color(0.9f, 0.9f, 0.9f, 0.58f));
            smR1.material.SetFloat("_Smoothness", 0.0f);
            smR1.shadowCastingMode = ShadowCastingMode.Off;

            GameObject smoke2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            smoke2.name = "ChimneySmoke_2";
            smoke2.transform.SetParent(houseObj.transform);
            smoke2.transform.position = chPot.transform.position + new Vector3(0.12f, 0.72f, 0.06f);
            smoke2.transform.localScale = new Vector3(0.48f, 0.48f, 0.48f);
            DestroyImmediate(smoke2.GetComponent<Collider>());

            Renderer smR2 = smoke2.GetComponent<Renderer>();
            smR2.material = smR1.material;
            smR2.shadowCastingMode = ShadowCastingMode.Off;
        }
    }

    private static Collider postL_collider_removal(GameObject obj)
    {
        return obj.GetComponent<Collider>();
    }

    // Modern architectural JRPG helpers
    private static void CreateCornerPost(GameObject parent, float x, float y, float h, float z, Material mat)
    {
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.name = "TudorCornerPost";
        post.transform.SetParent(parent.transform);
        post.transform.position = new Vector3(x, y + h / 2f, z);
        post.transform.localScale = new Vector3(0.24f, h, 0.24f);
        post.GetComponent<Renderer>().material = mat;
        post.isStatic = true;
        DestroyImmediate(post.GetComponent<Collider>());
    }

    private static void CreateVerticalPost(GameObject parent, float x, float y, float z, float h, float width, float thick, Material mat, bool isYZPlane)
    {
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.name = "TudorVerticalStud";
        post.transform.SetParent(parent.transform);
        post.transform.position = new Vector3(x, y, z);
        if (isYZPlane)
        {
            post.transform.localScale = new Vector3(thick, h, width);
        }
        else
        {
            post.transform.localScale = new Vector3(width, h, thick);
        }
        post.GetComponent<Renderer>().material = mat;
        post.isStatic = true;
        DestroyImmediate(post.GetComponent<Collider>());
    }

    private static void CreateHorizontalBeam(GameObject parent, float x, float y, float z, float length, float width, float thick, Material mat, bool isYZPlane)
    {
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "TudorHorizontalBeam";
        beam.transform.SetParent(parent.transform);
        beam.transform.position = new Vector3(x, y, z);
        if (isYZPlane)
        {
            beam.transform.localScale = new Vector3(thick, width, length);
        }
        else
        {
            beam.transform.localScale = new Vector3(length, width, thick);
        }
        beam.GetComponent<Renderer>().material = mat;
        beam.isStatic = true;
        DestroyImmediate(beam.GetComponent<Collider>());
    }

    private static void CreateRectFrame(GameObject parent, float cx, float cy, float cz, float w, float h, float thick, Material mat)
    {
        CreateHorizontalBeam(parent, cx, cy + h/2f - thick/2f, cz, w, thick, thick, mat, false);
        CreateHorizontalBeam(parent, cx, cy - h/2f + thick/2f, cz, w, thick, thick, mat, false);
        CreateVerticalPost(parent, cx - w/2f + thick/2f, cy, cz, h, thick, thick, mat, false);
        CreateVerticalPost(parent, cx + w/2f - thick/2f, cy, cz, h, thick, thick, mat, false);
    }

    private static void CreateDiagonalBrace(GameObject parent, Vector3 start, Vector3 end, Material mat, bool isYZPlane)
    {
        GameObject brace = GameObject.CreatePrimitive(PrimitiveType.Cube);
        brace.name = "TudorBrace";
        brace.transform.SetParent(parent.transform);
        brace.isStatic = true;
        DestroyImmediate(brace.GetComponent<Collider>());

        Vector3 dir = end - start;
        float len = dir.magnitude;
        brace.transform.position = start + dir * 0.5f;

        if (isYZPlane)
        {
            brace.transform.localScale = new Vector3(0.08f, len, 0.14f);
            float angle = Mathf.Atan2(dir.y, dir.z) * Mathf.Rad2Deg;
            brace.transform.rotation = Quaternion.Euler(-angle + 90f, 0f, 0f);
        }
        else
        {
            brace.transform.localScale = new Vector3(0.14f, len, 0.08f);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            brace.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
        brace.GetComponent<Renderer>().material = mat;
    }

    // Central Village Well (Revamped to High-Detail Medieval Masonry!)
    private static void BuildWell(GameObject parent, int cx, int cy, int cz, Material stoneMat, Material woodMat)
    {
        GameObject well = new GameObject("VillageWell");
        well.transform.SetParent(parent.transform);
        well.isStatic = true;

        // Elegant Layered Masonry Well Ring: 3 staggered tiers of stone blocks
        float radius = 1.0f;
        int segmentsPerTier = 8;
        float tierHeight = 0.33f;

        for (int tier = 0; tier < 3; tier++)
        {
            float yPos = cy + 0.18f + (tier * tierHeight);
            // Stagger alternate tiers by 22.5 degrees for realistic brick overlap
            float angleOffset = (tier % 2 == 1) ? 22.5f : 0f;

            for (int i = 0; i < segmentsPerTier; i++)
            {
                float angleDeg = i * 45f + angleOffset;
                float angleRad = angleDeg * Mathf.Deg2Rad;
                float rx = Mathf.Cos(angleRad) * radius;
                float rz = Mathf.Sin(angleRad) * radius;

                GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stone.name = $"WellStone_Tier{tier}_{i}";
                stone.transform.SetParent(well.transform);
                stone.transform.position = new Vector3(cx + rx, yPos, cz + rz);
                
                // Introduce subtle random variations for a hand-carved medieval aesthetic
                float randScaleX = Random.Range(0.65f, 0.75f);
                float randScaleY = Random.Range(0.28f, 0.34f);
                float randScaleZ = Random.Range(0.28f, 0.34f);
                
                stone.transform.localScale = new Vector3(randScaleX, randScaleY, randScaleZ);
                stone.transform.rotation = Quaternion.Euler(
                    Random.Range(-2f, 2f), 
                    -angleDeg + 90f + Random.Range(-4f, 4f), 
                    Random.Range(-2f, 2f)
                );
                
                stone.GetComponent<Renderer>().material = stoneMat;
                stone.isStatic = true;
            }
        }

        // Well Water center
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cube);
        water.name = "WellWater";
        water.transform.position = new Vector3(cx, cy + 0.55f, cz);
        water.transform.localScale = new Vector3(1.4f, 0.1f, 1.4f);
        water.transform.SetParent(well.transform);
        Material waterMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/WaterMat.mat");
        water.GetComponent<Renderer>().material = waterMat;
        DestroyImmediate(water.GetComponent<Collider>());

        // Wooden roof support posts (Thicker and taller)
        GameObject postL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        postL.transform.position = new Vector3(cx - 0.85f, cy + 1.6f, cz);
        postL.transform.localScale = new Vector3(0.18f, 2.4f, 0.18f);
        postL.transform.SetParent(well.transform);
        postL.GetComponent<Renderer>().material = woodMat;
        postL.isStatic = true;

        GameObject postR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        postR.transform.position = new Vector3(cx + 0.85f, cy + 1.6f, cz);
        postR.transform.localScale = new Vector3(0.18f, 2.4f, 0.18f);
        postR.transform.SetParent(well.transform);
        postR.GetComponent<Renderer>().material = woodMat;
        postR.isStatic = true;

        // Horizontal wooden winch spindle
        GameObject spindle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        spindle.name = "WinchSpindle";
        spindle.transform.position = new Vector3(cx, cy + 2.0f, cz);
        spindle.transform.localScale = new Vector3(1.6f, 0.12f, 0.12f);
        spindle.transform.SetParent(well.transform);
        spindle.GetComponent<Renderer>().material = woodMat;
        spindle.isStatic = true;
        DestroyImmediate(spindle.GetComponent<Collider>());

        // Winch crank handle
        GameObject crankArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crankArm.name = "CrankArm";
        crankArm.transform.position = new Vector3(cx + 0.9f, cy + 2.15f, cz);
        crankArm.transform.localScale = new Vector3(0.08f, 0.4f, 0.08f);
        crankArm.transform.rotation = Quaternion.Euler(0f, 0f, 30f);
        crankArm.transform.SetParent(well.transform);
        crankArm.GetComponent<Renderer>().material = woodMat;
        crankArm.isStatic = true;
        DestroyImmediate(crankArm.GetComponent<Collider>());

        GameObject crankHandle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crankHandle.name = "CrankHandle";
        crankHandle.transform.position = new Vector3(cx + 0.9f, cy + 2.3f, cz + 0.2f);
        crankHandle.transform.localScale = new Vector3(0.06f, 0.06f, 0.3f);
        crankHandle.transform.SetParent(well.transform);
        crankHandle.GetComponent<Renderer>().material = woodMat;
        crankHandle.isStatic = true;
        DestroyImmediate(crankHandle.GetComponent<Collider>());

        // Hanging Rope
        GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rope.name = "WellRope";
        rope.transform.position = new Vector3(cx - 0.1f, cy + 1.4f, cz);
        rope.transform.localScale = new Vector3(0.03f, 1.2f, 0.03f);
        rope.transform.SetParent(well.transform);
        rope.GetComponent<Renderer>().material = woodMat; // Reuse woodMat for rustic brown rope
        rope.isStatic = true;
        DestroyImmediate(rope.GetComponent<Collider>());

        // Hanging Wood Bucket
        GameObject bucket = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bucket.name = "WellBucket";
        bucket.transform.position = new Vector3(cx - 0.1f, cy + 0.85f, cz);
        bucket.transform.localScale = new Vector3(0.3f, 0.35f, 0.3f);
        bucket.transform.SetParent(well.transform);
        bucket.GetComponent<Renderer>().material = woodMat;
        bucket.isStatic = true;
        DestroyImmediate(bucket.GetComponent<Collider>());

        // Bucket metal band details
        GameObject bucketBand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bucketBand.name = "BucketBand";
        bucketBand.transform.position = new Vector3(cx - 0.1f, cy + 0.85f, cz);
        bucketBand.transform.localScale = new Vector3(0.32f, 0.08f, 0.32f);
        bucketBand.transform.SetParent(bucket.transform);
        bucketBand.GetComponent<Renderer>().material = stoneMat;
        bucketBand.isStatic = true;
        DestroyImmediate(bucketBand.GetComponent<Collider>());

        // Cozy Sloped Peaked Wooden Roof made of multiple overlapping shingles
        float rAngle = 22f;
        
        // 4 overlapping horizontal planks on left slope
        for (int row = 0; row < 4; row++)
        {
            // Calculate position along the slope
            float t = row / 3.0f; // 0 to 1
            float slopeOffset = -0.75f + t * 1.35f;
            float cosR = Mathf.Cos(rAngle * Mathf.Deg2Rad);
            float sinR = Mathf.Sin(rAngle * Mathf.Deg2Rad);
            
            GameObject shingleL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shingleL.name = $"WellRoofShingle_Left_{row}";
            shingleL.transform.position = new Vector3(
                cx + slopeOffset * cosR,
                cy + 2.5f - slopeOffset * sinR + 0.04f * row, // slight vertical offset to overlap
                cz
            );
            shingleL.transform.localScale = new Vector3(0.55f, 0.08f, 2.3f); // Shingles are wider on Z
            shingleL.transform.rotation = Quaternion.Euler(0f, 0f, rAngle);
            shingleL.transform.SetParent(well.transform);
            shingleL.GetComponent<Renderer>().material = woodMat;
            shingleL.isStatic = true;
            DestroyImmediate(shingleL.GetComponent<Collider>());

            GameObject shingleR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shingleR.name = $"WellRoofShingle_Right_{row}";
            shingleR.transform.position = new Vector3(
                cx - slopeOffset * cosR,
                cy + 2.5f - slopeOffset * sinR + 0.04f * row,
                cz
            );
            shingleR.transform.localScale = new Vector3(0.55f, 0.08f, 2.3f);
            shingleR.transform.rotation = Quaternion.Euler(0f, 0f, -rAngle);
            shingleR.transform.SetParent(well.transform);
            shingleR.GetComponent<Renderer>().material = woodMat;
            shingleR.isStatic = true;
            DestroyImmediate(shingleR.GetComponent<Collider>());
        }

        // Add a central capping ridge beam at the peak
        GameObject ridgeBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ridgeBeam.name = "WellRoofRidgeBeam";
        ridgeBeam.transform.position = new Vector3(cx, cy + 2.82f, cz);
        ridgeBeam.transform.localScale = new Vector3(0.18f, 0.18f, 2.4f);
        ridgeBeam.transform.SetParent(well.transform);
        ridgeBeam.GetComponent<Renderer>().material = woodMat;
        ridgeBeam.isStatic = true;
    }

    private static void BuildCampfireBench(GameObject root, Vector3 pos, Vector3 rotation, Material woodMat)
    {
        GameObject bench = new GameObject("CampfireBench");
        bench.transform.SetParent(root.transform);
        bench.transform.position = pos;
        bench.transform.rotation = Quaternion.Euler(rotation);
        bench.isStatic = true;

        // Seat slab
        GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seat.name = "SeatPlank";
        seat.transform.SetParent(bench.transform);
        seat.transform.localPosition = new Vector3(0f, 0.15f, 0f);
        seat.transform.localScale = new Vector3(1.2f, 0.08f, 0.35f);
        seat.GetComponent<Renderer>().material = woodMat;
        seat.isStatic = true;

        // Left Leg
        GameObject legL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        legL.name = "Leg_Left";
        legL.transform.SetParent(bench.transform);
        legL.transform.localPosition = new Vector3(-0.45f, 0.08f, 0f);
        legL.transform.localScale = new Vector3(0.08f, 0.16f, 0.26f);
        legL.GetComponent<Renderer>().material = woodMat;
        legL.isStatic = true;

        // Right Leg
        GameObject legR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        legR.name = "Leg_Right";
        legR.transform.SetParent(bench.transform);
        legR.transform.localPosition = new Vector3(0.45f, 0.08f, 0f);
        legR.transform.localScale = new Vector3(0.08f, 0.16f, 0.26f);
        legR.GetComponent<Renderer>().material = woodMat;
        legR.isStatic = true;
    }

    // Plaza Market Stalls (Revamped to beautiful sloped awnings!)
    private static void BuildMarketStall(GameObject parent, int sx, int sy, int sz, Material woodMat, Material stoneMat, Material clothRed, Material clothWhite, string name)
    {
        GameObject stall = new GameObject(name);
        stall.transform.SetParent(parent.transform);
        stall.isStatic = true;

        // Table Counter
        GameObject counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        counter.transform.position = new Vector3(sx, sy + 0.4f, sz);
        counter.transform.localScale = new Vector3(2.4f, 0.8f, 0.9f);
        counter.transform.SetParent(stall.transform);
        counter.GetComponent<Renderer>().material = woodMat;
        counter.isStatic = true;

        // Posts
        GameObject p1 = GameObject.CreatePrimitive(PrimitiveType.Cube); p1.transform.position = new Vector3(sx - 1.1f, sy + 1.4f, sz); p1.transform.localScale = new Vector3(0.12f, 2.0f, 0.12f); p1.transform.SetParent(stall.transform); p1.GetComponent<Renderer>().material = woodMat; p1.isStatic = true;
        GameObject p2 = GameObject.CreatePrimitive(PrimitiveType.Cube); p2.transform.position = new Vector3(sx + 1.1f, sy + 1.4f, sz); p2.transform.localScale = new Vector3(0.12f, 2.0f, 0.12f); p2.transform.SetParent(stall.transform); p2.GetComponent<Renderer>().material = woodMat; p2.isStatic = true;
        GameObject p3 = GameObject.CreatePrimitive(PrimitiveType.Cube); p3.transform.position = new Vector3(sx - 1.1f, sy + 1.4f, sz - 0.8f); p3.transform.localScale = new Vector3(0.12f, 2.0f, 0.12f); p3.transform.SetParent(stall.transform); p3.GetComponent<Renderer>().material = woodMat; p3.isStatic = true;
        GameObject p4 = GameObject.CreatePrimitive(PrimitiveType.Cube); p4.transform.position = new Vector3(sx + 1.1f, sy + 1.4f, sz - 0.8f); p4.transform.localScale = new Vector3(0.12f, 2.0f, 0.12f); p4.transform.SetParent(stall.transform); p4.GetComponent<Renderer>().material = woodMat; p4.isStatic = true;

        // Sloped Striped Canvas Canopy
        GameObject awningLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        awningLeft.name = "CanvasAwning_Left";
        awningLeft.transform.position = new Vector3(sx - 0.55f, sy + 2.5f, sz - 0.4f);
        awningLeft.transform.localScale = new Vector3(1.1f, 0.08f, 1.4f);
        awningLeft.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
        awningLeft.transform.SetParent(stall.transform);
        awningLeft.GetComponent<Renderer>().material = clothRed;
        awningLeft.isStatic = true;
        DestroyImmediate(awningLeft.GetComponent<Collider>()); // Destroy primitive solid collider

        GameObject awningRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        awningRight.name = "CanvasAwning_Right";
        awningRight.transform.position = new Vector3(sx + 0.55f, sy + 2.5f, sz - 0.4f);
        awningRight.transform.localScale = new Vector3(1.1f, 0.08f, 1.4f);
        awningRight.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
        awningRight.transform.SetParent(stall.transform);
        awningRight.GetComponent<Renderer>().material = clothWhite;
        awningRight.isStatic = true;
        DestroyImmediate(awningRight.GetComponent<Collider>()); // Destroy primitive solid collider
    }



    private static void SpawnBillboardTrees(GameObject root, Sprite treeSprite, Material spriteMat)
    {
        GameObject treesFolder = new GameObject("Trees");
        treesFolder.transform.SetParent(root.transform);

        Random.InitState(123); // Fixed seed for reproducible natural look

        // Hand-curated scenic tree positions with correct terrain heights
        Vector3[] treeCoords = {
            // === Left bank low meadow (Y=2, Z from -3 to -8) ===
            new Vector3(-15.0f, 2.0f, -4.5f),
            new Vector3(-13.5f, 2.0f, -6.0f),
            new Vector3(-16.5f, 2.0f, -7.0f),
            new Vector3( 12.0f, 2.0f, -5.5f),
            new Vector3( 14.5f, 2.0f, -3.5f),
            new Vector3(-3.5f,  2.0f, -6.5f),
            new Vector3( 5.5f,  2.0f, -7.5f),

            // === Left bank hilltop (Y=4, Z <= -10) ===
            new Vector3(-14.5f, 4.0f, -14.5f),
            new Vector3(-12.0f, 4.0f, -11.5f),
            new Vector3(-16.0f, 4.0f, -12.0f),
            new Vector3(-7.0f,  4.0f, -15.0f),
            new Vector3(-3.0f,  4.0f, -13.5f),
            new Vector3( 2.5f,  4.0f, -14.0f),
            new Vector3( 8.0f,  4.0f, -12.5f),
            new Vector3( 13.0f, 4.0f, -16.0f),
            new Vector3( 16.0f, 4.0f, -11.0f),
            new Vector3(-10.0f, 4.0f, -17.0f),
            new Vector3( 5.0f,  4.0f, -17.0f),

            // === Right bank grassy yards (Y=2, Z=13-14) ===
            new Vector3(-14.5f, 2.0f, 13.5f),
            new Vector3( 15.5f, 2.0f, 13.0f),
            new Vector3(-16.0f, 2.0f,  4.5f),
            new Vector3( 16.5f, 2.0f,  5.0f),

            // === Right bank hilltop (Y=4, Z >= 16) ===
            new Vector3( 14.0f, 4.0f, 17.0f),
            new Vector3(  7.0f, 4.0f, 16.5f),
            new Vector3(-5.0f,  4.0f, 17.5f),
            new Vector3(-12.0f, 4.0f, 16.0f),
            new Vector3( 0.0f,  4.0f, 17.0f),
            new Vector3( 16.0f, 4.0f, 16.5f),
            new Vector3(-16.0f, 4.0f, 17.0f),

            // === Scattered along the river edges for scenic framing ===
            new Vector3(-14.5f, 2.0f,  3.5f),
            new Vector3( 15.0f, 2.0f, -3.0f),
        };

        for (int i = 0; i < treeCoords.Length; i++)
        {
            Vector3 pos = treeCoords[i];
            
            GameObject tree = new GameObject($"PineTree_{i}");
            tree.transform.position = pos;
            // Introduce a subtle random height and width scale variation for extreme grand JRPG forest look!
            float scaleW = Random.Range(4.5f, 5.8f);
            float scaleH = Random.Range(8.5f, 10.8f);
            tree.transform.localScale = new Vector3(scaleW, scaleH, 1f);
            // Introduce subtle organic tilt for maximum natural look
            tree.transform.rotation = Quaternion.Euler(0f, Random.Range(-15f, 15f), Random.Range(-2f, 2f));
            tree.transform.SetParent(treesFolder.transform);

            SpriteRenderer sr = tree.AddComponent<SpriteRenderer>();
            sr.sprite = treeSprite;
            sr.material = spriteMat;
            sr.shadowCastingMode = ShadowCastingMode.On;
            sr.receiveShadows = true;

            HD2DBillboard billboard = tree.AddComponent<HD2DBillboard>();
            billboard.enableSway = true;
            billboard.swaySpeed = Random.Range(1.0f, 1.4f);
            billboard.swayAmount = Random.Range(1.8f, 2.8f);
            
            CapsuleCollider cc = tree.AddComponent<CapsuleCollider>();
            cc.center = new Vector3(0, 0.2f, 0);
            cc.radius = 0.25f;
            cc.height = 1.8f;
        }
    }

    private static void SpawnLanterns(GameObject root, Sprite lanternSprite, Material spriteMat)
    {
        GameObject lanternsFolder = new GameObject("Lanterns");
        lanternsFolder.transform.SetParent(root.transform);

        Vector3[] lanternPositions = {
            // Bridges
            new Vector3(0.9f, 2.1f, 2.7f),
            new Vector3(-0.9f, 2.1f, -2.7f),
            new Vector3(-10.1f, 2.1f, 2.7f),
            new Vector3(-11.9f, 2.1f, -2.7f),
            // Plaza
            new Vector3(0f, 2.1f, 9.2f)
        };

        for (int i = 0; i < lanternPositions.Length; i++)
        {
            Vector3 pos = lanternPositions[i];
            
            GameObject lantern = new GameObject($"BridgeLantern_{i}");
            lantern.transform.position = pos;
            lantern.transform.localScale = new Vector3(0.8f, 1.6f, 1f);
            lantern.transform.SetParent(lanternsFolder.transform);

            SpriteRenderer sr = lantern.AddComponent<SpriteRenderer>();
            sr.sprite = lanternSprite;
            sr.material = spriteMat;
            sr.shadowCastingMode = ShadowCastingMode.On;
            sr.receiveShadows = true;

            lantern.AddComponent<HD2DBillboard>();

            GameObject lightObj = new GameObject($"LanternLight_{i}");
            lightObj.transform.SetParent(lantern.transform);
            lightObj.transform.localPosition = new Vector3(0.2f, 0.4f, -0.05f);

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 7f;
            light.color = new Color(1f, 0.72f, 0.35f);
            light.intensity = 2.0f;
            light.shadows = LightShadows.Soft;
            light.enabled = false; // toggled at night by DayNightCycle
        }
    }

    private static void SpawnPropsAndNPCs(GameObject root, Sprite[] chestSprites, Sprite[] playerSprites, Sprite[] chiefSprites, Sprite[] merchantSprites, Sprite[] adventurerSprites, Material spriteMat)
    {
        // 1. Treasure Chest
        GameObject chest = new GameObject("TreasureChest");
        chest.transform.position = new Vector3(13f, 2.0f, 8.5f); // Beside tavern
        chest.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        chest.transform.SetParent(root.transform);

        SpriteRenderer chestSR = chest.AddComponent<SpriteRenderer>();
        chestSR.material = spriteMat;
        chestSR.shadowCastingMode = ShadowCastingMode.On;
        chestSR.receiveShadows = true;

        InteractiveChest interactChest = chest.AddComponent<InteractiveChest>();
        interactChest.chestSprites = chestSprites;

        BoxCollider chestCol = chest.AddComponent<BoxCollider>();
        chestCol.center = new Vector3(0, 0.4f, 0);
        chestCol.size = new Vector3(0.8f, 0.8f, 0.8f);

        GameObject chestTrigger = new GameObject("Trigger");
        chestTrigger.transform.SetParent(chest.transform);
        chestTrigger.transform.localPosition = Vector3.zero;
        BoxCollider rangeCol = chestTrigger.AddComponent<BoxCollider>();
        rangeCol.isTrigger = true;
        rangeCol.size = new Vector3(2.2f, 1.5f, 2.2f);
        TriggerForwarder chestForwarder = chestTrigger.AddComponent<TriggerForwarder>();
        chestForwarder.target = chest;

        GameObject chestPrompt = new GameObject("PromptPanel");
        chestPrompt.transform.SetParent(chest.transform);
        chestPrompt.transform.localPosition = new Vector3(0, 1.35f, 0);
        chestPrompt.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        chestPrompt.AddComponent<HD2DBillboard>();
        chestPrompt.AddComponent<PulsingMistyPrompt>();
        
        TextMesh promptTM = chestPrompt.AddComponent<TextMesh>();
        promptTM.text = "...";
        promptTM.fontSize = 24;
        promptTM.color = Color.white;
        promptTM.alignment = TextAlignment.Center;
        promptTM.anchor = TextAnchor.MiddleCenter;

        // Glowing JRPG mist bubble sphere backing
        GameObject backing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        backing.name = "Backing";
        backing.transform.SetParent(chestPrompt.transform);
        backing.transform.localPosition = new Vector3(0f, 0f, 0.05f); // slightly behind text
        backing.transform.localScale = new Vector3(12f, 8f, 2f);
        DestroyImmediate(backing.GetComponent<Collider>());

        Renderer rend = backing.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        rend.material.color = new Color(0.18f, 0.55f, 0.95f, 0.65f); // glowing JRPG soft blue mist
        
        interactChest.promptRenderer = chestPrompt.GetComponent<MeshRenderer>();

        // 2. High-Detail Premium Campfire and Benches
        GameObject campfire = new GameObject("Campfire");
        campfire.transform.SetParent(root.transform);
        campfire.transform.position = new Vector3(13.0f, 2.0f, 5.0f);
        
        Material stoneMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/StoneMat.mat");
        Material woodMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/WoodMat.mat");
        Sprite treeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/tree.png");

        // Sturdy circular stone fire ring: 12 detailed stones in a circle
        int stoneCount = 12;
        float fireRingRadius = 0.65f;
        for (int idx = 0; idx < stoneCount; idx++)
        {
            float angle = idx * (360f / stoneCount) * Mathf.Deg2Rad;
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(stone.GetComponent<Collider>());
            stone.name = $"CampfireRingStone_{idx}";
            stone.transform.SetParent(campfire.transform);
            stone.transform.localPosition = new Vector3(Mathf.Cos(angle) * fireRingRadius, 0.08f, Mathf.Sin(angle) * fireRingRadius);
            
            // Varied sizes and rotations for a rustic natural look
            float sSizeX = Random.Range(0.14f, 0.22f);
            float sSizeY = Random.Range(0.12f, 0.18f);
            float sSizeZ = Random.Range(0.18f, 0.26f);
            stone.transform.localScale = new Vector3(sSizeX, sSizeY, sSizeZ);
            stone.transform.localRotation = Quaternion.Euler(
                Random.Range(-10f, 10f),
                -(idx * (360f / stoneCount)) + 90f + Random.Range(-15f, 15f),
                Random.Range(-10f, 10f)
            );
            stone.GetComponent<Renderer>().material = stoneMat;
        }

        // Firewood Teepee Stack inside the ring
        int firewoodCount = 5;
        for (int i = 0; i < firewoodCount; i++)
        {
            float angle = i * (360f / firewoodCount) * Mathf.Deg2Rad;
            GameObject firewood = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(firewood.GetComponent<Collider>());
            firewood.name = $"CampfireWood_{i}";
            firewood.transform.SetParent(campfire.transform);
            
            // Positioned inside the ring and tilted inward like a pyramid/teepee
            firewood.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.25f, 0.18f, Mathf.Sin(angle) * 0.25f);
            firewood.transform.localScale = new Vector3(0.08f, 0.45f, 0.08f);
            
            // Tilt inward
            float tiltAngle = 35f;
            float rotY = -(i * (360f / firewoodCount)) * Mathf.Rad2Deg;
            firewood.transform.localRotation = Quaternion.Euler(tiltAngle, rotY, 0f);
            firewood.GetComponent<Renderer>().material = woodMat;
        }

        // Glowing hot coal bed underneath the wood
        GameObject coalBed = GameObject.CreatePrimitive(PrimitiveType.Cube);
        coalBed.name = "CampfireCoals";
        coalBed.transform.SetParent(campfire.transform);
        coalBed.transform.localPosition = new Vector3(0f, 0.04f, 0f);
        coalBed.transform.localScale = new Vector3(0.5f, 0.06f, 0.5f);
        DestroyImmediate(coalBed.GetComponent<Collider>());
        
        Renderer coalR = coalBed.GetComponent<Renderer>();
        coalR.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        coalR.material.SetColor("_BaseColor", new Color(0.2f, 0.05f, 0f, 1f));
        coalR.material.EnableKeyword("_EMISSION");
        coalR.material.SetColor("_EmissionColor", new Color(1.0f, 0.18f, 0.0f) * 2.5f); // Bright orange ember glow

        // Point light for dynamic warm flame illumination
        GameObject fireLight = new GameObject("CampfireLight");
        fireLight.transform.SetParent(campfire.transform);
        fireLight.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        Light fl = fireLight.AddComponent<Light>();
        fl.type = LightType.Point;
        fl.range = 7.5f;
        fl.color = new Color(1.0f, 0.42f, 0.05f); // Warm amber flame
        fl.intensity = 3.5f;
        fl.shadows = LightShadows.Soft;

        // Custom JRPG-style floating embers: small glowing orange cubes drifting upward!
        int emberCount = 6;
        for (int i = 0; i < emberCount; i++)
        {
            GameObject ember = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ember.name = $"CampfireEmber_{i}";
            ember.transform.SetParent(campfire.transform);
            DestroyImmediate(ember.GetComponent<Collider>());
            
            float rx = Random.Range(-0.2f, 0.2f);
            float rz = Random.Range(-0.2f, 0.2f);
            float ry = Random.Range(0.25f, 1.2f);
            ember.transform.localPosition = new Vector3(rx, ry, rz);
            
            float es = Random.Range(0.02f, 0.05f);
            ember.transform.localScale = new Vector3(es, es, es);
            
            Renderer embR = ember.GetComponent<Renderer>();
            embR.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            embR.material.color = new Color(1.0f, 0.55f, 0.1f, 0.9f);
            embR.material.EnableKeyword("_EMISSION");
            embR.material.SetColor("_EmissionColor", new Color(1.0f, 0.55f, 0.1f) * 3f);
        }

        // Beautiful campfire benches (facing the bonfire with support legs!)
        Vector3[] benchPos = {
            new Vector3(13.0f, 2.0f, 3.8f),  // bench front
            new Vector3(14.4f, 2.0f, 5.0f),  // bench right
            new Vector3(11.6f, 2.0f, 5.0f)   // bench left
        };
        Vector3[] benchRot = {
            new Vector3(0, 0, 0),
            new Vector3(0, 90f, 0),
            new Vector3(0, 90f, 0)
        };
        for (int i = 0; i < 3; i++)
        {
            BuildCampfireBench(root, benchPos[i], benchRot[i], woodMat);
        }

        // ==================== ABUNDANT DECORATIVE PROPS ("モノが少なすぎ") ====================
        
        // 2A. Wooden Fences along the riverbanks (Left and Right, avoiding bridge crossings)
        // Skipped post buffer increased from 1.4f to 2.2f so horizontal rails never block walkway entries!
        GameObject fenceFolder = new GameObject("RiverFences");
        fenceFolder.transform.SetParent(root.transform);
        fenceFolder.isStatic = true;
        for (float x = -18f; x <= 18f; x += 1.5f)
        {
            // Walkway gaps around the bridges (X = 0 and X = -11)
            if (Mathf.Abs(x - 0f) < 2.2f || Mathf.Abs(x - (-11f)) < 2.2f) continue;
            
            // Left bank fence line (Z = -2.6)
            BuildFencePost(fenceFolder, x, 2f, -2.6f, woodMat);
            // Right bank fence line (Z = 2.6)
            BuildFencePost(fenceFolder, x, 2f, 2.6f, woodMat);
        }

        // 2B. Stacked Wood Crates & Barrels next to shop, chief house, and plaza
        GameObject propFolder = new GameObject("Crates_And_Barrels");
        propFolder.transform.SetParent(root.transform);
        propFolder.isStatic = true;

        // Tavern Front Cluster
        SpawnCrate(propFolder, new Vector3(5.5f, 2f, 7.5f), new Vector3(0.7f, 0.7f, 0.7f), woodMat);
        SpawnCrate(propFolder, new Vector3(5.5f, 2.7f, 7.5f), new Vector3(0.6f, 0.6f, 0.6f), woodMat);
        SpawnBarrel(propFolder, new Vector3(6.3f, 2f, 7.5f), 0.3f, 0.8f, stoneMat); // stone-barrel/clay look

        // Chief Ronald Yard Cluster
        SpawnCrate(propFolder, new Vector3(-5.5f, 2f, 9.0f), new Vector3(0.7f, 0.7f, 0.7f), woodMat);
        SpawnBarrel(propFolder, new Vector3(-5.5f, 2f, 9.8f), 0.3f, 0.8f, stoneMat);
        SpawnBarrel(propFolder, new Vector3(-5.5f, 2f, 9.8f), 0.3f, 0.8f, stoneMat);

        // Shop Corner Cluster
        SpawnCrate(propFolder, new Vector3(14.5f, 2f, 8.5f), new Vector3(0.7f, 0.7f, 0.7f), woodMat);
        SpawnBarrel(propFolder, new Vector3(14.5f, 2f, 9.3f), 0.3f, 0.8f, stoneMat);

        // 2C. Cozy Plaza Benches (wooden rest benches near the well)
        GameObject benchFolder = new GameObject("PlazaBenches");
        benchFolder.transform.SetParent(root.transform);
        benchFolder.isStatic = true;
        BuildBench(benchFolder, -2.5f, 2f, 7f, woodMat);
        BuildBench(benchFolder, 2.5f, 2f, 7f, woodMat);

        // 2D. Stone-Bordered Flower Beds (with small custom plants inside)
        GameObject flowerFolder = new GameObject("FlowerBeds");
        flowerFolder.transform.SetParent(root.transform);
        flowerFolder.isStatic = true;
        BuildFlowerBed(flowerFolder, -4f, 2f, 5f, stoneMat, treeSprite, spriteMat, true);
        BuildFlowerBed(flowerFolder, -7f, 4f, -11f, stoneMat, treeSprite, spriteMat, false);

        // 2E. Cute Wooden Signposts
        GameObject signFolder = new GameObject("Signposts");
        signFolder.transform.SetParent(root.transform);
        signFolder.isStatic = true;
        BuildSignpost(signFolder, 1.5f, 2f, 3f, woodMat); // Village Entrance
        BuildSignpost(signFolder, 5.5f, 2f, 5f, woodMat); // Shop Welcomer

        // ==================== UNIQUE JRPG CHARACTERS ("キャラ全部一緒") ====================
        
        // 3. VILLAGER 1: Chief Ronald (stands near Well/Chief House X = -7, Z = 7.5)
        Sprite chiefSprite = (chiefSprites != null && chiefSprites.Length > 6) ? chiefSprites[6] : playerSprites[6];
        SpawnVillager(root, "Chief Ronald", new Vector3(-7f, 2.0f, 7.5f), chiefSprite, spriteMat, new string[] {
            "ロナルド: はじまりの村へようこそ、若き騎士よ！",
            "ロナルド: この平穏なる聖域は、何世代にもわたって多くの旅人たちを育んできたのじゃ。",
            "ロナルド: しかし最近、東の川に架かる橋の向こうから、不穏な闇の気配が漂い始めていてな…",
            "ロナルド: まずは旅の準備を整え、広場の商人たちと話し、君だけの伝説の物語を紡ぎ始めるのじゃ！"
        });

        // 4. VILLAGER 2: Merchant Marcus (stands near Potion Market Stall X = 4, Z = 4)
        Sprite merchantSprite = (merchantSprites != null && merchantSprites.Length > 6) ? merchantSprites[6] : playerSprites[6];
        SpawnVillager(root, "Merchant Marcus", new Vector3(4f, 2.0f, 4f), merchantSprite, spriteMat, new string[] {
            "マーカス: やあ、旅の御人！旅路に必要な回復アイテムはお探しかな？",
            "マーカス: 村の中央にある井戸から汲み上げた清らかな水を使って、特製のポーションを調合しているんだ。",
            "マーカス: もし身を守る武器が必要なら、酒場の横にある宝箱に古い鉄の剣が残されていたはずだよ。",
            "マーカス: 自由に宝箱を開けて、持っていくといい。道中、くれぐれも気をつけてな！"
        });

        // 5. VILLAGER 3: Adventurer Kaelen (stands near Campfire warming hands X = 13, Z = 3.5)
        Sprite adventurerSprite = (adventurerSprites != null && adventurerSprites.Length > 6) ? adventurerSprites[6] : playerSprites[6];
        SpawnVillager(root, "Adventurer Kaelen", new Vector3(13f, 2.0f, 3.5f), adventurerSprite, spriteMat, new string[] {
            "ケーレン: ふぅ… この焚き火のじんわりとした温もりは、冷えた体に染み渡るなぁ。",
            "ケーレン: 私はこの大陸中を旅してきたけれど、こののどかな村ほど居心地の良い場所は他にないよ。",
            "ケーレン: キーボードの[T]キーを押して、ゆっくりと陽が沈む様子を眺めてごらん。",
            "ケーレン: 夜になると、村の街灯が温かみのある光を放ち、本当に綺麗な夜景が広がるんだ…"
        });

        // ==================== 10 PATROLLING VILLAGERS ("喋る内容は全員日本語/指定ルート巡回") ====================
        
        // NPC 1: Plaza Kid (広場で遊ぶ子供 - runs in a loop near the plaza well)
        SpawnPatrollingNPC(root, "Plaza Kid", new Vector3[] {
            new Vector3(1f, 2f, 6.5f),
            new Vector3(1.5f, 2f, 10f),
            new Vector3(-1.5f, 2f, 10f),
            new Vector3(-1f, 2f, 6.5f)
        }, playerSprites, spriteMat, new string[] {
            "ココ: お兄ちゃん、こんにちは！私はココ！",
            "ココ: 今ね、広場でお友達とかくれんぼしてるの！",
            "ココ: お兄ちゃんも一緒に遊ぶ？おにっこする？"
        }, new Color(1f, 0.92f, 0.55f), new Vector3(1.2f, 1.8f, 1f)); // Smaller scale kid!

        // NPC 2: Baker Wife (パン屋のおかみさん - walks back and forth between chief's yard and merchant)
        SpawnPatrollingNPC(root, "Baker Wife", new Vector3[] {
            new Vector3(-5f, 2f, 8f),
            new Vector3(-1f, 2f, 8f),
            new Vector3(3f, 2f, 8f),
            new Vector3(-1f, 2f, 8f)
        }, merchantSprites, spriteMat, new string[] {
            "エルザ: 今日も素晴らしい青空ねぇ、気持ちがいいわ！",
            "エルザ: うちの主人が朝早くから焼き上げた絶品クロワッサンはいかが？",
            "エルザ: 焼きたてほかほかで、旅の疲れも吹き飛んじゃうわよ！"
        }, new Color(1f, 0.72f, 0.72f), new Vector3(1.5f, 2.3f, 1f));

        // NPC 3: Bridge Guard A (東の橋の見張り兵 - patrols back and forth on the eastern bridge Z-axis)
        SpawnPatrollingNPC(root, "Bridge Guard A", new Vector3[] {
            new Vector3(0f, 2f, -4.5f),
            new Vector3(0f, 2f, 4.5f),
            new Vector3(0f, 2f, -4.5f)
        }, adventurerSprites, spriteMat, new string[] {
            "ガード・アルフ: はっ！ただいま村の境界線を哨戒警備中である！",
            "ガード・アルフ: 川の向こう側から魔物が侵入せぬよう、目を光らせているのだ！",
            "ガード・アルフ: 若き騎士よ、この橋を渡る際はくれぐれも警戒を怠るなよ！"
        }, new Color(0.7f, 0.75f, 0.85f), new Vector3(1.6f, 2.5f, 1f)); // Tall guard!

        // NPC 4: Bridge Guard B (西の橋の見張り兵 - patrols back and forth on the western bridge Z-axis)
        SpawnPatrollingNPC(root, "Bridge Guard B", new Vector3[] {
            new Vector3(-11f, 2f, -4.5f),
            new Vector3(-11f, 2f, 4.5f),
            new Vector3(-11f, 2f, -4.5f)
        }, adventurerSprites, spriteMat, new string[] {
            "ガード・ルーク: ふぅ、今日も風が気持ちいいね。村の平和は異常なしさ！",
            "ガード・ルーク: 僕たちがここに立っている限り、悪い奴らは一歩も通さないよ！",
            "ガード・ルーク: もしケガをしたら、マーカスのポーションショップへ行くといいよ。"
        }, new Color(0.6f, 0.8f, 0.75f), new Vector3(1.6f, 2.5f, 1f));

        // NPC 5: Bard Lyra (旅の吟遊詩人 - wanders slowly around the Tavern front yard)
        SpawnPatrollingNPC(root, "Bard Lyra", new Vector3[] {
            new Vector3(8f, 2f, 6.2f),
            new Vector3(12f, 2f, 6.2f),
            new Vector3(10f, 2f, 4.2f)
        }, playerSprites, spriteMat, new string[] {
            "ライラ: ラララ〜♪ 川のせせらぎが、私の琴の音と美しく調和するわ…",
            "ライラ: 私は風の吹くままに旅をして、各地の伝説を歌に紡いでいるの。",
            "ライラ: あなたの旅路にも、いつか美しい祝福の光が満ち溢れますように。"
        }, new Color(0.85f, 0.75f, 1f), new Vector3(1.5f, 2.3f, 1f));

        // NPC 6: Granny Martha (川辺のおばあさん - walks slowly in a loop in the left bank low meadow)
        SpawnPatrollingNPC(root, "Granny Martha", new Vector3[] {
            new Vector3(-14f, 2f, -4.2f),
            new Vector3(-5f, 2f, -4.2f),
            new Vector3(-9.5f, 2f, -6.8f)
        }, chiefSprites, spriteMat, new string[] {
            "マーサ: おやおや、珍しいお客さまだねぇ。ゆっくり休んでいっておくれ。",
            "マーサ: この清らかな川の流れを見ていると、若い頃の冒険を思い出すよ。",
            "マーサ: 焦らず、自分のペースで一歩ずつ進むのが、長生きと冒険のコツさ。"
        }, new Color(0.72f, 0.9f, 0.72f), new Vector3(1.4f, 2.2f, 1f));

        // NPC 7: Fisherman Ted (釣り人の青年 - patrols back and forth along the right river edge)
        SpawnPatrollingNPC(root, "Fisherman Ted", new Vector3[] {
            new Vector3(2f, 2f, -4.5f),
            new Vector3(5.5f, 2f, -4.5f),
            new Vector3(8.5f, 2f, -4.5f)
        }, merchantSprites, spriteMat, new string[] {
            "テッド: シーッ！静かに！魚が逃げちまうだろ！",
            "テッド: 今日こそは、この川のヌシと呼ばれる大魚を釣り上げるんだ！",
            "テッド: 釣れたら酒場で豪快にソテーにして食うんだ、最高だろ？"
        }, new Color(0.7f, 0.88f, 0.98f), new Vector3(1.5f, 2.4f, 1f));

        // NPC 8: Woodsman Borin (丘の木こり - walks between pine trees on the left bank hilltop Y=4!)
        SpawnPatrollingNPC(root, "Woodsman Borin", new Vector3[] {
            new Vector3(-13f, 4f, -14.5f),
            new Vector3(-7f, 4f, -14.5f),
            new Vector3(-10f, 4f, -11.5f)
        }, adventurerSprites, spriteMat, new string[] {
            "ボリン: ヨイショ、ヨイショ！丘の上の松の木は、どれも立派で切りがいがあるぞ！",
            "ボリン: 巨大な木々から採れる極上の木材は、村の頑丈な家を建てるのに欠かせないんだ。",
            "ボリン: 若いのに冒険に出るのかい？大したもんだ、応援してるぞ！"
        }, new Color(0.88f, 0.78f, 0.68f), new Vector3(1.6f, 2.4f, 1f));

        // NPC 9: Botanist Flora (薬草学者のおばさん - loops on the right bank hilltop Y=4 seeking rare herbs)
        SpawnPatrollingNPC(root, "Botanist Flora", new Vector3[] {
            new Vector3(13f, 4f, 17f),
            new Vector3(7f, 4f, 17f),
            new Vector3(10f, 4f, 16.2f)
        }, chiefSprites, spriteMat, new string[] {
            "フローラ: あら？こんな風が冷たい丘の上まで登ってくるなんて、物好きねぇ。",
            "フローラ: この松の木の根元にはね、夜になると淡く光る希少な薬草が生えるのよ。",
            "フローラ: 調合した秘薬は、マーカスの店でポーションの原料として卸しているのさ。"
        }, new Color(0.98f, 0.92f, 0.7f), new Vector3(1.4f, 2.2f, 1f));

        // NPC 10: Village Girl Hana (村の少女 - loops in the plaza, flowerbeds, and well)
        SpawnPatrollingNPC(root, "Village Girl Hana", new Vector3[] {
            new Vector3(-2f, 2f, 9.2f),
            new Vector3(5f, 2f, 9.2f),
            new Vector3(1.5f, 2f, 7.8f)
        }, playerSprites, spriteMat, new string[] {
            "ハナ: お花畑できれいなお花をたくさん摘んできたの！",
            "ハナ: ラベンダーにタンポポ… 本当にとってもいい香りがするよ！",
            "ハナ: はい、これお兄ちゃんにあげる！旅の魔除けのお守りだよ！"
        }, new Color(0.98f, 0.8f, 0.7f), new Vector3(1.3f, 2.0f, 1f));
    }

    // Helper builders for rich props
    private static void BuildFencePost(GameObject parent, float x, float y, float z, Material woodMat)
    {
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.name = $"FencePost_{x}_{z}";
        post.transform.position = new Vector3(x, y + 0.4f, z);
        post.transform.localScale = new Vector3(0.12f, 0.8f, 0.12f);
        post.transform.SetParent(parent.transform);
        post.GetComponent<Renderer>().material = woodMat;
        post.isStatic = true;

        GameObject rail1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail1.name = "FenceRail_Top";
        rail1.transform.position = new Vector3(x + 0.75f, y + 0.6f, z);
        rail1.transform.localScale = new Vector3(1.5f, 0.08f, 0.08f);
        rail1.transform.SetParent(parent.transform);
        rail1.GetComponent<Renderer>().material = woodMat;
        rail1.isStatic = true;

        GameObject rail2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail2.name = "FenceRail_Bottom";
        rail2.transform.position = new Vector3(x + 0.75f, y + 0.3f, z);
        rail2.transform.localScale = new Vector3(1.5f, 0.08f, 0.08f);
        rail2.transform.SetParent(parent.transform);
        rail2.GetComponent<Renderer>().material = woodMat;
        rail2.isStatic = true;
    }

    private static void SpawnCrate(GameObject parent, Vector3 pos, Vector3 size, Material woodMat)
    {
        GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = "WoodCrate";
        crate.transform.SetParent(parent.transform);
        crate.transform.position = pos + new Vector3(0f, size.y / 2f, 0f);
        crate.transform.localScale = size;
        crate.GetComponent<Renderer>().material = woodMat;
        crate.isStatic = true;
    }

    private static void SpawnBarrel(GameObject parent, Vector3 pos, float radius, float height, Material darkMat)
    {
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.name = "WoodBarrel";
        barrel.transform.SetParent(parent.transform);
        barrel.transform.position = pos + new Vector3(0f, height / 2f, 0f);
        barrel.transform.localScale = new Vector3(radius * 2f, height, radius * 2f);
        barrel.GetComponent<Renderer>().material = darkMat;
        barrel.isStatic = true;
    }

    private static void BuildBench(GameObject parent, float x, float y, float z, Material woodMat)
    {
        GameObject bench = new GameObject("PlazaBench");
        bench.transform.SetParent(parent.transform);
        bench.transform.position = new Vector3(x, y, z);
        bench.isStatic = true;

        GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slab.transform.SetParent(bench.transform);
        slab.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        slab.transform.localScale = new Vector3(1.6f, 0.1f, 0.5f);
        slab.GetComponent<Renderer>().material = woodMat;
        slab.isStatic = true;

        GameObject legL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        legL.transform.SetParent(bench.transform);
        legL.transform.localPosition = new Vector3(-0.6f, 0.2f, 0f);
        legL.transform.localScale = new Vector3(0.15f, 0.4f, 0.4f);
        legL.GetComponent<Renderer>().material = woodMat;
        legL.isStatic = true;

        GameObject legR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        legR.transform.SetParent(bench.transform);
        legR.transform.localPosition = new Vector3(0.6f, 0.2f, 0f);
        legR.transform.localScale = new Vector3(0.15f, 0.4f, 0.4f);
        legR.GetComponent<Renderer>().material = woodMat;
        legR.isStatic = true;
    }

    private static void BuildFlowerBed(GameObject parent, float cx, float cy, float cz, Material stoneMat, Sprite treeSprite, Material spriteMat, bool isLargeTree = false)
    {
        GameObject bed = new GameObject("FlowerBed");
        bed.transform.SetParent(parent.transform);
        bed.transform.position = new Vector3(cx, cy, cz);
        bed.isStatic = true;

        float[,] stoneOffsets = {
            { -1f, 0f }, { 1f, 0f }, { 0f, -1f }, { 0f, 1f }
        };
        for (int i = 0; i < 4; i++)
        {
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.transform.SetParent(bed.transform);
            stone.transform.localPosition = new Vector3(stoneOffsets[i, 0], 0.15f, stoneOffsets[i, 1]);
            stone.transform.localScale = new Vector3(
                stoneOffsets[i, 0] != 0 ? 0.3f : 2.0f,
                0.3f,
                stoneOffsets[i, 1] != 0 ? 0.3f : 2.0f
            );
            stone.GetComponent<Renderer>().material = stoneMat;
            stone.isStatic = true;
        }

        GameObject flower = new GameObject(isLargeTree ? "CentralPlazaTree" : "BedFlower");
        flower.transform.SetParent(bed.transform);
        flower.transform.localPosition = new Vector3(0f, 0.15f, 0f);
        if (isLargeTree)
        {
            // Center plaza tree - make it the largest, grandest tree in the valley
            flower.transform.localScale = new Vector3(6.5f, 12.0f, 1.0f);
        }
        else
        {
            flower.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
        SpriteRenderer sr = flower.AddComponent<SpriteRenderer>();
        sr.sprite = treeSprite;
        sr.material = spriteMat;
        sr.shadowCastingMode = isLargeTree ? ShadowCastingMode.On : ShadowCastingMode.Off;
        sr.receiveShadows = true;

        HD2DBillboard billboard = flower.AddComponent<HD2DBillboard>();
        billboard.enableSway = true;
        billboard.swaySpeed = isLargeTree ? Random.Range(1.2f, 1.8f) : Random.Range(1.8f, 2.5f);
        billboard.swayAmount = isLargeTree ? Random.Range(2.0f, 3.5f) : Random.Range(3.5f, 5.5f);
    }

    private static void BuildSignpost(GameObject parent, float x, float y, float z, Material woodMat)
    {
        GameObject signObj = new GameObject("Signpost");
        signObj.transform.SetParent(parent.transform);
        signObj.transform.position = new Vector3(x, y, z);
        signObj.isStatic = true;

        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.transform.SetParent(signObj.transform);
        post.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        post.transform.localScale = new Vector3(0.12f, 1.2f, 0.12f);
        post.GetComponent<Renderer>().material = woodMat;
        post.isStatic = true;

        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.transform.SetParent(signObj.transform);
        board.transform.localPosition = new Vector3(0f, 1.0f, 0f);
        board.transform.localScale = new Vector3(0.8f, 0.4f, 0.1f);
        board.GetComponent<Renderer>().material = woodMat;
        board.isStatic = true;
    }

    // New optimized Tier ground colliders (resolves micro-bump stuck physics)
    private static void CreateTierColliders(GameObject root)
    {
        GameObject collidersFolder = new GameObject("TierColliders");
        collidersFolder.transform.SetParent(root.transform);

        // Tier 1: River bed (Y = 0)
        SpawnTierCollider(collidersFolder, new Vector3(0f, 0f, 0f), new Vector3(38f, 0.1f, 5f));
        // Tier 2: Left low bank (Y = 2)
        SpawnTierCollider(collidersFolder, new Vector3(0f, 2f, -5.5f), new Vector3(38f, 0.1f, 6f));
        // Tier 3: Right low bank (Y = 2)
        SpawnTierCollider(collidersFolder, new Vector3(0f, 2f, 8.5f), new Vector3(38f, 0.1f, 12f));
        // Tier 4: Left high bank (Y = 4)
        SpawnTierCollider(collidersFolder, new Vector3(0f, 4f, -14f), new Vector3(38f, 0.1f, 10f));
        // Tier 5: Right high bank (Y = 4)
        SpawnTierCollider(collidersFolder, new Vector3(0f, 4f, 16.5f), new Vector3(38f, 0.1f, 4f));
    }

    private static void SpawnTierCollider(GameObject parent, Vector3 pos, Vector3 size)
    {
        GameObject colObj = new GameObject("TierCollider");
        colObj.transform.position = pos;
        colObj.transform.SetParent(parent.transform);
        colObj.isStatic = true;
        
        BoxCollider bc = colObj.AddComponent<BoxCollider>();
        bc.center = new Vector3(0f, -size.y / 2f, 0f);
        bc.size = size;
    }

    // Lush flora spawner (2D point-filtered grass tufts and wildflower billboards in clusters!)
    private static void SpawnFlora(GameObject root, Sprite grassTuftSprite, Sprite treeSprite, Material spriteMat)
    {
        GameObject floraFolder = new GameObject("LushFlora");
        floraFolder.transform.SetParent(root.transform);

        // Load procedural flower and pebble sprites
        Sprite flowerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/flower.png");
        Sprite pebbleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/pebble.png");

        Random.InitState(77);

        int totalSpawned = 0;

        // Define grassy zones: (minX, maxX, minZ, maxZ, Y, clusterCount, itemsPerCluster)
        // Only spawn on grass - avoid cobblestone (Z=3-12), river (Z=-2 to 2), and building footprints
        float[][] zones = {
            // Left bank low meadow (Y=2)
            new float[]{ -17f, -1f, -8f, -3f, 2f, 6, 5 },
            new float[]{ 1f, 17f, -8f, -3f, 2f, 5, 5 },
            // Left bank hilltop (Y=4)
            new float[]{ -17f, 17f, -17f, -10f, 4f, 8, 6 },
            // Right bank grassy yards (Y=2, Z=13-14) - avoiding chief house at X=-12 to -6
            new float[]{ -17f, -13f, 13f, 14f, 2f, 3, 4 },
            new float[]{ -5f, 4f, 13f, 14f, 2f, 3, 4 },
            new float[]{ 14f, 17f, 13f, 14f, 2f, 2, 4 },
            // Right bank hilltop (Y=4, Z>=16)
            new float[]{ -17f, 17f, 16f, 17.5f, 4f, 6, 5 },
        };

        for (int z = 0; z < zones.Length; z++)
        {
            float minX = zones[z][0], maxX = zones[z][1];
            float minZ = zones[z][2], maxZ = zones[z][3];
            float cy = zones[z][4];
            int clusters = (int)zones[z][5];
            int perCluster = (int)zones[z][6];

            for (int c = 0; c < clusters; c++)
            {
                float cx = Random.Range(minX, maxX);
                float cz = Random.Range(minZ, maxZ);

                for (int i = 0; i < perCluster; i++)
                {
                    float rx = Mathf.Clamp(cx + Random.Range(-1.8f, 1.8f), minX, maxX);
                    float rz = Mathf.Clamp(cz + Random.Range(-1.2f, 1.2f), minZ, maxZ);

                    // Skip if in building footprint areas
                    // Chief house: X=-12 to -6, Z=9-14
                    if (rx >= -12f && rx <= -6f && rz >= 9f && rz <= 14f) continue;
                    // Tavern: X=4 to 14, Z=8-14
                    if (rx >= 4f && rx <= 14f && rz >= 8f && rz <= 14f) continue;

                    GameObject floraItem = new GameObject($"Flora_{totalSpawned}");
                    floraItem.transform.position = new Vector3(rx, cy, rz);
                    floraItem.transform.SetParent(floraFolder.transform);
                    floraItem.isStatic = true;

                    SpriteRenderer sr = floraItem.AddComponent<SpriteRenderer>();
                    sr.material = spriteMat;
                    sr.shadowCastingMode = ShadowCastingMode.Off;
                    sr.receiveShadows = true;

                    float rand = Random.value;
                    bool enableSway = true;

                    // 60% grass, 25% wildflowers, 15% pebbles
                    if (rand < 0.60f)
                    {
                        // Grass
                        sr.sprite = grassTuftSprite;
                        floraItem.transform.localScale = new Vector3(0.5f + Random.Range(0f, 0.25f), 0.5f + Random.Range(0f, 0.25f), 1f);
                        sr.color = Color.Lerp(Color.white, new Color(0.85f, 0.95f, 0.8f), Random.value);
                        enableSway = true;
                    }
                    else if (rand < 0.85f)
                    {
                        // Flowers
                        sr.sprite = flowerSprite != null ? flowerSprite : grassTuftSprite;
                        floraItem.transform.localScale = new Vector3(0.5f + Random.Range(0f, 0.20f), 0.5f + Random.Range(0f, 0.20f), 1f);
                        sr.color = Color.white; // Render pixel-art colored wildflower
                        enableSway = true;
                    }
                    else
                    {
                        // Pebbles
                        sr.sprite = pebbleSprite != null ? pebbleSprite : grassTuftSprite;
                        floraItem.transform.localScale = new Vector3(0.4f + Random.Range(0f, 0.15f), 0.4f + Random.Range(0f, 0.15f), 1f);
                        sr.color = Color.white; // Render grey pebbles
                        enableSway = false; // PEBBLES DO NOT SWAY!
                    }

                    HD2DBillboard billboard = floraItem.AddComponent<HD2DBillboard>();
                    billboard.enableSway = enableSway;
                    if (enableSway)
                    {
                        billboard.swaySpeed = Random.Range(2.0f, 3.2f);
                        billboard.swayAmount = Random.Range(3.5f, 6.5f);
                    }
                    totalSpawned++;
                }
            }
        }
        Debug.Log($"HD-2D: Spawned {totalSpawned} flora items (grass, flowers, pebbles) across {zones.Length} zones.");
    }

    private static void SpawnVillager(GameObject root, string name, Vector3 pos, Sprite sprite, Material spriteMat, string[] dialogLines)
    {
        GameObject npc = new GameObject(name);
        npc.transform.position = pos;
        npc.transform.localScale = new Vector3(1.6f, 2.4f, 1f); // matches Knight aspect ratio
        npc.transform.SetParent(root.transform);
        npc.isStatic = true;

        SpriteRenderer npcSR = npc.AddComponent<SpriteRenderer>();
        npcSR.sprite = sprite;
        npcSR.material = spriteMat;
        npcSR.shadowCastingMode = ShadowCastingMode.On;
        npcSR.receiveShadows = true;

        npc.AddComponent<HD2DBillboard>();
        NPCDialogue npcDialogue = npc.AddComponent<NPCDialogue>();
        npcDialogue.dialogueLines = dialogLines;

        CapsuleCollider npcCol = npc.AddComponent<CapsuleCollider>();
        npcCol.center = new Vector3(0f, 0.9f, 0f);
        npcCol.radius = 0.4f;
        npcCol.height = 1.8f;
        npcCol.isTrigger = true;

        GameObject npcTrigger = new GameObject("Trigger");
        npcTrigger.transform.SetParent(npc.transform);
        npcTrigger.transform.localPosition = Vector3.zero;
        BoxCollider npcRange = npcTrigger.AddComponent<BoxCollider>();
        npcRange.isTrigger = true;
        npcRange.size = new Vector3(2.5f, 1.5f, 2.5f);
        TriggerForwarder npcForwarder = npcTrigger.AddComponent<TriggerForwarder>();
        npcForwarder.target = npc;

        GameObject npcPrompt = new GameObject("PromptPanel");
        npcPrompt.transform.SetParent(npc.transform);
        npcPrompt.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        npcPrompt.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        npcPrompt.AddComponent<HD2DBillboard>();
        npcPrompt.AddComponent<PulsingMistyPrompt>();
        
        TextMesh npcPromptTM = npcPrompt.AddComponent<TextMesh>();
        npcPromptTM.text = "...";
        npcPromptTM.fontSize = 24;
        npcPromptTM.color = Color.white;
        npcPromptTM.alignment = TextAlignment.Center;
        npcPromptTM.anchor = TextAnchor.MiddleCenter;

        // Glowing JRPG mist bubble sphere backing
        GameObject backing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        backing.name = "Backing";
        backing.transform.SetParent(npcPrompt.transform);
        backing.transform.localPosition = new Vector3(0f, 0f, 0.05f); // slightly behind text
        backing.transform.localScale = new Vector3(12f, 8f, 2f);
        DestroyImmediate(backing.GetComponent<Collider>());

        Renderer rend = backing.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        rend.material.color = new Color(0.18f, 0.55f, 0.95f, 0.65f); // glowing JRPG soft blue mist

        npcDialogue.promptRenderer = npcPrompt.GetComponent<MeshRenderer>();
    }

    private static void SpawnPatrollingNPC(GameObject root, string name, Vector3[] waypoints, Sprite[] spriteSheet, Material spriteMat, string[] dialogLines, Color tint, Vector3 scale)
    {
        GameObject npc = new GameObject(name);
        npc.transform.position = waypoints[0];
        npc.transform.localScale = scale;
        npc.transform.SetParent(root.transform);
        npc.isStatic = false; // NPCs move, so they shouldn't be static!

        SpriteRenderer npcSR = npc.AddComponent<SpriteRenderer>();
        npcSR.sprite = (spriteSheet != null && spriteSheet.Length > 6) ? spriteSheet[6] : null;
        npcSR.material = spriteMat;
        npcSR.color = tint;
        npcSR.shadowCastingMode = ShadowCastingMode.On;
        npcSR.receiveShadows = true;

        HD2DBillboard billboard = npc.AddComponent<HD2DBillboard>();
        billboard.enableSway = false; // No wind sway for characters!

        NPCDialogue npcDialogue = npc.AddComponent<NPCDialogue>();
        npcDialogue.dialogueLines = dialogLines;

        CapsuleCollider npcCol = npc.AddComponent<CapsuleCollider>();
        npcCol.center = new Vector3(0f, 0.9f, 0f);
        npcCol.radius = 0.35f;
        npcCol.height = 1.8f;
        npcCol.isTrigger = true;

        // Attach custom patrolling walk controller
        HD2DNPCController walkCtrl = npc.AddComponent<HD2DNPCController>();
        walkCtrl.sprites = spriteSheet;
        walkCtrl.patrolWaypoints = waypoints;
        walkCtrl.moveSpeed = Random.Range(1.1f, 1.5f);
        walkCtrl.idleTimeAtWaypoint = Random.Range(1.5f, 3.5f);

        GameObject npcTrigger = new GameObject("Trigger");
        npcTrigger.transform.SetParent(npc.transform);
        npcTrigger.transform.localPosition = Vector3.zero;
        BoxCollider npcRange = npcTrigger.AddComponent<BoxCollider>();
        npcRange.isTrigger = true;
        npcRange.size = new Vector3(2.5f, 1.5f, 2.5f);
        TriggerForwarder npcForwarder = npcTrigger.AddComponent<TriggerForwarder>();
        npcForwarder.target = npc;

        GameObject npcPrompt = new GameObject("PromptPanel");
        npcPrompt.transform.SetParent(npc.transform);
        npcPrompt.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        npcPrompt.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        npcPrompt.AddComponent<HD2DBillboard>();
        npcPrompt.AddComponent<PulsingMistyPrompt>();
        
        TextMesh npcPromptTM = npcPrompt.AddComponent<TextMesh>();
        npcPromptTM.text = "...";
        npcPromptTM.fontSize = 24;
        npcPromptTM.color = Color.white;
        npcPromptTM.alignment = TextAlignment.Center;
        npcPromptTM.anchor = TextAnchor.MiddleCenter;

        GameObject backing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        backing.name = "Backing";
        backing.transform.SetParent(npcPrompt.transform);
        backing.transform.localPosition = new Vector3(0f, 0f, 0.05f);
        backing.transform.localScale = new Vector3(12f, 8f, 2f);
        DestroyImmediate(backing.GetComponent<Collider>());

        Renderer rend = backing.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        rend.material.color = new Color(0.18f, 0.55f, 0.95f, 0.65f);
        
        npcDialogue.promptRenderer = npcPrompt.GetComponent<MeshRenderer>();
    }

    private static GameObject SpawnPlayer(GameObject root, Sprite[] playerSprites, Material spriteMat)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player"; // Tag the player so trigger interactions function perfectly!
        player.transform.position = new Vector3(0f, 2.5f, 5.0f); // Starts in the village plaza well center
        player.transform.localScale = new Vector3(1.6f, 2.4f, 1f);
        player.transform.SetParent(root.transform);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        if (playerSprites.Length > 6) sr.sprite = playerSprites[6]; // Face Front Idle
        sr.material = spriteMat;
        sr.shadowCastingMode = ShadowCastingMode.On;
        sr.receiveShadows = true;

        player.AddComponent<HD2DBillboard>();
        HD2DPlayerController ctrl = player.AddComponent<HD2DPlayerController>();
        ctrl.sprites = playerSprites;

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 1.0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.useGravity = true;
        rb.freezeRotation = true;

        CapsuleCollider cc = player.AddComponent<CapsuleCollider>();
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.radius = 0.35f;
        cc.height = 1.8f;

        CinematicCameraController camCtrl = Camera.main?.GetComponent<CinematicCameraController>();
        if (camCtrl != null)
        {
            camCtrl.target = player.transform;
            camCtrl.offset = new Vector3(0f, 6.5f, -8.5f); // Perfect JRPG scenic offset
        }

        return player;
    }

    private static void ConfigurePostProcessing(GameObject root)
    {
        GameObject volObj = GameObject.Find("Global Volume");
        if (volObj == null)
        {
            volObj = new GameObject("Global Volume");
            volObj.AddComponent<Volume>();
        }
        volObj.transform.SetParent(root.transform);

        Volume volume = volObj.GetComponent<Volume>();
        volume.isGlobal = true;

        VolumeProfile profile = volume.profile;
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.profile = profile;
        }

        profile.components.Clear();

        // 1. Depth of Field (Tilt-Shift bokeh)
        DepthOfField dof = profile.Add<DepthOfField>();
        dof.active = true;
        dof.mode.Override(DepthOfFieldMode.Bokeh);
        dof.focusDistance.Override(10.7f); // Focused sharply on player plane
        dof.focalLength.Override(105f);     // Cinematic zoom length
        dof.aperture.Override(1.8f);       // Blurry premium bokeh

        // 2. High-End Dreamy Bloom
        Bloom bloom = profile.Add<Bloom>();
        bloom.active = true;
        bloom.threshold.Override(0.85f);    // Slightly lower threshold for richer glow
        bloom.intensity.Override(2.4f);     // Boosted intensity for magical atmosphere
        bloom.scatter.Override(0.72f);
        bloom.tint.Override(new Color(1f, 0.90f, 0.78f)); // Soft golden-hour glow

        // 3. Cinematic Vignette
        Vignette vignette = profile.Add<Vignette>();
        vignette.active = true;
        vignette.intensity.Override(0.35f);
        vignette.smoothness.Override(0.42f);
        vignette.color.Override(new Color(0.02f, 0.01f, 0.05f)); // Deep dark purple vignette instead of plain black for emotional depth!

        // 4. Cinematic Color Adjustments
        ColorAdjustments adj = profile.Add<ColorAdjustments>();
        adj.active = true;
        adj.postExposure.Override(0.18f);
        adj.contrast.Override(18f);
        adj.saturation.Override(26f); // Slightly boosted saturation for vivid dreamscape
        adj.colorFilter.Override(new Color(1f, 0.97f, 0.90f));

        // 5. ACES Filmic Tonemapping (The gold standard for rich cinematic color grading)
        Tonemapping tonemapping = profile.Add<Tonemapping>();
        tonemapping.active = true;
        tonemapping.mode.Override(TonemappingMode.ACES);

        // 6. White Balance (Cozy JRPG golden warmth)
        WhiteBalance wb = profile.Add<WhiteBalance>();
        wb.active = true;
        wb.temperature.Override(18f);
        wb.tint.Override(4f);

        // 7. Chromatic Aberration (Fringed color division for dreamy, nostalgic memory-like perspective)
        ChromaticAberration ca = profile.Add<ChromaticAberration>();
        ca.active = true;
        ca.intensity.Override(0.28f);

        // 8. Film Grain (Cinema noise to integrate sharp 2D retro pixels with URP 3D lighting seamlessly)
        FilmGrain fg = profile.Add<FilmGrain>();
        fg.active = true;
        fg.type.Override(FilmGrainLookup.Medium1);
        fg.intensity.Override(0.16f);

        // 9. Lens Distortion (Gentle cinematic curved camera zoom projection)
        LensDistortion ld = profile.Add<LensDistortion>();
        ld.active = true;
        ld.intensity.Override(-0.06f);
        ld.scale.Override(1.0f);
    }

    // Cozy interior rooms builder
    private static void BuildInteriorRooms(GameObject root, Material plasterMat, Material stoneMat, Material woodMat, Material roofMat)
    {
        GameObject interiorFolder = new GameObject("Interior_Rooms");
        interiorFolder.transform.SetParent(root.transform);

        // ==================== 1. CHIEF RONALD'S HOUSE INTERIOR ====================
        GameObject chiefRoom = new GameObject("RonaldHouse_Interior");
        chiefRoom.transform.SetParent(interiorFolder.transform);
        chiefRoom.isStatic = true;

        GameObject floor1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor1.name = "Floor";
        floor1.transform.SetParent(chiefRoom.transform);
        floor1.transform.position = new Vector3(50f, -25.5f, 50f);
        floor1.transform.localScale = new Vector3(8f, 1.0f, 6f);
        floor1.GetComponent<Renderer>().material = woodMat;
        floor1.isStatic = true;

        GameObject ceiling1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling1.name = "Ceiling";
        ceiling1.transform.SetParent(chiefRoom.transform);
        ceiling1.transform.position = new Vector3(50f, -20.5f, 50f);
        ceiling1.transform.localScale = new Vector3(8.4f, 1.0f, 6.4f);
        ceiling1.GetComponent<Renderer>().material = woodMat;
        ceiling1.isStatic = true;

        GameObject backW1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backW1.name = "BackWall";
        backW1.transform.SetParent(chiefRoom.transform);
        backW1.transform.position = new Vector3(50f, -23f, 53f);
        backW1.transform.localScale = new Vector3(8f, 4f, 0.2f);
        backW1.GetComponent<Renderer>().material = plasterMat;
        backW1.isStatic = true;

        GameObject leftW1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftW1.name = "LeftWall";
        leftW1.transform.SetParent(chiefRoom.transform);
        leftW1.transform.position = new Vector3(46f, -23f, 50f);
        leftW1.transform.localScale = new Vector3(0.2f, 4f, 6f);
        leftW1.GetComponent<Renderer>().material = plasterMat;
        leftW1.isStatic = true;

        GameObject rightW1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightW1.name = "RightWall";
        rightW1.transform.SetParent(chiefRoom.transform);
        rightW1.transform.position = new Vector3(54f, -23f, 50f);
        rightW1.transform.localScale = new Vector3(0.2f, 4f, 6f);
        rightW1.GetComponent<Renderer>().material = plasterMat;
        rightW1.isStatic = true;

        float frontSegWidth = 3.4f;
        GameObject fL1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fL1.name = "FrontWallLeft";
        fL1.transform.SetParent(chiefRoom.transform);
        fL1.transform.position = new Vector3(47.7f, -23f, 47f);
        fL1.transform.localScale = new Vector3(frontSegWidth, 4f, 0.2f);
        fL1.GetComponent<Renderer>().material = plasterMat;
        fL1.isStatic = true;

        GameObject fR1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fR1.name = "FrontWallRight";
        fR1.transform.SetParent(chiefRoom.transform);
        fR1.transform.position = new Vector3(52.3f, -23f, 47f);
        fR1.transform.localScale = new Vector3(frontSegWidth, 4f, 0.2f);
        fR1.GetComponent<Renderer>().material = plasterMat;
        fR1.isStatic = true;

        GameObject lintel1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lintel1.name = "FrontWallLintel";
        lintel1.transform.SetParent(chiefRoom.transform);
        lintel1.transform.position = new Vector3(50f, -21.5f, 47f);
        lintel1.transform.localScale = new Vector3(1.2f, 1.0f, 0.2f);
        lintel1.GetComponent<Renderer>().material = plasterMat;
        lintel1.isStatic = true;

        GameObject hearth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hearth.name = "HearthStone";
        hearth.transform.SetParent(chiefRoom.transform);
        hearth.transform.position = new Vector3(50f, -24.5f, 52.6f);
        hearth.transform.localScale = new Vector3(1.8f, 1f, 0.6f);
        hearth.GetComponent<Renderer>().material = stoneMat;
        hearth.isStatic = true;

        GameObject fireL = new GameObject("FireplaceGlow");
        fireL.transform.SetParent(hearth.transform);
        fireL.transform.localPosition = new Vector3(0f, 0.5f, -0.3f);
        Light fl = fireL.AddComponent<Light>();
        fl.type = LightType.Point;
        fl.range = 5.5f;
        fl.color = new Color(1.0f, 0.42f, 0.05f);
        fl.intensity = 2f;
        fl.shadows = LightShadows.Soft;

        GameObject exitDoor1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exitDoor1.name = "InteriorExitDoor_Ronald";
        exitDoor1.transform.SetParent(chiefRoom.transform);
        exitDoor1.transform.position = new Vector3(50f, -24.1f, 47.05f);
        exitDoor1.transform.localScale = new Vector3(1.1f, 1.8f, 0.1f);
        exitDoor1.GetComponent<Renderer>().material = woodMat;
        exitDoor1.isStatic = true;
        DestroyImmediate(exitDoor1.GetComponent<Collider>()); // Destroy primitive solid collider to allow trigger entry!

        BoxCollider trig1 = exitDoor1.AddComponent<BoxCollider>();
        trig1.isTrigger = true;
        trig1.size = new Vector3(2.2f, 1.8f, 22.0f);

        HouseDoorTransition trans1 = exitDoor1.AddComponent<HouseDoorTransition>();
        trans1.targetPosition = new Vector3(-6.5f, 2.3f, 7.8f); // Teleports back outside Ronald's house!

        // ==================== CHIEF RONALD'S cozy study ====================
        // 1. Woven Rug
        GameObject rug1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rug1.name = "WovenRug";
        rug1.transform.SetParent(chiefRoom.transform);
        rug1.transform.position = new Vector3(50f, -24.99f, 50f);
        rug1.transform.localScale = new Vector3(4.5f, 0.02f, 3.2f);
        rug1.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ClothRedMat.mat");
        DestroyImmediate(rug1.GetComponent<Collider>());

        // 2. Ronald's Bed
        GameObject bedFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bedFrame.name = "BedFrame";
        bedFrame.transform.SetParent(chiefRoom.transform);
        bedFrame.transform.position = new Vector3(47.4f, -24.7f, 51.4f);
        bedFrame.transform.localScale = new Vector3(1.5f, 0.6f, 2.2f);
        bedFrame.GetComponent<Renderer>().material = woodMat;

        GameObject bedPillow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bedPillow.name = "Pillow";
        bedPillow.transform.SetParent(chiefRoom.transform);
        bedPillow.transform.position = new Vector3(47.4f, -24.3f, 52.1f);
        bedPillow.transform.localScale = new Vector3(1.2f, 0.2f, 0.5f);
        bedPillow.GetComponent<Renderer>().material = plasterMat;
        DestroyImmediate(bedPillow.GetComponent<Collider>());

        GameObject bedBlanket = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bedBlanket.name = "RoyalBlanket";
        bedBlanket.transform.SetParent(chiefRoom.transform);
        bedBlanket.transform.position = new Vector3(47.4f, -24.35f, 50.8f);
        bedBlanket.transform.localScale = new Vector3(1.35f, 0.25f, 1.4f);
        bedBlanket.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ClothRedMat.mat");
        DestroyImmediate(bedBlanket.GetComponent<Collider>());

        // 3. Tall Bookshelf
        GameObject bookshelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bookshelf.name = "BookshelfFrame";
        bookshelf.transform.SetParent(chiefRoom.transform);
        bookshelf.transform.position = new Vector3(53.2f, -23.5f, 51.2f);
        bookshelf.transform.localScale = new Vector3(1.2f, 3.0f, 0.6f);
        bookshelf.GetComponent<Renderer>().material = woodMat;

        for (float yShelf = -24.5f; yShelf <= -21.5f; yShelf += 0.8f)
        {
            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.transform.SetParent(bookshelf.transform);
            board.transform.localPosition = new Vector3(0f, (yShelf - (-23.5f)) / 3.0f, 0f);
            board.transform.localScale = new Vector3(0.95f, 0.05f, 0.9f);
            board.GetComponent<Renderer>().material = woodMat;
            DestroyImmediate(board.GetComponent<Collider>());
            
            for (float xBook = -0.4f; xBook <= 0.4f; xBook += 0.16f)
            {
                if (Random.value > 0.15f)
                {
                    GameObject book = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    book.name = "Book";
                    book.transform.SetParent(bookshelf.transform);
                    book.transform.localPosition = new Vector3(xBook, ((yShelf + 0.3f) - (-23.5f)) / 3.0f, -0.1f);
                    book.transform.localScale = new Vector3(0.08f, 0.45f, 0.65f);
                    
                    Color[] bookColors = { Color.red, Color.blue, Color.green, new Color(1f, 0.8f, 0.2f) };
                    Renderer bookR = book.GetComponent<Renderer>();
                    bookR.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                    bookR.material.color = bookColors[Random.Range(0, bookColors.Length)];
                    DestroyImmediate(book.GetComponent<Collider>());
                }
            }
        }

        // 4. Study Desk & Chair with Candle
        GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        desk.name = "StudyDesk";
        desk.transform.SetParent(chiefRoom.transform);
        desk.transform.position = new Vector3(53.2f, -24.5f, 48.8f);
        desk.transform.localScale = new Vector3(1.2f, 1.0f, 0.7f);
        desk.GetComponent<Renderer>().material = woodMat;

        GameObject chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chair.name = "StudyChair";
        chair.transform.SetParent(chiefRoom.transform);
        chair.transform.position = new Vector3(52.2f, -24.6f, 48.8f);
        chair.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f);
        chair.GetComponent<Renderer>().material = woodMat;

        GameObject chairBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chairBack.transform.SetParent(chair.transform);
        chairBack.transform.localPosition = new Vector3(-0.4f, 0.6f, 0f);
        chairBack.transform.localScale = new Vector3(0.15f, 1.2f, 1f);
        chairBack.GetComponent<Renderer>().material = woodMat;
        DestroyImmediate(chairBack.GetComponent<Collider>());

        GameObject candle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        candle.name = "Candle";
        candle.transform.SetParent(desk.transform);
        candle.transform.localPosition = new Vector3(0.2f, 0.65f, 0.1f);
        candle.transform.localScale = new Vector3(0.12f, 0.25f, 0.12f);
        candle.GetComponent<Renderer>().material = plasterMat;
        DestroyImmediate(candle.GetComponent<Collider>());

        GameObject candleLightObj = new GameObject("CandleLight");
        candleLightObj.transform.SetParent(candle.transform);
        candleLightObj.transform.localPosition = new Vector3(0f, 1f, 0f);
        Light candleLight = candleLightObj.AddComponent<Light>();
        candleLight.type = LightType.Point;
        candleLight.range = 4f;
        candleLight.color = new Color(1.0f, 0.75f, 0.45f);
        candleLight.intensity = 1.2f;

        // 5. Circular Dining Table
        GameObject dTable = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        dTable.name = "DiningTable";
        dTable.transform.SetParent(chiefRoom.transform);
        dTable.transform.position = new Vector3(49.8f, -24.5f, 49.6f);
        dTable.transform.localScale = new Vector3(1.1f, 0.5f, 1.1f);
        dTable.GetComponent<Renderer>().material = woodMat;

        GameObject dPlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        dPlate.name = "Plate";
        dPlate.transform.SetParent(dTable.transform);
        dPlate.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        dPlate.transform.localScale = new Vector3(0.6f, 0.05f, 0.6f);
        dPlate.GetComponent<Renderer>().material = plasterMat;
        DestroyImmediate(dPlate.GetComponent<Collider>());

        GameObject dCup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        dCup.name = "Cup";
        dCup.transform.SetParent(dTable.transform);
        dCup.transform.localPosition = new Vector3(0.25f, 0.65f, 0.2f);
        dCup.transform.localScale = new Vector3(0.2f, 0.25f, 0.2f);
        dCup.GetComponent<Renderer>().material = stoneMat;
        DestroyImmediate(dCup.GetComponent<Collider>());


        // ==================== 2. TAVERN & SHOP INTERIOR ====================
        GameObject tavernRoom = new GameObject("TavernShop_Interior");
        tavernRoom.transform.SetParent(interiorFolder.transform);
        tavernRoom.isStatic = true;

        GameObject floor2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor2.name = "Floor";
        floor2.transform.SetParent(tavernRoom.transform);
        floor2.transform.position = new Vector3(75f, -25.5f, 50f);
        floor2.transform.localScale = new Vector3(10f, 1.0f, 8f);
        floor2.GetComponent<Renderer>().material = woodMat;
        floor2.isStatic = true;

        GameObject ceiling2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling2.name = "Ceiling";
        ceiling2.transform.SetParent(tavernRoom.transform);
        ceiling2.transform.position = new Vector3(75f, -20.5f, 50f);
        ceiling2.transform.localScale = new Vector3(10.4f, 1.0f, 8.4f);
        ceiling2.GetComponent<Renderer>().material = woodMat;
        ceiling2.isStatic = true;

        GameObject backW2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backW2.name = "BackWall";
        backW2.transform.SetParent(tavernRoom.transform);
        backW2.transform.position = new Vector3(75f, -23f, 54f);
        backW2.transform.localScale = new Vector3(10f, 4f, 0.2f);
        backW2.GetComponent<Renderer>().material = plasterMat;
        backW2.isStatic = true;

        GameObject leftW2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftW2.name = "LeftWall";
        leftW2.transform.SetParent(tavernRoom.transform);
        leftW2.transform.position = new Vector3(70f, -23f, 50f);
        leftW2.transform.localScale = new Vector3(0.2f, 4f, 8f);
        leftW2.GetComponent<Renderer>().material = plasterMat;
        leftW2.isStatic = true;

        GameObject rightW2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightW2.name = "RightWall";
        rightW2.transform.SetParent(tavernRoom.transform);
        rightW2.transform.position = new Vector3(80f, -23f, 50f);
        rightW2.transform.localScale = new Vector3(0.2f, 4f, 8f);
        rightW2.GetComponent<Renderer>().material = plasterMat;
        rightW2.isStatic = true;

        float frontWidth2 = 4.4f;
        GameObject fL2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fL2.name = "FrontWallLeft";
        fL2.transform.SetParent(tavernRoom.transform);
        fL2.transform.position = new Vector3(72.2f, -23f, 46f);
        fL2.transform.localScale = new Vector3(frontWidth2, 4f, 0.2f);
        fL2.GetComponent<Renderer>().material = plasterMat;
        fL2.isStatic = true;

        GameObject fR2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fR2.name = "FrontWallRight";
        fR2.transform.SetParent(tavernRoom.transform);
        fR2.transform.position = new Vector3(77.8f, -23f, 46f);
        fR2.transform.localScale = new Vector3(frontWidth2, 4f, 0.2f);
        fR2.GetComponent<Renderer>().material = plasterMat;
        fR2.isStatic = true;

        GameObject lintel2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lintel2.name = "FrontWallLintel";
        lintel2.transform.SetParent(tavernRoom.transform);
        lintel2.transform.position = new Vector3(75f, -21.5f, 46f);
        lintel2.transform.localScale = new Vector3(1.2f, 1.0f, 0.2f);
        lintel2.GetComponent<Renderer>().material = plasterMat;
        lintel2.isStatic = true;

        GameObject counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        counter.name = "TavernCounter";
        counter.transform.SetParent(tavernRoom.transform);
        counter.transform.position = new Vector3(73.5f, -24.6f, 51.5f);
        counter.transform.localScale = new Vector3(0.8f, 0.9f, 4f);
        counter.GetComponent<Renderer>().material = woodMat;
        counter.isStatic = true;

        for (float zStool = 50f; zStool <= 53f; zStool += 1f)
        {
            GameObject stool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stool.name = "Stool";
            stool.transform.SetParent(tavernRoom.transform);
            stool.transform.position = new Vector3(74.5f, -24.7f, zStool);
            stool.transform.localScale = new Vector3(0.35f, 0.4f, 0.35f);
            stool.GetComponent<Renderer>().material = woodMat;
            stool.isStatic = true;
            DestroyImmediate(stool.GetComponent<Collider>());
        }

        GameObject hearth2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hearth2.name = "HearthStone";
        hearth2.transform.SetParent(tavernRoom.transform);
        hearth2.transform.position = new Vector3(78.5f, -24.5f, 53.6f);
        hearth2.transform.localScale = new Vector3(1.6f, 1f, 0.6f);
        hearth2.GetComponent<Renderer>().material = stoneMat;
        hearth2.isStatic = true;

        GameObject fireL2 = new GameObject("FireplaceGlow");
        fireL2.transform.SetParent(hearth2.transform);
        fireL2.transform.localPosition = new Vector3(0f, 0.5f, -0.3f);
        Light fl2 = fireL2.AddComponent<Light>();
        fl2.type = LightType.Point;
        fl2.range = 6f;
        fl2.color = new Color(1.0f, 0.38f, 0.02f);
        fl2.intensity = 2.4f;
        fl2.shadows = LightShadows.Soft;

        for (int idx = 0; idx < 3; idx++)
        {
            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "TavernBarrel";
            barrel.transform.SetParent(tavernRoom.transform);
            barrel.transform.position = new Vector3(71.2f + idx * 0.5f, -24.5f, 53.3f);
            barrel.transform.localScale = new Vector3(0.35f, 0.7f, 0.35f);
            barrel.GetComponent<Renderer>().material = woodMat;
            barrel.isStatic = true;
        }

        GameObject exitDoor2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exitDoor2.name = "InteriorExitDoor_Tavern";
        exitDoor2.transform.SetParent(tavernRoom.transform);
        exitDoor2.transform.position = new Vector3(75f, -24.1f, 46.05f);
        exitDoor2.transform.localScale = new Vector3(1.1f, 1.8f, 0.1f);
        exitDoor2.GetComponent<Renderer>().material = woodMat;
        exitDoor2.isStatic = true;
        DestroyImmediate(exitDoor2.GetComponent<Collider>()); // Destroy primitive solid collider to allow trigger entry!

        BoxCollider trig2 = exitDoor2.AddComponent<BoxCollider>();
        trig2.isTrigger = true;
        trig2.size = new Vector3(2.2f, 1.8f, 22.0f);

        HouseDoorTransition trans2 = exitDoor2.AddComponent<HouseDoorTransition>();
        trans2.targetPosition = new Vector3(10.0f, 2.3f, 7.8f); // Teleports back outside Tavern!

        // ==================== TAVERN & SHOP cozy details ====================
        // 1. Potion Bar Shelves behind the counter
        GameObject tavernShelves = new GameObject("TavernBarShelves");
        tavernShelves.transform.SetParent(tavernRoom.transform);
        tavernShelves.transform.position = new Vector3(71.2f, -23.4f, 51.5f);
        tavernShelves.isStatic = true;

        GameObject shelfBacking = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shelfBacking.transform.SetParent(tavernShelves.transform);
        shelfBacking.transform.localPosition = Vector3.zero;
        shelfBacking.transform.localScale = new Vector3(0.2f, 2.8f, 3.2f);
        shelfBacking.GetComponent<Renderer>().material = woodMat;

        for (float yPlank = -1.1f; yPlank <= 1.1f; yPlank += 0.8f)
        {
            GameObject plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plank.transform.SetParent(tavernShelves.transform);
            plank.transform.localPosition = new Vector3(0.25f, yPlank, 0f);
            plank.transform.localScale = new Vector3(0.5f, 0.06f, 3.2f);
            plank.GetComponent<Renderer>().material = woodMat;
            DestroyImmediate(plank.GetComponent<Collider>());

            for (float zBottle = -1.2f; zBottle <= 1.2f; zBottle += 0.5f)
            {
                if (Random.value > 0.2f)
                {
                    GameObject potion = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    potion.name = "PotionBottle";
                    potion.transform.SetParent(tavernShelves.transform);
                    potion.transform.localPosition = new Vector3(0.25f, yPlank + 0.2f, zBottle);
                    potion.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);

                    Color[] potionColors = {
                        new Color(1.0f, 0.15f, 0.15f),
                        new Color(0.15f, 0.5f, 1.0f),
                        new Color(0.15f, 0.95f, 0.15f)
                    };
                    Color pColor = potionColors[Random.Range(0, potionColors.Length)];

                    Renderer potR = potion.GetComponent<Renderer>();
                    potR.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    potR.material.SetColor("_BaseColor", pColor);
                    potR.material.SetFloat("_Smoothness", 0.9f);
                    potR.material.EnableKeyword("_EMISSION");
                    potR.material.SetColor("_EmissionColor", pColor * 2.0f);
                    DestroyImmediate(potion.GetComponent<Collider>());

                    GameObject cork = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    cork.transform.SetParent(potion.transform);
                    cork.transform.localPosition = new Vector3(0f, 1.1f, 0f);
                    cork.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f);
                    cork.GetComponent<Renderer>().material = woodMat;
                    DestroyImmediate(cork.GetComponent<Collider>());
                }
            }
        }

        // 2. Tavern Dining Tables
        Vector3[] tablePos = {
            new Vector3(77.8f, -24.5f, 50.8f),
            new Vector3(77.8f, -24.5f, 48.0f)
        };
        for (int tIdx = 0; tIdx < 2; tIdx++)
        {
            GameObject tab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tab.name = $"DiningTable_{tIdx}";
            tab.transform.SetParent(tavernRoom.transform);
            tab.transform.position = tablePos[tIdx];
            tab.transform.localScale = new Vector3(1.2f, 0.9f, 1.2f);
            tab.GetComponent<Renderer>().material = woodMat;

            float[] zOffsets = { -0.8f, 0.8f };
            for (int sIdx = 0; sIdx < 2; sIdx++)
            {
                GameObject tStool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tStool.name = "Stool";
                tStool.transform.SetParent(tavernRoom.transform);
                tStool.transform.position = tablePos[tIdx] + new Vector3(0f, -0.2f, zOffsets[sIdx]);
                tStool.transform.localScale = new Vector3(0.35f, 0.5f, 0.35f);
                tStool.GetComponent<Renderer>().material = woodMat;
                tStool.isStatic = true;
                DestroyImmediate(tStool.GetComponent<Collider>());
            }

            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plate.name = "Plate";
            plate.transform.SetParent(tab.transform);
            plate.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            plate.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
            plate.GetComponent<Renderer>().material = plasterMat;
            DestroyImmediate(plate.GetComponent<Collider>());

            GameObject meat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meat.name = "RoastMeat";
            meat.transform.SetParent(plate.transform);
            meat.transform.localPosition = new Vector3(0f, 1f, 0f);
            meat.transform.localScale = new Vector3(0.5f, 0.6f, 0.5f);
            meat.GetComponent<Renderer>().material = stoneMat;
            DestroyImmediate(meat.GetComponent<Collider>());

            GameObject mug = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mug.name = "Tankard";
            mug.transform.SetParent(tab.transform);
            mug.transform.localPosition = new Vector3(0.25f, 0.6f, 0.25f);
            mug.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
            mug.GetComponent<Renderer>().material = woodMat;
            DestroyImmediate(mug.GetComponent<Collider>());
        }

        // 3. Storage Corner
        SpawnCrate(tavernRoom, new Vector3(71.5f, -25.0f, 47.6f), new Vector3(0.8f, 0.8f, 0.8f), woodMat);
        SpawnCrate(tavernRoom, new Vector3(71.5f, -24.2f, 47.6f), new Vector3(0.7f, 0.7f, 0.7f), woodMat);
        SpawnBarrel(tavernRoom, new Vector3(72.5f, -25.0f, 47.6f), 0.32f, 0.75f, woodMat);

        // 4. Wall Banners
        GameObject bannerRed = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bannerRed.name = "TapestryRed";
        bannerRed.transform.SetParent(tavernRoom.transform);
        bannerRed.transform.position = new Vector3(71.8f, -22.4f, 53.88f);
        bannerRed.transform.localScale = new Vector3(1.2f, 2.2f, 0.05f);
        bannerRed.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ClothRedMat.mat");
        DestroyImmediate(bannerRed.GetComponent<Collider>());

        GameObject bannerGreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bannerGreen.name = "TapestryGreen";
        bannerGreen.transform.SetParent(tavernRoom.transform);
        bannerGreen.transform.position = new Vector3(78.2f, -22.4f, 53.88f);
        bannerGreen.transform.localScale = new Vector3(1.2f, 2.2f, 0.05f);
        bannerGreen.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ClothGreenMat.mat");
        DestroyImmediate(bannerGreen.GetComponent<Collider>());

        SpawnTierCollider(interiorFolder, new Vector3(50f, -25f, 50f), new Vector3(8f, 0.1f, 6f));
        SpawnTierCollider(interiorFolder, new Vector3(75f, -25f, 50f), new Vector3(10f, 0.1f, 8f));
    }

    private static int GetSpriteIndex(Sprite s)
    {
        if (s == null) return 999;
        string[] parts = s.name.Split('_');
        int idx = 0;
        if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out idx))
        {
            return idx;
        }
        return 999;
    }
}
