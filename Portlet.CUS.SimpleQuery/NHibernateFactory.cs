using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jenzabar.Common.Configuration;
using Jenzabar.Portal.Framework.NHibernateFWK;
using NHibernate;
using NHibernate.Cfg;
using System.IO;
using System.Xml;


namespace CUS.ICS.SimpleQuery
{
    public class NHibernateFactory : ICustomSessionFactory
    {
        public ISessionFactory GetSessionFactory()
        {
            Configuration cfg = new Configuration().Configure(XmlReader.Create(new StringReader("<hibernate-configuration xmlns=\"urn:nhibernate-configuration-2.2\"><session-factory></session-factory></hibernate-configuration>")));
            this.SetNHibernateConfigProperties(cfg);
            return cfg.BuildSessionFactory();
        }

        private void SetNHibernateConfigProperties(Configuration _cfg)
        {
            Dictionary<string, string> newProperties = new Dictionary<string, string>();
            newProperties.Add("connection.connection_string", ConfigSettings.Current.DatabaseConnectionInfo.ConnectionString);
            newProperties.Add("dialect", "NHibernate.Dialect.MsSql2005Dialect");
            newProperties.Add("connection.isolation", "ReadUncommitted");
            newProperties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
            newProperties.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");
            newProperties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.LinFu.ProxyFactoryFactory, NHibernate.ByteCode.LinFu");
            _cfg.SetProperties(newProperties);
            _cfg.AddAssembly("Portlet.SimpleQuery");
        }
    }
}