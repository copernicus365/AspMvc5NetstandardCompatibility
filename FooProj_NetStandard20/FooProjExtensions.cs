using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FooProj_NetStandard20
{
    public static class FooProjExtensions
    {
		public static string ToFunkyCase(this string str, int upperEvery)
		{
			if (string.IsNullOrEmpty(str))
				return str;

			char[] chars = str.ToCharArray();

			for (int i = upperEvery; i < str.Length; i += upperEvery)
				chars[i] = char.ToUpper(chars[i]);

			string result = new string(chars);
			return result;
		}

		public static async Task<string> GetHttpString(string url)
		{
			HttpClient client = new HttpClient();

			string val = await client.GetStringAsync(url);
			return val;
		}
	}
}
