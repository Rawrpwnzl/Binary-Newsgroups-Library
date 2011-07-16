using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;

namespace NzbComet
{

    public class NzbParser
    {
        private string _nzbFilename;

        public NzbParser(string nzbFilename)
        {
            ArgumentChecker.ThrowIfNullOrWhitespace("nzbFilename", nzbFilename);

            _nzbFilename = nzbFilename;
        }

        public NzbDownload Parse()
        {
            var newDownload = new NzbDownload(_nzbFilename);

            var nzbFileXmlNodes = this.GetXmlNodesFromNzbFile(_nzbFilename);

            foreach (XmlNode currentNzbFileXmlNode in nzbFileXmlNodes.Where(node => node.Name.Equals("file", StringComparison.OrdinalIgnoreCase)))
            {
                NzbPart newPart = ParsePart(currentNzbFileXmlNode);

                newPart.Parent = newDownload;

                newDownload.Add(newPart);
            }

            return newDownload;
        }

        private NzbPart ParsePart(XmlNode currentNzbFileXmlNode)
        {
            NzbPart newPart = null;
            XmlAttributeCollection attributes = currentNzbFileXmlNode.Attributes;

            string subject = this.GetSubject(attributes);
            string poster = this.GetPoster(attributes);
            string date = this.GetDate(attributes);
            var groups = this.GetGroups(currentNzbFileXmlNode);

            newPart = new NzbPart();

            XmlNodeList segments = currentNzbFileXmlNode["segments"].GetElementsByTagName("segment");

            foreach (XmlNode currentSegment in segments)
            {
                var newSegment = ParseSegment(groups, currentSegment);

                newPart.Add(newSegment);

                newSegment.Parent = newPart;
            }

            return newPart;
        }

        private NzbSegment ParseSegment(List<string> groups, XmlNode currentSegment)
        {
            long totalBytesOfCurrentSegment = this.GetBytes(currentSegment.Attributes);
            string currentSegmentFile = currentSegment.InnerText;

            return new NzbSegment(totalBytesOfCurrentSegment, currentSegmentFile, groups);
        }

        private List<XmlNode> ConvertXmlNodeListToGenericList(XmlNodeList nodeList)
        {
            List<XmlNode> convertedGenericList = new List<XmlNode>();

            foreach (XmlNode node in nodeList)
            {
                convertedGenericList.Add(node);
            }

            return convertedGenericList;
        }

        private List<XmlNode> GetXmlNodesFromNzbFile(string nzbLocation)
        {
            XmlDocument document = new XmlDocument
            {
                XmlResolver = null,
                PreserveWhitespace = false
            };

            document.Load(nzbLocation);

            XmlNodeList xmlNodes = document.DocumentElement.SelectNodes("*");

            return ConvertXmlNodeListToGenericList(xmlNodes);
        }

        private string GetSubject(XmlAttributeCollection attributes)
        {
            var subjectAttribute = attributes["subject"];

            if (subjectAttribute == null)
            {
                return "Unknown_Subject_" + Guid.NewGuid().ToString();
            }

            return subjectAttribute.InnerText;
        }

        private string GetPoster(XmlAttributeCollection attributes)
        {
            var posterAttribute = attributes["poster"];

            if (posterAttribute == null)
            {
                return "Unknown_Poster_" + Guid.NewGuid().ToString();
            }

            return posterAttribute.InnerText;
        }

        private string GetDate(XmlAttributeCollection attributes)
        {
            var dateAttribute = attributes["date"];

            if (dateAttribute == null)
            {
                return DateTime.MinValue.ToString();
            }

            return dateAttribute.InnerText;
        }

        private long GetBytes(XmlAttributeCollection attributes)
        {
            var bytesAttribute = attributes["bytes"];
            long bytes = 0L;

            if (bytesAttribute == null)
            {
                return 0L;
            }

            if (long.TryParse(bytesAttribute.InnerText, out bytes))
            {
                return bytes;
            }

            return 0L;
        }

        private List<string> GetGroups(XmlNode currentNzbFileXmlNode)
        {
            List<string> allGroups = new List<string>();

            XmlNodeList groups = currentNzbFileXmlNode["groups"].GetElementsByTagName("group");

            foreach (XmlNode currentGroup in groups)
            {
                allGroups.Add(currentGroup.InnerText);
            }

            return allGroups;
        }

        private string ExtractFilenameFromSubject(string subject)
        {
            StringBuilder buffer = new StringBuilder();
            bool currentCharBelongsToFilename = false;

            foreach (char currentCharInSubject in subject)
            {
                if (!currentCharBelongsToFilename && currentCharInSubject == '"')
                {
                    currentCharBelongsToFilename = true;
                    continue;
                }

                if (currentCharBelongsToFilename)
                {
                    if (currentCharInSubject == '"')
                    {
                        return buffer.ToString();
                    }

                    buffer.Append(currentCharInSubject);
                }


            }

            return null;
        }
    }


}
