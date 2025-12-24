namespace TheFungalNetwork.Editor
{
    public class TogglePartyAction : UnitDrawerItemAction
    {
        public TogglePartyAction(bool isInParty, PartyInstanceService service, UnitInstance unitInstance)
        {
            text = isInParty ? "Remove from Party" : "Add to Party";
            emoji = "🎉";
            action = () =>
            {
                if (isInParty)
                    service.RemoveUnitInstanceFromParty(unitInstance);
                else
                    service.AddUnitInstanceToParty(unitInstance);
            };
            condition = () => true;
        }
    }
}
