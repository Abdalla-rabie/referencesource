﻿using System.Diagnostics.Contracts;

namespace System.Net.Http.Headers
{
    // Don't derive from BaseHeaderParser since parsing the Base64 string is delegated to Convert.FromBase64String() 
    // which will remove leading, trailing, and whitespaces in the middle of the string.
    internal class ByteArrayHeaderParser : HttpHeaderParser
    {
        internal static readonly ByteArrayHeaderParser Parser = new ByteArrayHeaderParser();

        private ByteArrayHeaderParser()
            : base(false)
        {
        }

        public override string ToString(object value)
        {
            Contract.Assert(value is byte[]);

            return Convert.ToBase64String((byte[])value);
        }

        public override bool TryParseValue(string value, object storeValue, ref int index, out object parsedValue)
        {
            parsedValue = null;

            // Some headers support empty/null values. This one doesn't.
            if (string.IsNullOrEmpty(value) || (index == value.Length))
            {
                return false;
            }
            
            string base64String = value;
            if (index > 0)
            {
                base64String = value.Substring(index);
            }

            // Try convert the string (we assume it's a valid Base64 string) to byte[].
            try
            {
                // The RFC specifies that the MD5 should be a 128 bit digest. However, we don't validate the length and
                // let the user decide whether or not the byte[] is a valid Content-MD5 value or not.
                parsedValue = Convert.FromBase64String(base64String);
                index = value.Length;
                return true;
            }
            catch (FormatException e)
            {
                if (Logging.On) Logging.PrintError(Logging.Http, string.Format(System.Globalization.CultureInfo.InvariantCulture, SR.net_http_parser_invalid_base64_string, base64String, e.Message));
            }

            return false;
        }
    }
}
