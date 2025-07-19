using UnityEngine;

namespace Assets.Scripts.Accessory
{
    internal class FolowObject : MonoBehaviour
    {
        GameObject following;
        public GameObject Following { get => following; set => following = value; }

        private void Update()
        {
            if (following != null)
            {
                transform.position = following.transform.position;
            }
        }

    }
}
