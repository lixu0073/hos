using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DynamoDBv2.DataModel;

namespace Hospital
{
	[DynamoDBTable("PackageHelpRequest")]
	public class PackageHelpRequest : CacheManager.IGetPublicSave
	{
		[DynamoDBHashKey]
		public string UserID;

		[DynamoDBProperty]
		public string BoxID;

		[DynamoDBProperty]
		public string ByWhom;

		[DynamoDBProperty]
		public bool helped = false;

        public string GetSaveID()
        {
            return ByWhom;
        }
    }
}
