using UnityEngine;

namespace Blades.Interaction
{
    public class BladesInteractor : MonoBehaviour
    {
        public void OnEnable()
        {
            GrassManager.InteractorManager.Add(gameObject);
        }

        public void OnDisable()
        {
            GrassManager.InteractorManager.Remove(gameObject);
        }
    }
}