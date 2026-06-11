using UnityEngine;
using UnityEngine.UI;

public class NoteObject : MonoBehaviour
{
    public DrumType drumType;
    public bool isJudged = false;

    [Header("스프라이트")]
    public Sprite noteSprite;
    public Sprite xSprite;

    public void Init(DrumType type, float speed)
    {
        drumType = type;

        Image img = GetComponent<Image>();

        if (img != null)
        {
            switch (type)
            {
                case DrumType.Kick:
                case DrumType.Snare:
                    img.sprite = noteSprite;
                    img.color = Color.black;
                    break;
                case DrumType.HiHat:
                case DrumType.Crash:
                    img.sprite = xSprite;
                    img.color = Color.black;
                    break;
            }
        }
    }
}