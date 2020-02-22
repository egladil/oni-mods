namespace Egladil
{
    public class LogicBase : KMonoBehaviour
    {
        [MyCmpGet]
        protected LogicPorts ports;

        private static readonly EventSystem.IntraObjectHandler<LogicBase> OnLogicValueChangedDelegate = new EventSystem.IntraObjectHandler<LogicBase>((LogicBase component, object data) => 
        {
            if (component == null || component.gameObject == null || component.ports == null) return;
            component.OnLogicValueChanged((LogicValueChanged)data);
        });

        protected override void OnSpawn()
        {
            Subscribe(-801688580, OnLogicValueChangedDelegate);
        }

        protected virtual void OnLogicValueChanged(LogicValueChanged data) { }
    }
}
