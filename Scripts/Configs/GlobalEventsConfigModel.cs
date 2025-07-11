using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;

    //[DynamoDBTable("Configs")]
    //public class GlobalEventsConfigModel
    //{
    //    [DynamoDBHashKey]
    //    public string Key { get; set; }

    //    [DynamoDBProperty]
    //    public string MinVersion { get; set; }

    //    [DynamoDBProperty]
    //    public string MaxVersion { get; set; }

    //    [DynamoDBProperty]
    //    public int LenghtInWeeks { get; set; }

    //    [DynamoDBProperty]
    //    public string StartTime { get; set; } //cron format

    //    [DynamoDBProperty]
    //    public List<int> RewardsRanges { get; set; }

    //    [DynamoDBProperty]
    //    public int LeaderboardLength { get; set; }

    //    [DynamoDBProperty(typeof(SingleGlobalEventModelListConverter))]
    //    public List<SingleGlobalEventModel> Events { get; set; }
    //}

    //public class SingleGlobalEventModel
    //{
    //    public string Type { get; set; }
    //    public int EventDuration { get; set; }
    //    public int WeekNumber { get; set; }
    //    public string StartTimeCron { get; set; }
    //    public int MinLevel { get; set; }
    //    public string FirstPlaceReward { get; set;}
    //    public string SecondPlaceReward { get; set;}
    //    public string ThirdPlaceReward { get; set; }
    //    public string LastReward { get; set; }
    //    public string TitleTerm { get; set; }
    //    public string DescriptionTerm { get; set; }
    //    public List<GlobalEventGoal> Goals { get; set; }
    //    public GlobalEventOtherParams OtherParams { get; set; }

    //    public DateTime StartDate { get; set; }
    //    public DateTime EndDate { get; set; }
    //}

    //public class SingleGlobalEventModelListConverter : IPropertyConverter
    //{
    //    private const string TypeAttribute = "Type";
    //    private const string EventDurationAttribute = "EventDuration";
    //    private const string WeekNumberAttribute = "WeekNumber";
    //    private const string StartTimeCronAttribute = "StartTimeCron";
    //    private const string MinLevelAttribute = "MinLevel";
    //    private const string FirstPlaceRewardAttribute = "FirstPlaceReward";
    //    private const string SecondPlaceRewardAttribute = "SecondPlaceReward";
    //    private const string ThirdPlaceRewardAttribute = "ThirdPlaceReward";
    //    private const string LastRewardAttribute = "LastReward";
    //    private const string TitleTermAttribute = "TitleTerm";
    //    private const string DescriptionTermAttribute = "DescriptionTerm";
    //    private const string GoalsAttribute = "Goals";
    //    private const string OtherParamsAttribute = "OtherParams";

    //    public object FromEntry(DynamoDBEntry entry)
    //    {
    //        List<Document> entries = entry.AsListOfDocument();
    //        List<SingleGlobalEventModel> model = new List<SingleGlobalEventModel>();

    //        for (int i = 0; i < entries.Count; ++i)
    //        {
    //            SingleGlobalEventModel singleModel = new SingleGlobalEventModel();
    //            if (entries[i].Keys.Contains(TypeAttribute))
    //            {
    //                singleModel.Type = entries[i][TypeAttribute].AsString();
    //            }
    //            if (entries[i].Keys.Contains(EventDurationAttribute))
    //            {
    //                singleModel.EventDuration = entries[i][EventDurationAttribute].AsInt();
    //            }
    //            if (entries[i].Keys.Contains(WeekNumberAttribute))
    //            {
    //                singleModel.WeekNumber = entries[i][WeekNumberAttribute].AsInt();
    //            }
    //            if (entries[i].Keys.Contains(StartTimeCronAttribute))
    //            {
    //                singleModel.StartTimeCron = entries[i][StartTimeCronAttribute].AsString();
    //            }
    //            if (entries[i].Keys.Contains(MinLevelAttribute))
    //            {
    //                singleModel.MinLevel = entries[i][MinLevelAttribute].AsInt();
    //            }
    //            if (entries[i].Keys.Contains(MinLevelAttribute))
    //            {
    //                singleModel.MinLevel = entries[i][MinLevelAttribute].AsInt();
    //            }
    //            if (entries[i].Keys.Contains(FirstPlaceRewardAttribute))
    //            {
    //                singleModel.FirstPlaceReward = entries[i][FirstPlaceRewardAttribute].AsString();
    //            }
    //            if (entries[i].Keys.Contains(SecondPlaceRewardAttribute))
    //            {
    //                singleModel.SecondPlaceReward = entries[i][SecondPlaceRewardAttribute].AsString();
    //            }
    //            if (entries[i].Keys.Contains(ThirdPlaceRewardAttribute))
    //            {
    //                singleModel.ThirdPlaceReward = entries[i][ThirdPlaceRewardAttribute].AsString();
    //            }
    //            if (entries[i].Keys.Contains(LastRewardAttribute))
    //            {
    //                singleModel.LastReward = entries[i][LastRewardAttribute].AsString();
    //            }
    //            if (entries[i].Keys.Contains(TitleTermAttribute))
    //            {
    //                singleModel.TitleTerm = entries[i][TitleTermAttribute].AsString();
    //            }
    //            if (entries[i].Keys.Contains(DescriptionTermAttribute))
    //            {
    //                singleModel.DescriptionTerm = entries[i][DescriptionTermAttribute].AsString();
    //            }
    //            if (entries[i].Keys.Contains(GoalsAttribute))
    //            {
    //                GlobalEventGoalModelListConverter converter = new GlobalEventGoalModelListConverter();

    //                singleModel.Goals = converter.FromEntry(entries[i][GoalsAttribute]) as List<GlobalEventGoal>;

    //                converter = null;
    //            }
    //            if (entries[i].Keys.Contains(OtherParamsAttribute))
    //            {
    //                GlobalEventOtherParamsConverter converter = new GlobalEventOtherParamsConverter();
    //                singleModel.OtherParams = converter.FromEntry(entries[i][OtherParamsAttribute]) as GlobalEventOtherParams;

    //                converter = null;
    //            }

    //            model.Add(singleModel);
    //        }

    //        return model;
    //    }

    //    public DynamoDBEntry ToEntry(object value)
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}

    //public class GlobalEventGoal
    //{
    //    public int Goal { get; set; }
    //    public string Reward { get; set; }
    //    public string[] ContributionRewards { get; set; }
    //}

    //public class GlobalEventGoalModelListConverter : IPropertyConverter
    //{
    //    private const string GoalAttribute = "Goal";
    //    private const string RewardAttribute = "Reward";
    //    private const string ContributionRewardsAttribute = "ContributionRewards";

    //public object FromEntry(DynamoDBEntry entry)
    //{
    //    List<Document> entries = entry.AsListOfDocument();
    //    List<GlobalEventGoal> model = new List<GlobalEventGoal>();

    //    for (int i = 0; i < entries.Count; ++i)
    //    {
    //        GlobalEventGoal singleModel = new GlobalEventGoal();

    //        if (entries[i].Keys.Contains(GoalAttribute))
    //        {
    //            singleModel.Goal = entries[i][GoalAttribute].AsInt();
    //        }
    //        if (entries[i].Keys.Contains(RewardAttribute))
    //        {
    //            singleModel.Reward = entries[i][RewardAttribute].AsString();
    //        }
    //        if (entries[i].Keys.Contains(ContributionRewardsAttribute))
    //        {
    //            singleModel.ContributionRewards = entries[i][ContributionRewardsAttribute].AsArrayOfString();
    //        }
    //        model.Add(singleModel);
    //    }

    //    return model;
    //}

    //public DynamoDBEntry ToEntry(object value)
    //{
    //    throw new System.NotImplementedException();
    //}
    //}

    //public class GlobalEventOtherParams
    //{
    //    public int MaxObjectsOnMap { get; set; }
    //    public int SpawnInterval { get; set; }
    //    public string MainActivitySprite { get; set; }
    //    public string MapSprite { get; set; }
    //    public string ParticleSprite { get; set; }
    //    public string IconActivitySprite { get; set; }
    //    public string Medicine { get; set; }
    //    public string RotatableTag { get; set; }
    //}

    //public class GlobalEventOtherParamsConverter : IPropertyConverter
    //{
    //    private const string MaxObjectsOnMapAttribute = "MaxObjectsOnMap";
    //    private const string SpawnIntervalAttribute = "SpawnInterval";
    //    private const string MainActivitySpriteAttribute = "MainActivitySprite";
    //    private const string MapSpriteAttribute = "MapSprite";
    //    private const string ParticleSpriteAttribute = "ParticleSprite";
    //    private const string IconActivitySpriteAttribute = "IconActivitySprite";
    //    private const string MedicineAttribute = "Medicine";
    //    private const string RotatableTagAttribute = "RotatableTag";

    //    public object FromEntry(DynamoDBEntry entry)
    //    {
    //        Document entries = entry.AsDocument();
    //        GlobalEventOtherParams model = new GlobalEventOtherParams();

    //        if (entries.Keys.Contains(MaxObjectsOnMapAttribute))
    //        {
    //            model.MaxObjectsOnMap = entries[MaxObjectsOnMapAttribute].AsInt();
    //        }
    //        if (entries.Keys.Contains(SpawnIntervalAttribute))
    //        {
    //            model.SpawnInterval = entries[SpawnIntervalAttribute].AsInt();
    //        }
    //        if (entries.Keys.Contains(MainActivitySpriteAttribute))
    //        {
    //            model.MainActivitySprite = entries[MainActivitySpriteAttribute].AsString();
    //        }
    //        if (entries.Keys.Contains(MapSpriteAttribute))
    //        {
    //            model.MapSprite = entries[MapSpriteAttribute].AsString();
    //        }
    //        if (entries.Keys.Contains(ParticleSpriteAttribute))
    //        {
    //            model.ParticleSprite = entries[ParticleSpriteAttribute].AsString();
    //        }
    //        if (entries.Keys.Contains(ParticleSpriteAttribute))
    //        {
    //            model.ParticleSprite = entries[ParticleSpriteAttribute].AsString();
    //        }
    //        if (entries.Keys.Contains(IconActivitySpriteAttribute))
    //        {
    //            model.IconActivitySprite = entries[IconActivitySpriteAttribute].AsString();
    //        }
    //        if (entries.Keys.Contains(MedicineAttribute))
    //        {
    //            model.Medicine = entries[MedicineAttribute].AsString();
    //        }
    //        if (entries.Keys.Contains(RotatableTagAttribute))
    //        {
    //            model.RotatableTag = entries[RotatableTagAttribute].AsString();
    //        }

    //        return model;
    //    }

    //    public DynamoDBEntry ToEntry(object value)
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}
