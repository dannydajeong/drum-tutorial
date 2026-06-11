public enum DrumType { Kick, Snare, HiHat, Crash }

[System.Serializable]
public class NoteData
{
    public DrumType drumType;
    public float beatPosition; // 몇 번째 8분음표인지
}