using System;
using System.Collections.Generic;
using UnityEngine;

namespace DarkMultiPlayer
{
    public class AsteroidWorker
    {
        //singleton
        private static AsteroidWorker singleton;
        //How many asteroids to spawn into the server
        public int maxNumberOfUntrackedAsteroids;
        public bool workerEnabled;
        //private state variables
        private float lastAsteroidCheck;
        private const float ASTEROID_CHECK_INTERVAL = 5f;
        ScenarioDiscoverableObjects scenarioController;
        private List<string> serverAsteroids = new List<string>();
        private Dictionary<string,string> serverAsteroidTrackStatus = new Dictionary<string,string>();
        private object serverAsteroidListLock = new object();

        public static AsteroidWorker fetch
        {
            get
            {
                return singleton;
            }
        }

        private void FixedUpdate()
        {
            if (workerEnabled)
            {
                if (scenarioController == null)
                {
                    foreach (ProtoScenarioModule psm in HighLogic.CurrentGame.scenarios)
                    {
                        if (psm != null)
                        {
                            if (psm.moduleName == "ScenarioDiscoverableObjects")
                            {
                                if (psm.moduleRef != null)
                                {
                                    scenarioController = (ScenarioDiscoverableObjects)psm.moduleRef;
                                    scenarioController.spawnInterval = float.MaxValue;
                                }
                            }
                        }
                    }
                }
            }
            if (workerEnabled && scenarioController != null)
            {
                if ((UnityEngine.Time.realtimeSinceStartup - lastAsteroidCheck) > ASTEROID_CHECK_INTERVAL)
                {
                    List<Vessel> asteroidList = GetAsteroidList();
                    lastAsteroidCheck = UnityEngine.Time.realtimeSinceStartup;
                    //Try to acquire the asteroid-spawning lock if nobody else has it.
                    if (!LockSystem.fetch.LockExists("asteroid-spawning"))
                    {
                        LockSystem.fetch.AcquireLock("asteroid-spawning", false);
                    }
                    //We have the spawn lock, lets do stuff.
                    if (LockSystem.fetch.LockIsOurs("asteroid-spawning"))
                    {
                        if (FlightGlobals.fetch.vessels != null ? FlightGlobals.fetch.vessels.Count > 0 : false)
                        {
                            lock (serverAsteroidListLock)
                            {

                                if (asteroidList.Count < maxNumberOfUntrackedAsteroids)
                                {
                                    DarkLog.Debug("Spawning asteroid, have " + asteroidList.Count + ", need " + maxNumberOfUntrackedAsteroids);
                                    scenarioController.SpawnAsteroid();
                                    asteroidList = GetAsteroidList();
                                    foreach (Vessel asteroid in asteroidList)
                                    {
                                        if (!serverAsteroids.Contains(asteroid.id.ToString()))
                                        {
                                            DarkLog.Debug("Spawned in new server asteroid!");
                                            serverAsteroids.Add(asteroid.id.ToString());
                                            VesselWorker.fetch.RegisterServerVessel(asteroid.id.ToString());
                                            NetworkWorker.fetch.SendVesselProtoMessage(asteroid.protoVessel, false, false);
                                        }
                                        if (serverAsteroids.Count >= maxNumberOfUntrackedAsteroids)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    foreach (Vessel asteroid in asteroidList)
                    {
                        if (!serverAsteroids.Contains(asteroid.id.ToString()))
                        {
                            DarkLog.Debug("Killing non-server asteroid " + asteroid.id);
                            try
                            {
                                asteroid.Die();
                            }
                            catch (Exception e)
                            {
                                DarkLog.Debug("Error killing asteroid " + asteroid.id + ", exception " + e);
                            }
                        }
                    }
                    //Check for changes to tracking
                    foreach (Vessel asteroid in asteroidList)
                    {
                        if (asteroid.state != Vessel.State.DEAD)
                        {
                            if (!serverAsteroidTrackStatus.ContainsKey(asteroid.id.ToString()))
                            {
                                serverAsteroidTrackStatus.Add(asteroid.id.ToString(), asteroid.DiscoveryInfo.trackingStatus.Value);
                            }
                            else
                            {
                                if (asteroid.DiscoveryInfo.trackingStatus.Value != serverAsteroidTrackStatus[asteroid.id.ToString()])
                                {
                                    ProtoVessel pv = asteroid.BackupVessel();
                                    if (pv.protoPartSnapshots.Count == 0)
                                    {
                                        DarkLog.Debug("Protovessel still has no parts");
                                        return;
                                    }
                                    DarkLog.Debug("Sending changed asteroid, new state: " + asteroid.DiscoveryInfo.trackingStatus.Value + "!");
                                    serverAsteroidTrackStatus[asteroid.id.ToString()] = asteroid.DiscoveryInfo.trackingStatus.Value;
                                    NetworkWorker.fetch.SendVesselProtoMessage(pv, false, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<Vessel> GetAsteroidList()
        {
            List<Vessel> returnList = new List<Vessel>();
            foreach (Vessel checkVessel in FlightGlobals.fetch.vessels)
            {
                if (checkVessel != null)
                {
                    if (checkVessel.vesselType == VesselType.SpaceObject)
                    {
                        if (checkVessel.protoVessel != null)
                        {
                            if (checkVessel.protoVessel.protoPartSnapshots != null)
                            {
                                if (checkVessel.protoVessel.protoPartSnapshots.Count == 1)
                                {
                                    if (checkVessel.protoVessel.protoPartSnapshots[0].partName == "PotatoRoid")
                                    {
                                        returnList.Add(checkVessel);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return returnList;
        }

        public void RegisterServerAsteroid(string asteroidID)
        {
            lock (serverAsteroidListLock)
            {
                if (!serverAsteroids.Contains(asteroidID))
                {
                    serverAsteroids.Add(asteroidID);
                }
                //This will ignore status changes so we don't resend the asteroid.
                if (serverAsteroidTrackStatus.ContainsKey(asteroidID))
                {
                    serverAsteroidTrackStatus.Remove(asteroidID);
                }
            }
        }

        public static void Reset()
        {
            lock (Client.eventLock)
            {
                if (singleton != null)
                {
                    singleton.workerEnabled = false;
                    Client.fixedUpdateEvent.Remove(singleton.FixedUpdate);
                }
                singleton = new AsteroidWorker();
                Client.fixedUpdateEvent.Add(singleton.FixedUpdate);
            }
        }
    }
}

