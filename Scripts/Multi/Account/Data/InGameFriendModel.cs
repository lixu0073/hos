using UnityEngine;
using System.Collections;
using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System;

namespace Hospital
{
    [DynamoDBTable("Friends")]
    public class InGameFriendModel
    {
        private const double EXPIRE_DAYS = 3;
        private const double TTL_DAY = 86400;
        private const double UNLIMITED_TIME = 365 * 19 * TTL_DAY;

        [DynamoDBHashKey]
        public string MeSaveID;

        [DynamoDBRangeKey]
        public string SaveID;

        [DynamoDBProperty]
        public bool Accepted = false;

        [DynamoDBProperty]
        public string InvitingSaveID;

        [DynamoDBProperty]
        public int InvitationTimestamp;

        [DynamoDBProperty]
        public int RemovingTime;

        public string Pretty()
        {
            return "MeSaveID: " + MeSaveID + ", SaveID: " + SaveID + ", Accepted: " + Accepted + ", InvitingSaveID: " + InvitingSaveID;
        }
        
        public bool IsFriend()
        {
            return Accepted;
        }

        public bool CanIDelete()
        {
            return Accepted;
        }

        public bool CanIAccept()
        {
            return !Accepted && !AmIInviting();
        }

        public bool CanIRevoke()
        {
            return !Accepted && AmIInviting();
        }

        public bool CanIReject()
        {
            return !Accepted && !AmIInviting();
        }

        public bool AmIInviting()
        {
            return MeSaveID == InvitingSaveID;
        }

        public static InGameFriendModel CreateRevesedModel(InGameFriendModel model)
        {
            return new InGameFriendModel()
            {
                MeSaveID = model.SaveID,
                SaveID = model.MeSaveID,
                Accepted = model.Accepted,
                InvitingSaveID = model.InvitingSaveID,
                InvitationTimestamp = model.InvitationTimestamp,
                RemovingTime = model.RemovingTime
            };
        }

        public void RemoveExpireTime()
        {
            RemovingTime = RemovingTime + Convert.ToInt32(UNLIMITED_TIME);
        }

        public static InGameFriendModel CreateInvite(string InvitedSaveID)
        {
            return new InGameFriendModel()
            {
                MeSaveID = CognitoEntry.SaveID,
                SaveID = InvitedSaveID,
                Accepted = false,
                InvitingSaveID = CognitoEntry.SaveID,
                InvitationTimestamp = Convert.ToInt32(ServerTime.getTime()),
                RemovingTime = Convert.ToInt32(ServerTime.getTime() + DefaultConfigurationProvider.GetConfigCData().SocialRequestTimeoutEPOCH)
            };
        }
    }
}