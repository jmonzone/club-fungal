namespace TheFungalNetwork.Editor
{
    public class ViewInstanceAction : UnitDrawerItemAction
    {
        public ViewInstanceAction(UnitInstance unitInstance)
        {
            text = "View Instance";
            emoji = "👁️";
            action = () =>
            {
                UnitInstanceInspectorWindow.ShowWindow(unitInstance);
            };
            condition = () => true;
        }
    }
}
