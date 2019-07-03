using System;
using System.ComponentModel;
using System.Resources;
using TfsAPI.Properties;

namespace TfsAPI.Attributes
{
    /// <summary>
    ///     Локализуемое описание
    /// </summary>
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _resourceKey;
        private readonly ResourceManager _resourceManager;

        public LocalizedDescriptionAttribute(string resourceKey, Type resourceType = null)
        {
            if (resourceType == null) resourceType = typeof(Resource);

            _resourceManager = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                var description = _resourceManager.GetString(_resourceKey);
                return string.IsNullOrWhiteSpace(description) ? string.Format("[[{0}]]", _resourceKey) : description;
            }
        }
    }
}