﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Illumina.BaseSpace.SDK.Types
{
    [DataContract]
    public class SearchResult
    {
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public ProjectCompact Project { get; set; }

        [DataMember]
        public AppResultCompact AppResult { get; set; }

        [DataMember]
        public SampleCompact Sample { get; set; }

        [DataMember]
        public FileCompact SampleFile { get; set; }

        [DataMember]
        public ApplicationCompact Application { get; set; }

        public override string ToString()
        {
            if (Project != null)
            {
                return Project.ToString();
            }
            if (AppResult != null)
            {
                return AppResult.ToString();
            }
            if (Sample != null)
            {
                return Sample.ToString();
            }
            if (SampleFile != null)
            {
                return SampleFile.ToString();
            }
            if (Application != null)
            {
                return Application.ToString();
            }
            return base.ToString();
        }
    }

    public enum SearchResultSortByParameters { Id, Name, DateCreated }
}