using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using FivePD.API.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ShotsFired
{
    [Guid("075ED322-CDF9-4FA3-BAE9-A195E991A453")]
    [CalloutProperties("Shots Fired", "DevKilo","1.0")]
    public class ShotsFired : Callout
    {
        Random rnd = new Random();
        List<Ped> suspects = new();
        bool endedEarly = true;
        public ShotsFired()
        {
            int distance = rnd.Next(200, 750);
            float offsetX = rnd.Next(-1 * distance, distance);
            float offsetY = rnd.Next(-1 * distance, distance);

            InitInfo(World.GetNextPositionOnStreet(Game.PlayerPed.GetOffsetPosition(new Vector3(offsetX, offsetY, 0))));
            ShortName = "Shots Fired";
            CalloutDescription = "911 Report: Shots have been reported in the area! Respond code 3.";
            ResponseCode = 3;
            StartDistance = 200f;
        }

        public override async Task OnAccept()
        {
            InitBlip();
            SpawnSuspects();
        }

        private async Task SpawnSuspects()
        {
            int suspectsNumber = rnd.Next(1, 4);
            for (int i = 0; i <= suspectsNumber; i++)
            {
                Ped suspect = await SpawnPed(RandomUtils.GetRandomPed(), Location);
                suspects.Add(suspect);
                suspect.Weapons.Give(RandomUtils.GetRandomWeapon(), 255, true, true);
                suspect.Task.WanderAround();
            }
        }
        public override void OnStart(Ped closest)
        {
            foreach (Ped suspect in suspects)
            {
                suspect.AttachBlip();
                suspect.AlwaysKeepTask = true;
                suspect.BlockPermanentEvents = true;

                if (!suspects.Contains(closest))
                {
                    suspect.Task.FightAgainst(closest);
                }
                
 
            }
            bool db = false;
            Tick += async () =>
            {
                if (!db)
                {
                    db = true;
                    float dist = 1000;
                    foreach (Ped player in AssignedPlayers)
                    {
                        foreach (Ped p in suspects)
                        {
                            if (p.IsDead) endedEarly = false;
                            float checkdist = p.Position.DistanceTo(player.Position);
                            if (checkdist < dist)
                            {
                                dist = checkdist;
                                p.Task.FightAgainst(player);
                            }
                        }
                    }
                    await BaseScript.Delay(1000);
                    db = false;
                }
                else return;
                
            };
        }
        public override void OnCancelBefore()
        {
            foreach (Ped suspect in suspects)
            {
                suspect?.AttachedBlip?.Delete();
                if (endedEarly)
                {
                    suspect?.Delete();
                }
                
            }
        }
        public override void OnCancelAfter()
        {
            Function.Call(Hash.CLEAR_AREA, Location.X, Location.Y, Location.Z, 20f, false, false, false, false, false);
        }
    }
}