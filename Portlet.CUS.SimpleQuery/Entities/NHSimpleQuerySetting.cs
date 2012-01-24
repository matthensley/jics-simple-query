using System;
using Jenzabar.Portal.Framework.NHibernateFWK;

namespace CUS.ICS.SimpleQuery.Entities
{
    public class NHSimpleQuerySetting : IPersistentObject
    {
        public virtual Guid ID { get; set; }
        public virtual Guid PortletID { get; set; }
        public virtual String Name { get; set; }
        public virtual String Value { get; set; }

        public bool BoolValue { get { return Convert.ToBoolean(this.Value); } }
    }
}