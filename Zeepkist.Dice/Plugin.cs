using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using ZeepkistClient;
using ZeepkistNetworking;
using ZeepSDK.Chat;
using ZeepSDK.Cosmetics;
using ZeepSDK.Messaging;
using ZeepSDK.Racing;
using ZeepSDK.Workshop;

namespace Zeepkist.Dice
{

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("ZeepSDK")]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony;

        public static ConfigEntry<int> MaxNumberofDice { get; private set; }
        public static ConfigEntry<int> MaxNumberofSides { get; private set; }
        public static Regex DICE_REGEX = new Regex("\\d+d\\d+");

        private void Awake()
        {
            harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            // Plugin startup logic
            Debug.Log($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            MaxNumberofDice = Config.Bind<int>("Mod", "Maximum number of dice", 10);
            MaxNumberofSides = Config.Bind<int>("Mod", "Maximum number of sides", 100);

            ZeepSDK.ChatCommands.ChatCommandApi.RegisterMixedChatCommand<DiceCommand>();
            DiceCommand.onHandle += DiceCommand_onHandle;
        }

        private void DiceCommand_onHandle(ulong user, string args)
        {
            Logger.LogMessage($"Received a roll command from {user} with arguments {args}");

            // Check to make sure args are correct
            if (!DICE_REGEX.IsMatch(args))
            {
                Logger.LogError($"The args {args} were not in the correct format!");

                if (user == 0)
                {
                    // Local command
                    MessengerApi.LogWarning("Roll command was not in the correct format!");
                }
                else
                {
                    // Remote command
                    ZeepSDK.Chat.ChatApi.SendMessage("The arguments for !roll were not correct.  Correct format is #d#!");
                }
                
                return;
            }

            int[] parts = args.Split('d').Select(x => int.Parse(x)).ToArray();

            // Check to make sure within ranges
            if (parts[0] > MaxNumberofDice.Value || parts[1] > MaxNumberofSides.Value)
            {
                if (user == 0)
                {
                    // Local command
                    MessengerApi.LogWarning("Roll command is over limits!");
                }
                else
                {
                    // Remote command
                    ZeepSDK.Chat.ChatApi.SendMessage($"The arguments for !roll were over limits of {MaxNumberofDice.Value}d${MaxNumberofSides.Value}, try again!");
                }

                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"Rolling {parts[0]} dice with {parts[1]} sides: ");

            List<int> values = new List<int>();
            for (int i = 0; i < parts[0]; i++) 
            {
                int value = RandomNumberGenerator.GetInt32(1, parts[1] + 1);
                values.Add(value);
            }

            if (values.Count == 1) 
            {
                stringBuilder.Append(values[0]);
            } else
            {
                stringBuilder.Append($"{String.Join(" ", values)} = {values.Sum(x => x)}");
            }

            ZeepSDK.Chat.ChatApi.SendMessage(stringBuilder.ToString());
        }

        public void OnDestroy()
        {
            harmony?.UnpatchSelf();
            harmony = null;
        }
    }
}