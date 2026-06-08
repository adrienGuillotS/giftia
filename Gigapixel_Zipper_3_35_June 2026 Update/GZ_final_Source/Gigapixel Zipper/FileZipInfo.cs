using System;

namespace Gigapixel_Zipper
{
  internal class FileZipInfo
  {
    public string   Id                   { get; set; }
    public string   FullPath             { get; set; }
    public string   Name                 { get; set; }
    public string   NameWithoutExtension { get; set; }
    public string   PathWithoutRoot      { get; set; }

    // Captured before any processing touches the file
    public DateTime OriginalLastWriteTime { get; set; }
    public DateTime OriginalCreationTime  { get; set; }

    // Computed after sorting — unique timestamp that preserves date/time
    // for files with distinct minutes, and adds 1-second offsets for
    // files that share the same minute (same-batch downloads).
    public DateTime FinalTimestamp        { get; set; }
  }
}
