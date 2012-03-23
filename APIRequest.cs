﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;

namespace HastyAPI {
	public class APIRequest {
		private string _url;
		private object _headers;
		private string _data;
		private NetworkCredential _credentials;
		private Encoding _encoding = Encoding.UTF8;

		public APIRequest(string url) {
			_url = url;
		}

		public APIRequest WithHeaders(object headers) {
			_headers = headers;
			return this;
		}

		public APIRequest WithVars(object vars) {
			string data = "";
			var vardic = vars.AsDictionary();
			foreach(var pair in vardic) {
				if(data.Length > 0) data += "&";
				var value = pair.Value;
				if(value == null) value = "";
				data += HttpUtility.UrlEncode(pair.Key) + "=" + HttpUtility.UrlEncode(value);
			}
			_data = data;
			return this;
		}

		public APIRequest WithData(string data) {
			_data = data;
			return this;
		}

		public APIRequest WithBasicCredentials(string username, string password) {
			_credentials = new NetworkCredential(username, password);
			return this;
		}

		public APIRequest WithEncoding(Encoding encoding) {
			_encoding = encoding;
			return this;
		}

		public APIResponse Post() {
			return Send("POST");
		}

		public APIResponse Get() {
			return Send("GET");
		}

		public APIResponse Put() {
			return Send("PUT");
		}

		public APIResponse Send(string method) {
			HttpWebRequest req = null;

			if(_data != null) {
				if(method.Equals("GET", StringComparison.OrdinalIgnoreCase)) {
					req = (HttpWebRequest)WebRequest.Create(_url + "?" + _data);
					req.WithCredentials(_credentials).WithHeaders(_headers).Method = method;

				} else {
					req = (HttpWebRequest)WebRequest.Create(_url);
					req.WithCredentials(_credentials).WithHeaders(_headers).Method = method;

					req.ContentType = "application/x-www-form-urlencoded";

					var dataBytes = _encoding.GetBytes(_data);
					req.ContentLength = dataBytes.Length;

					var reqStream = req.GetRequestStream();
					reqStream.Write(dataBytes, 0, dataBytes.Length);
					reqStream.Close();
				}
			} else {
				req = (HttpWebRequest)WebRequest.Create(_url);
				if(method.Equals("POST", StringComparison.OrdinalIgnoreCase)) {
					req.ContentLength = 0;
				}
				req.WithCredentials(_credentials).WithHeaders(_headers).Method = method;
			}

			req.AllowAutoRedirect = false;

			HttpWebResponse response;
			try {
				response = (HttpWebResponse)req.GetResponse();
			} catch(WebException e) {
				if(e.Status == WebExceptionStatus.ConnectFailure) {
					throw new Exception("Couldn't connect to " + req.RequestUri.GetLeftPart(UriPartial.Authority));
				}
				return e.Response.ToAPIResponse();
			}

			return response.ToAPIResponse();
		}
	}
}
