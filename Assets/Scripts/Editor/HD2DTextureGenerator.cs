using System.IO;
using UnityEditor;
using UnityEngine;

public class HD2DTextureGenerator : EditorWindow
{
    [MenuItem("Tools/HD-2D/Generate Textures")]
    public static void GenerateAllTextures()
    {
        string folderPath = "Assets/Textures";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 1. Terrain Tiles (32x32)
        GenerateGrassTexture(folderPath + "/grass.png");
        GenerateCliffTexture(folderPath + "/cliff.png");
        GenerateStoneTexture(folderPath + "/stone.png");
        GenerateWoodTexture(folderPath + "/wood.png");
        GenerateWaterTexture(folderPath + "/water.png");
        GenerateRoofTexture(folderPath + "/roof.png");
        GeneratePlasterTexture(folderPath + "/plaster.png");
        GenerateClothRedTexture(folderPath + "/cloth_red.png");
        GenerateClothWhiteTexture(folderPath + "/cloth_white.png");
        GenerateClothGreenTexture(folderPath + "/cloth_green.png");

        // 2. Props & Sprites
        GenerateTreeTexture(folderPath + "/tree.png");
        GenerateLanternTexture(folderPath + "/lantern.png");
        GenerateChestTexture(folderPath + "/chest.png");
        if (!File.Exists(folderPath + "/player.png"))
        {
            GeneratePlayerTexture(folderPath + "/player.png");
        }
        GenerateGrassTuftTexture(folderPath + "/grass_tuft.png");
        GenerateFlowerTexture(folderPath + "/flower.png");
        GeneratePebbleTexture(folderPath + "/pebble.png");
        
        // Differentiated NPC sheets!
        if (!File.Exists(folderPath + "/npc_chief.png"))
        {
            GenerateChiefTexture(folderPath + "/npc_chief.png");
        }
        if (!File.Exists(folderPath + "/npc_merchant.png"))
        {
            GenerateMerchantTexture(folderPath + "/npc_merchant.png");
        }
        if (!File.Exists(folderPath + "/npc_adventurer.png"))
        {
            GenerateAdventurerTexture(folderPath + "/npc_adventurer.png");
        }

        AssetDatabase.Refresh();

        // 3. Post-configure import settings
        ConfigureTextureImport(folderPath + "/grass.png", TextureImporterType.Default, TextureWrapMode.Repeat);
        ConfigureTextureImport(folderPath + "/cliff.png", TextureImporterType.Default, TextureWrapMode.Repeat);
        ConfigureTextureImport(folderPath + "/stone.png", TextureImporterType.Default, TextureWrapMode.Repeat);
        ConfigureTextureImport(folderPath + "/wood.png", TextureImporterType.Default, TextureWrapMode.Repeat);
        ConfigureTextureImport(folderPath + "/water.png", TextureImporterType.Default, TextureWrapMode.Repeat);
        ConfigureTextureImport(folderPath + "/roof.png", TextureImporterType.Default, TextureWrapMode.Repeat);
        ConfigureTextureImport(folderPath + "/plaster.png", TextureImporterType.Default, TextureWrapMode.Repeat);
        ConfigureTextureImport(folderPath + "/cloth_red.png", TextureImporterType.Default, TextureWrapMode.Repeat);
        ConfigureTextureImport(folderPath + "/cloth_white.png", TextureImporterType.Default, TextureWrapMode.Repeat);
        ConfigureTextureImport(folderPath + "/cloth_green.png", TextureImporterType.Default, TextureWrapMode.Repeat);
        
        ConfigureTextureImport(folderPath + "/tree.png", TextureImporterType.Sprite, TextureWrapMode.Clamp);
        ConfigureTextureImport(folderPath + "/lantern.png", TextureImporterType.Sprite, TextureWrapMode.Clamp);
        ConfigureTextureImport(folderPath + "/chest.png", TextureImporterType.Sprite, TextureWrapMode.Clamp, 3, 1);
        ConfigureTextureImport(folderPath + "/player.png", TextureImporterType.Sprite, TextureWrapMode.Clamp, 3, 4);
        ConfigureTextureImport(folderPath + "/npc_chief.png", TextureImporterType.Sprite, TextureWrapMode.Clamp, 3, 4);
        ConfigureTextureImport(folderPath + "/npc_merchant.png", TextureImporterType.Sprite, TextureWrapMode.Clamp, 3, 4);
        ConfigureTextureImport(folderPath + "/npc_adventurer.png", TextureImporterType.Sprite, TextureWrapMode.Clamp, 3, 4);
        ConfigureTextureImport(folderPath + "/grass_tuft.png", TextureImporterType.Sprite, TextureWrapMode.Clamp);
        ConfigureTextureImport(folderPath + "/flower.png", TextureImporterType.Sprite, TextureWrapMode.Clamp);
        ConfigureTextureImport(folderPath + "/pebble.png", TextureImporterType.Sprite, TextureWrapMode.Clamp);

        AssetDatabase.Refresh();
        Debug.Log("HD-2D: All premium textures & character sheets generated successfully!");
    }

    [MenuItem("Tools/HD-2D/Import Premium Sprites")]
    public static void ImportPremiumSprites()
    {
        string folderPath = "Assets/Textures";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string playerSource = @"C:\Users\kurif\.gemini\antigravity\brain\e89e3019-9642-4cb0-9461-5e591689b61e\player_spritesheet_1780326978667.png";
        string chiefSource = @"C:\Users\kurif\.gemini\antigravity\brain\e89e3019-9642-4cb0-9461-5e591689b61e\chief_spritesheet_1780327001297.png";
        string merchantSource = @"C:\Users\kurif\.gemini\antigravity\brain\e89e3019-9642-4cb0-9461-5e591689b61e\merchant_spritesheet_1780327019781.png";
        string adventurerSource = @"C:\Users\kurif\.gemini\antigravity\brain\e89e3019-9642-4cb0-9461-5e591689b61e\adventurer_spritesheet_1780327037316.png";

        ProcessAndSaveSprite(playerSource, folderPath + "/player.png", "player");
        ProcessAndSaveSprite(chiefSource, folderPath + "/npc_chief.png", "chief");
        ProcessAndSaveSprite(merchantSource, folderPath + "/npc_merchant.png", "merchant");
        ProcessAndSaveSprite(adventurerSource, folderPath + "/npc_adventurer.png", "adventurer");

        AssetDatabase.Refresh();

        ConfigureTextureImport(folderPath + "/player.png", TextureImporterType.Sprite, TextureWrapMode.Clamp, 3, 4);
        ConfigureTextureImport(folderPath + "/npc_chief.png", TextureImporterType.Sprite, TextureWrapMode.Clamp, 3, 4);
        ConfigureTextureImport(folderPath + "/npc_merchant.png", TextureImporterType.Sprite, TextureWrapMode.Clamp, 3, 4);
        ConfigureTextureImport(folderPath + "/npc_adventurer.png", TextureImporterType.Sprite, TextureWrapMode.Clamp, 3, 4);

        AssetDatabase.Refresh();
        Debug.Log("HD-2D: Premium JRPG sprite sheets imported, grid-aligned, background removed, and formatted successfully!");
    }

    private static void ProcessAndSaveSprite(string sourcePath, string destPath, string layoutType)
    {
        if (!File.Exists(sourcePath))
        {
            Debug.LogError("HD-2D Import: Source file not found: " + sourcePath);
            return;
        }

        byte[] bytes = File.ReadAllBytes(sourcePath);
        Texture2D srcTex = new Texture2D(2, 2);
        srcTex.LoadImage(bytes);

        int srcW = srcTex.width;
        int srcH = srcTex.height;

        int destW = 96;
        int destH = 192;
        Texture2D destTex = new Texture2D(destW, destH, TextureFormat.RGBA32, false);

        int cellW = 32;
        int cellH = 48;

        Color[] destColors = new Color[destW * destH];
        for (int i = 0; i < destColors.Length; i++) destColors[i] = new Color(0, 0, 0, 0);

        int srcCols = 4;
        int srcRows = 3;
        if (layoutType == "chief") { srcCols = 5; srcRows = 4; }
        else if (layoutType == "merchant") { srcCols = 4; srcRows = 4; }
        else if (layoutType == "adventurer") { srcCols = 4; srcRows = 3; }

        int srcCellW = srcW / srcCols;
        int srcCellH = srcH / srcRows;

        float borderPct = (layoutType == "merchant") ? 0.02f : 0.06f;

        for (int destRow = 0; destRow < 4; destRow++)
        {
            for (int destCol = 0; destCol < 3; destCol++)
            {
                int srcColIndex = 0;
                int srcRowIndex = 0;
                bool flipX = false;

                if (layoutType == "player")
                {
                    srcColIndex = destCol;
                    if (destRow == 0) // Back (destRow = 0 is bottom row of texture, maps to slices [9-11])
                    {
                        srcRowIndex = 0; 
                        flipX = false;
                    }
                    else if (destRow == 1) // Front (destRow = 1 is third row from top, maps to slices [6-8])
                    {
                        srcRowIndex = 2; 
                        flipX = false;
                    }
                    else if (destRow == 2) // Left (destRow = 2 is second row from top, maps to slices [3-5])
                    {
                        srcRowIndex = 1; 
                        flipX = true;
                    }
                    else if (destRow == 3) // Right (destRow = 3 is top row of texture, maps to slices [0-2])
                    {
                        srcRowIndex = 1; 
                        flipX = false;
                    }
                }
                else if (layoutType == "chief")
                {
                    srcColIndex = destCol;
                    if (destRow == 0) // Back
                    {
                        srcRowIndex = 2; 
                        flipX = false;
                    }
                    else if (destRow == 1) // Front
                    {
                        srcRowIndex = 3; 
                        flipX = false;
                    }
                    else if (destRow == 2) // Left
                    {
                        srcRowIndex = 1; 
                        flipX = true;
                    }
                    else if (destRow == 3) // Right
                    {
                        srcRowIndex = 1; 
                        flipX = false;
                    }
                }
                else if (layoutType == "merchant")
                {
                    srcColIndex = destCol;
                    if (destRow == 0) // Back
                    {
                        srcRowIndex = 0; 
                        flipX = false;
                    }
                    else if (destRow == 1) // Front
                    {
                        srcRowIndex = 3; 
                        flipX = false;
                    }
                    else if (destRow == 2) // Left
                    {
                        srcRowIndex = 1; 
                        flipX = false;
                    }
                    else if (destRow == 3) // Right
                    {
                        srcRowIndex = 2; 
                        flipX = false;
                    }
                }
                else if (layoutType == "adventurer")
                {
                    if (destRow == 0) srcColIndex = 2; // Back
                    else if (destRow == 1) srcColIndex = 0; // Front
                    else if (destRow == 2) srcColIndex = 1; // Left
                    else if (destRow == 3) srcColIndex = 3; // Right

                    if (destCol == 0) srcRowIndex = 2; // Frame 0 (top row)
                    else if (destCol == 1) srcRowIndex = 1; // Frame 1 (middle row)
                    else if (destCol == 2) srcRowIndex = 0; // Frame 2 (bottom row)

                    flipX = false;
                }

                int minX = Mathf.FloorToInt(srcColIndex * srcCellW + srcCellW * borderPct);
                int maxX = Mathf.CeilToInt((srcColIndex + 1) * srcCellW - srcCellW * borderPct);
                int minY = Mathf.FloorToInt(srcRowIndex * srcCellH + srcCellH * borderPct);
                int maxY = Mathf.CeilToInt((srcRowIndex + 1) * srcCellH - srcCellH * borderPct);

                int croppedW = maxX - minX;
                int croppedH = maxY - minY;

                int destStartX = destCol * cellW;
                int destStartY = destRow * cellH;

                for (int cy = 0; cy < cellH; cy++)
                {
                    float ratioY = (float)cy / cellH;
                    int sy = Mathf.Clamp(minY + Mathf.FloorToInt(ratioY * croppedH), 0, srcH - 1);

                    for (int cx = 0; cx < cellW; cx++)
                    {
                        int mappedCx = flipX ? (cellW - 1 - cx) : cx;
                        float ratioX = (float)mappedCx / cellW;
                        int sx = Mathf.Clamp(minX + Mathf.FloorToInt(ratioX * croppedW), 0, srcW - 1);

                        Color c = srcTex.GetPixel(sx, sy);

                        // Background extraction: turn white/light-grey background to transparent
                        if (c.r > 0.88f && c.g > 0.88f && c.b > 0.88f)
                        {
                            c = new Color(0, 0, 0, 0);
                        }

                        destColors[(destStartY + cy) * destW + (destStartX + cx)] = c;
                    }
                }
            }
        }

        destTex.SetPixels(destColors);
        destTex.Apply();

        byte[] pngBytes = destTex.EncodeToPNG();
        File.WriteAllBytes(destPath, pngBytes);

        DestroyImmediate(srcTex);
        DestroyImmediate(destTex);
    }

    private static void SaveTexture(Texture2D tex, string path)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        DestroyImmediate(tex);
    }

    private static void ConfigureTextureImport(string path, TextureImporterType type, TextureWrapMode wrap, int spriteCols = 1, int spriteRows = 1)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = type;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.wrapMode = wrap;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;

            if (type == TextureImporterType.Sprite && (spriteCols > 1 || spriteRows > 1))
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                
                int w = (path.Contains("player") || path.Contains("npc")) ? 96 : (path.Contains("chest") ? 48 : 32);
                int h = (path.Contains("player") || path.Contains("npc")) ? 192 : (path.Contains("chest") ? 16 : 32);
                int cellW = w / spriteCols;
                int cellH = h / spriteRows;

                SpriteMetaData[] sheet = new SpriteMetaData[spriteCols * spriteRows];
                int index = 0;
                for (int r = 0; r < spriteRows; r++)
                {
                    for (int c = 0; c < spriteCols; c++)
                    {
                        SpriteMetaData meta = new SpriteMetaData();
                        meta.name = Path.GetFileNameWithoutExtension(path) + "_" + index;
                        int yPos = h - ((r + 1) * cellH);
                        meta.rect = new Rect(c * cellW, yPos, cellW, cellH);
                        meta.alignment = (int)SpriteAlignment.BottomCenter;
                        meta.pivot = new Vector2(0.5f, 0f);
                        sheet[index++] = meta;
                    }
                }
                importer.spritesheet = sheet;
            }
            else if (type == TextureImporterType.Sprite)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                importer.SetTextureSettings(settings);
            }

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
    }

    #region Pixel Drawing Utilities
    private static void Fill(Color[] pixels, Color c)
    {
        for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
    }

    private static void SetPixel(Color[] p, int w, int x, int y, Color c)
    {
        if (x >= 0 && x < w && y >= 0 && y < p.Length / w)
        {
            p[y * w + x] = c;
        }
    }

    private static void DrawRect(Color[] p, int w, int x, int y, int rw, int rh, Color c)
    {
        for (int j = 0; j < rh; j++)
        {
            for (int i = 0; i < rw; i++)
            {
                SetPixel(p, w, x + i, y + j, c);
            }
        }
    }
    #endregion

    #region Terrain Texture Generators
    // Watercolor-style Soft Neighborhood Blender
    private static void SoftenTexture(Color[] p, int w, int h, float blurAmount = 0.5f)
    {
        Color[] temp = new Color[p.Length];
        System.Array.Copy(p, temp, p.Length);
        
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color sum = temp[y * w + x];
                float count = 1f;

                int[,] offsets = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };
                for (int i = 0; i < 4; i++)
                {
                    int nx = (x + offsets[i, 0] + w) % w;
                    int ny = (y + offsets[i, 1] + h) % h;
                    sum += temp[ny * w + nx] * blurAmount;
                    count += blurAmount;
                }
                p[y * w + x] = sum / count;
            }
        }
    }

    private static void GenerateGrassTexture(string path)
    {
        int w = 32;
        Texture2D tex = new Texture2D(w, w, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * w];
        
        Color grassBase = new Color(0.24f, 0.46f, 0.28f, 1f);  // Soft sage JRPG grass
        Color grassDark = new Color(0.16f, 0.32f, 0.18f, 1f);  // Soft forest blend
        Color grassLight = new Color(0.38f, 0.58f, 0.35f, 1f); // Warm soft moss green
        Color flowerYellow = new Color(0.95f, 0.78f, 0.15f, 1f);
        Color flowerWhite = new Color(0.96f, 0.96f, 0.96f, 1f);

        Fill(p, grassBase);

        // 1. Draw organic moss/clover shapes to blend
        Random.InitState(120);
        for (int i = 0; i < 15; i++)
        {
            int cx = Random.Range(4, w - 4);
            int cy = Random.Range(4, w - 4);
            int r = Random.Range(3, 7);
            for (int y = cy - r; y <= cy + r; y++)
            {
                for (int x = cx - r; x <= cx + r; x++)
                {
                    int tx = (x + w) % w;
                    int ty = (y + w) % w;
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (dist <= r)
                    {
                        p[ty * w + tx] = Color.Lerp(grassLight, grassDark, dist / r);
                    }
                }
            }
        }

        // Apply soft watercolor blend
        SoftenTexture(p, w, w, 0.9f);
        SoftenTexture(p, w, w, 0.5f);

        // 2. Add gorgeous soft daisies at fixed points
        int[,] flowers = {
            { 8, 10 }, { 24, 15 }, { 14, 24 }, { 28, 4 }
        };

        for (int i = 0; i < flowers.GetLength(0); i++)
        {
            int fx = flowers[i, 0];
            int fy = flowers[i, 1];
            SetPixel(p, w, fx, fy, flowerYellow);
            SetPixel(p, w, fx - 1, fy, flowerWhite);
            SetPixel(p, w, fx + 1, fy, flowerWhite);
            SetPixel(p, w, fx, fy - 1, flowerWhite);
            SetPixel(p, w, fx, fy + 1, flowerWhite);
            
            // soft shadow below flower
            SetPixel(p, w, fx, fy - 2, Color.Lerp(p[(fy - 2) * w + fx], grassDark, 0.5f));
        }

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateCliffTexture(string path)
    {
        int w = 32;
        Texture2D tex = new Texture2D(w, w, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * w];
        
        Color baseCliff = new Color(0.38f, 0.28f, 0.22f, 1f);  // Soft mud stone
        Color darkCliff = new Color(0.2f, 0.14f, 0.1f, 1f);    // Crevice
        Color lightCliff = new Color(0.48f, 0.38f, 0.32f, 1f);  // Highlight
        Color mossGreen = new Color(0.24f, 0.44f, 0.26f, 1f);  // Hanging moss

        Fill(p, baseCliff);

        // 1. Draw smooth strata cracks
        for (int y = 0; y < w; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (y == 8 || y == 16 || y == 24)
                {
                    SetPixel(p, w, x, y, darkCliff);
                }
                else if (y < 8 && (x == 6 || x == 22)) SetPixel(p, w, x, y, darkCliff);
                else if (y >= 8 && y < 16 && (x == 14 || x == 30)) SetPixel(p, w, x, y, darkCliff);
                else if (y >= 16 && y < 24 && (x == 10 || x == 26)) SetPixel(p, w, x, y, darkCliff);
                else if (y >= 24 && (x == 4 || x == 18)) SetPixel(p, w, x, y, darkCliff);
            }
        }

        // Apply soft blend on rock faces
        SoftenTexture(p, w, w, 0.8f);

        // 2. Add highlights on top edges
        for (int y = 1; y < w; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (p[(y - 1) * w + x] == darkCliff)
                {
                    SetPixel(p, w, x, y, Color.Lerp(p[y * w + x], lightCliff, 0.5f));
                }
            }
        }

        // 3. Soft moss hanging from top
        for (int x = 0; x < w; x++)
        {
            int mossDepth = (int)(Mathf.Sin(x * 0.8f) * 1.5f + 3f);
            for (int my = w - 1; my >= w - mossDepth; my--)
            {
                float t = (float)(w - 1 - my) / mossDepth;
                SetPixel(p, w, x, my, Color.Lerp(mossGreen, p[my * w + x], t));
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateStoneTexture(string path)
    {
        int w = 32;
        Texture2D tex = new Texture2D(w, w, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * w];
        
        Color groutColor = new Color(0.18f, 0.18f, 0.22f, 1f);  // Soft slate grout
        Color stoneBase = new Color(0.44f, 0.44f, 0.48f, 1f);   // River stone gray
        Color highlight = new Color(0.6f, 0.6f, 0.64f, 1f);     // Speclar bevel
        Color shadow = new Color(0.28f, 0.28f, 0.32f, 1f);      // Soft shadow
        Color mossSpot = new Color(0.22f, 0.34f, 0.24f, 1f);

        Fill(p, groutColor);

        // Staggered round stones with radial distance gradients
        int[,] stones = {
            { 8, 8, 6 }, { 24, 8, 6 }, { 16, 22, 7 },
            { -2, 22, 5 }, { 34, 22, 5 }, { 8, 34, 6 },
            { 24, 34, 6 }, { 8, -2, 6 }, { 24, -2, 6 }
        };

        for (int i = 0; i < stones.GetLength(0); i++)
        {
            int cx = stones[i, 0];
            int cy = stones[i, 1];
            int r = stones[i, 2];

            for (int y = cy - r; y <= cy + r; y++)
            {
                for (int x = cx - r; x <= cx + r; x++)
                {
                    int tx = (x + w) % w;
                    int ty = (y + w) % w;

                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist <= r)
                    {
                        float t = dist / r;
                        // Radial color transition (soft spherical highlight)
                        Color c = Color.Lerp(stoneBase, shadow, t * 0.6f);
                        
                        // Apply soft specular to top-left
                        if (dy < -0.5f && dx < -0.5f)
                        {
                            c = Color.Lerp(c, highlight, (1f - t) * 0.7f);
                        }
                        
                        p[ty * w + tx] = c;
                    }
                }
            }
        }

        // Apply double softening to grout and stone borders
        SoftenTexture(p, w, w, 0.9f);
        SoftenTexture(p, w, w, 0.4f);

        // Interspersed moss spots
        int[,] mossCoords = {
            { 0, 15 }, { 16, 0 }, { 16, 14 }, { 8, 15 }, { 24, 15 }
        };
        for (int i = 0; i < mossCoords.GetLength(0); i++)
        {
            int mx = mossCoords[i, 0];
            int my = mossCoords[i, 1];
            SetPixel(p, w, mx, my, Color.Lerp(p[my * w + mx], mossSpot, 0.7f));
            SetPixel(p, w, mx + 1, my, Color.Lerp(p[my * w + mx + 1], mossSpot, 0.5f));
        }

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateWoodTexture(string path)
    {
        int w = 32;
        Texture2D tex = new Texture2D(w, w, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * w];
        
        Color woodBase = new Color(0.48f, 0.32f, 0.2f, 1f);   // Rich oak base
        Color darkGrain = new Color(0.28f, 0.18f, 0.1f, 1f);   // Grain seams
        Color lightGrain = new Color(0.56f, 0.42f, 0.3f, 1f);  // Sanded bevel
        Color nailColor = new Color(0.35f, 0.35f, 0.38f, 1f);

        Fill(p, woodBase);

        // Plank divisions & vertical grain waves
        for (int y = 0; y < w; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int plankX = x % 8;

                if (plankX == 0)
                {
                    SetPixel(p, w, x, y, darkGrain);
                }
                else if (plankX == 1)
                {
                    SetPixel(p, w, x, y, lightGrain);
                }
                else
                {
                    int plankIdx = x / 8;
                    float wave = Mathf.Sin(y * 0.2f + plankIdx * 2.3f) * 1.5f;
                    int grainOffset = Mathf.RoundToInt(wave);
                    
                    if (plankX == 4 + grainOffset || plankX == 6 + grainOffset)
                    {
                        SetPixel(p, w, x, y, Color.Lerp(woodBase, darkGrain, 0.5f));
                    }
                }
            }
        }

        // Circular wood knots
        int[,] knots = { { 4, 12 }, { 20, 22 } };
        for (int i = 0; i < knots.GetLength(0); i++)
        {
            int kx = knots[i, 0];
            int ky = knots[i, 1];
            SetPixel(p, w, kx, ky, darkGrain);
            SetPixel(p, w, kx + 1, ky, darkGrain);
            SetPixel(p, w, kx, ky + 1, lightGrain);
        }

        // Apply softening for cozy sanded JRPG wood texture
        SoftenTexture(p, w, w, 0.8f);

        // Set iron nail dots post-blur so they stay crisp
        for (int plank = 0; plank < 4; plank++)
        {
            int nx = plank * 8 + 4;
            SetPixel(p, w, nx, 2, nailColor);
            SetPixel(p, w, nx, 30, nailColor);
        }

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateWaterTexture(string path)
    {
        int w = 32;
        Texture2D tex = new Texture2D(w, w, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * w];
        
        Color waterBase = new Color(0.12f, 0.48f, 0.65f, 1f);
        Color waveWhite = new Color(0.85f, 0.95f, 1.0f, 0.8f);
        Color deepBlue = new Color(0.06f, 0.3f, 0.48f, 1f);

        Fill(p, waterBase);

        for (int y = 0; y < w; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if ((x + y) % 16 == 0 && x > 2 && x < 28)
                {
                    SetPixel(p, w, x, y, waveWhite);
                    SetPixel(p, w, x + 1, y, waveWhite);
                    SetPixel(p, w, x - 1, y, deepBlue);
                }
                else if ((x - y + w) % 16 == 8 && x > 4 && x < 26)
                {
                    SetPixel(p, w, x, y, waveWhite);
                    SetPixel(p, w, x + 1, y, deepBlue);
                }
            }
        }

        // Soften water foam wave edges
        SoftenTexture(p, w, w, 0.7f);

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateRoofTexture(string path)
    {
        int w = 32;
        Texture2D tex = new Texture2D(w, w, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * w];
        
        Color terracottaBase = new Color(0.72f, 0.24f, 0.16f, 1f); // Terracotta JRPG red
        Color shingleDark = new Color(0.38f, 0.1f, 0.06f, 1f);
        Color shingleLight = new Color(0.86f, 0.45f, 0.3f, 1f);

        Fill(p, terracottaBase);

        // Scalloped shingle rows
        for (int y = 0; y < w; y++)
        {
            int rowIdx = y / 8;
            int localY = y % 8;

            for (int x = 0; x < w; x++)
            {
                int localX = (x + rowIdx * 4) % 8;

                if (localY == 0)
                {
                    SetPixel(p, w, x, y, shingleDark);
                }
                else if (localY == 7)
                {
                    if (localX >= 2 && localX <= 6) SetPixel(p, w, x, y, shingleLight);
                    else SetPixel(p, w, x, y, shingleDark);
                }
                else if (localX == 0 || localY == 1)
                {
                    SetPixel(p, w, x, y, shingleDark);
                }
            }
        }

        // Soften shingles for smooth molded clay relief
        SoftenTexture(p, w, w, 0.8f);

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GeneratePlasterTexture(string path)
    {
        int w = 32;
        Texture2D tex = new Texture2D(w, w, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * w];
        
        Color creamPlaster = new Color(0.94f, 0.91f, 0.85f, 1f); // Elegant warm cream shikkui
        Color dirtTan = new Color(0.8f, 0.72f, 0.62f, 1f);       // Soft tan weathering

        Fill(p, creamPlaster);

        // Apply a soft weathering vignette around the bottom and sides
        for (int y = 0; y < w; y++)
        {
            float bottomFactor = 1f - (float)y / 14f; // dirt rising from bottom
            if (bottomFactor < 0f) bottomFactor = 0f;

            for (int x = 0; x < w; x++)
            {
                float sideFactor = Mathf.Min(x, w - 1 - x) / 8f;
                sideFactor = 1f - sideFactor;
                if (sideFactor < 0f) sideFactor = 0f;

                float factor = Mathf.Max(bottomFactor, sideFactor * 0.4f);
                p[y * w + x] = Color.Lerp(creamPlaster, dirtTan, factor * 0.6f);
            }
        }

        // Apply double softening for perfect watercolor plaster look
        SoftenTexture(p, w, w, 0.9f);
        SoftenTexture(p, w, w, 0.6f);

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateClothRedTexture(string path)
    {
        int w = 32;
        Texture2D tex = new Texture2D(w, w, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * w];
        Color clothRed = new Color(0.85f, 0.28f, 0.25f, 1f); 
        Color darkRed = new Color(0.65f, 0.18f, 0.16f, 1f);  
        Fill(p, clothRed);
        for (int y = 0; y < w; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float fold = Mathf.Sin((float)x / (float)w * Mathf.PI * 4f); 
                if (fold < 0) fold = 0;
                float borderFactor = Mathf.Min(x, Mathf.Min(w - 1 - x, Mathf.Min(y, w - 1 - y))) / 16f;
                borderFactor = 1f - borderFactor;
                if (borderFactor < 0f) borderFactor = 0f;
                float factor = Mathf.Max(borderFactor * 0.5f, fold * 0.15f);
                p[y * w + x] = Color.Lerp(clothRed, darkRed, factor);
            }
        }
        SoftenTexture(p, w, w, 0.8f);
        SoftenTexture(p, w, w, 0.4f);
        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateClothWhiteTexture(string path)
    {
        int w = 32;
        Texture2D tex = new Texture2D(w, w, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * w];
        Color clothWhite = new Color(0.94f, 0.91f, 0.85f, 1f); 
        Color clothShade = new Color(0.8f, 0.74f, 0.67f, 1f); 
        Fill(p, clothWhite);
        for (int y = 0; y < w; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float fold = Mathf.Sin((float)x / (float)w * Mathf.PI * 4f);
                if (fold < 0) fold = 0;
                float borderFactor = Mathf.Min(x, Mathf.Min(w - 1 - x, Mathf.Min(y, w - 1 - y))) / 16f;
                borderFactor = 1f - borderFactor;
                if (borderFactor < 0f) borderFactor = 0f;
                float factor = Mathf.Max(borderFactor * 0.5f, fold * 0.15f);
                p[y * w + x] = Color.Lerp(clothWhite, clothShade, factor);
            }
        }
        SoftenTexture(p, w, w, 0.8f);
        SoftenTexture(p, w, w, 0.4f);
        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateClothGreenTexture(string path)
    {
        int w = 32;
        Texture2D tex = new Texture2D(w, w, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * w];
        Color clothGreen = new Color(0.24f, 0.58f, 0.36f, 1f); 
        Color darkGreen = new Color(0.15f, 0.4f, 0.23f, 1f);  
        Fill(p, clothGreen);
        for (int y = 0; y < w; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float fold = Mathf.Sin((float)x / (float)w * Mathf.PI * 4f); 
                if (fold < 0) fold = 0;
                float borderFactor = Mathf.Min(x, Mathf.Min(w - 1 - x, Mathf.Min(y, w - 1 - y))) / 16f;
                borderFactor = 1f - borderFactor;
                if (borderFactor < 0f) borderFactor = 0f;
                float factor = Mathf.Max(borderFactor * 0.5f, fold * 0.15f);
                p[y * w + x] = Color.Lerp(clothGreen, darkGreen, factor);
            }
        }
        SoftenTexture(p, w, w, 0.8f);
        SoftenTexture(p, w, w, 0.4f);
        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }
    #endregion

    #region Sprite Drawing Assets
    private static void GenerateTreeTexture(string path)
    {
        int w = 32;
        int h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * h];

        Color trans = new Color(0,0,0,0);
        Color leaf = new Color(0.12f, 0.36f, 0.18f, 1f);
        Color leafShadow = new Color(0.06f, 0.22f, 0.1f, 1f);
        Color leafLight = new Color(0.24f, 0.52f, 0.22f, 1f);
        Color wood = new Color(0.35f, 0.22f, 0.12f, 1f);

        Fill(p, trans);

        // Trunk
        DrawRect(p, w, 14, 0, 4, 10, wood);
        DrawRect(p, w, 14, 0, 1, 10, leafShadow);

        // Pine layers (stepped cones)
        int[] levels = { 26, 20, 12, 6 };
        int[] widths = { 10, 16, 22, 26 };
        for (int lvl = 0; lvl < 4; lvl++)
        {
            int startY = levels[lvl];
            int width = widths[lvl];
            int startX = 16 - width / 2;
            DrawRect(p, w, startX, startY, width, 6, leaf);
            // Highlights & Shading
            DrawRect(p, w, startX, startY, 1, 6, leafShadow);
            DrawRect(p, w, startX, startY, width, 2, leafShadow);
            DrawRect(p, w, startX + 1, startY + 4, width - 2, 2, leafLight);
        }

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateLanternTexture(string path)
    {
        int w = 32;
        int h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * h];

        Color trans = new Color(0,0,0,0);
        Color iron = new Color(0.15f, 0.15f, 0.18f, 1f);
        Color ironHighlight = new Color(0.28f, 0.28f, 0.32f, 1f);
        Color glass = new Color(0.96f, 0.7f, 0.2f, 1f); // Warm flame glass
        Color glow = new Color(1f, 0.9f, 0.4f, 1f);

        Fill(p, trans);

        // 1. Post (base to Y=20)
        DrawRect(p, w, 14, 0, 4, 20, iron);
        DrawRect(p, w, 14, 0, 1, 20, ironHighlight);

        // 2. Bracket hanger arm
        DrawRect(p, w, 14, 20, 10, 3, iron);
        DrawRect(p, w, 22, 16, 2, 4, iron);

        // 3. Lantern body hanging
        DrawRect(p, w, 20, 8, 6, 8, iron);
        DrawRect(p, w, 21, 9, 4, 6, glass);
        DrawRect(p, w, 22, 10, 2, 4, glow); // Inner light core

        // Top hanger loop
        DrawRect(p, w, 22, 15, 2, 1, iron);

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateChestTexture(string path)
    {
        int w = 48;
        int h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * h];

        Color trans = new Color(0,0,0,0);
        Color wood = new Color(0.42f, 0.24f, 0.12f, 1f);
        Color woodLight = new Color(0.58f, 0.35f, 0.18f, 1f);
        Color gold = new Color(0.95f, 0.75f, 0.15f, 1f);
        Color dark = new Color(0.1f, 0.1f, 0.12f, 1f);
        Color glow = new Color(1f, 0.95f, 0.7f, 1f);

        Fill(p, trans);

        // 3 Frames (width 16 each)
        for (int f = 0; f < 3; f++)
        {
            int offset = f * 16;

            if (f == 0) // Closed Chest
            {
                DrawRect(p, w, offset + 1, 0, 14, 11, wood);
                // Iron/Gold bands
                DrawRect(p, w, offset + 1, 0, 14, 1, dark); // base shadow
                DrawRect(p, w, offset + 3, 0, 2, 11, gold);
                DrawRect(p, w, offset + 11, 0, 2, 11, gold);
                DrawRect(p, w, offset + 7, 5, 2, 3, gold); // Lock plate
                SetPixel(p, w, offset + 7, 6, dark); // keyhole
                // Horizontal divider
                DrawRect(p, w, offset + 1, 6, 14, 1, dark);
            }
            else if (f == 1) // Opening (Radiating golden light!)
            {
                // Lid lifted up slightly
                DrawRect(p, w, offset + 1, 0, 14, 6, wood); // base
                DrawRect(p, w, offset + 3, 0, 2, 6, gold);
                DrawRect(p, w, offset + 11, 0, 2, 6, gold);

                // Golden core light shining out of mouth
                DrawRect(p, w, offset + 2, 6, 12, 4, glow);
                DrawRect(p, w, offset + 4, 7, 8, 2, Color.white);

                // Lid angled up Y=9..14
                DrawRect(p, w, offset + 1, 9, 14, 5, wood);
                DrawRect(p, w, offset + 3, 9, 2, 5, gold);
                DrawRect(p, w, offset + 11, 9, 2, 5, gold);
            }
            else // Fully Open
            {
                DrawRect(p, w, offset + 1, 0, 14, 6, wood);
                DrawRect(p, w, offset + 3, 0, 2, 6, gold);
                DrawRect(p, w, offset + 11, 0, 2, 6, gold);
                // Dark empty interior
                DrawRect(p, w, offset + 2, 5, 12, 2, dark);

                // Lid fully thrown back
                DrawRect(p, w, offset + 1, 7, 14, 4, wood);
                DrawRect(p, w, offset + 3, 7, 2, 4, gold);
                DrawRect(p, w, offset + 11, 7, 2, 4, gold);
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }
    #endregion

    #region High-Resolution 32x48 Character Sprite Sheet Drawings
    private static void GeneratePlayerTexture(string path)
    {
        // Sprite sheet: 3 columns x 4 rows. Cell size 32x48. Total size: 96x192.
        int fw = 32;
        int fh = 48;
        int w = fw * 3;
        int h = fh * 4;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * h];

        Color trans = new Color(0, 0, 0, 0);
        
        // 4-shade premium steel palette
        Color steel = new Color(0.48f, 0.55f, 0.68f, 1f);       // main steel
        Color steelLight = new Color(0.85f, 0.88f, 0.95f, 1f);  // bright steel highlight
        Color steelDark = new Color(0.3f, 0.35f, 0.45f, 1f);     // steel shade
        Color steelDeep = new Color(0.18f, 0.2f, 0.28f, 1f);     // visor/deep shadows
        
        // 3-shade premium gold palette
        Color gold = new Color(0.92f, 0.72f, 0.15f, 1f);        // gold trims
        Color goldLight = new Color(0.98f, 0.92f, 0.55f, 1f);   // gold highlight
        Color goldDark = new Color(0.65f, 0.45f, 0.05f, 1f);     // gold shadow
        
        // Plume
        Color plume = new Color(0.85f, 0.15f, 0.15f, 1f);
        Color plumeLight = new Color(0.98f, 0.45f, 0.45f, 1f);
        Color plumeDark = new Color(0.5f, 0.05f, 0.05f, 1f);
        
        // Cape
        Color cape = new Color(0.72f, 0.1f, 0.1f, 1f);
        Color capeLight = new Color(0.88f, 0.25f, 0.25f, 1f);
        Color capeDark = new Color(0.42f, 0.04f, 0.04f, 1f);
        
        Color leather = new Color(0.38f, 0.22f, 0.1f, 1f);
        Color shieldBlue = new Color(0.12f, 0.3f, 0.65f, 1f);
        Color shieldBlueDark = new Color(0.08f, 0.18f, 0.42f, 1f);
        Color visorGlow = new Color(0.2f, 0.82f, 1.0f, 1f); // Cyan glowing visor lens

        Fill(p, trans);

        for (int r = 0; r < 4; r++)
        {
            int rowOffset = r * fh;
            
            for (int f = 0; f < 3; f++)
            {
                int frameOffset = f * fw;

                if (r == 2) // FRONT (DOWN)
                {
                    // 1. Cape flowing behind
                    DrawRect(p, w, frameOffset + 5, rowOffset + 10, 4, 18, capeDark);
                    DrawRect(p, w, frameOffset + 23, rowOffset + 10, 4, 18, capeDark);
                    DrawRect(p, w, frameOffset + 4, rowOffset + 8, 2, 10, capeDark);
                    DrawRect(p, w, frameOffset + 26, rowOffset + 8, 2, 10, capeDark);

                    // 2. Shiny steel boots
                    int leftY = 0;
                    int rightY = 0;
                    if (f == 1) // Walk 1
                    {
                        leftY = 3;
                        rightY = 0;
                    }
                    else if (f == 2) // Walk 2
                    {
                        leftY = 0;
                        rightY = 3;
                    }
                    
                    // Left boot
                    DrawRect(p, w, frameOffset + 9, rowOffset + leftY, 5, 11, steel);
                    DrawRect(p, w, frameOffset + 9, rowOffset + leftY, 5, 2, steelDeep); // soles
                    DrawRect(p, w, frameOffset + 9, rowOffset + leftY + 2, 1, 9, steelDark); // left shade
                    DrawRect(p, w, frameOffset + 13, rowOffset + leftY + 2, 1, 9, steelLight); // right highlight
                    DrawRect(p, w, frameOffset + 10, rowOffset + leftY + 8, 3, 3, gold); // knee pads
                    DrawRect(p, w, frameOffset + 11, rowOffset + leftY + 9, 1, 1, goldLight);
                    
                    // Right boot
                    DrawRect(p, w, frameOffset + 18, rowOffset + rightY, 5, 11, steel);
                    DrawRect(p, w, frameOffset + 18, rowOffset + rightY, 5, 2, steelDeep); // soles
                    DrawRect(p, w, frameOffset + 18, rowOffset + rightY + 2, 1, 9, steelDark);
                    DrawRect(p, w, frameOffset + 22, rowOffset + rightY + 2, 1, 9, steelLight);
                    DrawRect(p, w, frameOffset + 19, rowOffset + rightY + 8, 3, 3, gold); // knee pads
                    DrawRect(p, w, frameOffset + 20, rowOffset + rightY + 9, 1, 1, goldLight);

                    // 3. Torso Armor
                    DrawRect(p, w, frameOffset + 8, rowOffset + 11, 16, 16, steel);
                    DrawRect(p, w, frameOffset + 8, rowOffset + 11, 1, 16, steelDark);
                    DrawRect(p, w, frameOffset + 23, rowOffset + 11, 1, 16, steelLight);
                    
                    // Leather belt and gold buckle
                    DrawRect(p, w, frameOffset + 8, rowOffset + 11, 16, 3, leather); 
                    DrawRect(p, w, frameOffset + 14, rowOffset + 11, 4, 3, gold); 
                    DrawRect(p, w, frameOffset + 15, rowOffset + 12, 2, 1, goldLight); 

                    // Chest center breastplate lining
                    DrawRect(p, w, frameOffset + 15, rowOffset + 14, 2, 10, steelLight);
                    DrawRect(p, w, frameOffset + 14, rowOffset + 14, 1, 10, steelDark);
                    DrawRect(p, w, frameOffset + 11, rowOffset + 16, 10, 2, steelDark);
                    
                    // Gold shoulder pauldrons
                    DrawRect(p, w, frameOffset + 6, rowOffset + 21, 5, 6, gold);
                    DrawRect(p, w, frameOffset + 6, rowOffset + 25, 5, 1, goldLight); // top highlight
                    DrawRect(p, w, frameOffset + 6, rowOffset + 21, 5, 1, goldDark);  // bottom shadow
                    DrawRect(p, w, frameOffset + 21, rowOffset + 21, 5, 6, gold);
                    DrawRect(p, w, frameOffset + 21, rowOffset + 25, 5, 1, goldLight);
                    DrawRect(p, w, frameOffset + 21, rowOffset + 21, 5, 1, goldDark);

                    // 4. Helmet Dome Y = 27..41
                    DrawRect(p, w, frameOffset + 8, rowOffset + 27, 16, 15, steel);
                    DrawRect(p, w, frameOffset + 8, rowOffset + 27, 16, 2, steelDark); // Neck collar
                    DrawRect(p, w, frameOffset + 8, rowOffset + 40, 3, 2, trans);
                    DrawRect(p, w, frameOffset + 21, rowOffset + 40, 3, 2, trans);
                    
                    // Helmet Shading
                    DrawRect(p, w, frameOffset + 8, rowOffset + 29, 2, 11, steelDark);
                    DrawRect(p, w, frameOffset + 22, rowOffset + 29, 2, 11, steelLight);
                    
                    // Visor Frame
                    DrawRect(p, w, frameOffset + 9, rowOffset + 31, 14, 6, steelDark);
                    DrawRect(p, w, frameOffset + 9, rowOffset + 31, 14, 1, gold); // Visor top gold trim
                    DrawRect(p, w, frameOffset + 10, rowOffset + 33, 12, 2, steelDeep); // Visor slit
                    
                    // Glowing cyan blue lens slit center
                    DrawRect(p, w, frameOffset + 14, rowOffset + 33, 4, 2, visorGlow);
                    DrawRect(p, w, frameOffset + 15, rowOffset + 34, 2, 1, Color.white); // spec
                    
                    // Dome highlight
                    DrawRect(p, w, frameOffset + 12, rowOffset + 37, 3, 3, steelLight);

                    // 5. Plume
                    DrawRect(p, w, frameOffset + 14, rowOffset + 41, 4, 7, plume);
                    DrawRect(p, w, frameOffset + 13, rowOffset + 43, 6, 4, plume);
                    DrawRect(p, w, frameOffset + 14, rowOffset + 46, 4, 2, plumeLight); // crest highlight
                    DrawRect(p, w, frameOffset + 13, rowOffset + 41, 1, 6, plumeDark); // left shade
                }
                else if (r == 3) // BACK (UP)
                {
                    // 1. Flowing cape with deep shaded folds
                    DrawRect(p, w, frameOffset + 6, rowOffset + 8, 20, 20, cape);
                    DrawRect(p, w, frameOffset + 6, rowOffset + 8, 2, 20, capeDark);
                    DrawRect(p, w, frameOffset + 24, rowOffset + 8, 2, 20, capeLight);
                    
                    // Cape folds
                    DrawRect(p, w, frameOffset + 10, rowOffset + 8, 3, 20, capeDark);
                    DrawRect(p, w, frameOffset + 13, rowOffset + 10, 2, 18, cape);
                    DrawRect(p, w, frameOffset + 19, rowOffset + 8, 3, 20, capeDark);
                    DrawRect(p, w, frameOffset + 22, rowOffset + 10, 2, 18, capeLight);

                    // 2. Boots showing under cape
                    int leftY = 0;
                    int rightY = 0;
                    if (f == 1)
                    {
                        leftY = 3;
                        rightY = 0;
                    }
                    else if (f == 2)
                    {
                        leftY = 0;
                        rightY = 3;
                    }
                    DrawRect(p, w, frameOffset + 9, rowOffset + leftY, 5, 8, steelDark);
                    DrawRect(p, w, frameOffset + 18, rowOffset + rightY, 5, 8, steelDark);

                    // 3. Pauldrons and back collar above cape
                    DrawRect(p, w, frameOffset + 6, rowOffset + 21, 5, 6, gold);
                    DrawRect(p, w, frameOffset + 6, rowOffset + 21, 5, 1, goldDark);
                    DrawRect(p, w, frameOffset + 21, rowOffset + 21, 5, 6, gold);
                    DrawRect(p, w, frameOffset + 21, rowOffset + 21, 5, 1, goldDark);
                    DrawRect(p, w, frameOffset + 11, rowOffset + 25, 10, 2, steel);

                    // 4. Back of Helmet
                    DrawRect(p, w, frameOffset + 8, rowOffset + 27, 16, 15, steel);
                    DrawRect(p, w, frameOffset + 8, rowOffset + 27, 16, 2, steelDark);
                    DrawRect(p, w, frameOffset + 8, rowOffset + 40, 3, 2, trans);
                    DrawRect(p, w, frameOffset + 21, rowOffset + 40, 3, 2, trans);
                    
                    // Shading lines
                    DrawRect(p, w, frameOffset + 8, rowOffset + 29, 2, 11, steelDark);
                    DrawRect(p, w, frameOffset + 22, rowOffset + 29, 2, 11, steelLight);
                    DrawRect(p, w, frameOffset + 15, rowOffset + 29, 2, 11, steelDark); // center ridge

                    // 5. Plume back view
                    DrawRect(p, w, frameOffset + 14, rowOffset + 41, 4, 7, plume);
                    DrawRect(p, w, frameOffset + 13, rowOffset + 43, 6, 4, plume);
                    DrawRect(p, w, frameOffset + 14, rowOffset + 45, 4, 3, plumeLight);
                    DrawRect(p, w, frameOffset + 13, rowOffset + 41, 1, 6, plumeDark);
                }
                else if (r == 1) // LEFT
                {
                    // 1. Cape blowing right with shading folds
                    DrawRect(p, w, frameOffset + 16, rowOffset + 8, 11, 20, cape);
                    DrawRect(p, w, frameOffset + 16, rowOffset + 8, 3, 20, capeDark);
                    DrawRect(p, w, frameOffset + 24, rowOffset + 8, 3, 19, capeLight);

                    // 2. Shiny steel boots
                    int frontX = 10, backX = 17;
                    int frontY = 0, backY = 0;
                    if (f == 1)
                    {
                        frontX = 7; frontY = 0;
                        backX = 19; backY = 2;
                    }
                    else if (f == 2)
                    {
                        frontX = 12; frontY = 2;
                        backX = 16; backY = 0;
                    }
                    // Front leg
                    DrawRect(p, w, frameOffset + frontX, rowOffset + frontY, 5, 11, steel);
                    DrawRect(p, w, frameOffset + frontX, rowOffset + frontY, 1, 11, steelDark);
                    DrawRect(p, w, frameOffset + frontX + 4, rowOffset + frontY, 1, 11, steelLight);
                    DrawRect(p, w, frameOffset + frontX + 1, rowOffset + frontY + 8, 3, 3, gold);
                    
                    // Back leg
                    DrawRect(p, w, frameOffset + backX, rowOffset + backY, 5, 11, steelDark);

                    // 3. Torso armor
                    DrawRect(p, w, frameOffset + 11, rowOffset + 11, 10, 16, steel);
                    DrawRect(p, w, frameOffset + 11, rowOffset + 11, 10, 3, leather); // Belt
                    DrawRect(p, w, frameOffset + 11, rowOffset + 11, 1, 16, steelDark);
                    DrawRect(p, w, frameOffset + 20, rowOffset + 11, 1, 16, steelLight);
                    
                    // Shoulder gold guard
                    DrawRect(p, w, frameOffset + 13, rowOffset + 21, 6, 6, gold);
                    DrawRect(p, w, frameOffset + 13, rowOffset + 21, 6, 1, goldDark);
                    DrawRect(p, w, frameOffset + 13, rowOffset + 25, 6, 1, goldLight);

                    // 4. Gold and Blue Shield
                    DrawRect(p, w, frameOffset + 6, rowOffset + 12, 7, 11, gold);
                    DrawRect(p, w, frameOffset + 6, rowOffset + 12, 7, 1, goldDark);
                    DrawRect(p, w, frameOffset + 7, rowOffset + 13, 5, 9, shieldBlue);
                    DrawRect(p, w, frameOffset + 7, rowOffset + 13, 1, 9, shieldBlueDark);
                    DrawRect(p, w, frameOffset + 8, rowOffset + 15, 3, 5, steelLight); // emblem

                    // 5. Helmet side profile
                    DrawRect(p, w, frameOffset + 9, rowOffset + 27, 14, 15, steel);
                    DrawRect(p, w, frameOffset + 9, rowOffset + 27, 14, 2, steelDark);
                    DrawRect(p, w, frameOffset + 9, rowOffset + 29, 2, 11, steelDark);
                    DrawRect(p, w, frameOffset + 21, rowOffset + 29, 2, 11, steelLight);
                    
                    // Visor side
                    DrawRect(p, w, frameOffset + 8, rowOffset + 31, 6, 6, steelDark);
                    DrawRect(p, w, frameOffset + 8, rowOffset + 31, 6, 1, gold);
                    DrawRect(p, w, frameOffset + 8, rowOffset + 33, 5, 2, steelDeep); // Visor slit
                    DrawRect(p, w, frameOffset + 8, rowOffset + 33, 2, 2, visorGlow);  // visor glow side view

                    // 6. Plume flowing right
                    DrawRect(p, w, frameOffset + 16, rowOffset + 41, 4, 7, plume);
                    DrawRect(p, w, frameOffset + 18, rowOffset + 43, 8, 4, plume);
                    DrawRect(p, w, frameOffset + 19, rowOffset + 45, 6, 2, plumeLight);
                    DrawRect(p, w, frameOffset + 16, rowOffset + 41, 1, 6, plumeDark);
                }
                else if (r == 0) // RIGHT
                {
                    // 1. Cape blowing left with shading folds
                    DrawRect(p, w, frameOffset + 5, rowOffset + 8, 11, 20, cape);
                    DrawRect(p, w, frameOffset + 5, rowOffset + 8, 3, 20, capeDark);
                    DrawRect(p, w, frameOffset + 13, rowOffset + 8, 3, 19, capeLight);

                    // 2. Shiny steel boots
                    int frontX = 17, backX = 10;
                    int frontY = 0, backY = 0;
                    if (f == 1)
                    {
                        frontX = 20; frontY = 0;
                        backX = 8; backY = 2;
                    }
                    else if (f == 2)
                    {
                        frontX = 15; frontY = 2;
                        backX = 11; backY = 0;
                    }
                    // Front leg
                    DrawRect(p, w, frameOffset + frontX, rowOffset + frontY, 5, 11, steel);
                    DrawRect(p, w, frameOffset + frontX, rowOffset + frontY, 1, 11, steelDark);
                    DrawRect(p, w, frameOffset + frontX + 4, rowOffset + frontY, 1, 11, steelLight);
                    DrawRect(p, w, frameOffset + frontX + 1, rowOffset + frontY + 8, 3, 3, gold);
                    
                    // Back leg
                    DrawRect(p, w, frameOffset + backX, rowOffset + backY, 5, 11, steelDark);

                    // 3. Torso armor
                    DrawRect(p, w, frameOffset + 11, rowOffset + 11, 10, 16, steel);
                    DrawRect(p, w, frameOffset + 11, rowOffset + 11, 10, 3, leather); // Belt
                    DrawRect(p, w, frameOffset + 11, rowOffset + 11, 1, 16, steelDark);
                    DrawRect(p, w, frameOffset + 20, rowOffset + 11, 1, 16, steelLight);
                    
                    // Shoulder gold guard
                    DrawRect(p, w, frameOffset + 13, rowOffset + 21, 6, 6, gold);
                    DrawRect(p, w, frameOffset + 13, rowOffset + 21, 6, 1, goldDark);
                    DrawRect(p, w, frameOffset + 13, rowOffset + 25, 6, 1, goldLight);

                    // 4. Gold and Blue Shield
                    DrawRect(p, w, frameOffset + 19, rowOffset + 12, 7, 11, gold);
                    DrawRect(p, w, frameOffset + 19, rowOffset + 12, 7, 1, goldDark);
                    DrawRect(p, w, frameOffset + 20, rowOffset + 13, 5, 9, shieldBlue);
                    DrawRect(p, w, frameOffset + 20, rowOffset + 13, 1, 9, shieldBlueDark);
                    DrawRect(p, w, frameOffset + 21, rowOffset + 15, 3, 5, steelLight); // emblem

                    // 5. Helmet side profile
                    DrawRect(p, w, frameOffset + 9, rowOffset + 27, 14, 15, steel);
                    DrawRect(p, w, frameOffset + 9, rowOffset + 27, 14, 2, steelDark);
                    DrawRect(p, w, frameOffset + 9, rowOffset + 29, 2, 11, steelDark);
                    DrawRect(p, w, frameOffset + 21, rowOffset + 29, 2, 11, steelLight);
                    
                    // Visor side
                    DrawRect(p, w, frameOffset + 18, rowOffset + 31, 6, 6, steelDark);
                    DrawRect(p, w, frameOffset + 18, rowOffset + 31, 6, 1, gold);
                    DrawRect(p, w, frameOffset + 19, rowOffset + 33, 5, 2, steelDeep); // Visor slit
                    DrawRect(p, w, frameOffset + 22, rowOffset + 33, 2, 2, visorGlow);  // visor glow side view

                    // 6. Plume flowing left
                    DrawRect(p, w, frameOffset + 12, rowOffset + 41, 4, 7, plume);
                    DrawRect(p, w, frameOffset + 6, rowOffset + 43, 8, 4, plume);
                    DrawRect(p, w, frameOffset + 7, rowOffset + 45, 6, 2, plumeLight);
                    DrawRect(p, w, frameOffset + 13, rowOffset + 41, 1, 6, plumeDark);
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateChiefTexture(string path)
    {
        // Chief Sheet: Flowing white beard, elegant blue JRPG elder robes with golden trim, staff
        int fw = 32;
        int fh = 48;
        int w = fw * 3;
        int h = fh * 4;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * h];

        Color trans = new Color(0,0,0,0);
        Color skin = new Color(1.0f, 0.8f, 0.65f, 1f);
        Color skinShade = new Color(0.85f, 0.62f, 0.48f, 1f);
        
        // 4-shade grand robe palette (royal indigo/purple)
        Color robe = new Color(0.22f, 0.15f, 0.42f, 1f);        // main deep indigo
        Color robeLight = new Color(0.42f, 0.32f, 0.72f, 1f);   // robe highlight
        Color robeDark = new Color(0.12f, 0.08f, 0.28f, 1f);    // robe shadow
        Color robeDeep = new Color(0.06f, 0.04f, 0.16f, 1f);    // deep folds
        
        Color gold = new Color(0.92f, 0.72f, 0.15f, 1f);
        Color goldLight = new Color(0.98f, 0.92f, 0.55f, 1f);
        Color goldDark = new Color(0.65f, 0.45f, 0.05f, 1f);

        // 3-shade flowing white beard
        Color beard = new Color(0.95f, 0.95f, 0.96f, 1f);
        Color beardShade = new Color(0.78f, 0.8f, 0.85f, 1f);
        Color beardDeep = new Color(0.58f, 0.6f, 0.68f, 1f);
        
        // Staff
        Color staff = new Color(0.38f, 0.2f, 0.1f, 1f);
        Color staffHighlight = new Color(0.55f, 0.35f, 0.18f, 1f);
        Color staffOrb = new Color(0.1f, 0.85f, 0.85f, 1f);       // Glowing sapphire crystal
        Color staffOrbGlow = new Color(0.7f, 1.0f, 1.0f, 1f);

        Fill(p, trans);

        for (int r = 0; r < 4; r++)
        {
            int rowOffset = r * fh;
            for (int f = 0; f < 3; f++)
            {
                int frameOffset = f * fw;

                int yStep = (f > 0) ? 2 : 0;

                if (r == 2) // FRONT (Chief with robe and staff)
                {
                    // Robe body Y=0..25, X=9..22 with beautiful shaded folds
                    DrawRect(p, w, frameOffset + 9, rowOffset + yStep, 14, 25 - yStep, robe);
                    DrawRect(p, w, frameOffset + 9, rowOffset + yStep, 3, 25 - yStep, robeDark); // left shade
                    DrawRect(p, w, frameOffset + 20, rowOffset + yStep, 3, 25 - yStep, robeLight); // right highlight
                    DrawRect(p, w, frameOffset + 14, rowOffset + yStep, 4, 25 - yStep, robeDeep); // center fold
                    
                    // Robe gold hem
                    DrawRect(p, w, frameOffset + 9, rowOffset + yStep, 14, 2, gold); 
                    DrawRect(p, w, frameOffset + 9, rowOffset + yStep + 1, 14, 1, goldLight);
                    DrawRect(p, w, frameOffset + 9, rowOffset + yStep, 14, 1, goldDark);
                    
                    // Center gold trim vertical lining
                    DrawRect(p, w, frameOffset + 15, rowOffset + yStep + 2, 2, 23 - yStep, gold); 
                    DrawRect(p, w, frameOffset + 16, rowOffset + yStep + 2, 1, 23 - yStep, goldLight); 

                    // Head Skin Y=26..37
                    DrawRect(p, w, frameOffset + 11, rowOffset + 26, 10, 11, skin);
                    DrawRect(p, w, frameOffset + 11, rowOffset + 26, 2, 11, skinShade); // side shade
                    
                    // Eyes (JRPG expressive dots)
                    SetPixel(p, w, frameOffset + 14, rowOffset + 32, robeDark);
                    SetPixel(p, w, frameOffset + 17, rowOffset + 32, robeDark);
                    SetPixel(p, w, frameOffset + 14, rowOffset + 33, Color.white);
                    SetPixel(p, w, frameOffset + 17, rowOffset + 33, Color.white);

                    // Flowing White Beard
                    DrawRect(p, w, frameOffset + 10, rowOffset + 20, 12, 7, beard);
                    DrawRect(p, w, frameOffset + 12, rowOffset + 16, 8, 4, beard);
                    DrawRect(p, w, frameOffset + 10, rowOffset + 20, 12, 1, beardShade);
                    DrawRect(p, w, frameOffset + 10, rowOffset + 19, 12, 1, beardDeep);
                    DrawRect(p, w, frameOffset + 13, rowOffset + 16, 6, 1, beardShade);
                    
                    // Beard highlights
                    DrawRect(p, w, frameOffset + 11, rowOffset + 23, 2, 4, Color.white);
                    DrawRect(p, w, frameOffset + 19, rowOffset + 23, 2, 4, Color.white);

                    // Elder hair cap
                    DrawRect(p, w, frameOffset + 10, rowOffset + 34, 12, 4, beard);
                    DrawRect(p, w, frameOffset + 10, rowOffset + 34, 12, 1, beardShade);

                    // Mahogany Staff
                    DrawRect(p, w, frameOffset + 6, rowOffset + 2, 2, 28, staff);
                    DrawRect(p, w, frameOffset + 7, rowOffset + 2, 1, 28, staffHighlight);
                    DrawRect(p, w, frameOffset + 5, rowOffset + 29, 4, 4, staff);
                    
                    // Glowing Sapphire crystal orb
                    DrawRect(p, w, frameOffset + 6, rowOffset + 32, 2, 2, staffOrb); 
                    DrawRect(p, w, frameOffset + 6, rowOffset + 33, 1, 1, staffOrbGlow); // shiny reflection
                }
                else if (r == 3) // BACK view
                {
                    // Full indigo robe back with folds
                    DrawRect(p, w, frameOffset + 9, rowOffset + yStep, 14, 26 - yStep, robe);
                    DrawRect(p, w, frameOffset + 9, rowOffset + yStep, 3, 26 - yStep, robeDark);
                    DrawRect(p, w, frameOffset + 20, rowOffset + yStep, 3, 26 - yStep, robeLight);
                    DrawRect(p, w, frameOffset + 9, rowOffset + yStep, 14, 2, gold);
                    
                    // White hair covering back of head
                    DrawRect(p, w, frameOffset + 10, rowOffset + 24, 12, 14, beard);
                    DrawRect(p, w, frameOffset + 10, rowOffset + 24, 12, 2, beardShade);
                    DrawRect(p, w, frameOffset + 11, rowOffset + 21, 10, 3, beardShade);
                    DrawRect(p, w, frameOffset + 11, rowOffset + 21, 10, 1, beardDeep);
                    DrawRect(p, w, frameOffset + 13, rowOffset + 27, 6, 9, Color.white); // highlights
                }
                else if (r == 1) // LEFT view
                {
                    // Robe side profile
                    DrawRect(p, w, frameOffset + 10, rowOffset + yStep, 12, 25 - yStep, robe);
                    DrawRect(p, w, frameOffset + 10, rowOffset + yStep, 3, 25 - yStep, robeDark);
                    DrawRect(p, w, frameOffset + 19, rowOffset + yStep, 3, 25 - yStep, robeLight);
                    DrawRect(p, w, frameOffset + 10, rowOffset + yStep, 12, 2, gold);
                    
                    // Skin
                    DrawRect(p, w, frameOffset + 11, rowOffset + 26, 9, 11, skin);
                    DrawRect(p, w, frameOffset + 11, rowOffset + 26, 2, 11, skinShade);
                    
                    // Beard profile
                    DrawRect(p, w, frameOffset + 8, rowOffset + 18, 5, 9, beard);
                    DrawRect(p, w, frameOffset + 8, rowOffset + 18, 5, 1, beardShade);
                    DrawRect(p, w, frameOffset + 8, rowOffset + 22, 2, 5, Color.white);
                    
                    // Hair back
                    DrawRect(p, w, frameOffset + 15, rowOffset + 20, 6, 17, beard);
                    DrawRect(p, w, frameOffset + 15, rowOffset + 20, 6, 2, beardShade);
                    
                    // Staff
                    DrawRect(p, w, frameOffset + 6, rowOffset + 2, 2, 32, staff);
                    DrawRect(p, w, frameOffset + 7, rowOffset + 2, 1, 32, staffHighlight);
                    DrawRect(p, w, frameOffset + 6, rowOffset + 33, 2, 2, staffOrb);
                    DrawRect(p, w, frameOffset + 6, rowOffset + 34, 1, 1, staffOrbGlow);
                }
                else // RIGHT view
                {
                    // Robe side profile
                    DrawRect(p, w, frameOffset + 10, rowOffset + yStep, 12, 25 - yStep, robe);
                    DrawRect(p, w, frameOffset + 10, rowOffset + yStep, 3, 25 - yStep, robeDark);
                    DrawRect(p, w, frameOffset + 19, rowOffset + yStep, 3, 25 - yStep, robeLight);
                    DrawRect(p, w, frameOffset + 10, rowOffset + yStep, 12, 2, gold);
                    
                    // Skin
                    DrawRect(p, w, frameOffset + 12, rowOffset + 26, 9, 11, skin);
                    DrawRect(p, w, frameOffset + 19, rowOffset + 26, 2, 11, skinShade);
                    
                    // Beard profile
                    DrawRect(p, w, frameOffset + 19, rowOffset + 18, 5, 9, beard);
                    DrawRect(p, w, frameOffset + 19, rowOffset + 18, 5, 1, beardShade);
                    DrawRect(p, w, frameOffset + 22, rowOffset + 22, 2, 5, Color.white);
                    
                    // Hair back
                    DrawRect(p, w, frameOffset + 11, rowOffset + 20, 6, 17, beard);
                    DrawRect(p, w, frameOffset + 11, rowOffset + 20, 6, 2, beardShade);
                    
                    // Staff
                    DrawRect(p, w, frameOffset + 24, rowOffset + 2, 2, 32, staff);
                    DrawRect(p, w, frameOffset + 24, rowOffset + 2, 1, 32, staffHighlight);
                    DrawRect(p, w, frameOffset + 24, rowOffset + 33, 2, 2, staffOrb);
                    DrawRect(p, w, frameOffset + 24, rowOffset + 34, 1, 1, staffOrbGlow);
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateMerchantTexture(string path)
    {
        // Merchant Sheet: Green tunic, leather vest/apron, brown trousers, carrying satchel bag
        int fw = 32;
        int fh = 48;
        int w = fw * 3;
        int h = fh * 4;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * h];

        Color trans = new Color(0,0,0,0);
        Color skin = new Color(1.0f, 0.82f, 0.68f, 1f);
        Color skinShade = new Color(0.85f, 0.65f, 0.5f, 1f);
        
        // 3-shade hair palette
        Color hair = new Color(0.42f, 0.28f, 0.16f, 1f);
        Color hairLight = new Color(0.65f, 0.48f, 0.3f, 1f);
        Color hairDark = new Color(0.28f, 0.18f, 0.1f, 1f);
        
        // 3-shade JRPG forest green tunic
        Color tunic = new Color(0.24f, 0.52f, 0.35f, 1f);
        Color tunicLight = new Color(0.35f, 0.68f, 0.48f, 1f);
        Color tunicDark = new Color(0.15f, 0.38f, 0.24f, 1f);
        
        // 3-shade Leather apron
        Color apron = new Color(0.38f, 0.24f, 0.15f, 1f);
        Color apronLight = new Color(0.55f, 0.38f, 0.25f, 1f);
        Color apronDark = new Color(0.25f, 0.15f, 0.08f, 1f);
        
        Color trousers = new Color(0.25f, 0.18f, 0.12f, 1f);
        Color boots = new Color(0.18f, 0.12f, 0.08f, 1f);
        Color bootsLight = new Color(0.28f, 0.2f, 0.15f, 1f);
        
        Color gold = new Color(0.92f, 0.72f, 0.15f, 1f);
        Color goldLight = new Color(0.98f, 0.92f, 0.55f, 1f);
        Color bag = new Color(0.55f, 0.38f, 0.24f, 1f); // Satchel pouch
        Color bagStrap = new Color(0.28f, 0.18f, 0.1f, 1f);

        Fill(p, trans);

        for (int r = 0; r < 4; r++)
        {
            int rowOffset = r * fh;
            for (int f = 0; f < 3; f++)
            {
                int frameOffset = f * fw;
                
                int leftY = 0, rightY = 0;
                if (f == 1) { leftY = 3; rightY = 0; }
                else if (f == 2) { leftY = 0; rightY = 3; }

                if (r == 2) // FRONT (Merchant)
                {
                    // Trousers / Boots
                    DrawRect(p, w, frameOffset + 10, rowOffset + leftY, 4, 8, boots);
                    DrawRect(p, w, frameOffset + 10, rowOffset + leftY + 6, 4, 2, trousers);
                    DrawRect(p, w, frameOffset + 13, rowOffset + leftY + 1, 1, 7, bootsLight); // highlight
                    
                    DrawRect(p, w, frameOffset + 18, rowOffset + rightY, 4, 8, boots);
                    DrawRect(p, w, frameOffset + 18, rowOffset + rightY + 6, 4, 2, trousers);
                    DrawRect(p, w, frameOffset + 21, rowOffset + rightY + 1, 1, 7, bootsLight);

                    // Tunic Y=8..25 with aprons
                    DrawRect(p, w, frameOffset + 8, rowOffset + 8, 16, 17, tunic);
                    DrawRect(p, w, frameOffset + 8, rowOffset + 8, 2, 17, tunicDark); // left shade
                    DrawRect(p, w, frameOffset + 22, rowOffset + 8, 2, 17, tunicLight); // right highlight
                    
                    DrawRect(p, w, frameOffset + 10, rowOffset + 8, 12, 14, apron); 
                    DrawRect(p, w, frameOffset + 10, rowOffset + 8, 2, 14, apronDark); // apron shade
                    DrawRect(p, w, frameOffset + 20, rowOffset + 8, 2, 14, apronLight); // apron highlight
                    
                    // Apron gold belt buckle
                    DrawRect(p, w, frameOffset + 10, rowOffset + 21, 12, 1, gold);
                    DrawRect(p, w, frameOffset + 15, rowOffset + 21, 2, 1, goldLight);

                    // Head Skin Y=26..37
                    DrawRect(p, w, frameOffset + 11, rowOffset + 26, 10, 11, skin);
                    DrawRect(p, w, frameOffset + 11, rowOffset + 26, 2, 11, skinShade); // shading

                    // Expressive JRPG vertical eyes with catchlight
                    SetPixel(p, w, frameOffset + 14, rowOffset + 31, hairDark);
                    SetPixel(p, w, frameOffset + 17, rowOffset + 31, hairDark);
                    SetPixel(p, w, frameOffset + 14, rowOffset + 32, Color.white);
                    SetPixel(p, w, frameOffset + 17, rowOffset + 32, Color.white);

                    // Cute hair caps & sideburns
                    DrawRect(p, w, frameOffset + 10, rowOffset + 35, 12, 4, hair);
                    DrawRect(p, w, frameOffset + 10, rowOffset + 38, 12, 1, hairLight); // hair shine top
                    DrawRect(p, w, frameOffset + 10, rowOffset + 31, 2, 4, hairDark); // sideburns
                    DrawRect(p, w, frameOffset + 20, rowOffset + 31, 2, 4, hair);

                    // Leather satchel pouch
                    DrawRect(p, w, frameOffset + 7, rowOffset + 11, 4, 5, bag);
                    DrawRect(p, w, frameOffset + 7, rowOffset + 11, 4, 1, apronDark); // bag flap
                    SetPixel(p, w, frameOffset + 9, rowOffset + 12, goldLight); // bag button
                    // Strap
                    for (int i = 0; i < 11; i++)
                    {
                        SetPixel(p, w, frameOffset + 8 + i, rowOffset + 16 + i, bagStrap);
                    }
                }
                else if (r == 3) // BACK
                {
                    DrawRect(p, w, frameOffset + 10, rowOffset + leftY, 4, 8, boots);
                    DrawRect(p, w, frameOffset + 18, rowOffset + rightY, 4, 8, boots);
                    
                    // Full green tunic back
                    DrawRect(p, w, frameOffset + 8, rowOffset + 8, 16, 17, tunic);
                    DrawRect(p, w, frameOffset + 8, rowOffset + 8, 3, 17, tunicDark);
                    DrawRect(p, w, frameOffset + 21, rowOffset + 8, 3, 17, tunicLight);
                    
                    // Hair back
                    DrawRect(p, w, frameOffset + 10, rowOffset + 26, 12, 13, hair);
                    DrawRect(p, w, frameOffset + 10, rowOffset + 36, 12, 3, hairLight);
                    DrawRect(p, w, frameOffset + 10, rowOffset + 26, 12, 2, hairDark);
                }
                else if (r == 1) // LEFT
                {
                    DrawRect(p, w, frameOffset + 11, rowOffset + leftY, 4, 8, boots);
                    DrawRect(p, w, frameOffset + 17, rowOffset + rightY, 4, 8, boots);
                    
                    DrawRect(p, w, frameOffset + 9, rowOffset + 8, 14, 17, tunic);
                    DrawRect(p, w, frameOffset + 9, rowOffset + 8, 3, 17, tunicDark);
                    DrawRect(p, w, frameOffset + 20, rowOffset + 8, 3, 17, tunicLight);
                    DrawRect(p, w, frameOffset + 9, rowOffset + 8, 4, 14, apron); // apron front profile
                    
                    DrawRect(p, w, frameOffset + 11, rowOffset + 26, 9, 11, skin);
                    DrawRect(p, w, frameOffset + 11, rowOffset + 26, 2, 11, skinShade);
                    DrawRect(p, w, frameOffset + 14, rowOffset + 31, 6, 7, hair);
                    DrawRect(p, w, frameOffset + 14, rowOffset + 31, 2, 7, hairDark);
                }
                else // RIGHT
                {
                    DrawRect(p, w, frameOffset + 11, rowOffset + leftY, 4, 8, boots);
                    DrawRect(p, w, frameOffset + 17, rowOffset + rightY, 4, 8, boots);
                    
                    DrawRect(p, w, frameOffset + 9, rowOffset + 8, 14, 17, tunic);
                    DrawRect(p, w, frameOffset + 9, rowOffset + 8, 3, 17, tunicDark);
                    DrawRect(p, w, frameOffset + 20, rowOffset + 8, 3, 17, tunicLight);
                    DrawRect(p, w, frameOffset + 19, rowOffset + 8, 4, 14, apron); // apron front profile
                    
                    DrawRect(p, w, frameOffset + 12, rowOffset + 26, 9, 11, skin);
                    DrawRect(p, w, frameOffset + 19, rowOffset + 26, 2, 11, skinShade);
                    DrawRect(p, w, frameOffset + 12, rowOffset + 31, 6, 7, hair);
                    DrawRect(p, w, frameOffset + 16, rowOffset + 31, 2, 7, hairLight);
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateAdventurerTexture(string path)
    {
        // Adventurer: Brown leather vest, green traveler hood/cloak, back-strapped sword
        int fw = 32;
        int fh = 48;
        int w = fw * 3;
        int h = fh * 4;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * h];

        Color trans = new Color(0,0,0,0);
        Color skin = new Color(1.0f, 0.82f, 0.68f, 1f);
        Color skinShaded = new Color(0.72f, 0.52f, 0.38f, 1f); // face shaded by hood!
        
        // 3-shade forest green traveler hood
        Color hood = new Color(0.15f, 0.38f, 0.24f, 1f);
        Color hoodLight = new Color(0.25f, 0.55f, 0.36f, 1f);
        Color hoodShadow = new Color(0.08f, 0.24f, 0.15f, 1f);
        
        // 3-shade leather armor vest
        Color leather = new Color(0.42f, 0.25f, 0.15f, 1f);
        Color leatherLight = new Color(0.58f, 0.38f, 0.24f, 1f);
        Color leatherDark = new Color(0.28f, 0.15f, 0.08f, 1f);
        
        Color boots = new Color(0.22f, 0.15f, 0.1f, 1f);
        Color bootsLight = new Color(0.35f, 0.25f, 0.18f, 1f);
        
        // Sword colors
        Color steel = new Color(0.7f, 0.72f, 0.75f, 1f);
        Color steelLight = new Color(0.9f, 0.9f, 0.95f, 1f);
        Color steelDark = new Color(0.4f, 0.42f, 0.45f, 1f);
        Color gold = new Color(0.92f, 0.72f, 0.15f, 1f);

        Fill(p, trans);

        for (int r = 0; r < 4; r++)
        {
            int rowOffset = r * fh;
            for (int f = 0; f < 3; f++)
            {
                int frameOffset = f * fw;

                int leftY = 0, rightY = 0;
                if (f == 1) { leftY = 3; rightY = 0; }
                else if (f == 2) { leftY = 0; rightY = 3; }

                if (r == 2) // FRONT (Adventurer)
                {
                    // Boots
                    DrawRect(p, w, frameOffset + 10, rowOffset + leftY, 4, 8, boots);
                    DrawRect(p, w, frameOffset + 13, rowOffset + leftY + 1, 1, 7, bootsLight);
                    DrawRect(p, w, frameOffset + 18, rowOffset + rightY, 4, 8, boots);
                    DrawRect(p, w, frameOffset + 21, rowOffset + rightY + 1, 1, 7, bootsLight);

                    // Leather Vest Y=8..25
                    DrawRect(p, w, frameOffset + 8, rowOffset + 8, 16, 17, leather);
                    DrawRect(p, w, frameOffset + 8, rowOffset + 8, 2, 17, leatherDark);
                    DrawRect(p, w, frameOffset + 22, rowOffset + 8, 2, 17, leatherLight);
                    
                    // Shoulder cloak draping
                    DrawRect(p, w, frameOffset + 7, rowOffset + 18, 2, 7, hood);
                    DrawRect(p, w, frameOffset + 7, rowOffset + 18, 1, 7, hoodShadow);
                    DrawRect(p, w, frameOffset + 23, rowOffset + 18, 2, 7, hood);
                    DrawRect(p, w, frameOffset + 24, rowOffset + 18, 1, 7, hoodLight);

                    // Head Skin Y=26..37 (shaded by hood at top Y=32..37)
                    DrawRect(p, w, frameOffset + 11, rowOffset + 26, 10, 6, skin);
                    DrawRect(p, w, frameOffset + 11, rowOffset + 32, 10, 5, skinShaded); // shadow
                    
                    // JRPG cute glowing eyes
                    SetPixel(p, w, frameOffset + 14, rowOffset + 31, leatherDark);
                    SetPixel(p, w, frameOffset + 17, rowOffset + 31, leatherDark);
                    SetPixel(p, w, frameOffset + 14, rowOffset + 32, Color.white);
                    SetPixel(p, w, frameOffset + 17, rowOffset + 32, Color.white);

                    // Traveler Hood wrapping head
                    DrawRect(p, w, frameOffset + 9, rowOffset + 32, 14, 8, hood);
                    DrawRect(p, w, frameOffset + 9, rowOffset + 32, 2, 8, hoodShadow); // hood left shade
                    DrawRect(p, w, frameOffset + 21, rowOffset + 32, 2, 8, hoodLight);  // hood right light
                    DrawRect(p, w, frameOffset + 10, rowOffset + 37, 12, 3, hoodLight);
                }
                else if (r == 3) // BACK view (carrying steel greatsword strapped diagonally)
                {
                    DrawRect(p, w, frameOffset + 10, rowOffset + leftY, 4, 8, boots);
                    DrawRect(p, w, frameOffset + 18, rowOffset + rightY, 4, 8, boots);
                    
                    // Green traveler cloak back
                    DrawRect(p, w, frameOffset + 7, rowOffset + 8, 18, 22, hood);
                    DrawRect(p, w, frameOffset + 7, rowOffset + 8, 3, 22, hoodShadow);
                    DrawRect(p, w, frameOffset + 22, rowOffset + 8, 3, 22, hoodLight);
                    
                    // Sword strapped diagonally across back Y=10..26, X=8..24
                    for (int sVal = 0; sVal < 16; sVal++)
                    {
                        // Double wide detailed sheathed steel sword
                        SetPixel(p, w, frameOffset + 8 + sVal, rowOffset + 10 + sVal, steel);
                        SetPixel(p, w, frameOffset + 8 + sVal, rowOffset + 9 + sVal, steelDark);
                        SetPixel(p, w, frameOffset + 9 + sVal, rowOffset + 10 + sVal, steelLight); // metallic highlight
                    }
                    // Gold crossguard at base
                    DrawRect(p, w, frameOffset + 8, rowOffset + 10, 3, 3, gold);
                    SetPixel(p, w, frameOffset + 9, rowOffset + 11, Color.white);
                }
                else if (r == 1) // LEFT view
                {
                    DrawRect(p, w, frameOffset + 10, rowOffset + leftY, 4, 8, boots);
                    DrawRect(p, w, frameOffset + 18, rowOffset + rightY, 4, 8, boots);
                    
                    DrawRect(p, w, frameOffset + 9, rowOffset + 8, 14, 17, leather);
                    DrawRect(p, w, frameOffset + 9, rowOffset + 8, 3, 17, leatherDark);
                    DrawRect(p, w, frameOffset + 20, rowOffset + 8, 3, 17, leatherLight);
                    
                    DrawRect(p, w, frameOffset + 9, rowOffset + 22, 13, 17, hood); // hood side profile
                    DrawRect(p, w, frameOffset + 9, rowOffset + 22, 2, 17, hoodShadow);
                    DrawRect(p, w, frameOffset + 10, rowOffset + 26, 6, 8, skin); // face sliver
                    DrawRect(p, w, frameOffset + 10, rowOffset + 30, 6, 4, skinShaded); // face shade
                }
                else // RIGHT view
                {
                    DrawRect(p, w, frameOffset + 10, rowOffset + leftY, 4, 8, boots);
                    DrawRect(p, w, frameOffset + 18, rowOffset + rightY, 4, 8, boots);
                    
                    DrawRect(p, w, frameOffset + 9, rowOffset + 8, 14, 17, leather);
                    DrawRect(p, w, frameOffset + 9, rowOffset + 8, 3, 17, leatherDark);
                    DrawRect(p, w, frameOffset + 20, rowOffset + 8, 3, 17, leatherLight);
                    
                    DrawRect(p, w, frameOffset + 10, rowOffset + 22, 13, 17, hood); // hood side profile
                    DrawRect(p, w, frameOffset + 21, rowOffset + 22, 2, 17, hoodLight);
                    DrawRect(p, w, frameOffset + 16, rowOffset + 26, 6, 8, skin); // face sliver
                    DrawRect(p, w, frameOffset + 16, rowOffset + 30, 6, 4, skinShaded); // face shade
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateGrassTuftTexture(string path)
    {
        int w = 16;
        int h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * h];

        Color trans = new Color(0f, 0f, 0f, 0f);
        Color grassDark = new Color(0.1f, 0.28f, 0.14f, 1f);
        Color grassLight = new Color(0.32f, 0.58f, 0.28f, 1f);

        Fill(p, trans);

        // Draw 3 nice blades of grass in a tuft
        // Blade 1 (Left, angled left)
        SetPixel(p, w, 5, 0, grassDark);
        SetPixel(p, w, 4, 1, grassDark);
        SetPixel(p, w, 3, 2, grassLight);
        SetPixel(p, w, 2, 3, grassLight);
        SetPixel(p, w, 1, 4, grassLight);
        SetPixel(p, w, 2, 2, grassLight);

        // Blade 2 (Center, pointing up)
        SetPixel(p, w, 8, 0, grassDark);
        SetPixel(p, w, 8, 1, grassDark);
        SetPixel(p, w, 8, 2, grassLight);
        SetPixel(p, w, 8, 3, grassLight);
        SetPixel(p, w, 9, 4, grassLight);
        SetPixel(p, w, 9, 5, grassLight);
        SetPixel(p, w, 7, 2, grassLight);

        // Blade 3 (Right, angled right)
        SetPixel(p, w, 10, 0, grassDark);
        SetPixel(p, w, 11, 1, grassDark);
        SetPixel(p, w, 12, 2, grassLight);
        SetPixel(p, w, 13, 3, grassLight);
        SetPixel(p, w, 14, 4, grassLight);
        SetPixel(p, w, 13, 2, grassLight);

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GenerateFlowerTexture(string path)
    {
        int w = 16;
        int h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * h];

        Color trans = new Color(0f, 0f, 0f, 0f);
        Color stemGreen = new Color(0.12f, 0.45f, 0.2f, 1f);
        Color leafGreen = new Color(0.24f, 0.58f, 0.28f, 1f);
        Color petalRed = new Color(0.92f, 0.22f, 0.28f, 1f);
        Color centerYellow = new Color(0.98f, 0.88f, 0.2f, 1f);

        Fill(p, trans);

        // Draw Stem
        for (int y = 0; y <= 8; y++)
        {
            SetPixel(p, w, 8, y, stemGreen);
        }
        
        // Draw Leaves
        SetPixel(p, w, 7, 3, leafGreen);
        SetPixel(p, w, 9, 4, leafGreen);
        SetPixel(p, w, 6, 2, leafGreen);
        SetPixel(p, w, 10, 3, leafGreen);

        // Draw Flower Head (blossoming circular daisy)
        SetPixel(p, w, 8, 9, centerYellow);
        SetPixel(p, w, 8, 10, petalRed);
        SetPixel(p, w, 8, 8, petalRed);
        SetPixel(p, w, 7, 9, petalRed);
        SetPixel(p, w, 9, 9, petalRed);
        
        // Diagonal petals
        SetPixel(p, w, 7, 10, petalRed);
        SetPixel(p, w, 9, 10, petalRed);
        SetPixel(p, w, 7, 8, petalRed);
        SetPixel(p, w, 9, 8, petalRed);

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }

    private static void GeneratePebbleTexture(string path)
    {
        int w = 16;
        int h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] p = new Color[w * h];

        Color trans = new Color(0f, 0f, 0f, 0f);
        Color shadow = new Color(0.18f, 0.2f, 0.25f, 1f);
        Color mainStone = new Color(0.48f, 0.52f, 0.58f, 1f);
        Color lightStone = new Color(0.72f, 0.75f, 0.8f, 1f);

        Fill(p, trans);

        // Main larger pebble (X: 3..7, Y: 1..4)
        DrawRect(p, w, 3, 1, 5, 1, shadow); // bottom shadow
        DrawRect(p, w, 3, 2, 5, 2, mainStone);
        DrawRect(p, w, 4, 3, 3, 1, lightStone); // top highlight
        SetPixel(p, w, 3, 2, shadow); // shade left side
        SetPixel(p, w, 7, 2, shadow); // shade right side

        // Second smaller pebble (X: 10..13, Y: 2..4)
        DrawRect(p, w, 10, 2, 4, 1, shadow);
        DrawRect(p, w, 10, 3, 4, 1, mainStone);
        SetPixel(p, w, 11, 3, lightStone);

        // Tiny scatter dots
        SetPixel(p, w, 2, 5, mainStone);
        SetPixel(p, w, 9, 1, shadow);

        tex.SetPixels(p);
        tex.Apply();
        SaveTexture(tex, path);
    }
    #endregion
}
