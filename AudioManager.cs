using BepInEx;
using HarmonyLib;
using UnityEngine;
using Jotunn.Utils;
using Photon.Pun;
using System.Linq;
using R3DCore.Menu;

namespace R3DCore.Audio
{
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("ROUNDS 3D.exe")]
    [HarmonyPatch]
    public class AudioManager : BaseUnityPlugin
    {
        private const string ModId = "koala.audio.manager";
        private const string ModName = "KAM";
        public const string Version = "0.0.0";

        public static AssetBundle bumble;
        public static AudioManager instance { get; private set; }
        private void Awake()
        {
            instance = this;
            new Harmony(ModId).PatchAll();
            bumble = AssetUtils.LoadAssetBundleFromResources("audioclips", typeof(AudioManager).Assembly);
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "Awake")]
        static void Awake(Player __instance)
        {
            var a = __instance.gameObject.AddComponent<AudioSource>();
            a.spatialBlend = 1;
            a.rolloffMode = AudioRolloffMode.Linear;
            a.maxDistance = 750;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "FireProjectile")]
        public static void PatchFireProjectile(Player __instance)
        {
            var proj = FindObjectsOfType<Projectile>().Where((p) => p.gameObject.GetPhotonView().IsMine && p.gameObject.GetComponent<AudioSource>() == false).ToArray()[0];
            var audioSource = __instance.GetComponent<AudioSource>();
            AudioClip c = bumble.LoadAsset<AudioClip>("shoot");
            bool isShotgun = false;
            // Order in prio <- to ->: explosive, cold, normal
            bool[] stuf = new[] { false, false };
            if(__instance.stats.NrOfProjectiles.baseValue > 3)
            {
                isShotgun = true;
            }
            foreach(var ca in __instance.cards)
            {
                if (ca.name == "C_ExplosiveP")
                {
                    stuf[0] = true;
                }
                if (ca.name == "C_ColdBullets")
                {
                    stuf[1] = true;
                }
            }
            if (isShotgun)
            {
                if (stuf[0])
                {
                    c = bumble.LoadAsset<AudioClip>("explo-shotgun");
                }
                else if (stuf[1])
                {
                    c = bumble.LoadAsset<AudioClip>("cold-shotgun");
                }
                else
                {
                    c = bumble.LoadAsset<AudioClip>("bas-shotgun");
                }
            }
            if (!isShotgun)
            {
                if (stuf[0])
                {
                    c = bumble.LoadAsset<AudioClip>("explo-sin");
                }
                else if (stuf[1])
                {
                    c = bumble.LoadAsset<AudioClip>("cold-sin");
                }
                else
                {
                    c = bumble.LoadAsset<AudioClip>("shoot");
                }
            }
            audioSource.clip = c;
            audioSource.Play();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pl_Movement), "Jump")]
        public static void PatchJump(Player __instance)
        {
            var audioSource = __instance.GetComponent<AudioSource>();
            audioSource.PlayOneShot(bumble.LoadAsset<AudioClip>("jump"));
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Connection), "OnPlayerEnteredRoom")]
        public static void PatchOnPlayerEnteredRoom()
        {
            var self = FindObjectsOfType<PhotonView>().Where((pv) => pv.IsMine == true).ToArray()[0];
            var audioSource = self.gameObject.GetComponent<AudioSource>();
            audioSource.PlayOneShot(bumble.LoadAsset<AudioClip>("join"));
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PL_Damagable), "Die")]
        public static void PatchDie(PL_Damagable __instance)
        {
            var audioSource = __instance.gameObject.GetComponent<AudioSource>();
            audioSource.clip = bumble.LoadAsset<AudioClip>("die");
            audioSource.Play();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PL_Damagable), "Revive")]
        public static void PatchRevive(PL_Damagable __instance)
        {
            var audioSource = __instance.gameObject.GetComponent<AudioSource>();
            audioSource.PlayOneShot(bumble.LoadAsset<AudioClip>("revive"));
        }
    }
}
