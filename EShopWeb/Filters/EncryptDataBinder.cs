using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Cryptography;
using System.Text;

namespace EShopWeb.Filters
{
    public static class Crypto
    {
        public static string Encrypt(Dictionary<string,string> keyValues)
        {
            string QueryResult = "";
            for (int i = 0; i < keyValues.Count; i++)
            {
                if (i == 0)
                {
                    QueryResult += "?";
                }
                QueryResult += keyValues.Keys.ElementAt(i) + "=" + keyValues.Values.ElementAt(i);
            }
 
            string key = "jknm982387#";
            //EncryptKey = Array.Empty<byte>();
            byte[] IV = { 75, 65, 34, 56, 90, 99, 76, 34 };
            byte[] EncryptKey = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, 8));
            DESCryptoServiceProvider des = new();
            byte[] inputByte = Encoding.UTF8.GetBytes(QueryResult);
            MemoryStream mStream = new();
            CryptoStream cStream = new(mStream, des.CreateEncryptor(EncryptKey, IV), CryptoStreamMode.Write);
            cStream.Write(inputByte, 0, inputByte.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());
        }

        public static Dictionary<string,object> Decrypt(string enCryptedText)
        {
            Dictionary<string, object> keyValues = new();
            string key = "jknm982387#"; 
            byte[] IV = { 75, 65, 34, 56, 90, 99, 76, 34 };
            // inputBytes = new byte[enCryptedText.Length];
            byte[] DecryptKey = System.Text.Encoding.UTF8.GetBytes(key.Substring(0,8));
            DESCryptoServiceProvider serviceProvider = new();
            while (enCryptedText.Length % 4 != 0)
            {
                enCryptedText += "=";
            }
            enCryptedText = enCryptedText.Replace("-", "+").Replace("_", "/");
            byte[] inputBytes = Convert.FromBase64String(enCryptedText);
            MemoryStream mstream = new();
            CryptoStream cryptoStream = new(mstream, serviceProvider.CreateDecryptor(DecryptKey, IV),CryptoStreamMode.Write);
            cryptoStream.Write(inputBytes, 0, inputBytes.Length);
            cryptoStream.FlushFinalBlock();
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            string[] paraArray = encoding.GetString(mstream.ToArray()).Split('?');

            for (int i = 0; i < paraArray.Length; i++)
            {
                string[] paraArr = paraArray[i].Split('=');
                keyValues.Add(paraArr[0].ToUpper(), paraArr[1]);
            }
            return keyValues;
        }
    }

    public class EncryptDataBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (bindingContext.ModelType == typeof(int))
            {
                CompositeValueProvider providers = (CompositeValueProvider)bindingContext.ValueProvider;
                List<IValueProvider> values = providers.ToList();
                foreach (IValueProvider item in values)
                {
                    if (item is CryptoValueProvider)
                    {
                        if (item.ContainsPrefix("id"))
                        {
                            ValueProviderResult result = item.GetValue("id");
                            bindingContext.Result = ModelBindingResult.Success(int.Parse(result.FirstValue));
                            return Task.CompletedTask;
                        } 
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
