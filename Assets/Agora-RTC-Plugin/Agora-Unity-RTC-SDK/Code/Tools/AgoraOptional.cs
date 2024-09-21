#define AGORA_RTC


using System;
using System.Collections;
using System.Collections.Specialized;

#if AGORA_RTC
using Agora.Rtc.LitJson;
#elif AGORA_RTM
using Agora.Rtm.LitJson;
#endif

#if AGORA_RTC
namespace Agora.Rtc
#elif AGORA_RTM
namespace Agora.Rtm
#endif
{
    public class Optional<T>
    {
        private T value;
        private bool hasValue;
        private CLIENT_ROLE_TYPE cLIENT_ROLE_BROADCASTER;
        private CHANNEL_PROFILE_TYPE cHANNEL_PROFILE_COMMUNICATION;

        public bool Value { get; set; }


        public Optional(bool v)
        {
            hasValue = false;
        }

        public Optional(CLIENT_ROLE_TYPE cLIENT_ROLE_BROADCASTER)
        {
            this.cLIENT_ROLE_BROADCASTER = cLIENT_ROLE_BROADCASTER;
        }

        public Optional(CHANNEL_PROFILE_TYPE cHANNEL_PROFILE_COMMUNICATION)
        {
            this.cHANNEL_PROFILE_COMMUNICATION = cHANNEL_PROFILE_COMMUNICATION;
        }

        public Optional()
        {
        }


        public bool HasValue()
        {
            return hasValue;
        }

        public T GetValue()
        {
            return this.value;
        }

        public void SetValue(T val)
        {
            this.hasValue = true;
            this.value = val;
        }

        public void SetEmpty()
        {
            this.hasValue = false;
        }

        public static implicit operator Optional<T>(bool v)
        {
            throw new NotImplementedException();
        }
    }

    public interface IOptionalJsonParse
    {
        void ToJson(JsonWriter writer);
    }
}
