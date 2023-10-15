//using Unity.Entities;

//namespace Prototype
//{
//    public static class EventECBExt
//    {
//        public static void ProduceEntityEvent<T>(this EntityCommandBuffer ecb, Entity e, T comp = default) where T : unmanaged, IComponentData, IEnableableComponent
//        {
//            ecb.AddComponent(e, comp);
//            ecb.SetComponentEnabled<T>(e, true);
//        }
//    }
//}