
using UnityEngine;

namespace Assets.Scripts.Environment
{
    internal class GameButton : LogicComponent
    {

        protected int countDown = 0;
        [SerializeField] protected int threshold = 1;

        [SerializeField] CollisionDetector collisionDetector;
        [SerializeField] Sprite[] sprites = new Sprite[2];
        [SerializeField] SpriteRenderer spriteRenderer;


        protected override void Start()
        {
            if (!spriteRenderer)
                spriteRenderer = GetComponent<SpriteRenderer>();
            collisionDetector.onTriggerEnter.AddListener(Press);
            collisionDetector.onTriggerExit.AddListener(Unpress);
        }

        protected override bool ActivationFunc()
        {
            return countDown >= threshold;
        }

        private void SetSprite()
        {
            if (IsEnable)
            {
                spriteRenderer.sprite = sprites[1];
            }
            else
            {
                spriteRenderer.sprite = sprites[0];
            }
        }

        public void Press(GameObject gameObject)
        {
            countDown++;
            IsEnable = ActivationFunc();
            SetSprite();
        }

        public void Unpress(GameObject gameObject)
        {
            countDown--;
            IsEnable = ActivationFunc();
            SetSprite();
        }

    }
}
