using UnityEngine;

public class BeeDance : MonoBehaviour
{
    [Header("Настройки общения")]
    [SerializeField] private float interactionRadius = 4f;
    [SerializeField] private float communicationDistance = 0.6f;

    [Header("Настройки танца")]
    [SerializeField] private float danceDuration = 2f;
    [SerializeField] private float danceSpeed = 8f;
    [SerializeField] private float danceAngle = 15f;
    [SerializeField] private float danceCooldown = 4f;

    [Header("Подлёт к пчеле")]
    [SerializeField] private float approachSpeed = 2f;

    private Bee bee;
    private Bee targetBee;
    private BeeDance targetDance;

    private bool isApproaching;
    private bool isDancing;
    private bool isReceivingDance;

    private float danceTimer;
    private float cooldownTimer;

    private Quaternion originalRotation;

    private void Start()
    {
        bee = GetComponent<Bee>();
        originalRotation = transform.localRotation;
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (isDancing)
        {
            PerformDance();
            return;
        }

        if (isReceivingDance)
        {
            PerformReceivingDance();
            return;
        }

        if (isApproaching)
        {
            ApproachBee();
            return;
        }

        if (bee == null)
            return;

        if (!bee.HasLeftHive())
            return;

        if (cooldownTimer > 0f)
            return;

        FindNearbyBee();
    }

    private void FindNearbyBee()
    {
        Bee[] bees = FindObjectsByType<Bee>(FindObjectsSortMode.None);

        foreach (Bee otherBee in bees)
        {
            if (otherBee == bee)
                continue;

            if (!otherBee.HasLeftHive())
                continue;

            BeeDance otherDance = otherBee.GetComponent<BeeDance>();

            if (otherDance == null)
                continue;

            if (otherDance.IsBusy())
                continue;

            float distance = Vector3.Distance(
                transform.position,
                otherBee.transform.position
            );

            if (distance <= interactionRadius)
            {
                targetBee = otherBee;
                targetDance = otherDance;
                isApproaching = true;
                return;
            }
        }
    }

    private void ApproachBee()
    {
        if (targetBee == null || targetDance == null)
        {
            StopApproaching();
            return;
        }

        if (!targetBee.HasLeftHive() || targetDance.IsBusy())
        {
            StopApproaching();
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            targetBee.transform.position
        );

        if (distance <= communicationDistance)
        {
            isApproaching = false;
            StartDanceWithBee();
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetBee.transform.position,
            approachSpeed * Time.deltaTime
        );
    }

    private void StartDanceWithBee()
    {
        if (targetBee == null || targetDance == null)
            return;

        isDancing = true;
        danceTimer = danceDuration;

        originalRotation = transform.localRotation;

        bee.SetDancePaused(true);

        targetDance.StartReceivingDance(this);
    }

    public void StartReceivingDance(BeeDance sender)
    {
        if (sender == null)
            return;

        if (isDancing || isReceivingDance || isApproaching)
            return;

        targetDance = sender;

        isReceivingDance = true;
        danceTimer = danceDuration;

        originalRotation = transform.localRotation;

        bee.SetDancePaused(true);
    }

    private void PerformDance()
    {
        danceTimer -= Time.deltaTime;

        float angle = Mathf.Sin(
            Time.time * danceSpeed
        ) * danceAngle;

        transform.localRotation =
            originalRotation * Quaternion.Euler(0f, 0f, angle);

        if (danceTimer <= 0f)
        {
            FinishDance();
        }
    }

    private void PerformReceivingDance()
    {
        danceTimer -= Time.deltaTime;

        float angle = Mathf.Sin(
            Time.time * danceSpeed
        ) * danceAngle;

        transform.localRotation =
            originalRotation * Quaternion.Euler(0f, 0f, -angle);

        if (danceTimer <= 0f)
        {
            FinishReceivingDance();
        }
    }

    private void FinishDance()
    {
        transform.localRotation = originalRotation;

        isDancing = false;
        cooldownTimer = danceCooldown;

        bee.SetDancePaused(false);

        if (targetDance != null)
        {
            targetDance.FinishReceivingDance();
        }

        targetBee = null;
        targetDance = null;
    }

    public void FinishReceivingDance()
    {
        transform.localRotation = originalRotation;

        isReceivingDance = false;
        cooldownTimer = danceCooldown;

        bee.SetDancePaused(false);

        targetDance = null;
    }

    private void StopApproaching()
    {
        isApproaching = false;
        targetBee = null;
        targetDance = null;
    }

    public bool IsBusy()
    {
        return isApproaching || isDancing || isReceivingDance;
    }
}