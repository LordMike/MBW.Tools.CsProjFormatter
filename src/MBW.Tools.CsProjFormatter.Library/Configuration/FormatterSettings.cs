using System;

namespace MBW.Tools.CsProjFormatter.Library.Configuration
{
    public class FormatterSettings
    {
        public string NewlineCharacter { get; set; } = Environment.NewLine;

        public IndentStyle IndentStyle { get; set; } = IndentStyle.Space;

        public int IndentCount { get; set; } = 2;

        /// <summary>
        /// Note: Only applicable for formatting of files
        /// </summary>
        public bool InsertFinalNewline { get; set; } = true;

        public bool SplitTopLevelElements { get; set; } = true;

        public bool SortPackageProjectReferences { get; set; } = true;

        public bool PreferPackageReferenceAttributes { get; set; } = true;

        /// <summary>
        /// Note: Only applicable for formatting of files
        /// </summary>
        public string Encoding { get; set; } = "utf-8";
    }
}