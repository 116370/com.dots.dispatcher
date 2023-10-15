using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class SampleDataAuthoring : MonoBehaviour
{
    public SampleType sampleType;
    void OnEnable() { }

    class Baker : Baker<SampleDataAuthoring>
    {
        public override void Bake(SampleDataAuthoring authoring)
        {
            if (!authoring.enabled)
                return;

            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new SampleSettingC
            {
                 value = authoring.sampleType
            });
        }
    }
}
public enum SampleType
{
    MainThread,
    Job,
    JobParallel
}

public struct SampleSettingC : IComponentData
{
    public SampleType value;
}

