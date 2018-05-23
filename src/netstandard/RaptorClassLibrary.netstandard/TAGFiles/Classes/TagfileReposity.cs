﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using VSS.TRex.TAGFiles.Classes.Validator;

namespace VSS.TRex.TAGFiles.Classes
{

    /// <summary>
    /// Archiving will intially write tagfiles toa local folder before another process either internal or external will move the files to a S3 bucket on Amazon
    /// Ideas. Add functions like get all tagfiles for productid
    /// </summary>


    [Serializable]
    public class TagfileMetaData
    {
        [XmlAttribute]
        public Guid projectId;
        public Guid assetId;
        public string tagFileName;
        public string tccOrgId;
        public bool IsJohnDoe;
    }


    public static class TagfileReposity
    {

        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);


        private static string MakePath(TagfileDetail td)
        {
            if (TRexConfig.TagFileArchiveFolder != String.Empty)
                return Path.Combine(TRexConfig.TagFileArchiveFolder, td.projectId.ToString(),td.assetId.ToString());
            else
                return Path.Combine(Path.GetTempPath(), "TRexIgniteData","TagFileArchive",td.projectId.ToString(), td.assetId.ToString());
        }

        /// <summary>
        /// Achives successfully processed tagfiles for reprocessing when required
        /// </summary>
        /// <param name="tagDetail"></param>
        /// <returns></returns>
        public static bool ArchiveTagfile(TagfileDetail tagDetail)
        {
            string thePath = MakePath(tagDetail);
            if (!Directory.Exists(thePath))
                Directory.CreateDirectory(thePath);

            string ArchiveTagfilePath = Path.Combine(thePath, tagDetail.tagFileName);

            // We dont keep dups
            if (File.Exists(ArchiveTagfilePath))
                File.Delete(ArchiveTagfilePath);


            try
            {
                using (FileStream file = new FileStream(ArchiveTagfilePath, FileMode.Create, System.IO.FileAccess.Write))
                {
                    file.Write(tagDetail.tagFileContent, 0, tagDetail.tagFileContent.Length);
                }

                Log.LogDebug($"Tagfile archived to {ArchiveTagfilePath}");


                if (TRexConfig.EnableTagfileArchivingMetaData)
                {
                    string ArchiveTagfileMetaDataPath = Path.ChangeExtension(ArchiveTagfilePath, ".xml");

                    if (File.Exists(ArchiveTagfileMetaDataPath))
                        File.Delete(ArchiveTagfileMetaDataPath);

                    // Tagfile MetaData
                    TagfileMetaData tmd = new TagfileMetaData()
                                          {
                                                  projectId = tagDetail.projectId,
                                                  assetId = tagDetail.assetId,
                                                  tccOrgId = tagDetail.tccOrgId,
                                                  tagFileName = tagDetail.tagFileName,
                                                  IsJohnDoe = tagDetail.IsJohnDoe
                                          };

                    using (FileStream file = new FileStream(ArchiveTagfileMetaDataPath, FileMode.Create,
                            System.IO.FileAccess.Write))
                    {
                        new XmlSerializer(typeof(TagfileMetaData)).Serialize(file, tmd);
                    }

                }

                // Another process should move tagfiles eventually to S3 bucket

                return true;
            }



            catch (System.Exception e)
            {
                Log.LogWarning(String.Format("Exception occured saving tagfile {0}", e.Message));
                return false;
            }
        }

        public static bool MoveToUnableToProcess(TagfileDetail tagDetail)
        {
            // todo Should be moved to a common location. To preserve state I sugest saving all details as a json file which includes the state and binary content. 
            return true;
        }


        public static bool RemoveTagfileArchive(TagfileDetail tagDetail)
        {
            // todo 
            return true;
        }
        /// <summary>
        /// Returns tagfile content and meta data for an archived tagfile. Input requires filename and projectid
        /// </summary>
        /// <param name="tagDetail"></param>
        /// <returns></returns>
        public static TagfileDetail GetTagfile(TagfileDetail tagDetail)
        {
            // just requires the projectid and tagfile name to be set
            string ArchiveTagfilePath = Path.Combine(MakePath(tagDetail), tagDetail.tagFileName);
            string ArchiveTagfileMetaDataPath = Path.ChangeExtension(ArchiveTagfilePath, ".xml");

            if (!File.Exists(ArchiveTagfilePath))
               return tagDetail;

            using (FileStream file = new FileStream(ArchiveTagfilePath, FileMode.Open, FileAccess.Read))
            {
                tagDetail.tagFileContent = new byte[(int)file.Length];
                file.Read(tagDetail.tagFileContent, 0, (int)file.Length);
            }

            // load xml data ArchiveTagfileMetaDataPath and put into tagDetail
            // if using location oly for metadata then you would have to extract it fromn the path

            if (TRexConfig.EnableTagfileArchivingMetaData && File.Exists(ArchiveTagfileMetaDataPath))
                {
                    FileStream ReadFileStream = new FileStream(ArchiveTagfileMetaDataPath, FileMode.Open,FileAccess.Read, FileShare.Read);
                    XmlSerializer SerializerObj = new XmlSerializer(typeof(TagfileMetaData));

                    // Load the object saved above by using the Deserialize function
                    TagfileMetaData tmd = (TagfileMetaData) SerializerObj.Deserialize(ReadFileStream);
                    tagDetail.IsJohnDoe = tmd.IsJohnDoe;
                    tagDetail.projectId = tmd.projectId;
                    tagDetail.assetId = tmd.assetId;
                    tagDetail.tagFileName = tmd.tagFileName;
                    tagDetail.tccOrgId = tmd.tccOrgId;
                    // Cleanup
                    ReadFileStream.Close();
                }

            return tagDetail;
        }

    }
}
