using System.Collections.Generic;
using UnityEngine;

namespace NeuroQuest.Core
{
    [CreateAssetMenu(menuName = "NeuroQuest/Assessment Profile")]
    public class AssessmentProfile : ScriptableObject
    {
        [Header("Profile Info")]
        [SerializeField] private string profileId;
        [SerializeField] private string displayName;

        [Header("Enabled Domains")]
        [SerializeField] private List<AssessmentDomain> enabledDomains = new();

        public string ProfileId => profileId;
        public string DisplayName => displayName;
        public IReadOnlyList<AssessmentDomain> EnabledDomains => enabledDomains;

        public bool IsDomainEnabled(AssessmentDomain domain)
        {
            return enabledDomains.Contains(domain);
        }

        public bool SupportsAnyDomain(IReadOnlyList<AssessmentDomain> domains)
        {
            if (domains == null || domains.Count == 0)
            {
                return false;
            }

            foreach (AssessmentDomain domain in domains)
            {
                if (IsDomainEnabled(domain))
                {
                    return true;
                }
            }

            return false;
        }
    }
}