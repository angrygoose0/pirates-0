using UnityEngine;

public abstract class Active : ScriptableObject
{
    public float cooldown;
    public abstract void Activate(ItemObject item, Vector3 position);
}

[CreateAssetMenu(fileName = "NewActive", menuName = "Active/ExplosionActive")]
public class ExplosionActive : Active
{
    public float forceMagnitude;
    public float damageAmount;

    public override void Activate(ItemObject item, Vector3 position)
    {

        ShipMovement shipMovement = ShipMovement.Instance;
        if (shipMovement != null && shipMovement.currentVelocity != Vector2.zero)
        {
            // Get the direction of the current velocity
            Vector2 forceDirection = shipMovement.currentVelocity.normalized;

            // Calculate the force vector
            Vector2 force = forceDirection * forceMagnitude;
            //shipMovement.currentVelocity += force;

            shipMovement.ApplyRecoilForce(force);
        }
    }
}

