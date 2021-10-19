using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("No Restore In Dynamic PVP Zone", "WhiteThunder", "1.0.0")]
    [Description("Prevents player inventories from being restored after dying in a Dynamic PVP zone.")]
    internal class NoRestoreInDynamicPVPZone : CovalencePlugin
    {
        [PluginReference]
        private Plugin DynamicPVP, ZoneManager;

        // This hook is exposed by Restore Upon Death.
        // It is called at the following times.
        // - After the player has died and their inventory is about to be saved.
        // - After the player has respawned and their saved inventory is about to be restored.
        private bool? OnRestoreUponDeath(BasePlayer player)
        {
            if (player.IsAlive())
            {
                // Called after respawn, so do nothing.
                // We don't care if the player happens to respawn into a dynamic PVP zone.
                return null;
            }
            
            var inPvpDelay = DynamicPVP?.Call("IsPlayerInPVPDelay", player.userID);
            if (inPvpDelay is bool && (bool)inPvpDelay)
            {
                // Player recently exited a PVP zone and can still be killed.
                return false;
            }

            var zoneIdList = ZoneManager?.Call("GetPlayerZoneIDs", player) as string[];
            if (zoneIdList == null)
            {
                // Player is not in a zone, or Zone Manager is not loaded.
                return null;
            }

            foreach (var zoneId in zoneIdList)
            {
                var isPvpZone = DynamicPVP?.Call("IsDynamicPVPZone", zoneId);
                if (isPvpZone is bool && (bool)isPvpZone)
                {
                    // Player is in a dynamic PVP zone, so block saving their inventory.
                    return false;
                }
            }

            return null;
        }
    }
}
