using UnityEngine;
using UnityEngine.UI;

public class StaffRenderer : MonoBehaviour
{
    [Header("UI 연결")]
    public RectTransform staffContainer;

    [Header("설정")]
    public float staffWidth = 2000f;
    public float lineThickness = 4f;
    public float lineSpacing = 80f;  // 줄 간격
    public float bottomY = 0f;       // 맨 아래 줄 Y

    void Start()
    {
        DrawStaffLines();
        DrawBarLines();
        DrawLabels();
    }

    void DrawStaffLines()
    {
	
        for (int i = 0; i < 5; i++)
        {
            float y = bottomY + (i * lineSpacing);

            GameObject line = new GameObject("StaffLine" + i);
            line.transform.SetParent(staffContainer);

            RectTransform rt = line.AddComponent<RectTransform>();
            rt.localPosition = new Vector3(0, y, 0);
            rt.sizeDelta = new Vector2(staffWidth, lineThickness);

            Image img = line.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.8f); 
        }
    }

   void DrawBarLines()
{
    float[] xPositions = { -980f, 20f };  // 2마디 = 세로선 2개
    float totalHeight = lineSpacing * 4 + 20f;
    float centerY = bottomY + lineSpacing * 2f;


    foreach (float x in xPositions)
    {
        GameObject line = new GameObject("BarLine");
        line.transform.SetParent(staffContainer);

        RectTransform rt = line.AddComponent<RectTransform>();
        rt.localPosition = new Vector3(x, centerY, 0);
        rt.sizeDelta = new Vector2(lineThickness, totalHeight);

        Image img = line.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.8f); 
    }
}

    void DrawLabels()
    {
        // 줄1=0, 줄2=80, 줄3=160, 줄4=240, 줄5=320
	
        string[] names = { "CRASH", "HIHAT", "SNARE", "KICK" };
        float[] yPositions = {
            bottomY + lineSpacing * 4 + lineSpacing * 0.5f,  // Crash: 줄5 위
            bottomY + lineSpacing * 3 + lineSpacing * 0.5f,  // HiHat: 줄4~5 사이
            bottomY + lineSpacing * 1 + lineSpacing * 0.5f,  // Snare: 줄2~3 사이
            bottomY + lineSpacing * 0.5f,                    // Kick: 줄1~2 사이
        };

        for (int i = 0; i < names.Length; i++)
        {
            GameObject label = new GameObject(names[i] + "Label");
            label.transform.SetParent(staffContainer);

            RectTransform rt = label.AddComponent<RectTransform>();
            rt.localPosition = new Vector3(-1050f, yPositions[i], 0);
            rt.sizeDelta = new Vector2(100f, 40f);

            Text txt = label.AddComponent<Text>();
            txt.text = names[i];
            txt.fontSize = 30;
            txt.color = Color.black;
            txt.alignment = TextAnchor.MiddleRight;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }

public void Hide()
{
    staffContainer.gameObject.SetActive(false);
}

public void Show()
{
    staffContainer.gameObject.SetActive(true);
}
}