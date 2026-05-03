using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace forceClassChange
{
    public class forceClassChangeModSystem : ModSystem
    {
        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Event.PlayerJoin += (IServerPlayer byplayer) =>
            {
                int requiredVersion = api.World.Config.GetInt("forceClassVersion", 0);
                int playerVersion = byplayer.Entity.WatchedAttributes.GetInt("forceClassVersion", 0);

                if (playerVersion < requiredVersion)
                {
                    ResetPlayerClass(byplayer, requiredVersion);
                }
            };

            api.ChatCommands
                .Create("forceclasschange")
                .WithDescription("Force all players to reselect their character class")
                .RequiresPrivilege(Privilege.root)
                .HandleWith(args =>
                {
                    int nextVersion = api.World.Config.GetInt("forceClassVersion", 0) + 1;
                    api.World.Config.SetInt("forceClassVersion", nextVersion);

                    foreach (IServerPlayer player in api.World.AllOnlinePlayers) { 
                        player.Disconnect();
                    }

                    return TextCommandResult.Success($"Bumped Version to {nextVersion}. All players were kicked.");
                });
        }

        private void ResetPlayerClass(IServerPlayer player, int newVersion) {
            player.RemoveModdata("createCharacter");

            player.Entity.WatchedAttributes.RemoveAttribute("allowcharselonce");
            player.Entity.WatchedAttributes.RemoveAttribute("characterClass");
            player.Entity.WatchedAttributes.RemoveAttribute("extraTraits");

            player.Entity.WatchedAttributes.SetInt("forceClassVersion", newVersion);
            player.Entity.WatchedAttributes.MarkAllDirty();
        }
    }
}
