using UnityEngine;

namespace Blades.Interaction
{
    public class BladesInteractor : MonoBehaviour
    {
        public void OnEnable()
        {
            BladesManager.InteractorManager.Add(gameObject);
        }

        public void OnDisable()
        {
            BladesManager.InteractorManager.Remove(gameObject);
        }
    }
}