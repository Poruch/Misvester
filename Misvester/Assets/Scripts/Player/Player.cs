using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Accessory.structures;
using Assets.Scripts.Accessory;
using Assets.Scripts.GeneralGame.Entities.Player;

public class Player : MonoBehaviour
{
    [SerializeField] int speed = 5;
    PlayerAnimationController animController;
    [SerializeField] RuntimeAnimatorController runtimeAnimatorController;

    private Vector2 lastDirection = Vector2.down;
    private Vector2 firstPressedDirection = Vector2.down; 
    private bool isFirstPress = true; 
    PlayerControls playerControls;
    AccessoryTypes.Orientation orientation;
    [SerializeField]MovementController movementController;

    [SerializeField]Timer timerAttack = TimeManager.Instance.CreateTimer(1);

    [SerializeField]DetectionArea interactArea;
    void Start()
    {
        playerControls = new PlayerControls();
        playerControls.Enable();
        animController = gameObject.AddComponent<PlayerAnimationController>();
        animController.SetConfig(runtimeAnimatorController);
        interactArea.OnColliderEnter.AddListener(OnInteract);
    }


    void OnInteract(GameObject gameObject)
    {
        Debug.Log("Something detected");
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

            if(firstPressedDirection.y != discreteDirection.y || firstPressedDirection.x != discreteDirection.x)
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
            movementController.Move(new Vector3(
                speed * discreteDirection.x * Time.deltaTime,
                speed * discreteDirection.y * Time.deltaTime));
            interactArea.SetPosition(discreteDirection);
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
            // При остановке используем последнее сохраненное направление
            animController.Animator.SetFloat("Horizontal", lastDirection.x);
            animController.Animator.SetFloat("Vertical", lastDirection.y);
            animController.Animator.SetBool("IsMoving", false);
        }

        if (playerControls.Iteract.Attack.IsPressed())
        {
            if(timerAttack.IsTime)
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