using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimationController : MonoBehaviour
{
    private Animator _animator;
    private int _isRunningHash;
    private int _shootHash;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _isRunningHash = Animator.StringToHash("IsRunning");
        _shootHash = Animator.StringToHash("Shoot");
    }

    private void Update()
    {
        HandleMovementInput();
        HandleActionInput();
    }

    private void HandleMovementInput()
    {
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        _animator.SetBool(_isRunningHash, isMoving);
    }

    private void HandleActionInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            _animator.SetTrigger(_shootHash);
        }
    }
}