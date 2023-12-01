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

[assembly: MelonInfo(typeof(USReducedLethalityMod), "US Reduced Lethality", "1.1.2", "ATLAS")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace USReducedLethality
{
    public class USReducedLethalityMod : MelonMod
    {
        MelonPreferences_Category cfg;
        MelonPreferences_Entry<string> m60a1_ap;
        MelonPreferences_Entry<string> m60a3_ap;
        MelonPreferences_Entry<string> m1_ap;
        MelonPreferences_Entry<string> m1ip_ap;

        private GameObject[] vic_gos;
        private GameObject game_manager;
        private PlayerInput player_manager;
        private CameraManager camera_manager;
        private string[] invalid_scenes = new string[] {"MainMenu2_Scene", "LOADER_MENU", "LOADER_INITIAL", "t64_menu"};

        // m392 
        private AmmoClipCodexScriptable clip_codex_m392;
        private AmmoType.AmmoClip clip_m392;
        private AmmoCodexScriptable ammo_codex_m392;
        private AmmoType ammo_m392;

        // m735
        private AmmoClipCodexScriptable clip_codex_m735;
        private AmmoType.AmmoClip clip_m735;
        private AmmoCodexScriptable ammo_codex_m735;
        private AmmoType ammo_m735;

        // hep 
        private AmmoClipCodexScriptable clip_codex_m393;
        private AmmoType.AmmoClip clip_m393;
        private AmmoCodexScriptable ammo_codex_m393;
        private AmmoType ammo_m393;

        private AmmoType ammo_m833;
        private AmmoType ammo_m456;

        private GameObject ammo_m392_vis = null;
        private GameObject ammo_m393_vis = null;
        private GameObject ammo_m735_vis = null;

        // m774
        private AmmoClipCodexScriptable clip_codex_m774;


        public override void OnInitializeMelon()
        {
            cfg = MelonPreferences.CreateCategory("US Reduced Lethality");
            m60a1_ap = cfg.CreateEntry<string>("M60A1 AP", "M392");
            m60a1_ap.Description = "AP round used by M60s and M1s: M774, M735, M392";
            m60a3_ap = cfg.CreateEntry<string>("M60A3 AP", "M392");
            m1_ap = cfg.CreateEntry<string>("M1 AP", "M774");
            m1ip_ap = cfg.CreateEntry<string>("M1IP AP", "M774");
        }

        // fix for GAS reticle
        public override void OnUpdate()
        {
            if (game_manager == null) return;

            FieldInfo currentCamSlot = typeof(CameraManager).GetField("_currentCamSlot", BindingFlags.Instance | BindingFlags.NonPublic);
            CameraSlot cam = (CameraSlot)currentCamSlot.GetValue(camera_manager);

            if (cam == null) return;
            if (cam.name != "Aux sight M105D" && cam.name != "Aux sight (GAS)") return;

            AmmoType current_ammo = player_manager.CurrentPlayerWeapon.FCS.CurrentAmmoType;
            string[] valid_ammo = new string[] {"M392A2 APDS-T", "M393A2 HEP-T", "M735 APFSDS-T"};  
            if (!valid_ammo.Contains(current_ammo.Name)) return;

            GameObject reticle = cam.transform.GetChild(0).gameObject;

            if (!reticle.activeSelf)
            {
                reticle.SetActive(true);
            }
        }

        public override async void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (invalid_scenes.Contains(sceneName)) return; 
            vic_gos = GameObject.FindGameObjectsWithTag("Vehicle");

            while (vic_gos.Length == 0)
            {
                vic_gos = GameObject.FindGameObjectsWithTag("Vehicle");
                await Task.Delay(1);
            }

            await Task.Delay(3000);

            game_manager = GameObject.Find("_APP_GHPC_");
            camera_manager = game_manager.GetComponent<CameraManager>();
            player_manager = game_manager.GetComponent<PlayerInput>();

            if (ammo_m393 == null)
            {
                foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
                {
                    if (s.AmmoType.Name == "M833 APFSDS-T") ammo_m833 = s.AmmoType;
                    if (s.AmmoType.Name == "M456 HEAT-FS-T") ammo_m456 = s.AmmoType;
                }

                foreach (AmmoClipCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoClipCodexScriptable)))
                {
                    if (s.name == "clip_M774") clip_codex_m774 = s;
                }

                // m392 
                ammo_m392 = new AmmoType();
                Util.ShallowCopy(ammo_m392, ammo_m833);
                ammo_m392.Name = "M392A2 APDS-T";
                ammo_m392.RhaPenetration = 310f;
                ammo_m392.MuzzleVelocity = 1478f;
                ammo_m392.Mass = 4.04f;

                ammo_codex_m392 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_m392.AmmoType = ammo_m392;
                ammo_codex_m392.name = "ammo_m392";

                clip_m392 = new AmmoType.AmmoClip();
                clip_m392.Capacity = 1;
                clip_m392.Name = "M392A2 APDS-T";
                clip_m392.MinimalPattern = new AmmoCodexScriptable[1];
                clip_m392.MinimalPattern[0] = ammo_codex_m392;

                clip_codex_m392 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_m392.name = "clip_m392";
                clip_codex_m392.ClipType = clip_m392;

                // m393 
                ammo_m393 = new AmmoType();
                Util.ShallowCopy(ammo_m393, ammo_m456);
                ammo_m393.Name = "M393A2 HEP-T";
                ammo_m393.RhaPenetration = 30f;
                ammo_m393.MuzzleVelocity = 731.5f;
                ammo_m393.Mass = 11.3f;
                ammo_m393.TntEquivalentKg = 3.26f;
                ammo_m393.CertainRicochetAngle = 5f; 
                ammo_m393.MinSpallRha = 20f;
                ammo_m393.MaxSpallRha = 50f;
                ammo_m393.Coeff = 0.26f;
                ammo_m393.Category = AmmoType.AmmoCategory.Explosive;
                ammo_m393.ShatterOnRicochet = false;
                ammo_m393.ImpactFuseTime = 0.005f;
                ammo_m393.SpallMultiplier = 2;
                ammo_m393.DetonateSpallCount = 80;
                ammo_m393.ForcedSpallAngle = 0; 
                ammo_m393.ImpactTypeFuzed = ParticleEffectsManager.EffectVisualType.MainGunImpactHighExplosive;
                ammo_m393.ImpactTypeFuzedTerrain = ParticleEffectsManager.EffectVisualType.MainGunImpactExplosiveTerrain;
                ammo_m393.ImpactTypeUnfuzed = ParticleEffectsManager.EffectVisualType.MainGunImpactHighExplosive;
                ammo_m393.ImpactTypeUnfuzedTerrain = ParticleEffectsManager.EffectVisualType.MainGunImpactExplosiveTerrain;
                ammo_m393.ShortName = AmmoType.AmmoShortName.He; 

                ammo_codex_m393 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_m393.AmmoType = ammo_m393;
                ammo_codex_m393.name = "ammo_m393";

                clip_m393 = new AmmoType.AmmoClip();
                clip_m393.Capacity = 1;
                clip_m393.Name = "M393A2 HEP-T";
                clip_m393.MinimalPattern = new AmmoCodexScriptable[1];
                clip_m393.MinimalPattern[0] = ammo_codex_m393;

                clip_codex_m393 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_m393.name = "clip_m393";
                clip_codex_m393.ClipType = clip_m393;

                // m735 
                ammo_m735 = new AmmoType();
                Util.ShallowCopy(ammo_m735, ammo_m833);
                ammo_m735.Name = "M735 APFSDS-T";
                ammo_m735.RhaPenetration = 400f;
                ammo_m735.MuzzleVelocity = 1501f;
                ammo_m735.Mass = 2.16f;

                ammo_codex_m735 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_m735.AmmoType = ammo_m735;
                ammo_codex_m735.name = "ammo_m735";

                clip_m735 = new AmmoType.AmmoClip();
                clip_m735.Capacity = 1;
                clip_m735.Name = "M735 APFSDS-T";
                clip_m735.MinimalPattern = new AmmoCodexScriptable[1];
                clip_m735.MinimalPattern[0] = ammo_codex_m735;

                clip_codex_m735 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_m735.name = "clip_m735";
                clip_codex_m735.ClipType = clip_m735;
            }

            var ap_rounds = new Dictionary<string, AmmoClipCodexScriptable>() {
                ["M774"] = clip_codex_m774,
                ["M735"] = clip_codex_m735,
                ["M392"] = clip_codex_m392
            };

            foreach (GameObject vic_go in vic_gos)
            {
                Vehicle vic = vic_go.GetComponent<Vehicle>();

                if (vic == null) continue;
                if (!new string[] {"M60A3 TTS", "M60A1 RISE (Passive)", "M1", "M1IP"}.Contains(vic.FriendlyName)) continue;

                if (ammo_m392_vis == null)
                {
                    ammo_m392_vis = GameObject.Instantiate(ammo_m833.VisualModel);
                    ammo_m392_vis.name = "M392 visual";
                    ammo_m392.VisualModel = ammo_m392_vis;
                    ammo_m392.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_m392;
                    ammo_m392.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_m392;

                    ammo_m735_vis = GameObject.Instantiate(ammo_m833.VisualModel);
                    ammo_m735_vis.name = "M735 visual";
                    ammo_m735.VisualModel = ammo_m735_vis;
                    ammo_m735.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_m735;
                    ammo_m735.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_m735;

                    ammo_m393_vis = GameObject.Instantiate(ammo_m456.VisualModel);
                    ammo_m393_vis.name = "M393 visual";
                    ammo_m393.VisualModel = ammo_m393_vis;
                    ammo_m393.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_m393;
                    ammo_m393.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_m393;
                }

                string name = vic.FriendlyName;

                WeaponsManager weaponsManager = vic.GetComponent<WeaponsManager>();
                WeaponSystemInfo mainGunInfo = weaponsManager.Weapons[0];
                WeaponSystem mainGun = mainGunInfo.Weapon;

                LoadoutManager loadoutManager = vic.GetComponent<LoadoutManager>();
                AmmoType.AmmoClip[] ammo_clip_types = new AmmoType.AmmoClip[] { };
                int total_racks = 5;

                AmmoClipCodexScriptable clip_codex_m456 = loadoutManager.LoadedAmmoTypes[1];

                if (name == "M60A3 TTS" || name == "M60A1 RISE (Passive)") {
                    AmmoClipCodexScriptable ap = ap_rounds[name == "M60A3 TTS" ? m60a3_ap.Value : m60a1_ap.Value];
                    loadoutManager.TotalAmmoCounts = new int[] {30, 23, 10};
                    loadoutManager.LoadedAmmoTypes = new AmmoClipCodexScriptable[] {ap, clip_codex_m456, clip_codex_m393};
                    ammo_clip_types = new AmmoType.AmmoClip[] {ap.ClipType, clip_codex_m456.ClipType, clip_m393};

                    FieldInfo total_ammo_types = typeof(LoadoutManager).GetField("_totalAmmoTypes", BindingFlags.NonPublic | BindingFlags.Instance);
                    total_ammo_types.SetValue(loadoutManager, 3);
                }

                if (name == "M1" || name == "M1IP") {
                    AmmoClipCodexScriptable ap = ap_rounds[name == "M1" ? m1_ap.Value : m1ip_ap.Value];
                    loadoutManager.LoadedAmmoTypes[0] = ap;
                    ammo_clip_types = new AmmoType.AmmoClip[] {ap.ClipType, clip_codex_m456.ClipType};
                    total_racks = 3; 
                }

                for (int i = 0; i < total_racks; i++)
                {
                    GHPC.Weapons.AmmoRack rack = loadoutManager.RackLoadouts[i].Rack;
                    rack.ClipTypes = ammo_clip_types;
                    Util.EmptyRack(rack);
                }

                loadoutManager.SpawnCurrentLoadout();

                PropertyInfo roundInBreech = typeof(AmmoFeed).GetProperty("AmmoTypeInBreech");
                roundInBreech.SetValue(mainGun.Feed, null);

                MethodInfo refreshBreech = typeof(AmmoFeed).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic); 
                refreshBreech.Invoke(mainGun.Feed, new object[] {});

                MethodInfo registerAllBallistics = typeof(LoadoutManager).GetMethod("RegisterAllBallistics", BindingFlags.Instance | BindingFlags.NonPublic);
                registerAllBallistics.Invoke(loadoutManager, new object[] {});

            }
        }
    }
}
