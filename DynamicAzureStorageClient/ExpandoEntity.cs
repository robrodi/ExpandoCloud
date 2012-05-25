using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Linq;

namespace DynamicAzureStorageClient
{
    public class Expando : DynamicObject, IDynamicMetaObjectProvider
    {
        private readonly object _instance;
        private Type _instanceType;
        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

        public Expando()
        {
            _instance = this;
            _instanceType = GetType();
        }
        public Expando(object instance)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            _instance = instance;
            _instanceType = instance.GetType();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {

            // Check properties.
            if (_properties.ContainsKey(binder.Name))
            {
                result = _properties[binder.Name];
                return true;
            }

            // Check actual properties
            var property = GetProperty(binder.Name);
            if (property != null && _instance != null)
            {
                result = property.GetValue(_instance, null);
                return true;
            }

            result = null;
            return false;
        }

        private PropertyInfo GetProperty(string name)
        {
            return _instanceType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var property = GetProperty(binder.Name);
            if (property != null)
            {
                property.SetValue(_instance, value, null);
                return true;
            }

            return false;
        }

        public object this[string key]
        {
            get
            {
                if (_properties.ContainsKey(key))
                    return _properties[key];

                var property = GetProperty(key);
                if (property != null)
                    return property.GetValue(_instance, null);
                return null;
            }
            set
            {
                if (_properties.ContainsKey(key))
                {
                    _properties[key] = value;
                    return;
                }
                var property = GetProperty(key);
                if (property != null)
                    property.SetValue(_instance, value, null);
            }
        }
    }
    // Idea from http://www.west-wind.com/weblog/posts/2012/Feb/08/Creating-a-dynamic-extensible-C-Expando-Object
    public class ExpandoEntity : Expando
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

    }
}