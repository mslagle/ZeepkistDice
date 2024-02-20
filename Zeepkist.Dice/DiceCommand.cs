using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeepSDK.ChatCommands;

namespace Zeepkist.Dice
{
    public class DiceCommand : IMixedChatCommand, IChatCommand
    {
        public string Prefix => "!";

        public string Command => "roll";

        public string Description => "Rolls a dice in the format of /roll [number of dice]d[number of sides].  Ex: 1d20 = 1 20-sided dice.";

        public void Handle(ulong playerId, string arguments)
        {
            Action<ulong, string> onHandle = DiceCommand.onHandle;
            if (onHandle != null) onHandle(playerId, arguments);
        }

        public void Handle(string arguments)
        {
            Action<ulong, string> onHandle = DiceCommand.onHandle;
            if (onHandle != null) onHandle(0, arguments);
        }

        public static event Action<ulong, string> onHandle;
    }
}
