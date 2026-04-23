using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimationController : MonoBehaviour
{
    [Tooltip("开启后由本脚本直接读取玩家输入；关闭后可由AI/外部脚本驱动动画。")]
    [SerializeField] private bool usePlayerInput = true;
    [SerializeField] private string runBoolName = "IsRunning";
    [SerializeField] private string shootTriggerName = "shoot";

    private Animator _animator;
    private int _isRunningHash;
    private int _shootHash;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _isRunningHash = Animator.StringToHash(runBoolName);
        _shootHash = Animator.StringToHash(shootTriggerName);
    }

    private void Update()
    {
        if (!usePlayerInput)
        {
            return;
        }

        HandleMovementInput();
        HandleActionInput();
    }

    private void HandleMovementInput()
    {
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        SetRunning(isMoving);
    }

    private void HandleActionInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            TriggerShoot();
        }
    }

    public void SetUsePlayerInput(bool enabled)
    {
        usePlayerInput = enabled;
    }

    public void SetRunning(bool isRunning)
    {
        _animator.SetBool(_isRunningHash, isRunning);
    }

    public void TriggerShoot()
    {
        _animator.SetTrigger(_shootHash);
    }
}
