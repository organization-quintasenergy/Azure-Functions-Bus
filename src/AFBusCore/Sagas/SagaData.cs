﻿using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    /// <summary>
    /// Base class defining the saga persistence.
    /// </summary>
    public class SagaData : TableEntity
    {
        public bool IsDeleted { get; set; }

        public DateTime? CreationTimeStamp { get; set; }

        public DateTime? FinishingTimeStamp { get; set; }

        [IgnoreProperty]
        public string LockID { get; set; }

        /// <summary>
        /// Prefix for indexing into the blob storage.
        /// 
        /// = PartitionKey + RowKey
        /// </summary>
        public string Prefix
        {
            get
            {
                try
                {
                    return this.PartitionKey + this.RowKey;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
    }
}
