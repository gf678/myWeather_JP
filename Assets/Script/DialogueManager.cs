using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI 요소")]
    public TMP_Text textNameSpace;
    public TMP_Text textTalkSpace;
    public Button textTalkButton;
    public Button[] choiceButtons;

    [Header("대화 시퀀스 목록 (Resources 폴더 내 JSON 파일 이름)")]
    public string[] dialogueSequences;

    private int dialogueIndex = 0;
    private bool waitingForChoice = false;
    private bool dialogueActive = false;
    private bool isWeatherDialogueActive = false;
    private DialogueData dialogueData;

    [SerializeField] private Animator characterAnimator;

    // ------------------------------------------------
    // ✅ 내부 클래스 정의
    // ------------------------------------------------
    [System.Serializable]
    public class Choice
    {
        public string text;
        public string next; // 다음으로 이동할 ID
    }

    [System.Serializable]
    public class Dialogue
    {
        public string id;
        public string speaker;
        public string text;
        public float emoX;
        public float emoY;
        public float poseX;
        public float poseY;
        public bool hasChoices;
        public Choice[] choices;
        public bool isEnd; // ✅ 마지막 분기 대사 여부
    }

    [System.Serializable]
    public class DialogueData
    {
        public Dialogue[] dialogues;
    }

    // ------------------------------------------------
    // ✅ 초기화
    // ------------------------------------------------
    void Start()
    {
        HideChoices();
        textTalkButton.onClick.AddListener(OnNextClicked);
        dialogueActive = false;
        textTalkSpace.text = "";
        textNameSpace.text = "";
    }

    // ------------------------------------------------
    // ✅ 타이핑 코루틴
    // ------------------------------------------------
    IEnumerator TypeTextCoroutine(Dialogue current)
    {
        textTalkButton.interactable = false;
        float delay = 0.03f;
        string fullText = current.text;
        textTalkSpace.text = "";

        // 표정/자세 보간 시작
        StartCoroutine(SmoothBlendToTarget(
            current.emoX, current.emoY, current.poseX, current.poseY, 1.0f));

        // 타이핑 효과
        foreach (char c in fullText)
        {
            textTalkSpace.text += c;
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(0.2f);

        // 타이핑이 끝난 뒤 — 선택지가 있으면 표시
        if (current.hasChoices && current.choices != null && current.choices.Length > 0)
        {
            ShowChoices(current.choices);
        }
        else
        {
            // 클릭 대기 상태
            textTalkButton.interactable = true;
            waitingForChoice = false;
        }
    }

    // ------------------------------------------------
    // ✅ 날씨 대사 호출
    // ------------------------------------------------
    public void ShowWeatherDialogue(string weather, float temp, DateTime sunrise, DateTime sunset)
    {
        if (dialogueActive)
        {
            Debug.Log("현재 대화 중이라 날씨 대사 스킵됨");
            return;
        }
        isWeatherDialogueActive = true;

        // 🌅 시간대 계산 (모바일 호환용)
        DateTime now = DateTime.UtcNow.AddHours(9); // UTC+9 = JST (일본 표준시)

        string timeOfDay;
        if (now < sunrise.AddHours(3))
            timeOfDay = "morning";
        else if (now < sunset.AddHours(-1))
            timeOfDay = "day";
        else
            timeOfDay = "night";

        if (weather == "drizzle")
            weather = "Rain";

        string tempState = "";
        if (weather == "Clouds" || weather == "Clear")
            if (timeOfDay == "day")
            {
                if (temp >= 28)
                    tempState = "_hot";
                else if (temp <= 10)
                    tempState = "_cold";
                else
                    tempState = "_normal";
            }

        string jsonName = $"Dialogues/weather_{weather.ToLower()}_{timeOfDay.ToLower()}{tempState}";
        Debug.Log($"📖 불러오는 JSON: {jsonName}");

        StartDialogue(jsonName);
    }

    // ------------------------------------------------
    // ✅ 일상 대화 호출
    // ------------------------------------------------
    public void ShowDailyDialogue()
    {
        if (dialogueActive)
        {
            Debug.Log("대화 중이라 일상 대사 스킵됨");
            return;
        }

        if (dialogueSequences == null || dialogueSequences.Length == 0)
        {
            Debug.LogWarning("등록된 일상 대화 시퀀스가 없습니다!");
            return;
        }

        string randomFile = dialogueSequences[UnityEngine.Random.Range(0, dialogueSequences.Length)];
        Debug.Log($"🌤️ 불러오는 일상 대화 JSON: {randomFile}");
        StartDialogue(randomFile);
    }

    // ------------------------------------------------
    // ✅ JSON 로드 및 시작
    // ------------------------------------------------
    public void StartDialogue(string jsonFileName)
    {
        if (isWeatherDialogueActive)
            ClearWeatherDialogue();

        TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName);
        if (jsonFile == null)
        {
            Debug.LogWarning($"{jsonFileName}.json が見つかりません。スキップします。");
            return;
        }

        dialogueData = JsonUtility.FromJson<DialogueData>(jsonFile.text);
        dialogueIndex = 0;
        dialogueActive = true;
        ShowDialogue();
    }

    // ------------------------------------------------
    // ✅ 대화 표시
    // ------------------------------------------------
    void ShowDialogue()
    {
        HideChoices();

        if (dialogueData == null || dialogueIndex >= dialogueData.dialogues.Length)
        {
            EndDialogue();
            return;
        }

        Dialogue current = dialogueData.dialogues[dialogueIndex];
        textNameSpace.text = current.speaker;
        textTalkSpace.text = "";

        StartCoroutine(TypeTextCoroutine(current));
    }

    // ------------------------------------------------
    // ✅ 선택지 표시
    // ------------------------------------------------
    void ShowChoices(Choice[] choices)
    {
        waitingForChoice = true;
        textTalkButton.interactable = false;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<TMP_Text>().text = choices[i].text;

                string nextId = choices[i].next;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(nextId));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // ------------------------------------------------
    // ✅ 선택지 처리
    // ------------------------------------------------
    void OnChoiceSelected(string nextId)
    {
        waitingForChoice = false;
        textTalkButton.interactable = true;
        HideChoices();

        int nextIndex = FindDialogueIndexById(nextId);
        if (nextIndex != -1)
        {
            dialogueIndex = nextIndex;
            ShowDialogue();
        }
        else
        {
            EndDialogue();
        }
    }

    // ------------------------------------------------
    // ✅ ID 기반 인덱스 찾기
    // ------------------------------------------------
    int FindDialogueIndexById(string id)
    {
        for (int i = 0; i < dialogueData.dialogues.Length; i++)
        {
            if (dialogueData.dialogues[i].id == id)
                return i;
        }
        Debug.LogWarning($"⚠️ ID '{id}' に該当する 대사가 없습니다.");
        return -1;
    }

    void HideChoices()
    {
        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);
    }

    // ------------------------------------------------
    // ✅ 다음 버튼 클릭 시
    // ------------------------------------------------
    void OnNextClicked()
    {
        if (!dialogueActive || waitingForChoice) return;

        Dialogue current = dialogueData.dialogues[dialogueIndex];

        // 🔹 isEnd 대사 클릭 시 → 종료
        if (current.isEnd)
        {
            Debug.Log($"🖱️ {current.id} : 클릭으로 시퀀스 종료됨");
            EndDialogue();
            return;
        }

        // 🔹 일반 대사 클릭 시 → 다음으로
        dialogueIndex++;
        ShowDialogue();
    }

    // ------------------------------------------------
    // ✅ 종료 및 초기화
    // ------------------------------------------------
    void EndDialogue()
    {
        dialogueActive = false;
        textTalkSpace.text = "";
        textNameSpace.text = "";
        HideChoices();

        if (isWeatherDialogueActive)
        {
            isWeatherDialogueActive = false;
            Debug.Log("☀️ 날씨 대화 완전 종료됨");
        }

        StartCoroutine(SmoothBlendToIdle(1.2f));
    }

    void ClearWeatherDialogue()
    {
        if (!isWeatherDialogueActive) return;
        isWeatherDialogueActive = false;
        textTalkSpace.text = "";
        textNameSpace.text = "";
    }

    // ------------------------------------------------
    // ✅ 애니메이션 보간 유지
    // ------------------------------------------------
    IEnumerator SmoothBlendToIdle(float duration)
    {
        float startEmoX = characterAnimator.GetFloat("emoX");
        float startEmoY = characterAnimator.GetFloat("emoY");
        float startPoseX = characterAnimator.GetFloat("poseX");
        float startPoseY = characterAnimator.GetFloat("poseY");

        float targetEmoX = -0.15f;
        float targetEmoY = 1.57f;
        float targetPoseX = 0f;
        float targetPoseY = 0f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);

            // 💫 Idle 복귀용 스윙바이 (살짝만 튕기도록 완화)
            float swing = EaseInOutBackSmooth(normalized * 0.9f);

            float poseX = Mathf.Lerp(startPoseX, targetPoseX, swing);
            float poseY = Mathf.Lerp(startPoseY, targetPoseY, swing);
            float emoX = Mathf.Lerp(startEmoX, targetEmoX, swing);
            float emoY = Mathf.Lerp(startEmoY, targetEmoY, swing);

            characterAnimator.SetFloat("poseX", poseX);
            characterAnimator.SetFloat("poseY", poseY);
            characterAnimator.SetFloat("emoX", emoX);
            characterAnimator.SetFloat("emoY", emoY);

            yield return null;
        }

        // 🔚 마지막 고정
        characterAnimator.SetFloat("poseX", targetPoseX);
        characterAnimator.SetFloat("poseY", targetPoseY);
        characterAnimator.SetFloat("emoX", targetEmoX);
        characterAnimator.SetFloat("emoY", targetEmoY);
    }


    IEnumerator SmoothBlendToTarget(float targetEmoX, float targetEmoY, float targetPoseX, float targetPoseY, float duration)
    {
        float startPoseX = characterAnimator.GetFloat("poseX");
        float startPoseY = characterAnimator.GetFloat("poseY");
        float startEmoX = characterAnimator.GetFloat("emoX");
        float startEmoY = characterAnimator.GetFloat("emoY");

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);

            // ✨ 완화된 EaseInOutBack (감속 범위 확대, 튕김 최소화)
            float swing = EaseInOutBackSmooth(normalized);

            float poseX = Mathf.Lerp(startPoseX, targetPoseX, swing);
            float poseY = Mathf.Lerp(startPoseY, targetPoseY, swing);
            float emoX = Mathf.Lerp(startEmoX, targetEmoX, swing);
            float emoY = Mathf.Lerp(startEmoY, targetEmoY, swing);

            characterAnimator.SetFloat("poseX", poseX);
            characterAnimator.SetFloat("poseY", poseY);
            characterAnimator.SetFloat("emoX", emoX);
            characterAnimator.SetFloat("emoY", emoY);

            yield return null;
        }

        // 🔚 끊김 방지용 마지막 고정
        characterAnimator.SetFloat("poseX", targetPoseX);
        characterAnimator.SetFloat("poseY", targetPoseY);
        characterAnimator.SetFloat("emoX", targetEmoX);
        characterAnimator.SetFloat("emoY", targetEmoY);
    }

    // --------------------------------------------------
    // 💫 부드러운 EaseInOutBack (감속 길고 튕김 완화)
    // --------------------------------------------------
    float EaseInOutBackSmooth(float x)
    {
        const float c1 = 0.40158f; // ← 기존보다 작게 (감속 완화)
        const float c2 = c1 * 1.525f;

        return x < 0.5
            ? (Mathf.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
            : (Mathf.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
    }

    // ------------------------------------------------
    // ✅ 캐릭터 클릭 시
    // ------------------------------------------------
    public void OnCharacterClicked()
    {
        Debug.Log($"👆 캐릭터 클릭 감지됨 / dialogueActive={dialogueActive}, isWeatherDialogueActive={isWeatherDialogueActive}");

        if (isWeatherDialogueActive)
        {
            Debug.Log("☀️ 날씨 대화 종료 후 일상 대화로 전환");
            isWeatherDialogueActive = false;
            EndDialogue();
            ShowDailyDialogue();
            return;
        }

        if (!dialogueActive)
        {
            ShowDailyDialogue();
            return;
        }

        Debug.Log("대화 중이라 새 대화 시작 안함");
    }
}
