using UnityEngine;


namespace Vanguards
{
	public class Obstacle : MonoBehaviour
	{

        private void Start() => Refresh();
        private void OnValidate() => Refresh();

        void Refresh()
        {
            gameObject.layer = LayerMask.NameToLayer("Obstacle");
        }
    };
};
