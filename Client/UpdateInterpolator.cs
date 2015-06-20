using System;
using System.Collections.Generic;
using UnityEngine;

namespace DarkMultiPlayer
{
    public class UpdateInterpolator
    {
        //Singleton
        private static UpdateInterpolator singleton = new UpdateInterpolator();
        public static UpdateInterpolator fetch
        {
            get
            {
                return singleton;
            }
        }

        //Vessel Update Storage
        private static Dictionary<Guid, List<VesselVectorEntry>> interpoUpdates = new Dictionary<Guid, List<VesselVectorEntry>> ();
        private static Dictionary<Guid, bool> canInterpolate = new Dictionary<Guid, bool> ();

        public UpdateInterpolator ()
        {
            //Client.updateEvent.Add (this.Update);
        }


        public void Update()
        {
            //Update every vessel's position we have data on
            //Vector3d updatePostion = updateBody.GetWorldSurfacePosition (interpolatedPosition [0], interpolatedPosition [1], interpolatedPosition [2] + altitudeFudge); // + positionFudge;
            //foreach (Vessel updateVessel in FlightGlobals.Vessels) {
                //Check if we are tracking the vessel
                //if (!updatesRemaining.ContainsKey (updateVessel.id))
                //{
                //    continue;
                //}

                //if (updatesRemaining [updateVessel.id] > 0) {
                //    VesselUpdate latestUpdate = interpoUpdates [updateVessel.id] [interpoUpdates [updateVessel.id].Count - 1];

                //    CelestialBody updateBody = FlightGlobals.Bodies.Find (b => b.bodyName == latestUpdate.bodyName);
                //    if (updateBody == null) {
                        //DarkLog.Debug("ApplyVesselUpdate - updateBody not found");
                //        continue;
                //    }

                //   Quaternion normalRotate = Quaternion.identity;


                    //Get Interpolated position
                //    double[] interpoPosition = GetVesselInterpoPosition (updateVessel.id);

                //    double altitudeFudge = 0;
                //    VesselUtil.DMPRaycastPair dmpRaycast = VesselUtil.RaycastGround(interpoPosition[0], interpoPosition[1], updateBody);
                //    if (dmpRaycast.altitude != -1d && interpoPosition[3] != -1d)
                //    {

                //        Vector3 theirNormal = new Vector3(latestUpdate.terrainNormal[0], latestUpdate.terrainNormal[1], latestUpdate.terrainNormal[2]);
                //        altitudeFudge = dmpRaycast.altitude - interpoPosition[3];
                //        if (Math.Abs(interpoPosition[2] - latestUpdate.position[3]) < 50f)
                //        {
                //            normalRotate = Quaternion.FromToRotation(theirNormal, dmpRaycast.terrainNormal);
                //        }
                //    }

                //    double planetariumDifference = Planetarium.GetUniversalTime() - latestUpdate.planetTime;

                    //Velocity fudge
                //    Vector3d updateAcceleration = updateBody.bodyTransform.rotation * new Vector3d(latestUpdate.acceleration[0], latestUpdate.acceleration[1], latestUpdate.acceleration[2]);
                //    Vector3d velocityFudge = Vector3d.zero;
                //    if (Math.Abs(planetariumDifference) < 3f)
                //    {
                        //Velocity = a*t
                //        velocityFudge = updateAcceleration * planetariumDifference;
                //    }

                    //Position fudge
                //    Vector3d updateVelocity = updateBody.bodyTransform.rotation * new Vector3d(latestUpdate.velocity[0], latestUpdate.velocity[1], latestUpdate.velocity[2]) + velocityFudge;



                //    Vector3d updatePosition = updateBody.GetWorldSurfacePosition (interpoPosition [0], interpoPosition [1], interpoPosition [2] + altitudeFudge);




                    //double latitude = updateBody.GetLatitude (updatePosition);
                    //double longitude = updateBody.GetLongitude (updatePosition);
                    //double altitude = updateBody.GetAltitude (updatePosition);
                    //updateVessel.latitude = latitude;
                    //updateVessel.longitude = longitude;
                    //updateVessel.altitude = altitude;
                    //updateVessel.protoVessel.latitude = latitude;
                    //updateVessel.protoVessel.longitude = longitude;
                    //updateVessel.protoVessel.altitude = altitude;

                    //if (updateVessel.packed) {
                    //    if (!updateVessel.LandedOrSplashed) {
                    //        //Not landed but under 10km.
                    //        Vector3d orbitalPos = updatePosition - updateBody.position;
                    //        Vector3d surfaceOrbitVelDiff = updateBody.getRFrmVel (updatePosition);
                    //        Vector3d orbitalVel = updateVelocity + surfaceOrbitVelDiff;
                    //        updateVessel.orbitDriver.orbit.UpdateFromStateVectors (orbitalPos.xzy, orbitalVel.xzy, updateBody, Planetarium.GetUniversalTime ());
                    //        updateVessel.orbitDriver.pos = updateVessel.orbitDriver.orbit.pos.xzy;
                    //        updateVessel.orbitDriver.vel = updateVessel.orbitDriver.orbit.vel;
                    //    }
                    //} else {
                    //    Vector3d velocityOffset = updateVelocity - updateVessel.srf_velocity;
                    //    updateVessel.SetPosition (updatePosition, true);
                    //    updateVessel.ChangeWorldVelocity (velocityOffset);
                    //}

                    //updatesRemaining [updateVessel.id]--;
                //}
            //}
        }

        public static bool CanInterpolate(Guid vesselID)
        {
            if (!interpoUpdates.ContainsKey(vesselID) && !canInterpolate.ContainsKey(vesselID))
            {
                //Return false if the vessel has no tracked updates or no state tracking
                return false;
            }
            return canInterpolate [vesselID];
        }

        public static void AddVesselCoordinates(Guid vesselID, VesselVectorEntry entry)
        {
            //Make sure storage and state tracking contains key
            if (!interpoUpdates.ContainsKey(vesselID))
            {
                interpoUpdates.Add(vesselID, new List<VesselVectorEntry>());
            }
            if (!canInterpolate.ContainsKey (vesselID))
            {
                canInterpolate.Add (vesselID, false);
            }

            //Check if we're at 4 updates
            if (interpoUpdates [vesselID].Count == 4) {
                //Remove 0-index Update
                interpoUpdates[vesselID].RemoveAt(0);
                //Free to interpolate points at 4 updates
                canInterpolate[vesselID] = true;
            }

            interpoUpdates [vesselID].Add (entry);
        }

        public static VesselVectorEntry GetVesselInterpoPosition(Guid vesselID, Vector3d positionFudge)
        {
            //Get list of VesselUpdates for coresponding GUID
            List<VesselVectorEntry> vesselUpdateList = interpoUpdates[vesselID];

            //Return value of latest VesselUpdate if we can't interpolate
            if (!canInterpolate [vesselID])
            {
                //Get position from last VesselUpdate
                return vesselUpdateList [vesselUpdateList.Count - 1];
            }


            //Do Interpolation
            double timeSnapshot = Planetarium.GetUniversalTime();
            double mu = (timeSnapshot - vesselUpdateList[1].entryTime) / (vesselUpdateList[2].entryTime - vesselUpdateList[1].entryTime);

            if (mu > 1 || mu < 0)
            {
                //Don't interpolate outside mu range - use positionFudge instead
                VesselVectorEntry latestUpdate = vesselUpdateList [vesselUpdateList.Count - 1];
                CelestialBody updateBody = FlightGlobals.Bodies.Find(b => b.bodyName == latestUpdate.bodyName);

                //Add fudge and return ice cream
                Vector3d updateWithSideOfFudge = updateBody.GetWorldSurfacePosition (latestUpdate.latitude, latestUpdate.longitude, latestUpdate.altitude) + positionFudge;
                VesselVectorEntry deliciousIceCream = new VesselVectorEntry (updateBody.GetLongitude (updateWithSideOfFudge), updateBody.GetLatitude (updateWithSideOfFudge), updateBody.GetAltitude (updateWithSideOfFudge), timeSnapshot, latestUpdate.bodyName);
                return deliciousIceCream;
            }
            return InterpolateVectorEntries (vesselUpdateList [0], vesselUpdateList [1], vesselUpdateList [2], vesselUpdateList [3], mu);
        }

        private static VesselVectorEntry InterpolateVectorEntries(VesselVectorEntry entry1, VesselVectorEntry entry2, VesselVectorEntry entry3, VesselVectorEntry entry4, double mu)
        {
            VesselVectorEntry interpolatedVector;

            double[] interpolatedVectorArray = new double[3];
            interpolatedVectorArray[0] = CubicInterpolate (entry1.longitude, entry2.longitude, entry3.longitude, entry4.longitude, mu);
            interpolatedVectorArray[1] = CubicInterpolate (entry1.latitude, entry2.latitude, entry3.latitude, entry4.latitude, mu);
            interpolatedVectorArray[2] = CubicInterpolate (entry1.altitude, entry2.altitude, entry3.altitude, entry4.altitude, mu);

            interpolatedVector = new VesselVectorEntry(interpolatedVectorArray[0], interpolatedVectorArray[1], interpolatedVectorArray[2], mu, entry2.bodyName);

            return interpolatedVector;
        }

        private static double CubicInterpolate(double s0, double s1, double s2, double s3, double mu)
        {
            double a0,a1,a2,a3,mu2;

            mu2 = mu*mu;
            a0 = -0.5*s0 + 1.5*s1 - 1.5*s2 + 0.5*s3;
            a1 = s0 - 2.5*s1 + 2*s2 - 0.5*s3;
            a2 = -0.5*s0 + 0.5*s2;
            a3 = s1;
            
            return(a0*mu*mu2+a1*mu2+a2*mu+a3);
        }
    }

    public class VesselVectorEntry
    {
        public readonly double latitude;
        public readonly double longitude;
        public readonly double altitude;
        public readonly double entryTime;
        public readonly string bodyName;

        public VesselVectorEntry(double longitude, double latitude, double altitude, double entryTime, string bodyName)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.altitude = altitude;
            this.entryTime = entryTime;
            this.bodyName = bodyName;
        }
    }
}

