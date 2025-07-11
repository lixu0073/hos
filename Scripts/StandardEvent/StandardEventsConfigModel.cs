using System;
using System.Collections.Generic;
using UnityEngine;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Hospital
{
    //[DynamoDBTable("Configs")]
    //public class StandardEventsConfigModel
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

    //    [DynamoDBProperty(typeof(SingleStandardEventModelListConverter))]
    //    public List<SingleStandardEventModel> Events { get; set; }


    //}

    //public class SingleStandardEventModel
    //{
    //    public string EventUniqueID { get; set; }
    //    public int EventDuration { get; set; }
    //    public int WeekNumber { get; set; }
    //    public string StartTimeCron { get; set; }
    //    public string ArtDecPoint { get; set; }
    //    public List<EventEffectModel> Effects { get; set; }


    //    public DateTime StartDate { get; set; }
    //    public DateTime EndDate { get; set; }
    //}

    //public class EventEffectModel
    //{
    //    public int MinLevel { get; set; }
    //    public string Type { get; set; }
    //    public string Parameter { get; set; }
    //}

    //public class SingleStandardEventModelListConverter : IPropertyConverter
    //{
    //    private const string EventUniqueIDAttribute = "EventUniqueID";
    //    private const string EventDurationAttribute = "EventDuration";
    //    private const string ArtDecPointAttribute = "ArtDecPoint";
    //    private const string StartTimeCronAttribute = "StartTimeCron";
    //    private const string WeekNumberAttribute = "WeekNumber";
    //    private const string EffectsAttribute = "Effects";

    //    public object FromEntry(DynamoDBEntry entry)
    //    {
    //        List<Document> entries = entry.AsListOfDocument();
    //        List<SingleStandardEventModel> model = new List<SingleStandardEventModel>();
            
    //        for (int i = 0; i < entries.Count; ++i)
    //        {
    //            SingleStandardEventModel singleModel = new SingleStandardEventModel();
    //            if (entries[i].Keys.Contains(EventUniqueIDAttribute))
    //            {
    //                singleModel.EventUniqueID = entries[i][EventUniqueIDAttribute].AsString();
    //            }
    //            if (entries[i].Keys.Contains(EventDurationAttribute))
    //            {
    //                singleModel.EventDuration = entries[i][EventDurationAttribute].AsInt();
    //            }
    //            if (entries[i].Keys.Contains(ArtDecPointAttribute))
    //            {
    //                singleModel.ArtDecPoint = entries[i][ArtDecPointAttribute].AsString();
    //            }
    //            if (entries[i].Keys.Contains(StartTimeCronAttribute))
    //            {
    //                singleModel.StartTimeCron = entries[i][StartTimeCronAttribute].AsString();
    //            }
    //            if (entries[i].Keys.Contains(WeekNumberAttribute))
    //            {
    //                singleModel.WeekNumber = entries[i][WeekNumberAttribute].AsInt();
    //            }
    //            if (entries[i].Keys.Contains(EffectsAttribute))
    //            {
    //                singleModel.Effects = new List<EventEffectModel>();

    //                EventEffectModelListConverter converter = new EventEffectModelListConverter();

    //                singleModel.Effects = converter.FromEntry(entries[i][EffectsAttribute]) as List<EventEffectModel>;

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

    //public class EventEffectModelListConverter : IPropertyConverter
    //{
    //    private const string MinLevelAttribute = "MinLevel";
    //    private const string ParameterAttribute = "Parameter";
    //    private const string TypeAttribute = "Type";

    //    public object FromEntry(DynamoDBEntry entry)
    //    {
    //        List<Document> entries = entry.AsListOfDocument();
    //        List<EventEffectModel> model = new List<EventEffectModel>();

    //        for (int i = 0; i < entries.Count; ++i)
    //        {
    //            EventEffectModel singleModel = new EventEffectModel();

    //            if (entries[i].Keys.Contains(MinLevelAttribute))
    //            {
    //                singleModel.MinLevel = entries[i][MinLevelAttribute].AsInt();
    //            }
    //            if (entries[i].Keys.Contains(ParameterAttribute))
    //            {
    //                singleModel.Parameter = entries[i][ParameterAttribute].AsString();
    //            }
    //            if (entries[i].Keys.Contains(TypeAttribute))
    //            {
    //                singleModel.Type = entries[i][TypeAttribute].AsString();
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
}
