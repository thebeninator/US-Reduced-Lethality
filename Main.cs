using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using GHPC;
using USReducedLethality;
using GHPC.Weapons;
using GHPC.Effects;
using UnityEngine;
using System.Reflection;
using GHPC.Vehicle;
using GHPC.Camera;
using GHPC.Player;
using HarmonyLib;
using GHPC.State;
using System.Collections;

[assembly: MelonInfo(typeof(USReducedLethalityMod), "US Reduced Lethality", "1.1.4", "ATLAS")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace USReducedLethality
{
    public class USReducedLethalityMod : MelonMod
    {
        public static GameObject[] vic_gos;
        public static GameObject game_manager;
        public static CameraManager cam_manager;
        public static PlayerInput player_manager;

        public IEnumerator GetVics(GameState _)
        {
            vic_gos = GameObject.FindGameObjectsWithTag("Vehicle");

            yield break;
        }

        public override void OnInitializeMelon()
        {
            MelonPreferences_Category cfg = MelonPreferences.CreateCategory("US Reduced Lethality");
            ReducedLethality.Config(cfg);
        }

        public override void OnUpdate()
        {
            ReducedLethality.Update();
        }

        public override void OnSceneWasLoaded(int idx, string scene_name)
        {
            if (Util.menu_screens.Contains(scene_name)) return;

            game_manager = GameObject.Find("_APP_GHPC_");
            cam_manager = game_manager.GetComponent<CameraManager>();
            player_manager = game_manager.GetComponent<PlayerInput>();

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(GetVics), GameStatePriority.Medium);

            ReducedLethality.Init();
        }
    }
}
