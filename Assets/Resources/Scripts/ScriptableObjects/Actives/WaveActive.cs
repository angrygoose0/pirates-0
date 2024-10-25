using UnityEngine;

[CreateAssetMenu(fileName = "NewActive", menuName = "Active/WaveActive")]
public class WaveActive : ItemAbilityClass
{
    public float forceMagnitude;

    public override void Activate(ItemObject item, Vector3 position, Vector2 blockDirection)
    {
        Vector2 force = blockDirection.normalized * forceMagnitude;
        SingletonManager.Instance.shipMovement.ApplyRecoilForce(force);
    }
}