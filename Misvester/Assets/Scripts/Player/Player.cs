using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Accessory.structures;
using Assets.Scripts.Accessory;
using Assets.Scripts.GeneralGame.Entities.Player;
using Assets.Scripts.NPC;

public class Player : MonoBehaviour
{
    [SerializeField] int speed = 5;
    [SerializeField] PlayerAnimationController animController;
    [SerializeField] RuntimeAnimatorController runtimeAnimatorController;

    private Vector2 lastDirection = Vector2.down;
    private Vector2 firstPressedDirection = Vector2.down;
    private bool isFirstPress = true;
    PlayerControls playerControls;
    AccessoryTypes.Orientation orientation;
    [SerializeField] MovementController movementController;

    [SerializeField] Timer timerAttack = TimeManager.Instance.CreateTimer(1);

    [SerializeField] DetectionArea interactArea;
    void Start()
    {
        playerControls = new PlayerControls();
        playerControls.Enable();
        playerControls.Interact.Interact.performed += InteractPerformed;
        if (!animController)
            animController = gameObject.AddComponent<PlayerAnimationController>();
        animController.SetConfig(runtimeAnimatorController);
        interactArea.OnColliderEnter.AddListener(OnInteract);
        interactArea.OnColliderExit.AddListener(OnExitInteract);
    }

    private void InteractPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Interactive interactive = interactArea.GetFirstByType<Interactive>();
        if (!interactive) return;
        interactive.Interact(new InteractArgs());
    }

    void OnInteract(GameObject gameObject)
    {
        Debug.Log("Something detected");
        Interactive interactive = gameObject.GetComponent<Interactive>();
        if (!interactive) return;

        interactive.CanInteracted = true;
    }
    void OnExitInteract(GameObject gameObject)
    {
        Debug.Log("Something detected");
        Interactive interactive = gameObject.GetComponent<Interactive>();
        if (!interactive) return;

        interactive.CanInteracted = false;
    }

    Vector2 completeDirection = Vector2.zero;
    private void FixedUpdate()
    {

    }

    void Update()
    {
        Vector2 rawDirection = playerControls.Movement.Move.ReadValue<Vector2>();
        bool hasInput = rawDirection.magnitude > 0.1f;

        if (hasInput)
        {
            // Определяем дискретное направление (только по осям, без диагоналей)
            Vector2 discreteDirection = GetDiscreteDirection(rawDirection);

            // Если это первое нажатие после остановки - сохраняем направление
            if (isFirstPress)
            {
                firstPressedDirection = discreteDirection;
                isFirstPress = false;
            }

            if (firstPressedDirection.y != discreteDirection.y || firstPressedDirection.x != discreteDirection.x)
            {
                firstPressedDirection = discreteDirection;
                isFirstPress = false;
            }

            // Для движения используем rawDirection (с диагоналями)
            //transform.position += new Vector3(
            //    speed * discreteDirection.x * Time.deltaTime,
            //    speed * discreteDirection.y * Time.deltaTime,
            //    0
            //);
            completeDirection = new Vector2(
                speed * discreteDirection.x,
                speed * discreteDirection.y);
            interactArea.SetPosition(discreteDirection);

            movementController.SetDirection(completeDirection);
            // Для анимации используем ПЕРВОЕ нажатое направление
            animController.Animator.SetFloat("Horizontal", firstPressedDirection.x);
            animController.Animator.SetFloat("Vertical", firstPressedDirection.y);
            animController.Animator.SetBool("IsMoving", true);

            // Сохраняем последнее направление для idle
            lastDirection = firstPressedDirection;
        }
        else
        {
            // Сброс флага первого нажатия при остановке
            isFirstPress = true;
            interactArea.SetPosition(lastDirection);
            completeDirection = Vector2.zero;

            movementController.SetDirection(completeDirection);
            // При остановке используем последнее сохраненное направление
            animController.Animator.SetFloat("Horizontal", lastDirection.x);
            animController.Animator.SetFloat("Vertical", lastDirection.y);
            animController.Animator.SetBool("IsMoving", false);
        }

        if (playerControls.Interact.Attack.IsPressed())
        {
            if (timerAttack.IsTime)
                animController.Animator.SetTrigger("IsAttack");
            // Здесь нужно будет сбросить IsAttack после анимации
        }

    }

    // Метод для получения дискретного направления (только по осям)
    private Vector2 GetDiscreteDirection(Vector2 rawInput)
    {
        // Если нажаты обе оси - используем ту, у которой больше значение
        if (Mathf.Abs(rawInput.x) > 0 && Mathf.Abs(rawInput.y) > 0)
        {
            // Можно изменить приоритет (горизонталь или вертикаль)
            if (Mathf.Abs(rawInput.x) <= Mathf.Abs(rawInput.y))
                return new Vector2(Mathf.Sign(rawInput.x), 0);
            else
                return new Vector2(0, Mathf.Sign(rawInput.y));
        }

        // Если нажата только одна ось
        if (Mathf.Abs(rawInput.x) > 0)
            return new Vector2(Mathf.Sign(rawInput.x), 0);
        if (Mathf.Abs(rawInput.y) > 0)
            return new Vector2(0, Mathf.Sign(rawInput.y));

        return Vector2.zero;
    }

    void OnDestroy()
    {
        if (playerControls != null)
        {
            playerControls.Disable();
        }
    }




}