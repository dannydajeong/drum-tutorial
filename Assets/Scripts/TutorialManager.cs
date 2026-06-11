using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [Header("UI 연결")]
    public RectTransform judgeLine;
    public RectTransform judgeLineSong;
    public GameObject notePrefab;
    public GameObject xPrefab;
    public Text judgementText;
    public Text comboText;
    public Text phaseText;
    public Text resultText;
    public Text countText;
    public RectTransform noteContainer;

    [Header("실전 모드")]
    public AudioSource songAudio;
    private StaffRendererSong staffRendererSong;

    [Header("악보 설정")]
    public float drumBpm = 80f;
    public float songBpm = 88f;
    public float measureWidth = 850f;
    public float judgeLineX = -980f;
    public float hitWindow = 0.15f;

    [Header("레인 Y 위치 (튜토리얼)")]
    public float crashY = 360f;
    public float hiHatY = 255f;
    public float snareY = -25f;
    public float kickY = -100f;

    private float currentBpm;
    private float secondsPerBeat;
    private float pixelsPerBeat;
    private float judgeLineSpeed;
    private List<NoteObject> activeNotes = new List<NoteObject>();
    private int totalNotes = 0;
    private int hitNotes = 0;
    private int combo = 0;
    private int maxCombo = 0;
    private bool isPlaying = false;
    private bool isSongPlayMode = false;
    private int currentMeasureIndex = 0;
    private Coroutine fadeCoroutine;
    private List<(NoteData note, int measureIndex)> allSongNotes = new List<(NoteData, int)>();

    private AudioSource kick;
    private AudioSource snare;
    private AudioSource hiHat;
    private AudioSource crash;

    void Start()
    {
        currentBpm = drumBpm;
        UpdateBpmSettings();

        kick  = GameObject.Find("Kick").GetComponent<AudioSource>();
        snare = GameObject.Find("Snare drum").GetComponent<AudioSource>();
        hiHat = GameObject.Find("HiHat2").GetComponent<AudioSource>();
        crash = GameObject.Find("CrashCymbal").GetComponent<AudioSource>();

        staffRendererSong = GetComponent<StaffRendererSong>();

        StartCoroutine(RunTutorial());
    }

    void UpdateBpmSettings()
    {
        secondsPerBeat = 60f / currentBpm / 2f;
        if (isSongPlayMode)
            pixelsPerBeat = staffRendererSong.measureWidth / 8f;
        else
            pixelsPerBeat = measureWidth / 8f;
        judgeLineSpeed = pixelsPerBeat / secondsPerBeat;
    }

    void Update()
    {
        if (!isPlaying) return;

        if (isSongPlayMode)
            judgeLineSong.localPosition += Vector3.right * judgeLineSpeed * Time.deltaTime;
        else
            judgeLine.localPosition += Vector3.right * judgeLineSpeed * Time.deltaTime;

        foreach (var note in activeNotes)
{
    float jx = isSongPlayMode ? judgeLineSong.localPosition.x : judgeLine.localPosition.x;
    if (!note.isJudged && note.transform.localPosition.x < jx - 30f)
    {
        note.isJudged = true;
        note.GetComponent<Image>().color = Color.red;
        combo = 0;
        UpdateCombo();
    }
}

        if (Keyboard.current.spaceKey.wasPressedThisFrame) Judge(DrumType.Kick);
        if (Keyboard.current.sKey.wasPressedThisFrame)     Judge(DrumType.Snare);
        if (Keyboard.current.kKey.wasPressedThisFrame)     Judge(DrumType.HiHat);
        if (Keyboard.current.qKey.wasPressedThisFrame)     Judge(DrumType.Crash);
    }

    // ═══════════════════════════════════════════════════
    // 전체 튜토리얼 흐름
    // ═══════════════════════════════════════════════════
    IEnumerator RunTutorial()
    {
        currentBpm = drumBpm;
        UpdateBpmSettings();

        yield return StartCoroutine(IntroPhase());

        yield return StartCoroutine(ShowPhase("KICK 연습\n(Space)"));
        yield return StartCoroutine(PlayPattern(MakeSimple(DrumType.Kick, 4), 3));

        yield return StartCoroutine(ShowPhase("HIHAT 연습\n(K키)"));
        yield return StartCoroutine(PlayPattern(MakeSimple(DrumType.HiHat, 8), 3));

        yield return StartCoroutine(ShowPhase("SNARE 연습\n(S키)"));
        yield return StartCoroutine(PlayPattern(MakeSimple(DrumType.Snare, 4), 3));

        yield return StartCoroutine(ShowPhase("CRASH 연습\n(Q키)"));
        yield return StartCoroutine(PlayPattern(MakeCrash(), 3));

        yield return StartCoroutine(ShowPhase("HIHAT + SNARE"));
        yield return StartCoroutine(PlayPattern(MakeHiHatSnare(), 3));

        yield return StartCoroutine(ShowPhase("HIHAT + SNARE + KICK"));
        yield return StartCoroutine(PlayPattern(MakeHiHatSnareKick(), 3));

        currentBpm = songBpm;
        UpdateBpmSettings();

        yield return StartCoroutine(ShowPhase("🎵 Song Tutorial 시작!\nYellow - Coldplay"));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(ShowPhase("패턴 1"));
        yield return StartCoroutine(PlaySongPattern(1, 5));

        yield return StartCoroutine(ShowPhase("패턴 2"));
        yield return StartCoroutine(PlaySongPattern(2, 5));

        yield return StartCoroutine(ShowPhase("패턴 3"));
        yield return StartCoroutine(PlaySongPattern(3, 5));

        yield return StartCoroutine(ShowPhase("패턴 1 + 2"));
        yield return StartCoroutine(PlaySongCombined(1, 2, 3));

        yield return StartCoroutine(ShowPhase("패턴 2 + 3"));
        yield return StartCoroutine(PlaySongCombined(2, 3, 3));

        ShowResult();
    }

    // ═══════════════════════════════════════════════════
    // DRUM TUTORIAL
    // ═══════════════════════════════════════════════════
    IEnumerator IntroPhase()
    {
        string[] names   = { "KICK (Space)", "SNARE (S키)", "HIHAT (K키)", "CRASH (Q키)" };
        DrumType[] types = { DrumType.Kick, DrumType.Snare, DrumType.HiHat, DrumType.Crash };

        for (int i = 0; i < types.Length; i++)
        {
            phaseText.text = names[i];
            yield return StartCoroutine(PlayPattern(MakeSimple(types[i], 4), 1));
            yield return new WaitForSeconds(0.5f);
        }
    }

    List<NoteData> MakeSimple(DrumType type, int count)
    {
        var list = new List<NoteData>();
        for (int i = 0; i < count; i++)
            list.Add(new NoteData { drumType = type, beatPosition = i * (8f / count) });
        return list;
    }

    List<NoteData> MakeCrash()
    {
        return new List<NoteData>
        {
            new NoteData { drumType = DrumType.Crash, beatPosition = 0 },
            new NoteData { drumType = DrumType.Crash, beatPosition = 4 }
        };
    }

    List<NoteData> MakeHiHatSnare()
    {
        var list = new List<NoteData>();
        for (int i = 0; i < 8; i++)
            list.Add(new NoteData { drumType = DrumType.HiHat, beatPosition = i });
        list.Add(new NoteData { drumType = DrumType.Snare, beatPosition = 2 });
        list.Add(new NoteData { drumType = DrumType.Snare, beatPosition = 6 });
        return list;
    }

    List<NoteData> MakeHiHatSnareKick()
    {
        var list = MakeHiHatSnare();
        list.Add(new NoteData { drumType = DrumType.Kick, beatPosition = 0 });
        list.Add(new NoteData { drumType = DrumType.Kick, beatPosition = 4 });
        return list;
    }

    // ═══════════════════════════════════════════════════
    // SONG TUTORIAL
    // ═══════════════════════════════════════════════════
    IEnumerator PlaySongPattern(int patternNum, int repeat)
    {
        for (int r = 0; r < repeat; r++)
        {
            ClearNotes();
            judgeLine.localPosition = new Vector3(judgeLineX, 330f, 0);

            bool isFirst = (r == 0);
            List<NoteData> pattern = GetSongPattern(patternNum, isFirst);

            foreach (var note in pattern) { SpawnNote(note); totalNotes++; }

            yield return StartCoroutine(Countdown());
            float duration = secondsPerBeat * 8f + 1f;
            isPlaying = true;
            yield return new WaitForSeconds(duration);
            isPlaying = false;
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator PlaySongCombined(int pattern1, int pattern2, int repeat)
    {
        for (int r = 0; r < repeat; r++)
        {
            ClearNotes();
            judgeLine.localPosition = new Vector3(judgeLineX, 330f, 0);

            List<NoteData> combined = new List<NoteData>();
            List<NoteData> p1 = GetSongPattern(pattern1, r == 0);
            List<NoteData> p2 = GetSongPattern(pattern2, false);

            combined.AddRange(p1);
            foreach (var note in p2)
                combined.Add(new NoteData { drumType = note.drumType, beatPosition = note.beatPosition + 16f });

            foreach (var note in combined) { SpawnNote(note); totalNotes++; }

            yield return StartCoroutine(Countdown());
            float duration = secondsPerBeat * 16f + 1f;
            isPlaying = true;
            yield return new WaitForSeconds(duration);
            isPlaying = false;
            yield return new WaitForSeconds(0.5f);
        }
    }

    List<NoteData> GetSongPattern(int patternNum, bool useCrash)
    {
        var list = new List<NoteData>();

        if (useCrash)
        {
            list.Add(new NoteData { drumType = DrumType.Crash, beatPosition = 0 });
            for (int i = 1; i < 8; i++)
                list.Add(new NoteData { drumType = DrumType.HiHat, beatPosition = i });
        }
        else
        {
            for (int i = 0; i < 8; i++)
                list.Add(new NoteData { drumType = DrumType.HiHat, beatPosition = i });
        }

        list.Add(new NoteData { drumType = DrumType.Snare, beatPosition = 2 });
        list.Add(new NoteData { drumType = DrumType.Snare, beatPosition = 6 });

        switch (patternNum)
        {
            case 1:
                list.Add(new NoteData { drumType = DrumType.Kick, beatPosition = 0 });
                list.Add(new NoteData { drumType = DrumType.Kick, beatPosition = 4 });
                list.Add(new NoteData { drumType = DrumType.Kick, beatPosition = 5 });
                break;
            case 2:
                list.Add(new NoteData { drumType = DrumType.Kick, beatPosition = 0 });
                list.Add(new NoteData { drumType = DrumType.Kick, beatPosition = 1 });
                list.Add(new NoteData { drumType = DrumType.Kick, beatPosition = 4 });
                list.Add(new NoteData { drumType = DrumType.Kick, beatPosition = 5 });
                break;
            case 3:
                list.Add(new NoteData { drumType = DrumType.Kick, beatPosition = 0 });
                list.Add(new NoteData { drumType = DrumType.Kick, beatPosition = 3 });
                list.Add(new NoteData { drumType = DrumType.Kick, beatPosition = 5 });
                break;
        }

        return list;
    }

    // ═══════════════════════════════════════════════════
    // 실전 모드 (Yellow - Coldplay)
    // ═══════════════════════════════════════════════════
    public void GoToSongPlay()
    {
        StopAllCoroutines();
        ClearNotes();
        isPlaying = false;
        isSongPlayMode = true;
        if (songAudio != null) songAudio.Stop();

        measureWidth = staffRendererSong.measureWidth;
        currentBpm = songBpm;
        UpdateBpmSettings();

        GetComponent<StaffRenderer>().Hide();
        staffRendererSong.Show();

        // JudgeLine 전환
        judgeLine.gameObject.SetActive(false);
        judgeLineSong.gameObject.SetActive(true);

        currentMeasureIndex = 0;
        totalNotes = 0;
        hitNotes = 0;
        combo = 0;
        maxCombo = 0;
        allSongNotes.Clear();

        BuildAllSongNotes();
        StartCoroutine(RunSongPlay());
    }

    void BuildAllSongNotes()
    {
        int m = 0;
        m += 4;
        AddSection(1, 8, "every2", ref m);
        AddSection(2, 21, "firstOnly", ref m);
        AddSection(3, 6, "every2", ref m);
        AddCrashKick(ref m);
        m += 1;
        AddSection(1, 8, "every2", ref m);
        AddSection(2, 14, "firstOnly", ref m);
        AddSection(3, 6, "every2", ref m);
        AddCrashKick(ref m);
        m += 1;
        AddSection(1, 20, "every2", ref m);
        AddCrashKick(ref m);
    }

    void AddSection(int patternNum, int measures, string crashMode, ref int m)
    {
        for (int i = 0; i < measures; i++)
        {
            bool useCrash = false;
            if (crashMode == "every2") useCrash = (i % 2 == 0);
            else if (crashMode == "firstOnly") useCrash = (i == 0);

            List<NoteData> pattern = GetSongPattern(patternNum, useCrash);
            foreach (var note in pattern)
                allSongNotes.Add((note, m));
            m++;
        }
    }

    void AddCrashKick(ref int m)
    {
        allSongNotes.Add((new NoteData { drumType = DrumType.Crash, beatPosition = 0 }, m));
        allSongNotes.Add((new NoteData { drumType = DrumType.Kick, beatPosition = 0 }, m));
        m++;
    }

    void SpawnMeasureGroup(int startMeasure)
    {
        //ClearNotes();
        for (int i = 0; i < 8; i++)
        {
            int mIdx = startMeasure + i;
            foreach (var (note, idx) in allSongNotes)
            {
                if (idx == mIdx)
                {
                    SpawnNoteSong(note, mIdx - startMeasure);
                    totalNotes++;
                }
            }

        }

 	judgeLineSong.localPosition = new Vector3(
    staffRendererSong.startX,
    staffRendererSong.GetTopStaffY(),
    0
);
    }

    IEnumerator RunSongPlay()
{
    phaseText.text = "Yellow - Coldplay";
    yield return new WaitForSeconds(1f);

    if (songAudio != null) songAudio.Play();

    int totalMeasures = 92;

    // 첫 8마디 스폰
    SpawnMeasureGroup(0);

    while (currentMeasureIndex < totalMeasures)
    {
        if (currentMeasureIndex % 8 == 0 && currentMeasureIndex != 0)
            SpawnMeasureGroup(currentMeasureIndex);

        isPlaying = (currentMeasureIndex >= 4);
        yield return new WaitForSeconds(secondsPerBeat * 8f);
        isPlaying = false;

        currentMeasureIndex++;

        // 4마디 끝난 후에 2줄로 점프
        if (currentMeasureIndex % 8 == 4)
        {
            judgeLineSong.localPosition = new Vector3(
                staffRendererSong.startX,
                staffRendererSong.GetBottomStaffY(),
                0
            );
        }

        // 8마디 끝난 후에 1줄로 리셋
        if (currentMeasureIndex % 8 == 0 && currentMeasureIndex != 0)
        {
            judgeLineSong.localPosition = new Vector3(
                staffRendererSong.startX,
                staffRendererSong.GetTopStaffY(),
                0
            );
        }
    }

    if (songAudio != null) songAudio.Stop();
    ShowResult();
}

    void SpawnNoteSong(NoteData data, int measureOffsetInGroup)
    {
        GameObject prefab = (data.drumType == DrumType.HiHat || data.drumType == DrumType.Crash)
            ? xPrefab : notePrefab;

        GameObject obj = Instantiate(prefab, noteContainer);

        RectTransform rt = obj.GetComponent<RectTransform>();
        if (data.drumType == DrumType.HiHat || data.drumType == DrumType.Crash)
            rt.sizeDelta = new Vector2(43f, 35f);
        else
            rt.sizeDelta = new Vector2(40f, 110f);

        bool isTopStaff = measureOffsetInGroup < 4;
        int posInRow = measureOffsetInGroup % 4;
        float xPos = staffRendererSong.startX + posInRow * staffRendererSong.measureWidth
                     + (data.beatPosition * (staffRendererSong.measureWidth / 8f)) + 34f;
        float yPos = staffRendererSong.GetLaneY(data.drumType, isTopStaff);

        obj.transform.localPosition = new Vector3(xPos, yPos, 0);
        NoteObject note = obj.GetComponent<NoteObject>();
        note.Init(data.drumType, 0f);
        activeNotes.Add(note);
    }

    // ═══════════════════════════════════════════════════
    // 공통 패턴 재생 (튜토리얼용)
    // ═══════════════════════════════════════════════════
    IEnumerator PlayPattern(List<NoteData> pattern, int repeat)
    {
        for (int r = 0; r < repeat; r++)
        {
            ClearNotes();
            judgeLine.localPosition = new Vector3(judgeLineX, 330f, 0);

            foreach (var note in pattern) { SpawnNote(note); totalNotes++; }

            yield return StartCoroutine(Countdown());
            float duration = secondsPerBeat * 8f + 1f;
            isPlaying = true;
            yield return new WaitForSeconds(duration);
            isPlaying = false;
            yield return new WaitForSeconds(0.5f);
        }
    }

    void SpawnNote(NoteData data)
    {
        GameObject prefab = (data.drumType == DrumType.HiHat || data.drumType == DrumType.Crash)
            ? xPrefab : notePrefab;

        GameObject obj = Instantiate(prefab, noteContainer);
        float xPos = judgeLineX + 100f + (data.beatPosition * pixelsPerBeat);
        float yPos = GetLaneY(data.drumType);

        obj.transform.localPosition = new Vector3(xPos, yPos, 0);
        NoteObject note = obj.GetComponent<NoteObject>();
        note.Init(data.drumType, 0f);
        activeNotes.Add(note);
    }

    float GetLaneY(DrumType type)
    {
        switch (type)
        {
            case DrumType.Kick:  return kickY;
            case DrumType.Snare: return snareY;
            case DrumType.HiHat: return hiHatY;
            case DrumType.Crash: return crashY;
            default: return 0;
        }
    }

    // ═══════════════════════════════════════════════════
    // 판정 시스템
    // ═══════════════════════════════════════════════════
    void Judge(DrumType type)
    {
        PlaySound(type);

        float judgeX = isSongPlayMode ? judgeLineSong.localPosition.x : judgeLine.localPosition.x;
        NoteObject closest = null;
        float minDist = float.MaxValue;

        foreach (var note in activeNotes)
        {
            if (note.isJudged || note.drumType != type) continue;
            float dist = Mathf.Abs(note.transform.localPosition.x - judgeX);
            if (dist < minDist) { minDist = dist; closest = note; }
        }

        float allowance = pixelsPerBeat * 2f;
foreach (var note in activeNotes)
{
    if (!note.isJudged && note.drumType == type)
        Debug.Log($"NoteX: {note.transform.localPosition.x}, type: {note.drumType}");
}

        if (closest != null && minDist < allowance * 0.2f)
        {
            closest.isJudged = true; hitNotes++; combo++;
            closest.GetComponent<Image>().color = Color.green;
            ShowJudgement("PERFECT", Color.cyan);
        }
        else if (closest != null && minDist < allowance * 0.4f)
        {
            closest.isJudged = true; hitNotes++; combo++;
            closest.GetComponent<Image>().color = Color.green;
            ShowJudgement("GREAT", Color.yellow);
        }
        else if (closest != null && minDist < allowance * 0.7f)
        {
            closest.isJudged = true; hitNotes++; combo++;
            closest.GetComponent<Image>().color = Color.yellow;
            ShowJudgement("GOOD", Color.white);
        }
        else if (closest != null && minDist < allowance)
        {
            closest.isJudged = true; combo = 0;
            closest.GetComponent<Image>().color = Color.red;
            ShowJudgement("BAD", Color.red);
        }
        else
        {
            combo = 0;
            ShowJudgement("MISS", Color.red);
        }

        if (combo > maxCombo) maxCombo = combo;
        UpdateCombo();
    }

    void UpdateCombo()
    {
        if (combo == 0) { comboText.text = ""; return; }
        comboText.text = combo + " COMBO!";
        if (combo >= 25) { comboText.color = Color.red; comboText.fontSize = 60; }
        else if (combo >= 10) { comboText.color = Color.yellow; comboText.fontSize = 50; }
        else { comboText.color = Color.white; comboText.fontSize = 40; }
    }

    void ShowJudgement(string text, Color color)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeJudgement(text, color));
    }

    IEnumerator FadeJudgement(string text, Color color)
    {
        judgementText.text = text;
        judgementText.color = color;
        judgementText.fontSize = 50;
        yield return new WaitForSeconds(0.5f);
        judgementText.text = "";
    }

    void PlaySound(DrumType type)
    {
        AudioSource src = null;
        switch (type)
        {
            case DrumType.Kick:  src = kick;  break;
            case DrumType.Snare: src = snare; break;
            case DrumType.HiHat: src = hiHat; break;
            case DrumType.Crash: src = crash; break;
        }
        if (src != null && src.clip != null) src.PlayOneShot(src.clip);
    }

    // ═══════════════════════════════════════════════════
    // 네비게이션 버튼
    // ═══════════════════════════════════════════════════
    public void GoToIndividualPractice()
    {
        StopAllCoroutines();
        ClearNotes();
        isPlaying = false;
        isSongPlayMode = false;
        staffRendererSong.Hide();
        GetComponent<StaffRenderer>().Show();
        judgeLine.gameObject.SetActive(true);
        judgeLineSong.gameObject.SetActive(false);
        currentBpm = drumBpm;
        UpdateBpmSettings();
        StartCoroutine(RunIndividualPractice());
    }

    IEnumerator RunIndividualPractice()
    {
        yield return StartCoroutine(ShowPhase("KICK 연습\n(Space)"));
        yield return StartCoroutine(PlayPattern(MakeSimple(DrumType.Kick, 4), 3));

        yield return StartCoroutine(ShowPhase("HIHAT 연습\n(K키)"));
        yield return StartCoroutine(PlayPattern(MakeSimple(DrumType.HiHat, 8), 3));

        yield return StartCoroutine(ShowPhase("SNARE 연습\n(S키)"));
        yield return StartCoroutine(PlayPattern(MakeSimple(DrumType.Snare, 4), 3));

        yield return StartCoroutine(ShowPhase("CRASH 연습\n(Q키)"));
        yield return StartCoroutine(PlayPattern(MakeCrash(), 3));

        yield return StartCoroutine(ShowPhase("HIHAT + SNARE"));
        yield return StartCoroutine(PlayPattern(MakeHiHatSnare(), 3));

        yield return StartCoroutine(ShowPhase("HIHAT + SNARE + KICK"));
        yield return StartCoroutine(PlayPattern(MakeHiHatSnareKick(), 3));

        ShowResult();
    }

    public void GoToSongTutorial()
    {
        StopAllCoroutines();
        ClearNotes();
        isPlaying = false;
        isSongPlayMode = false;
        staffRendererSong.Hide();
        GetComponent<StaffRenderer>().Show();
        judgeLine.gameObject.SetActive(true);
        judgeLineSong.gameObject.SetActive(false);
        currentBpm = songBpm;
        UpdateBpmSettings();
        StartCoroutine(StartSongTutorial());
    }

    IEnumerator StartSongTutorial()
    {
        yield return StartCoroutine(ShowPhase("🎵 Song Tutorial 시작!\nYellow - Coldplay"));
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(ShowPhase("패턴 1"));
        yield return StartCoroutine(PlaySongPattern(1, 5));

        yield return StartCoroutine(ShowPhase("패턴 2"));
        yield return StartCoroutine(PlaySongPattern(2, 5));

        yield return StartCoroutine(ShowPhase("패턴 3"));
        yield return StartCoroutine(PlaySongPattern(3, 5));

        yield return StartCoroutine(ShowPhase("패턴 1 + 2"));
        yield return StartCoroutine(PlaySongCombined(1, 2, 3));

        yield return StartCoroutine(ShowPhase("패턴 2 + 3"));
        yield return StartCoroutine(PlaySongCombined(2, 3, 3));

        ShowResult();
    }

    public void SkipCurrent()
    {
        StopAllCoroutines();
        ClearNotes();
        isPlaying = false;
        ShowResult();
    }

    // ═══════════════════════════════════════════════════
    // 유틸리티
    // ═══════════════════════════════════════════════════
    IEnumerator ShowPhase(string name)
    {
        phaseText.text = name;
        yield return new WaitForSeconds(2f);
    }

    IEnumerator Countdown()
    {
        for (int i = 3; i >= 1; i--)
        {
            countText.text = i.ToString();
            yield return new WaitForSeconds(0.5f);
        }
        countText.text = "GO!";
        yield return new WaitForSeconds(0.3f);
        countText.text = "";
    }

    void ClearNotes()
    {
        foreach (var note in activeNotes)
            if (note != null) Destroy(note.gameObject);
        activeNotes.Clear();
    }

    void ShowResult()
    {
        isPlaying = false;
        float pct = totalNotes > 0 ? (float)hitNotes / totalNotes * 100f : 0f;
        resultText.text = $"결과: {hitNotes}/{totalNotes} ({pct:F1}%)\n최고 콤보: {maxCombo}";
        phaseText.text = "완료!";
    }
}