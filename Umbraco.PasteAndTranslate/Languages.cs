using System;
using System.Collections.Generic;
using System.Linq;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.PasteAndTranslate
{
    static internal class Languages
    {
        public static IEnumerable<NodeLanguageDto> FirstLanguageSet(IContent content)
        {
            return FirstLanguageSet(GetPath(content));
        }

        public static IEnumerable<NodeLanguageDto> FirstLanguageSet(IEnumerable<int> path)
        {
            return path
                .Reverse()
                .SelectMany(GetDomains)
                .Select(NodeLanguage)
                .Distinct()
                .GroupBy(RootNodeId)
                .FirstOrDefault(HasLanguages);
        }

        public static IEnumerable<int> GetPath(IContent content)
        {
            return GetPath(content.Path);
        }

        public static IEnumerable<int> GetPath(string path)
        {
            return path
                .Split(',')
                .Select(s => Convert.ToInt32(s))
                .Skip(1);
        }

        public static IEnumerable<Domain> GetDomains(int id)
        {
            return Domain.GetDomains(true).Where(d => d.RootNodeId == id);
        }

        private static NodeLanguageDto NodeLanguage(Domain d)
        {
            return new NodeLanguageDto(d);
        }

        private static int RootNodeId(NodeLanguageDto l)
        {
            return l.RootNodeId;
        }

        private static bool HasLanguages(IGrouping<int, NodeLanguageDto> g)
        {
            return g.Any();
        }
    }

    public class NodeLanguageDto : IEquatable<NodeLanguageDto>
    {
        public int RootNodeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public NodeLanguageDto()
        {
        }

        public NodeLanguageDto(Domain domain)
        {
            RootNodeId = domain.RootNodeId;
            Code = domain.Language.CultureAlias;
            Name = domain.Language.FriendlyName;
        }

        public bool IsEmpty()
        {
            return String.IsNullOrWhiteSpace(Code);
        }

        public bool Equals(NodeLanguageDto other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RootNodeId == other.RootNodeId && string.Equals(Code, other.Code) && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NodeLanguageDto) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = RootNodeId;
                hashCode = (hashCode*397) ^ (Code != null ? Code.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(NodeLanguageDto left, NodeLanguageDto right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NodeLanguageDto left, NodeLanguageDto right)
        {
            return !Equals(left, right);
        }
    }
}