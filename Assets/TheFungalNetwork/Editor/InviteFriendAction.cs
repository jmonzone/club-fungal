using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class InviteFriendAction : UnitDrawerItemAction
    {
        public InviteFriendAction(UnitControllerService service, UnitController controller)
        {
            text = "Invite Friend";
            emoji = "📨";
            action = () =>
            {
                if (controller != null)
                {
                    service.InviteFriend(controller);
                }
                else
                {
                    Debug.LogWarning($"No controller found for unit instance '{controller.Instance.Id}' to invite friend.");
                }
            };
            condition = () => true;
        }
    }
}
