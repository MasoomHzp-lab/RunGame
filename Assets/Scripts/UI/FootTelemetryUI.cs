using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FootTelemetryUI : MonoBehaviour
{
    [Header("Fill Images")]
    [SerializeField] private Image leftFill;
    [SerializeField] private Image rightFill;

    [Header("Glow Images")]
    [SerializeField] private Image leftGlow;
    [SerializeField] private Image rightGlow;

    [Header("Texts")]
    [SerializeField] private TMP_Text leftPercentText;
    [SerializeField] private TMP_Text rightPercentText;

    [Header("Containers")]
    [SerializeField] private RectTransform leftContainer;
    [SerializeField] private RectTransform rightContainer;

    [Header("Fill Settings")]
    [SerializeField] private float fillSpeed = 2.5f;
    [SerializeField] private float returnToZeroSpeed = 1.2f;
    [SerializeField] private float reachThreshold = 0.002f;

    [Header("Glow Settings")]
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float glowMinAlpha = 0.08f;
    [SerializeField] private float glowMaxAlpha = 0.45f;

    [Header("Scale Settings")]
    [SerializeField] private float activeScale = 1.06f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float scaleLerpSpeed = 10f;

    [Header("Step Motion")]
    [SerializeField] private float stepPopDistance = 8f;
    [SerializeField] private float returnSpeed = 14f;

    [Header("Hold At Final Value")]
    [SerializeField] private float holdDuration = 3f;

    private float targetLeftFill;
    private float targetRightFill;
    private float currentLeftFill;
    private float currentRightFill;

    private float leftHoldTimer;
    private float rightHoldTimer;

    private bool leftWaitingToStartHold;
    private bool rightWaitingToStartHold;

    private FootSide? activeFoot;

    private Vector2 leftDefaultPos;
    private Vector2 rightDefaultPos;

    private bool initialized;

    private void Awake()
    {
        Init();
        SetupFillImage(leftFill, true);
        SetupFillImage(rightFill, false);

        SetGlowAlpha(leftGlow, 0f);
        SetGlowAlpha(rightGlow, 0f);

        currentLeftFill = 0f;
        currentRightFill = 0f;
        targetLeftFill = 0f;
        targetRightFill = 0f;

        ApplyFillVisuals();
        RefreshTexts();
    }

    private void Init()
    {
        if (initialized) return;

        if (leftContainer != null)
            leftDefaultPos = leftContainer.anchoredPosition;

        if (rightContainer != null)
            rightDefaultPos = rightContainer.anchoredPosition;

        initialized = true;
    }

    private void SetupFillImage(Image image, bool leftToRight)
    {
        if (image == null) return;

        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = leftToRight
            ? (int)Image.OriginHorizontal.Left
            : (int)Image.OriginHorizontal.Right;
        image.fillAmount = 0f;
    }

    private void Update()
    {
        UpdateFillBehaviour();
        UpdateGlowPulse();
        UpdateContainerScale();
        ReturnContainers();
        RefreshTexts();
    }

    private void UpdateFillBehaviour()
    {
        UpdateSingleFoot(
            ref currentLeftFill,
            ref targetLeftFill,
            ref leftHoldTimer,
            ref leftWaitingToStartHold,
            leftFill);

        UpdateSingleFoot(
            ref currentRightFill,
            ref targetRightFill,
            ref rightHoldTimer,
            ref rightWaitingToStartHold,
            rightFill);

        if (leftHoldTimer <= 0f &&
            rightHoldTimer <= 0f &&
            currentLeftFill <= reachThreshold &&
            currentRightFill <= reachThreshold &&
            targetLeftFill <= reachThreshold &&
            targetRightFill <= reachThreshold)
        {
            activeFoot = null;
        }
    }

    private void UpdateSingleFoot(
        ref float current,
        ref float target,
        ref float holdTimer,
        ref bool waitingToStartHold,
        Image fillImage)
    {
        bool returningToZero = target <= reachThreshold;
        float speed = returningToZero ? returnToZeroSpeed : fillSpeed;

        current = Mathf.MoveTowards(current, target, speed * Time.deltaTime);

        if (!returningToZero && waitingToStartHold && Mathf.Abs(current - target) <= reachThreshold)
        {
            current = target;
            holdTimer = holdDuration;
            waitingToStartHold = false;
        }

        if (holdTimer > 0f)
        {
            holdTimer -= Time.deltaTime;
            if (holdTimer <= 0f)
            {
                holdTimer = 0f;
                target = 0f;
            }
        }

        if (returningToZero && current <= reachThreshold)
        {
            current = 0f;
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = current;
        }
    }

    private void ApplyFillVisuals()
    {
        if (leftFill != null)
            leftFill.fillAmount = currentLeftFill;

        if (rightFill != null)
            rightFill.fillAmount = currentRightFill;
    }

    private void UpdateGlowPulse()
    {
        float pulse = Mathf.Lerp(
            glowMinAlpha,
            glowMaxAlpha,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);

        if (activeFoot == FootSide.Left)
        {
            SetGlowAlpha(leftGlow, pulse);
            SetGlowAlpha(rightGlow, 0f);
        }
        else if (activeFoot == FootSide.Right)
        {
            SetGlowAlpha(leftGlow, 0f);
            SetGlowAlpha(rightGlow, pulse);
        }
        else
        {
            SetGlowAlpha(leftGlow, 0f);
            SetGlowAlpha(rightGlow, 0f);
        }
    }

    private void UpdateContainerScale()
    {
        if (leftContainer != null)
        {
            float targetScale = activeFoot == FootSide.Left ? activeScale : normalScale;
            leftContainer.localScale = Vector3.Lerp(
                leftContainer.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * scaleLerpSpeed);
        }

        if (rightContainer != null)
        {
            float targetScale = activeFoot == FootSide.Right ? activeScale : normalScale;
            rightContainer.localScale = Vector3.Lerp(
                rightContainer.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * scaleLerpSpeed);
        }
    }

    private void ReturnContainers()
    {
        if (leftContainer != null)
        {
            leftContainer.anchoredPosition = Vector2.Lerp(
                leftContainer.anchoredPosition,
                leftDefaultPos,
                Time.deltaTime * returnSpeed);
        }

        if (rightContainer != null)
        {
            rightContainer.anchoredPosition = Vector2.Lerp(
                rightContainer.anchoredPosition,
                rightDefaultPos,
                Time.deltaTime * returnSpeed);
        }
    }

    private void RefreshTexts()
    {
        if (leftPercentText != null)
            leftPercentText.text = Mathf.RoundToInt(currentLeftFill * 100f) + "%";

        if (rightPercentText != null)
            rightPercentText.text = Mathf.RoundToInt(currentRightFill * 100f) + "%";
    }

    private void SetGlowAlpha(Image img, float alpha)
    {
        if (img == null) return;

        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    public void SetLeftPercent(float value)
    {
        targetLeftFill = Mathf.Clamp01(value);
        leftWaitingToStartHold = targetLeftFill > reachThreshold;
        leftHoldTimer = 0f;

        if (targetLeftFill <= reachThreshold)
        {
            targetLeftFill = 0f;
            leftWaitingToStartHold = false;
        }
    }

    public void SetRightPercent(float value)
    {
        targetRightFill = Mathf.Clamp01(value);
        rightWaitingToStartHold = targetRightFill > reachThreshold;
        rightHoldTimer = 0f;

        if (targetRightFill <= reachThreshold)
        {
            targetRightFill = 0f;
            rightWaitingToStartHold = false;
        }
    }

    public void SetActiveFoot(FootSide side)
    {
        activeFoot = side;
    }

    public void AnimateFootStep(FootSide side, float normalizedPower)
    {
        Init();

        float pop = stepPopDistance * Mathf.Clamp01(normalizedPower);

        if (side == FootSide.Left && leftContainer != null)
        {
            leftContainer.anchoredPosition = leftDefaultPos + new Vector2(0f, pop);
        }
        else if (side == FootSide.Right && rightContainer != null)
        {
            rightContainer.anchoredPosition = rightDefaultPos + new Vector2(0f, pop);
        }

        activeFoot = side;
    }

    public void ResetHighlights()
    {
        activeFoot = null;

        leftHoldTimer = 0f;
        rightHoldTimer = 0f;

        leftWaitingToStartHold = false;
        rightWaitingToStartHold = false;

        targetLeftFill = 0f;
        targetRightFill = 0f;
        currentLeftFill = 0f;
        currentRightFill = 0f;

        ApplyFillVisuals();

        SetGlowAlpha(leftGlow, 0f);
        SetGlowAlpha(rightGlow, 0f);

        if (leftContainer != null)
        {
            leftContainer.localScale = Vector3.one * normalScale;
            leftContainer.anchoredPosition = leftDefaultPos;
        }

        if (rightContainer != null)
        {
            rightContainer.localScale = Vector3.one * normalScale;
            rightContainer.anchoredPosition = rightDefaultPos;
        }

        RefreshTexts();
    }
}