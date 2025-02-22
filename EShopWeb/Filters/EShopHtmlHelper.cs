using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;
using System.Text;

namespace EShopWeb
{
    public static class EShopHtmlHelper
    {  
        public static IHtmlContent EShopCoreEncryptedActionButtonLink(this IHtmlHelper helper, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes)
        { 
            TagBuilder anchortag = new("a");

            TagBuilder imagetag = new("i");
            imagetag.Attributes["title"] = linkText;

            if (htmlAttributes != null)
            {
                RouteValueDictionary d = new(htmlAttributes);
                for (int i = 0; i < d.Keys.Count; i++)
                {
                    if (d.Keys.ElementAt(i) == "class")
                    {
                        anchortag.AddCssClass(d.Values.ElementAt(i).ToString());
                    } 
                }
            } 

            IHtmlContent htmlContent = HtmlHelperLinkExtensions.ActionLink(helper,linkText, actionName, controllerName, routeValues, htmlAttributes);
            string result = GetString(htmlContent);
            var content = new HtmlContentBuilder();
            content.AppendHtml(new HtmlString(result));
            return content;
        }
 
        public static String GetString(IHtmlContent content)
        {
            using var writer = new System.IO.StringWriter();
            content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
            return System.Web.HttpUtility.HtmlDecode(writer.ToString());
        }
 
        public static string Encrypt(string plainText)
        {
            string key = "jknm982387#"; 
            byte[] IV = { 75, 65, 34, 56, 90, 99, 76, 34 };
            byte[] EncryptKey = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, 8));
            DESCryptoServiceProvider des = new();
            byte[] inputByte = Encoding.UTF8.GetBytes(plainText);
            MemoryStream mStream = new();
            CryptoStream cStream = new(mStream, des.CreateEncryptor(EncryptKey, IV), CryptoStreamMode.Write);
            cStream.Write(inputByte, 0, inputByte.Length);
            cStream.FlushFinalBlock();
            //string gg = Convert.ToBase64String(mStream.ToArray()).Replace('+', '-').Replace('/', '_').Replace("=", "");
            return Convert.ToBase64String(mStream.ToArray()).Replace('+', '-').Replace('/', '_').Replace("=", "");
        }
  
        public static IHtmlContent EnumToString<T>(this IHtmlHelper helper)
        {
           return helper.EnumToString<T>();
        }
 }
}