using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeStopController : MonoBehaviour
{
    [SerializeField, Tooltip("시간 정지 지속 시간(초)")]
    private float duration = 4f;

    [SerializeField, Tooltip("시간 정지 중 화면 색감 오버레이 패널(Image)")]
    private Image screenTintPanel;

    [SerializeField, Tooltip("시간 정지 중 적용할 반투명 색상")]
    private Color screenTintColor = new Color(0.45f, 0.55f, 0.8f, 0.35f);

    [SerializeField, Tooltip("시간 정지 대상 레이어 이름")]
    private string[] affectedLayerNames = { "monster", "enemy", "enemyprojectile", "monsterprojectile" };

    [SerializeField, Tooltip("시간 정지 대상 태그")]
    private string[] affectedTags = { "Enemy", "Monster" };

    [SerializeField, Tooltip("활성 중 새 적 탐색 주기(초)")]
    private float rescanInterval = 0.2f;

    public static bool IsTimeStopped { get; private set; }

    private static TimeStopController activeController;

    private readonly Dictionary<int, FrozenAnimatorState> frozenAnimators = new Dictionary<int, FrozenAnimatorState>();
    private readonly Dictionary<int, FrozenRigidbody2DState> frozenRigidbodies2D = new Dictionary<int, FrozenRigidbody2DState>();

    private float stopEndTime = -1f;
    private float nextRescanTime;

    private struct FrozenAnimatorState
    {
        public Animator animator;
        public float speed;
    }

    private struct FrozenRigidbody2DState
    {
        public Rigidbody2D rigidbody;
        public bool simulated;
        public Vector2 velocity;
        public float angularVelocity;
    }

    private void Awake()
    {
        if (activeController != null && activeController != this)
        {
            Debug.LogWarning("Multiple TimeStopController instances found. Using latest enabled instance.");
        }

        activeController = this;
        duration = Mathf.Max(0.05f, duration);
        rescanInterval = Mathf.Max(0.05f, rescanInterval);
        SetTintActive(false);
    }

    private void OnDisable()
    {
        if (activeController == this)
        {
            activeController = null;
        }

        if (IsTimeStopped)
        {
            ReleaseAllFrozenTargets();
            IsTimeStopped = false;
        }

        SetTintActive(false);
    }

    private void Update()
    {
        if (!IsTimeStopped)
        {
            return;
        }

        float now = Time.unscaledTime;
        if (now >= nextRescanTime)
        {
            FreezeCurrentTargets();
            nextRescanTime = now + rescanInterval;
        }

        if (now >= stopEndTime)
        {
            EndTimeStop();
        }
    }

    public static void TriggerFromPickup()
    {
        if (activeController != null)
        {
            activeController.ActivateTimeStop();
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("TimeStop pickup collected but Player not found.");
            return;
        }

        TimeStopController controller = player.GetComponent<TimeStopController>();
        if (controller == null)
        {
            controller = player.AddComponent<TimeStopController>();
        }

        controller.ActivateTimeStop();
    }

    public void ActivateTimeStop()
    {
        float now = Time.unscaledTime;
        stopEndTime = now + Mathf.Max(0.05f, duration);
        nextRescanTime = now;

        if (!IsTimeStopped)
        {
            IsTimeStopped = true;
            FreezeCurrentTargets();
        }

        ApplyTintVisual();
    }

    private void EndTimeStop()
    {
        ReleaseAllFrozenTargets();
        SetTintActive(false);
        IsTimeStopped = false;
        stopEndTime = -1f;
    }

    private void FreezeCurrentTargets()
    {
        MonsterController[] monsters = FindObjectsByType<MonsterController>(FindObjectsSortMode.None);
        for (int i = 0; i < monsters.Length; i++)
        {
            if (monsters[i] != null)
            {
                FreezeGameObject(monsters[i].gameObject);
            }
        }

        Rigidbody2D[] rigidbodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Rigidbody2D rb = rigidbodies[i];
            if (rb == null)
            {
                continue;
            }

            if (!IsAffectedObject(rb.gameObject))
            {
                continue;
            }

            FreezeRigidbody2D(rb);
            FreezeAnimatorsInHierarchy(rb.gameObject);
        }
    }

    private void FreezeGameObject(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        FreezeAnimatorsInHierarchy(root);

        Rigidbody2D[] rigidbodies = root.GetComponentsInChildren<Rigidbody2D>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            FreezeRigidbody2D(rigidbodies[i]);
        }
    }

    private void FreezeAnimatorsInHierarchy(GameObject root)
    {
        Animator[] animators = root.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (animator == null)
            {
                continue;
            }

            int id = animator.GetInstanceID();
            if (frozenAnimators.ContainsKey(id))
            {
                continue;
            }

            FrozenAnimatorState state = new FrozenAnimatorState
            {
                animator = animator,
                speed = animator.speed
            };
            frozenAnimators.Add(id, state);
            animator.speed = 0f;
        }
    }

    private void FreezeRigidbody2D(Rigidbody2D rb)
    {
        if (rb == null)
        {
            return;
        }

        int id = rb.GetInstanceID();
        if (frozenRigidbodies2D.ContainsKey(id))
        {
            return;
        }

        FrozenRigidbody2DState state = new FrozenRigidbody2DState
        {
            rigidbody = rb,
            simulated = rb.simulated,
            velocity = rb.linearVelocity,
            angularVelocity = rb.angularVelocity
        };

        frozenRigidbodies2D.Add(id, state);
        rb.simulated = false;
    }

    private void ReleaseAllFrozenTargets()
    {
        foreach (KeyValuePair<int, FrozenAnimatorState> pair in frozenAnimators)
        {
            Animator animator = pair.Value.animator;
            if (animator != null)
            {
                animator.speed = pair.Value.speed;
            }
        }

        frozenAnimators.Clear();

        foreach (KeyValuePair<int, FrozenRigidbody2DState> pair in frozenRigidbodies2D)
        {
            Rigidbody2D rb = pair.Value.rigidbody;
            if (rb == null)
            {
                continue;
            }

            rb.simulated = pair.Value.simulated;
            rb.linearVelocity = pair.Value.velocity;
            rb.angularVelocity = pair.Value.angularVelocity;
        }

        frozenRigidbodies2D.Clear();
    }

    private bool IsAffectedObject(GameObject obj)
    {
        if (obj == null || obj.CompareTag("Player"))
        {
            return false;
        }

        string objectLayerName = LayerMask.LayerToName(obj.layer);
        for (int i = 0; i < affectedLayerNames.Length; i++)
        {
            if (string.Equals(objectLayerName, affectedLayerNames[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        for (int i = 0; i < affectedTags.Length; i++)
        {
            if (obj.CompareTag(affectedTags[i]))
            {
                return true;
            }
        }

        return obj.GetComponentInParent<MonsterController>() != null;
    }

    private void ApplyTintVisual()
    {
        if (screenTintPanel == null)
        {
            return;
        }

        screenTintPanel.color = screenTintColor;
        SetTintActive(true);
    }

    private void SetTintActive(bool isActive)
    {
        if (screenTintPanel == null)
        {
            return;
        }

        if (screenTintPanel.color != screenTintColor)
        {
            screenTintPanel.color = screenTintColor;
        }

        screenTintPanel.gameObject.SetActive(isActive);
    }

    private void OnValidate()
    {
        duration = Mathf.Max(0.05f, duration);
        rescanInterval = Mathf.Max(0.05f, rescanInterval);
    }
}
