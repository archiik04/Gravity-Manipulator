using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit;

namespace Weapons
{
    public class Gun : MonoBehaviour
    {
        [SerializeField] private XRGrabInteractable _grabInteractable;
        [SerializeField] protected Transform _gunBarrel;

        // Start is called before the first frame update
        protected virtual void Start()
        {
            Assert.IsNotNull(_grabInteractable, "You have not assigned a grab interactable to gun "+name);
            Assert.IsNotNull(_gunBarrel, "You have not assigned a gun barrel to gun: "+name);

            _grabInteractable.activated.AddListener(Fire);
        }

        protected virtual void Fire(ActivateEventArgs arg0)
        {
            // Base firing functionality - can be extended by child classes
        }
    }
}