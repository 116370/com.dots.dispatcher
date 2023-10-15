using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHeathAuthoring : MonoBehaviour
{
    public int PlayerHeath = 100;
    void OnEnable() { }

    class Baker : Baker<PlayerHeathAuthoring>
    {
        public override void Bake(PlayerHeathAuthoring authoring)
        {
            if (!authoring.enabled)
                return;

            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new PlayerHeathC
            {
                currentHealth = authoring.PlayerHeath,
                maxHealth = authoring.PlayerHeath,
            });
        }
    }
}

public struct PlayerHeathC : IComponentData
{
    public int maxHealth;
    public int currentHealth;

    public override string ToString()
    {
        return $"maxHealth: {maxHealth} currentHealth {currentHealth}";
    }
}

