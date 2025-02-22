using System.Text.Json;
using System.Text.Json.Serialization;

namespace EShopWeb.Common
{
    public class TimeSpanToStringConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            int miliseconds = 0;
            TimeSpan model = new();
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                return model;
            }

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.ValueSpan.ToString();
                    switch (propertyName)
                    {
                        case "days":
                            days = (int)reader.GetInt32();
                            break;
                        case "hours":
                            hours = (int)reader.GetInt32();
                            break;
                        case "milliseconds":
                            miliseconds = (int)reader.GetInt32();
                            break;
                        case "minutes":
                            minutes = (int)reader.GetInt32();
                            break;
                        case "seconds":
                            seconds = (int)reader.GetInt32();
                            break;
                        default:
                            break;
                    }
                }
            }
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                model = new TimeSpan(days, hours, minutes, seconds, miliseconds);
            }
            return model;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
            return;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return base.CanConvert(typeToConvert);
        }
    }
}
