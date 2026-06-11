using UnityEngine;
using UnityEngine.UI;

public class StaffRendererSong : MonoBehaviour
{
    [Header("UI 연결")]
    public RectTransform staffContainer;

    [Header("설정")]
    public float staffWidth = 2000f;
    public float lineThickness = 4f;
    public float lineSpacing = 55f;
    public float topStaffBottomY = 300f;
    public float bottomStaffBottomY = 0f;
    public float measureWidth = 200f;
    public float startX = -980f;

    [Header("레인 Y 위치")]
    public float crashLaneY = 220f;
    public float hiHatLaneY = 165f;
    public float snareLaneY = 55f;
    public float kickLaneY = 0f;

    void Awake()
    {
        DrawStaff(topStaffBottomY);
        DrawStaff(bottomStaffBottomY);
        DrawBarLines(topStaffBottomY);
        DrawBarLines(bottomStaffBottomY);
        DrawLabels(topStaffBottomY);
        DrawLabels(bottomStaffBottomY);
        staffContainer.gameObject.SetActive(false);
    }

    float[] GetBarLineXs()
    {
        float[] xs = new float[5];
        for (int i = 0; i < 5; i++)
            xs[i] = startX + i * measureWidth;
        return xs;
    }

    void DrawStaff(float bottomY)
    {
        for (int i = 0; i < 5; i++)
        {
            float y = bottomY + (i * lineSpacing);
            GameObject line = new GameObject("StaffLine");
            line.transform.SetParent(staffContainer);
            RectTransform rt = line.AddComponent<RectTransform>();
            rt.localPosition = new Vector3(startX + measureWidth * 2f, y, 0);
            rt.sizeDelta = new Vector2(staffWidth, lineThickness);
            Image img = line.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.8f);
        }
    }

    void DrawBarLines(float bottomY)
    {
        float totalHeight = lineSpacing * 4 + 20f;
        float centerY = bottomY + lineSpacing * 2f;
        float[] xs = GetBarLineXs();

        foreach (float x in xs)
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

    void DrawLabels(float bottomY)
    {
        string[] names = { "CRASH", "HIHAT", "SNARE", "KICK" };
        float[] yOffsets = {
            lineSpacing * 4 + lineSpacing * 0.5f,
            lineSpacing * 3 + lineSpacing * 0.5f,
            lineSpacing * 1 + lineSpacing * 0.5f,
            lineSpacing * 0.5f,
        };

        for (int i = 0; i < names.Length; i++)
        {
            GameObject label = new GameObject(names[i] + "Label");
            label.transform.SetParent(staffContainer);
            RectTransform rt = label.AddComponent<RectTransform>();
            rt.localPosition = new Vector3(startX - 70f, bottomY + yOffsets[i], 0);
            rt.sizeDelta = new Vector2(100f, 40f);
            Text txt = label.AddComponent<Text>();
            txt.text = names[i];
            txt.fontSize = 25;
            txt.color = Color.black;
            txt.alignment = TextAnchor.MiddleRight;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }

    public float GetLaneY(DrumType type, bool isTopStaff)
    {
        float offset = isTopStaff ? topStaffBottomY : bottomStaffBottomY;
        switch (type)
        {
            case DrumType.Crash: return offset + crashLaneY;
            case DrumType.HiHat: return offset + hiHatLaneY;
            case DrumType.Snare: return offset + snareLaneY;
            case DrumType.Kick:  return offset + kickLaneY;
            default: return offset;
        }
    }

    public float GetTopStaffY()
    {
        return topStaffBottomY + lineSpacing * 2f;
    }

    public float GetBottomStaffY()
    {
        return bottomStaffBottomY + lineSpacing * 2f;
    }

    public bool IsTopStaff(int measureIndex)
    {
        return (measureIndex % 8) < 4;
    }

    public float GetMeasureStartX(int measureIndex)
    {
        int posInRow = measureIndex % 4;
        return startX + posInRow * measureWidth;
    }

    public void Show() { staffContainer.gameObject.SetActive(true); }
    public void Hide() { staffContainer.gameObject.SetActive(false); }
}