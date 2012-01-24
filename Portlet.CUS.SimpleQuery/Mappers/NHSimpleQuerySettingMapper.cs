using System;
using System.Collections.Generic;
using System.Linq;
using Jenzabar.Portal.Framework.NHibernateFWK;
using CUS.ICS.SimpleQuery.Entities;

namespace CUS.ICS.SimpleQuery.Mappers
{
    public class NHSimpleQuerySettingsMapper : JICSBaseFacade<NHSimpleQuerySetting>
    {
        public IEnumerable<NHSimpleQuerySetting> GetSettings(Guid portletID)
        {
            return this.GetList(x => x.PortletID == portletID);
        }

        public void DeleteSettings(Guid portletId)
        {
            foreach (var setting in GetSettings(portletId))
            {
                Delete(setting);
            }
        }


        public NHSimpleQuerySetting AddSetting(Guid portletID, String name, String value)
        {
            var sqs = new NHSimpleQuerySetting {PortletID = portletID, Name = name, Value = value};
            this.Save(sqs);
            return sqs;
        }

        public void UpdateSetting(NHSimpleQuerySetting sqs, string value)
        {
            sqs.Value = value;
            this.Save(sqs);
        }

        public void UpdateSetting(NHSimpleQuerySetting sqs, bool value)
        {
            sqs.Value = value.ToString();
            this.Save(sqs);
        }
    }
    public class SettingsHelper
    {
        private List<NHSimpleQuerySetting> _settings;
        private Guid _portletId;
        private NHSimpleQuerySettingsMapper _mapper;

        public SettingsHelper(List<NHSimpleQuerySetting> settings, Guid portletId, NHSimpleQuerySettingsMapper mapper)
        {
            this._settings = settings;
            this._portletId = portletId;
            this._mapper = mapper;
        }

    public NHSimpleQuerySetting GetSetting(string settingName, string defaultVale = "")
        {
            var setting = (from x in _settings where x.Name == settingName select x).SingleOrDefault();
            if (setting == null)
            {
                setting = _mapper.AddSetting(_portletId, settingName, defaultVale);
                _settings.Add(setting);
            }
            return setting;
        }

        public NHSimpleQuerySetting GetSetting(string settingName, bool defaultValue)
        {
            return GetSetting(settingName, defaultValue.ToString());
        }

        public NHSimpleQuerySetting GetSetting(string settingName, int defaultValue)
        {
            return GetSetting(settingName, defaultValue.ToString());
        }
    }
}