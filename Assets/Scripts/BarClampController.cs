using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BarClampAnimation : MonoBehaviour
{
    public enum BarClampStatus
    {
        rotating,
        extending
    }
    public CharacterController characterController;
    private PlayerController playerController = null;
    public BarClampStatus barClampStatus = BarClampStatus.rotating;
    public Image bar;       
    public float clampOffset = -1500f;
    public Clamp clamp;
    public float fillSpeed = 0.5f;
    public float startingFill = 0.03f;
    private float targetFill = 0f;
    private bool isFilling = true;
    public float rotationSpeed = 50f;
    public float maxRotation = 50f;
    public float minRotation = -40f;
    private float currentRotation = 0f;
    private bool rotatingToMax = true;

    void Start()
    {
        this.resetBarClamp();
        this.StartRotation();
        if (this.characterController != null)
        {
            this.characterController.OnPointerClickEvent += this.StartClamp;
        }
    }

    private void resetBarClamp()
    {
        this.barClampStatus = BarClampStatus.rotating;
        this.targetFill = this.startingFill;
    }

    void FixedUpdate()
    {
        if(this.bar == null || this.clamp == null)
            return;

        switch (this.barClampStatus)
        {
            case BarClampStatus.rotating:
                this.HandleRotatingState();
                break;
            case BarClampStatus.extending:
                this.HandleExtendingState();
                break;
        }
    }

    private void HandleRotatingState()
    {
        this.StartRotation();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.barClampStatus = BarClampStatus.extending;
        }
    }

    private void HandleExtendingState()
    {
        if (this.isFilling)
        {
            this.UpdateFillAmount();
        }
        else
        {
            this.DecreaseFillAmount();
        }

        this.bar.fillAmount = targetFill;
        this.UpdateClampPosition();
    }

    private void UpdateFillAmount()
    {
        this.targetFill += this.fillSpeed * Time.deltaTime;
        if (this.targetFill >= 1f)
        {
            this.clamp.clampStatus = Clamp.ClampStatus.clamped;
            this.isFilling = false;
        }
        else if (this.clamp.clampStatus == Clamp.ClampStatus.outScreen ||
                 this.clamp.clampStatus == Clamp.ClampStatus.getWord)
        {
            this.isFilling = false;
        }
    }

    private void DecreaseFillAmount()
    {
        this.targetFill -= this.fillSpeed * Time.deltaTime;
        if (this.targetFill <= this.startingFill)
        {
            ProcessClampReset();
        }
    }

    private void ProcessClampReset()
    {
        if (this.characterController == null)
            return;
        this.playerController = this.characterController.playerController;
        if (this.clamp.clampStatus == Clamp.ClampStatus.getWord)
        {
            GameController.Instance.checkAnswerResult(this.playerController.UserId, this.clamp);
        }
        this.targetFill = this.startingFill;
        this.isFilling = true;
        this.clamp.resetClamp();
        this.barClampStatus = BarClampStatus.rotating;
        if(!this.playerController.IsTriggerToNextQuestion)
            this.characterController.TriggerActive(true);
    }

    public void StartClamp(BaseEventData data)
    {
        this.barClampStatus = BarClampStatus.extending;
        this.characterController.TriggerActive(false);
    }

    private void UpdateClampPosition()
    {
        RectTransform clampRect = this.clamp.GetComponent<RectTransform>();
        float barHeight = bar.rectTransform.rect.height;
        float clampHeight = clampRect.rect.height;
        float newY = this.bar.rectTransform.anchoredPosition.y + (barHeight * targetFill) - (clampHeight / 2) + this.clampOffset;
        clampRect.anchoredPosition = new Vector2(clampRect.anchoredPosition.x, newY);
    }

    private void StartRotation()
    {
        // Determine the target rotation based on current direction
        float targetRotation = rotatingToMax ? maxRotation : minRotation;

        // Rotate toward the target rotation
        currentRotation = Mathf.MoveTowards(currentRotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        // Check if we reached the target rotation to switch direction
        if (Mathf.Abs(currentRotation - targetRotation) < 0.01f)
        {
            rotatingToMax = !rotatingToMax; // Switch direction
        }
    }


}
