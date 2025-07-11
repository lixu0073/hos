using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DynamoDBv2.DataModel;

namespace Hospital
{
	[DynamoDBTable("HelpRequest")]
	public class HelpRequest
	{
		[DynamoDBHashKey]
		public string UserID;

		[DynamoDBRangeKey]
		public string PlaceID;

		[DynamoDBProperty]
		public string ByWhom;

		[DynamoDBProperty]
		public bool helped = false;

        public PublicSaveModel user;

	}
}
