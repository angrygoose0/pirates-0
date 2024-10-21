using UnityEngine;

public abstract class Active : ScriptableObject
{
    public float cooldown;
    public abstract void Activate(ItemObject item, Vector3 position, Vector2 blockDirection);
}

[CreateAssetMenu(fileName = "NewActive", menuName = "Active/ExplosionActive")]
public class ExplosionActive : Active
{
    public float forceMagnitude;
    public float damageAmount;

    public override void Activate(ItemObject item, Vector3 position, Vector2 blockDirection)
    {
        // Calculate the force vector
        Vector2 force = blockDirection.normalized * forceMagnitude;
        SingletonManager.Instance.shipMovement.ApplyRecoilForce(force);
    }
}



