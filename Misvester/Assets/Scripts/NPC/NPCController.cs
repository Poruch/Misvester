using Assets.Scripts.Accessory.structures;
using Assets.Scripts.Accessory;
using Assets.Scripts.GeneralGame.Entities.Player;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.NPC
{
    internal class NPCController : MonoBehaviour
    {
        [SerializeField] int speed = 5;
        [SerializeField] Animator animator;

        private Vector2 lastDirection = Vector2.down;
        private Vector2 firstPressedDirection = Vector2.down;
        private bool isFirstPress = true;
        AccessoryTypes.Orientation orientation;
        [SerializeField]MovementController movementController;

        [SerializeField]Timer IDLETimer = TimeManager.Instance.CreateTimer(1);

        void Start()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        bool isIDLE = true;
        Vector2 rawDirection = Vector2.zero;
        void Update()
        {
            if (isIDLE && IDLETimer.IsTime)
            {
                int direction = Random.Range(0, 4);
                switch (direction)
                {
                    case 0: rawDirection = Vector2.up; break;      // Вверх
                    case 1: rawDirection = Vector2.down; break;    // Вниз
                    case 2: rawDirection = Vector2.right; break;   // Вправо
                    case 3: rawDirection = Vector2.left; break;    // Влево
                }
                IDLETimer = TimeManager.Instance.CreateTimer(Random.Range(0, 3));
                isIDLE = false;
            }
            else if (!isIDLE)
            {
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
                    movementController.Move(new Vector3(
                        speed * discreteDirection.x * Time.deltaTime,
                        speed * discreteDirection.y * Time.deltaTime,
                        0
                    ));

                    // Для анимации используем ПЕРВОЕ нажатое направление
                    animator.SetFloat("Horizontal", firstPressedDirection.x);
                    animator.SetFloat("Vertical", firstPressedDirection.y);
                    animator.SetBool("IsMoving", true);

                    // Сохраняем последнее направление для idle
                    lastDirection = firstPressedDirection;
                }                
            }
            if (IDLETimer.IsTime)
            {
                isIDLE = true;
                isFirstPress = true;

                // При остановке используем последнее сохраненное направление
                animator.SetFloat("Horizontal", lastDirection.x);
                animator.SetFloat("Vertical", lastDirection.y);
                animator.SetBool("IsMoving", false);
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



    }
}
