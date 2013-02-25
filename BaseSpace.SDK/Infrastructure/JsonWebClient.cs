﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Util;
using Common.Logging;
using Illumina.BaseSpace.SDK.ServiceModels;
using Illumina.BaseSpace.SDK.Types;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceModel.Serialization;
using ServiceStack.Text;

namespace Illumina.BaseSpace.SDK
{
    public class JsonWebClient : IWebClient
    {
		private readonly JsonServiceClient client;

	    private readonly ILog logger;

		private readonly IClientSettings settings;

        public IRequestOptions DefaultRequestOptions { get; private set; }

        private static JsonSerializer<Notification<Agreement>> agreementSerializer = new JsonSerializer<Notification<Agreement>>();

        private static JsonSerializer<Notification<ScheduledDowntime>> scheduledSerializer = new JsonSerializer<Notification<ScheduledDowntime>>();

        static JsonWebClient()
        {
			// setting this just to make sure it's not set in Linux
			JsonDataContractDeserializer.Instance.UseBcl = false;
			// BaseSpace uses this format for DateTime
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			JsConfig<Uri>.DeSerializeFn = s => new Uri(s, s.StartsWith("http") ? UriKind.Absolute : UriKind.Relative);
			//handle complex parsing of references
			JsConfig<IContentReference<IAbstractResource>>.RawDeserializeFn = ResourceDeserializer;

			JsConfig<INotification<object>>.RawDeserializeFn = NotificationDeserializer;
        }

        public JsonWebClient(IClientSettings settings, IRequestOptions defaultOptions = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

			logger = LogManager.GetCurrentClassLogger();
            this.settings = settings;

            // call something on this object so it gets initialized in single threaded context
            HttpEncoder.Default.SerializeToString();
            HttpEncoder.Current.SerializeToString();
            if (defaultOptions == null)
            {
                defaultOptions = new RequestOptions();
            }
           
            SetDefaultRequestOptions(defaultOptions);

			client = new JsonServiceClient(settings.BaseSpaceApiUrl);
			client.LocalHttpWebRequestFilter += WebRequestFilter;
        }

		private void WebRequestFilter(HttpWebRequest req)
		{
			settings.Authentication.UpdateHttpHeader(req);
		}

		public void SetDefaultRequestOptions(IRequestOptions options)
		{
			DefaultRequestOptions = options;
		}

		public TReturn Send<TReturn>(AbstractRequest<TReturn> request, IRequestOptions options = null)
			where TReturn : class
		{
			try
			{
				TReturn result = null;
				RetryLogic.DoWithRetry(DefaultRequestOptions.RetryAttempts, request.GetName(), () => result = request.GetFunc(client, options)(), logger);
				return result;
			}
			catch (Exception wex)
			{
				throw new BaseSpaceException(request.GetName() + " failed", wex);
			}
		}

        private static INotification<object> NotificationDeserializer(string source)
        {
            //determine type, then use appropriate deserializer
            var asValues = JsonSerializer.DeserializeFromString<Dictionary<string, string>>(source);
            string type = asValues["Type"];

            object o = null;

            switch (type.ToLower())
            {
                case "agreement":
                    o = agreementSerializer.DeserializeFromString(source).Item;
                    break;
                case "scheduleddowntime":
                    o = scheduledSerializer.DeserializeFromString(source).Item;
                    break;
            }
            return new Notification<object> { Item = o, Type = type };
        }

        internal static IContentReference<IAbstractResource> ResourceDeserializer(string source)
        {
            //determine type, then use appropriate deserializer
            var asValues = JsonSerializer.DeserializeFromString<Dictionary<string, string>>(source);

            string type = asValues["Type"];

            switch (type.ToLower())
            {
                case "file":
                    return JsonSerializer.DeserializeFromString<ContentReference<File>>(source);

                case "appresult":
                    return JsonSerializer.DeserializeFromString<ContentReference<AppResult>>(source);

                case "sample":
                    return JsonSerializer.DeserializeFromString<ContentReference<Sample>>(source);

                case "project":
                    return JsonSerializer.DeserializeFromString<ContentReference<Project>>(source);

                case "run":
                    return JsonSerializer.DeserializeFromString<ContentReference<Run>>(source);
            }

            return null;
        }
    }
}