using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

public class CreateHuTieuSign
{
    // ===================== CO VIET NAM =====================
    [MenuItem("FPTSim/Create Co Viet Nam")]
    static void CreateFlag()
    {
        GameObject cart = GameObject.Find("vietnamese_hu_tieu_cart_-_stylized_3d_model");
        if (cart == null) { Debug.LogError("Khong tim thay cart!"); return; }

        // Xoa cu neu co
        foreach (string n in new[] { "CoVietNam" })
        {
            Transform old = cart.transform.Find(n);
            if (old != null) Object.DestroyImmediate(old.gameObject);
        }

        // ---- Tao texture co Viet Nam ----
        string texPath = "Assets/Materials/VietnamFlag_Tex.asset";
        string matFlagPath = "Assets/Materials/VietnamFlag_Mat.mat";
        if (!Directory.Exists("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        Texture2D flagTex = CreateVietnamFlagTexture();
        if (File.Exists(texPath)) AssetDatabase.DeleteAsset(texPath);
        AssetDatabase.CreateAsset(flagTex, texPath);
        AssetDatabase.SaveAssets();

        Material matFlag;
        if (File.Exists(matFlagPath)) AssetDatabase.DeleteAsset(matFlagPath);
        matFlag = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        matFlag.mainTexture = flagTex;
        AssetDatabase.CreateAsset(matFlag, matFlagPath);
        AssetDatabase.SaveAssets();

        // Material cot (xam toi)
        Material matPole = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        matPole.color = new Color(0.55f, 0.45f, 0.25f, 1f); // nau vang

        // ---- Root container ----
        GameObject flagRoot = new GameObject("CoVietNam");
        flagRoot.transform.SetParent(cart.transform, false);
        // Dat sang ben trai, ngang bang hieu
        // rotation X=90 de chong lai X=-90 cua cart -> vertical trong world
        flagRoot.transform.localPosition = new Vector3(-2.8f, 1.5f, 0.5f);
        flagRoot.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        flagRoot.transform.localScale = Vector3.one;

        // ---- Cot co (Cylinder doc) ----
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Cot";
        pole.transform.SetParent(flagRoot.transform, false);
        pole.transform.localPosition = new Vector3(0f, 0f, 0f);
        pole.transform.localRotation = Quaternion.identity;
        pole.transform.localScale = new Vector3(0.06f, 2.2f, 0.06f);
        pole.GetComponent<Renderer>().sharedMaterial = matPole;
        Object.DestroyImmediate(pole.GetComponent<CapsuleCollider>());

        // ---- Mat co (Quad) ----
        GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Quad);
        flag.name = "MatCo";
        flag.transform.SetParent(flagRoot.transform, false);
        // Dau tren cot, lech sang phai
        flag.transform.localPosition = new Vector3(1.0f, 1.8f, 0f);
        flag.transform.localRotation = Quaternion.identity;
        flag.transform.localScale = new Vector3(2.0f, 1.33f, 1f); // ty le 3:2
        flag.GetComponent<Renderer>().sharedMaterial = matFlag;
        Object.DestroyImmediate(flag.GetComponent<MeshCollider>());

        // ---- Dau nhon cot ----
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.name = "DauCot";
        tip.transform.SetParent(flagRoot.transform, false);
        tip.transform.localPosition = new Vector3(0f, 2.25f, 0f);
        tip.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
        Material matGold = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        matGold.color = new Color(1f, 0.8f, 0f, 1f);
        tip.GetComponent<Renderer>().sharedMaterial = matGold;
        Object.DestroyImmediate(tip.GetComponent<SphereCollider>());

        MarkDirty(cart);
        Selection.activeGameObject = flagRoot;
        Debug.Log("[HuTieu] Tao Co Viet Nam thanh cong!");
    }

    static Texture2D CreateVietnamFlagTexture()
    {
        int w = 600, h = 400; // resolution cao hon cho sao min hon
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

        Color red    = new Color(0.82f, 0.06f, 0.06f, 1f);
        Color yellow = new Color(1f, 0.86f, 0f, 1f);

        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = red;

        // Tinh cac dinh ngoi sao 5 canh
        Vector2 center = new Vector2(w * 0.5f, h * 0.5f);
        float outerR = h * 0.33f;
        float innerR = outerR * 0.382f;
        Vector2[] starPts = GetStarPoints(center, outerR, innerR, 5);

        // To mau sao
        int minY = Mathf.FloorToInt(center.y - outerR);
        int maxY = Mathf.CeilToInt(center.y + outerR);
        int minX = Mathf.FloorToInt(center.x - outerR);
        int maxX = Mathf.CeilToInt(center.x + outerR);

        for (int y = minY; y <= maxY; y++)
        for (int x = minX; x <= maxX; x++)
        {
            if (x < 0 || x >= w || y < 0 || y >= h) continue;
            if (PointInPolygon(new Vector2(x, y), starPts))
                pixels[y * w + x] = yellow;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Vector2[] GetStarPoints(Vector2 center, float outerR, float innerR, int points)
    {
        Vector2[] verts = new Vector2[points * 2];
        for (int i = 0; i < points * 2; i++)
        {
            // +PI/2 = bat dau tu dinh (1 diem len tren) - chuan co VN
            float angle = (i * Mathf.PI / points) + Mathf.PI / 2f;
            float r = (i % 2 == 0) ? outerR : innerR;
            verts[i] = center + new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
        }
        return verts;
    }

    static bool PointInPolygon(Vector2 p, Vector2[] poly)
    {
        bool inside = false;
        int n = poly.Length;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if ((poly[i].y > p.y) != (poly[j].y > p.y) &&
                p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) /
                       (poly[j].y - poly[i].y) + poly[i].x)
                inside = !inside;
        }
        return inside;
    }
    // =======================================================


    [MenuItem("FPTSim/Create Hu Tieu Sign")]
    static void CreateSign()
    {
        GameObject cart = GameObject.Find("vietnamese_hu_tieu_cart_-_stylized_3d_model");
        if (cart == null) { Debug.LogError("Khong tim thay cart!"); return; }

        Transform old = cart.transform.Find("BangHieu_HuTieu");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        GameObject sign = new GameObject("BangHieu_HuTieu");
        sign.transform.SetParent(cart.transform, false);

        TextMeshPro tmp = sign.AddComponent<TextMeshPro>();
        tmp.text = "H\u1ee7 Ti\u1ebfu\nNam Vang";
        tmp.fontSize = 3.5f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.85f, 0f, 1f);
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = new Color(0.75f, 0f, 0f, 1f);

        RectTransform rt = sign.GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = new Vector2(5f, 2.5f);

        sign.transform.localPosition = new Vector3(0f, 3.5f, 0.5f);
        sign.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        sign.transform.localScale = Vector3.one;

        MarkDirty(cart);
        Selection.activeGameObject = sign;
        Debug.Log("[HuTieu] Tao bang hieu thanh cong!");
    }

    [MenuItem("FPTSim/Create Den Long Do")]
    static void CreateLanterns()
    {
        GameObject cart = GameObject.Find("vietnamese_hu_tieu_cart_-_stylized_3d_model");
        if (cart == null) { Debug.LogError("Khong tim thay cart!"); return; }

        // Xoa den long cu neu co
        foreach (string n in new[] { "DenLong_Trai", "DenLong_Phai" })
        {
            Transform old = cart.transform.Find(n);
            if (old != null) Object.DestroyImmediate(old.gameObject);
        }

        // Tao material den long do
        string matPath = "Assets/Materials/DenLong_Red.mat";
        if (!Directory.Exists("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        Material matRed;
        if (!File.Exists(matPath))
        {
            matRed = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            matRed.color = new Color(0.9f, 0.05f, 0.05f, 1f);
            // Emission do am
            matRed.EnableKeyword("_EMISSION");
            matRed.SetColor("_EmissionColor", new Color(0.8f, 0.1f, 0.1f, 1f) * 1.5f);
            AssetDatabase.CreateAsset(matRed, matPath);
            AssetDatabase.SaveAssets();
        }
        else
        {
            matRed = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        }

        // Vi tri: 2 ben cua bang hieu (local space cua cart)
        Vector3[] positions = {
            new Vector3(-1.8f, 3.5f, 0.5f),  // Trai
            new Vector3( 1.8f, 3.5f, 0.5f),  // Phai
        };
        string[] names = { "DenLong_Trai", "DenLong_Phai" };

        for (int i = 0; i < 2; i++)
        {
            // --- Than den long (sphere dep hon) ---
            GameObject lantern = new GameObject(names[i]);
            lantern.transform.SetParent(cart.transform, false);
            lantern.transform.localPosition = positions[i];
            lantern.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f); // bau duc doc

            // Body sphere
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(lantern.transform, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = Vector3.one;
            body.GetComponent<Renderer>().material = matRed;
            Object.DestroyImmediate(body.GetComponent<SphereCollider>());

            // Cap tren
            GameObject capTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            capTop.name = "CapTop";
            capTop.transform.SetParent(lantern.transform, false);
            capTop.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            capTop.transform.localScale = new Vector3(0.15f, 0.12f, 0.15f);
            capTop.GetComponent<Renderer>().material = matRed;
            Object.DestroyImmediate(capTop.GetComponent<CapsuleCollider>());

            // Cap duoi
            GameObject capBot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            capBot.name = "CapBot";
            capBot.transform.SetParent(lantern.transform, false);
            capBot.transform.localPosition = new Vector3(0f, -0.65f, 0f);
            capBot.transform.localScale = new Vector3(0.15f, 0.12f, 0.15f);
            capBot.GetComponent<Renderer>().material = matRed;
            Object.DestroyImmediate(capBot.GetComponent<CapsuleCollider>());

            // Tua duoi (cylinder nho)
            GameObject tassel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tassel.name = "Tassel";
            tassel.transform.SetParent(lantern.transform, false);
            tassel.transform.localPosition = new Vector3(0f, -1.0f, 0f);
            tassel.transform.localScale = new Vector3(0.04f, 0.2f, 0.04f);

            Material matGold = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            matGold.color = new Color(1f, 0.8f, 0f, 1f);
            tassel.GetComponent<Renderer>().material = matGold;
            Object.DestroyImmediate(tassel.GetComponent<CapsuleCollider>());

            // --- Point Light ben trong ---
            GameObject lightGO = new GameObject("Light");
            lightGO.transform.SetParent(lantern.transform, false);
            lightGO.transform.localPosition = Vector3.zero;

            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.55f, 0.1f, 1f); // cam am
            light.intensity = 1.8f;
            light.range = 3.5f;
            light.shadows = LightShadows.None;
        }

        MarkDirty(cart);
        Debug.Log("[HuTieu] Tao 2 den long do thanh cong!");
    }

    [MenuItem("FPTSim/Remove Memory Pause Panel")]
    static void RemoveMemoryPause()
    {
        string[] names = { "PausePanel", "PauseButton" };
        int removed = 0;
        foreach (string n in names)
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == n && go.scene.IsValid())
                {
                    Debug.Log($"[Memory] Xoa: {go.name}");
                    Object.DestroyImmediate(go);
                    removed++;
                    break;
                }
            }
        }
        if (removed > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[Memory] Da xoa {removed} object(s) va luu scene.");
        }
        else
            Debug.LogWarning("[Memory] Khong tim thay PausePanel/PauseButton trong scene hien tai.");
    }

    static void MarkDirty(GameObject go)
    {
        EditorUtility.SetDirty(go);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
